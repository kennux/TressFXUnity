using UnityEngine;
using System.Collections.Generic;
using System;

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

	// Configuration
	public float stiffnessForGlobalShapeMatching = 0.8f;
	public float globalShapeMatchingEffectiveRange = 0.5f;
	public float damping = 0.5f;

	// Kernel ID's
	private int IntegrationAndGlobalShapeConstraintsKernelId;
	private int LocalShapeConstraintsKernelId;
	private int CollisionCheckKernelId;

	// Buffers
	private ComputeBuffer colliderBuffer;
	private ComputeBuffer debugBuffer;

	/// <summary>
	/// This loads the kernel ids from the compute buffer and also sets it's TressFX master.
	/// </summary>
	public void Initialize(TressFXCapsuleCollider headCollider)
	{
		this.master = this.GetComponent<TressFX>();
		if (this.master == null)
		{
			Debug.LogError ("TressFXSimulation doesnt have a master (TressFX)!");
		}

		// Initialize compute buffer
		this.IntegrationAndGlobalShapeConstraintsKernelId = this.HairSimulationShader.FindKernel("IntegrationAndGlobalShapeConstraints");
		this.LocalShapeConstraintsKernelId = this.HairSimulationShader.FindKernel("LocalShapeConstraints");
		this.CollisionCheckKernelId = this.HairSimulationShader.FindKernel("CollisionCheck");

		// Initialize collision buffer
		this.colliderBuffer = new ComputeBuffer(1, 32);
		this.colliderBuffer.SetData(new TressFXCapsuleCollider[] { headCollider });

		this.debugBuffer = new ComputeBuffer(this.master.vertexCount, 12 * 2);
	}

	/// <summary>
	/// This functions dispatches the compute shader functions to simulate the hair behaviour
	/// </summary>
	public void Update()
	{
		long ticks = DateTime.Now.Ticks;

		// Set configuration
		this.HairSimulationShader.SetFloat("timeT", Time.time);
		this.HairSimulationShader.SetFloat("timeStep", Time.deltaTime);
		this.HairSimulationShader.SetFloat("damping", this.damping);
		this.HairSimulationShader.SetVector("windForce", new Vector4(20.0f,0.0f,0.0f,1.0f));
		this.HairSimulationShader.SetFloat("globalShapeMatchingEffectiveRange", this.globalShapeMatchingEffectiveRange);
		this.HairSimulationShader.SetFloat("stiffnessForGlobalShapeMatching", this.stiffnessForGlobalShapeMatching);

		// Set Matrices
		this.SetMatrices();

		// Configure & Dispatch simulation shader (wind force)
		this.SetStrandInfoBuffers(this.IntegrationAndGlobalShapeConstraintsKernelId);

		this.HairSimulationShader.Dispatch(this.IntegrationAndGlobalShapeConstraintsKernelId, this.master.vertexCount, 1, 1);

		// Configure & Dispatch Shape Constraint shader
		this.SetStrandInfoBuffers(this.LocalShapeConstraintsKernelId);

		this.HairSimulationShader.Dispatch(this.LocalShapeConstraintsKernelId, this.master.vertexCount, 1, 1);

		// Set last inverse matrix
		this.HairSimulationShader.SetFloats("LastInverseHeadModelMatrix", this.MatrixToFloatArray(this.transform.localToWorldMatrix.inverse));

		// Set collider buffer
		this.HairSimulationShader.SetBuffer(this.CollisionCheckKernelId, "b_Colliders", this.colliderBuffer);

		this.SetStrandInfoBuffers(this.CollisionCheckKernelId);
		this.HairSimulationShader.SetBuffer(this.CollisionCheckKernelId, "debug", this.debugBuffer);

		TressFXCapsuleCollider[] c = new TressFXCapsuleCollider[1];
		this.colliderBuffer.GetData(c);

		// Dispatch collision check shader
		this.HairSimulationShader.Dispatch(this.CollisionCheckKernelId, this.master.vertexCount, 1, 1);
		
		Vector3[] debug = new Vector3[this.master.vertexCount];
		Vector3[] debug2 = new Vector3[this.master.vertexCount];
		this.debugBuffer.GetData (debug);
		this.master.VertexPositionBuffer.GetData(debug2);

		if (debug2[0].x != 0)
		{
			int i = 0;
		}

		this.computationTime = ((float) (ticks - DateTime.Now.Ticks) / 10.0f) / 1000.0f;
	}

	/// <summary>
	/// Sets the matrices needed by the compute shader.
	/// </summary>
	private void SetMatrices()
	{
		Matrix4x4 HeadModelMatrix = this.transform.localToWorldMatrix;

		this.HairSimulationShader.SetFloats("InverseHeadModelMatrix", this.MatrixToFloatArray(HeadModelMatrix.inverse));
		this.HairSimulationShader.SetFloats("HeadModelMatrix", this.MatrixToFloatArray(HeadModelMatrix));
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
	/// Sets the strand info buffers to a kernel with the given id
	/// </summary>
	private void SetStrandInfoBuffers(int kernelId)
	{
		this.HairSimulationShader.SetBuffer(kernelId, "b_InitialVertexPosition", this.master.InitialVertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(kernelId, "b_CurrentVertexPosition", this.master.VertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(kernelId, "b_LastVertexPosition", this.master.LastVertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(kernelId, "b_StrandIndices", this.master.StrandIndicesBuffer);
	}
}
