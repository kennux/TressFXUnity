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

	// Config
	public float[] globalStiffness;
	public float[] globalStiffnessMatchingRange;
	public float[] localStiffness;
	public float[] damping;
	
	public float gravityMagnitude = 9.82f;
	public int lengthConstraintIterations = 5;
	public int localShapeConstraintIterations = 2;
	
	public Vector4 windForce1;
	public Vector4 windForce2;
	public Vector4 windForce3;
	public Vector4 windForce4;

	private ComputeBuffer hairStrandVerticeNums;

	/// <summary>
	/// This loads the kernel ids from the compute buffer and also sets it's TressFX master.
	/// </summary>
	public void Initialize(float[] hairRestLengths, Vector3[] referenceVectors, int[] verticesOffsets,
	                       Quaternion[] localRotations, Quaternion[] globalRotations)
	{
		this.master = this.GetComponent<TressFX>();
		if (this.master == null)
		{
			Debug.LogError ("TressFXSimulation doesnt have a master (TressFX)!");
		}

		// Initialize compute buffer
		this.IntegrationAndGlobalShapeConstraintsKernelId = this.HairSimulationShader.FindKernel("IntegrationAndGlobalShapeConstraints");
		this.LocalShapeConstraintsKernelId = this.HairSimulationShader.FindKernel("LocalShapeConstraints");
		this.CollisionAndTangentsKernelId = this.HairSimulationShader.FindKernel("CollisionAndTangents");
		this.LengthConstraintsAndWindKernelId = this.HairSimulationShader.FindKernel("LengthConstraintsAndWind");
		this.SkipSimulationKernelId = this.HairSimulationShader.FindKernel ("SkipSimulateHair");

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
		
		this.HairSimulationShader.SetFloats("g_ModelPrevInvTransformForHead", this.MatrixToFloatArray(this.transform.localToWorldMatrix.inverse));
	}

	/// <summary>
	/// This functions dispatches the compute shader functions to simulate the hair behaviour
	/// </summary>
	public void LateUpdate()
	{
		long ticks = DateTime.Now.Ticks;
		
		this.SetResources();
		this.DispatchKernels();
		
		this.computationTime = ((float) (DateTime.Now.Ticks - ticks) / 10.0f) / 1000.0f;
		
		// Set last inverse matrix
		this.HairSimulationShader.SetFloats("g_ModelPrevInvTransformForHead", this.MatrixToFloatArray(this.transform.localToWorldMatrix.inverse));
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

		// Set vertex position buffers to skip simulate kernel
		this.SetVerticeInfoBuffers(this.SkipSimulationKernelId);
		this.SetVerticeInfoBuffers(this.IntegrationAndGlobalShapeConstraintsKernelId);
		this.SetVerticeInfoBuffers(this.LocalShapeConstraintsKernelId);
		this.SetVerticeInfoBuffers(this.LengthConstraintsAndWindKernelId);
		this.SetVerticeInfoBuffers(this.CollisionAndTangentsKernelId);
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
		Matrix4x4 HeadModelMatrix = this.transform.localToWorldMatrix;

		// this.HairSimulationShader.SetFloats("InverseHeadModelMatrix", this.MatrixToFloatArray(HeadModelMatrix.inverse));
		this.HairSimulationShader.SetFloats("g_ModelTransformForHead", this.MatrixToFloatArray(HeadModelMatrix));
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
}
