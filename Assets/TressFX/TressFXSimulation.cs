using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

/// <summary>
/// This class is responsible for simulating the hair behaviour.
/// It will use ComputeShaders in order to do physics calculations on the GPU
/// </summary>
[RequireComponent(typeof(TressFX))]
public class TressFXSimulation : MonoBehaviour
{
	public ComputeShader HairSimulationShader;
	private TressFX master;

	/// <summary>
	/// Holds the time the compute shader needed to simulate in milliseconds.
	/// </summary>
	[HideInInspector]
	public float computationTime;

	public SphereCollider[] headColliders;

	// Kernel ID's
	private int IntegrationAndGlobalShapeConstraintsKernelId;
	private int LocalShapeConstraintsKernelId;
	private int LengthConstraintsAndWindKernelId;
	private int CollisionAndTangentsKernelId;
	private int SkipSimulationKernelId;

	// Buffers
	private ComputeBuffer hairLengthsBuffer;
	private ComputeBuffer globalRotationBuffer;
	private ComputeBuffer localRotationBuffer;
	private ComputeBuffer referenceBuffer;
	private ComputeBuffer verticeOffsetBuffer;
	private ComputeBuffer configBuffer;
	private ComputeBuffer colliderBuffer;

	// Config
	private float[] globalStiffness;
	private float[] globalStiffnessMatchingRange;
	private float[] localStiffness;
	private float[] damping;
	
	public float gravityMagnitude = 9.82f;
	public int lengthConstraintIterations = 5;
	public int localShapeConstraintIterations = 2;

	public Vector4 windDirection;
	public float windMagnitude;

	private Vector4 windForce1;
	private Vector4 windForce2;
	private Vector4 windForce3;
	private Vector4 windForce4;

	private ComputeBuffer hairStrandVerticeNums;
	private ComputeBuffer collisionTargetBuffer;

	private Matrix4x4 lastModelMatrix;

	/// <summary>
	/// The targets for checking the collisions.
	/// </summary>
	private List<Collider> collisionCheckTargets;

	public bool skipSimulation = false;

	private struct ColliderObject
	{
		/// <summary>
		/// The center position in local space.
		/// </summary>
		public Vector3 centerPosition;
		
		public float radius;

		/// <summary>
		/// The radius².
		/// </summary>
		public float radius2;
	}

	/// <summary>
	/// This loads the kernel ids from the compute buffer and also sets it's TressFX master.
	/// </summary>
	public void Initialize(float[] hairRestLengths, Vector3[] referenceVectors, int[] verticesOffsets,
	                       Quaternion[] localRotations, Quaternion[] globalRotations)
	{
		this.master = this.GetComponent<TressFX>();

		// Initialize config values
		this.globalStiffness = new float[this.master.hairData.Length];
		this.globalStiffnessMatchingRange = new float[this.master.hairData.Length];
		this.localStiffness = new float[this.master.hairData.Length];
		this.damping = new float[this.master.hairData.Length];

		// Load config
		for (int i = 0; i < this.master.hairData.Length; i++)
		{
			this.globalStiffness[i] = this.master.hairData[i].globalStiffness;
			this.globalStiffnessMatchingRange[i] = this.master.hairData[i].globalStiffnessMatchingRange;
			this.localStiffness[i] = this.master.hairData[i].localStiffness;
			this.damping[i] = this.master.hairData[i].damping;
		}

		// Initialize collision check targets
		this.collisionCheckTargets = new List<Collider>();

		this.lastModelMatrix = this.transform.localToWorldMatrix;

		if (this.master == null)
		{
			Debug.LogError ("TressFXSimulation doesnt have a master (TressFX)!");
		}

		// Initialize compute shader kernels
		this.IntegrationAndGlobalShapeConstraintsKernelId = this.HairSimulationShader.FindKernel("IntegrationAndGlobalShapeConstraints");
		this.LocalShapeConstraintsKernelId = this.HairSimulationShader.FindKernel("LocalShapeConstraints");
		this.CollisionAndTangentsKernelId = this.HairSimulationShader.FindKernel("CollisionAndTangents");
		this.LengthConstraintsAndWindKernelId = this.HairSimulationShader.FindKernel("LengthConstraintsAndWind");
		this.SkipSimulationKernelId = this.HairSimulationShader.FindKernel ("SkipSimulateHair");

		// Initialize collider buffer
		ColliderObject[] colliders = new ColliderObject[this.headColliders.Length];
		this.colliderBuffer = new ComputeBuffer (this.headColliders.Length, 20);

		for (int i = 0; i < this.headColliders.Length; i++)
		{
			// Scale collider
			float scale = Mathf.Max (new float[] { this.headColliders[i].transform.localScale.x, this.headColliders[i].transform.localScale.y, this.headColliders[i].transform.localScale.z }); 

			colliders[i] = new ColliderObject();
			colliders[i].centerPosition = this.headColliders[i].transform.localPosition + this.headColliders[i].center;
			colliders[i].radius = this.headColliders[i].radius * scale;
			colliders[i].radius2 = colliders[i].radius*colliders[i].radius;
		}

		this.colliderBuffer.SetData (colliders);

		// Set length buffer
		this.hairLengthsBuffer = new ComputeBuffer(this.master.vertexCount,4);
		this.hairLengthsBuffer.SetData(hairRestLengths);

		// Set rotation buffers
		this.globalRotationBuffer = new ComputeBuffer(this.master.vertexCount, 16);
		this.localRotationBuffer = new ComputeBuffer(this.master.vertexCount, 16);

		this.globalRotationBuffer.SetData(globalRotations);
		this.localRotationBuffer.SetData(localRotations);

		// Set reference buffers
		this.referenceBuffer = new ComputeBuffer(this.master.vertexCount, 12);
		this.referenceBuffer.SetData (referenceVectors);

		// Set offset buffer
		this.verticeOffsetBuffer = new ComputeBuffer(this.master.strandCount, 4);
		this.verticeOffsetBuffer.SetData (verticesOffsets);

		// Generate config buffer
		TressFXHairConfig[] hairConfig = new TressFXHairConfig[this.globalStiffness.Length];

		for (int i = 0; i < this.globalStiffness.Length; i++)
		{
			hairConfig[i] = new TressFXHairConfig();
			hairConfig[i].globalStiffness = this.globalStiffness[i];
			hairConfig[i].globalStiffnessMatchingRange = this.globalStiffnessMatchingRange[i];
			hairConfig[i].localStiffness = this.localStiffness[i];
			hairConfig[i].damping = this.damping[i];
		}

		this.configBuffer = new ComputeBuffer(hairConfig.Length, 16);
		this.configBuffer.SetData(hairConfig);

		// Initialize simulation
		this.SetResources ();
		Vector3 pos = this.transform.position;

		this.transform.position = Vector3.zero;
		this.HairSimulationShader.Dispatch (this.SkipSimulationKernelId, this.master.vertexCount, 1, 1);

		this.transform.position = pos;
		this.HairSimulationShader.Dispatch (this.SkipSimulationKernelId, this.master.vertexCount, 1, 1);
	}

	/// <summary>
	/// This functions dispatches the compute shader functions to simulate the hair behaviour
	/// </summary>
	public void LateUpdate()
	{
		long ticks = DateTime.Now.Ticks;

		// Simulate wind
		float wM = windMagnitude * (Mathf.Pow( Mathf.Sin(Time.frameCount*0.05f), 2.0f ) + 0.5f);
		
		Vector3 windDirN = this.windDirection.normalized;
		
		Vector3 XAxis = new Vector3(1,0,0);
		Vector3 xCrossW = Vector3.Cross (XAxis, windDirN);
		
		Quaternion rotFromXAxisToWindDir = Quaternion.identity;
		
		float angle = Mathf.Asin(xCrossW.magnitude);
		
		if ( angle > 0.001 )
		{
			rotFromXAxisToWindDir = Quaternion.AngleAxis(angle, xCrossW.normalized);
		}
		
		float angleToWideWindCone = 40.0f;
		
		{
			Vector3 rotAxis = new Vector3(0, 1.0f, 0);

			// Radians?
			Quaternion rot = Quaternion.AngleAxis(angleToWideWindCone, rotAxis);
			Vector3 newWindDir = rotFromXAxisToWindDir * rot * XAxis; 
			this.windForce1 = new Vector4(newWindDir.x * wM, newWindDir.y * wM, newWindDir.z * wM, Time.frameCount);
		}
		
		{
			Vector3 rotAxis = new Vector3(0, -1.0f, 0);
			Quaternion rot = Quaternion.AngleAxis(angleToWideWindCone, rotAxis);
			Vector3 newWindDir = rotFromXAxisToWindDir * rot * XAxis;
			this.windForce2 = new Vector4(newWindDir.x * wM, newWindDir.y * wM, newWindDir.z * wM, Time.frameCount);
		}
		
		{
			Vector3 rotAxis = new Vector3(0, 0, 1.0f);
			Quaternion rot = Quaternion.AngleAxis(angleToWideWindCone, rotAxis);
			Vector3 newWindDir = rotFromXAxisToWindDir * rot * XAxis;
			this.windForce3 = new Vector4(newWindDir.x * wM, newWindDir.y * wM, newWindDir.z * wM, Time.frameCount);
		}
		
		{
			Vector3 rotAxis = new Vector3(0, 0, -1.0f);
			Quaternion rot = Quaternion.AngleAxis(angleToWideWindCone, rotAxis);
			Vector3 newWindDir = rotFromXAxisToWindDir * rot * XAxis;
			this.windForce4 = new Vector4(newWindDir.x * wM, newWindDir.y * wM, newWindDir.z * wM, Time.frameCount);
		}
		this.SetResources();

		if (!this.skipSimulation)
		{
			this.DispatchKernels ();
			// Do collision check
			this.CheckCollisionTargets ();
		}
		else
		{
			this.HairSimulationShader.Dispatch (this.SkipSimulationKernelId, this.master.vertexCount, 1, 1);
		}
		
		this.computationTime = ((float) (DateTime.Now.Ticks - ticks) / 10.0f) / 1000.0f;
	}

	/// <summary>
	/// Checks the collision targets.
	/// This will rebuild the collisionTargetBuffer and fill it with the collision check target data.
	/// It will also dispatch the collision check kernel.
	/// </summary>
	private void CheckCollisionTargets()
	{
		TressFXColliderData[] colliders = new TressFXColliderData[this.collisionCheckTargets.Count];

		int index = 0;

		foreach(Collider c in this.collisionCheckTargets)
		{
			colliders[index].centerPoint = c.bounds.center;
			colliders[index].colliderType = 0; // Box collider
			colliders[index].size = c.bounds.size;
			index++;
		}
	}

	/// <summary>
	/// Sets the buffers and config values to all kernels in the compute shader.
	/// </summary>
	private void SetResources()
	{
		// Set main config
		this.HairSimulationShader.SetFloat ("g_TimeStep", Time.deltaTime);
		this.HairSimulationShader.SetInt ("NumStrands", this.master.strandCount);
		this.HairSimulationShader.SetFloat ("GravityMagnitude", this.gravityMagnitude);
		this.HairSimulationShader.SetInt ("NumLengthConstraintIterations", this.lengthConstraintIterations);
		this.HairSimulationShader.SetVector ("g_Wind", this.windForce1);
		this.HairSimulationShader.SetVector ("g_Wind2", this.windForce2);
		this.HairSimulationShader.SetVector ("g_Wind3", this.windForce3);
		this.HairSimulationShader.SetVector ("g_Wind4", this.windForce4);

		// Set matrices
		this.SetMatrices();

		// Set model rotation quaternion
		this.HairSimulationShader.SetFloats ("g_ModelRotateForHead", this.QuaternionToFloatArray(this.transform.rotation));

		this.HairSimulationShader.SetBuffer (this.LengthConstraintsAndWindKernelId, "g_HairVerticesOffsetsSRV", this.verticeOffsetBuffer);
		this.HairSimulationShader.SetBuffer (this.CollisionAndTangentsKernelId, "g_HairVerticesOffsetsSRV", this.verticeOffsetBuffer);

		// Set rest lengths buffer
		this.HairSimulationShader.SetBuffer(this.LengthConstraintsAndWindKernelId, "g_HairRestLengthSRV", this.hairLengthsBuffer);
		
		// Set vertex position buffers to all kernels
		this.SetVerticeInfoBuffers(this.SkipSimulationKernelId);
		this.SetVerticeInfoBuffers(this.IntegrationAndGlobalShapeConstraintsKernelId);
		this.SetVerticeInfoBuffers(this.LocalShapeConstraintsKernelId);
		this.SetVerticeInfoBuffers(this.LengthConstraintsAndWindKernelId);
		this.SetVerticeInfoBuffers(this.CollisionAndTangentsKernelId);

		// Set collider buffer to collision kernel
		this.HairSimulationShader.SetBuffer (this.CollisionAndTangentsKernelId, "g_Colliders", this.colliderBuffer);
		this.HairSimulationShader.SetFloat ("g_ColliderCount", this.headColliders.Length);
	}

	/// <summary>
	/// Sets the local shape constraints resources.
	/// This got moved into an own function because the local shape constraints can get dispatched iterative.
	/// </summary>
	private void SetLocalShapeConstraintsResources()
	{
		// Offsets buffer
		this.HairSimulationShader.SetBuffer (this.LocalShapeConstraintsKernelId, "g_HairVerticesOffsetsSRV", this.verticeOffsetBuffer);
		
		// Set rotation buffers
		this.HairSimulationShader.SetBuffer (this.LocalShapeConstraintsKernelId, "g_GlobalRotations", this.globalRotationBuffer);
		this.HairSimulationShader.SetBuffer (this.LocalShapeConstraintsKernelId, "g_LocalRotations", this.localRotationBuffer);
		
		// Set reference position buffers
		this.HairSimulationShader.SetBuffer(this.LocalShapeConstraintsKernelId, "g_HairRefVecsInLocalFrame", this.referenceBuffer);

		this.SetVerticeInfoBuffers(this.LocalShapeConstraintsKernelId);
	}

	/// <summary>
	/// Dispatchs the compute shader kernels.
	/// </summary>
	private void DispatchKernels()
	{
		// this.HairSimulationShader.Dispatch(this.SkipSimulationKernelId, this.master.vertexCount, 1, 1);
		this.HairSimulationShader.Dispatch(this.IntegrationAndGlobalShapeConstraintsKernelId, this.master.strandCount / 2, 1, 1);


		for (int i = 0; i < this.localShapeConstraintIterations; i++)
		{
			this.SetLocalShapeConstraintsResources();
			this.HairSimulationShader.Dispatch(this.LocalShapeConstraintsKernelId, Mathf.CeilToInt((float) this.master.strandCount / 64.0f), 1, 1);
		}

		this.HairSimulationShader.Dispatch(this.LengthConstraintsAndWindKernelId, this.master.strandCount / 2, 1, 1);
		this.HairSimulationShader.Dispatch(this.CollisionAndTangentsKernelId, this.master.strandCount / 2, 1, 1);
	}

	/// <summary>
	/// Sets the matrices needed by the compute shader.
	/// </summary>
	private void SetMatrices()
	{
		// Set last inverse matrix
		this.HairSimulationShader.SetFloats("g_ModelPrevInvTransformForHead", this.MatrixToFloatArray(this.lastModelMatrix.inverse));
		
		// Set current matrix
		this.HairSimulationShader.SetFloats ("g_ModelTransformForHead", this.MatrixToFloatArray (this.transform.localToWorldMatrix));
		
		this.lastModelMatrix = this.transform.localToWorldMatrix;
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	public void OnDestroy()
	{
		this.hairLengthsBuffer.Release ();
		this.localRotationBuffer.Release ();
		this.globalRotationBuffer.Release ();
		this.referenceBuffer.Release ();
		this.verticeOffsetBuffer.Release ();
		this.configBuffer.Release ();
		this.colliderBuffer.Release ();
	}

	/// <summary>
	/// Convertes a Matrix4x4 to a float array.
	/// </summary>
	/// <returns>The to float array.</returns>
	/// <param name="matrix">Matrix.</param>
	private float[] MatrixToFloatArray(Matrix4x4 matrix)
	{
		return new float[] 
		{
			matrix.m00, matrix.m01, matrix.m02, matrix.m03,
			matrix.m10, matrix.m11, matrix.m12, matrix.m13,
			matrix.m20, matrix.m21, matrix.m22, matrix.m23,
			matrix.m30, matrix.m31, matrix.m32, matrix.m33
		};
	}

	/// <summary>
	/// Quaternion to float array for passing to compute shader
	/// </summary>
	/// <returns>The to float array.</returns>
	/// <param name="quaternion">Quaternion.</param>
	private float[] QuaternionToFloatArray(Quaternion quaternion)
	{
		return new float[]
		{
			quaternion.x,
			quaternion.y,
			quaternion.z,
			quaternion.w
		};
	}

	/// <summary>
	/// Sets the strand info buffers to a kernel with the given id
	/// </summary>
	private void SetVerticeInfoBuffers(int kernelId)
	{
		this.HairSimulationShader.SetBuffer(kernelId, "g_InitialHairPositions", this.master.InitialVertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(kernelId, "g_HairVertexPositions", this.master.VertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(kernelId, "g_HairVertexPositionsPrev", this.master.LastVertexPositionBuffer);

		// Set vertice config / indices
		this.HairSimulationShader.SetBuffer (kernelId, "g_HairVerticesOffsetsSRV", this.verticeOffsetBuffer);
		this.HairSimulationShader.SetBuffer (kernelId, "g_HairStrandType", this.master.HairIndicesBuffer);
		this.HairSimulationShader.SetBuffer (kernelId, "g_Config", this.configBuffer);
	}
	
	public void AddCollisionTarget(Collider collider)
	{
		this.collisionCheckTargets.Add(collider);
		Debug.Log ("TressFX Collision target added: " + collider);
	}
	
	public void RemoveCollisionTarget(Collider collider)
	{
		this.collisionCheckTargets.Remove(collider);
		Debug.Log ("TressFX Collision target removed: " + collider);
	}
}
