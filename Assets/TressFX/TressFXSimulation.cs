using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for simulating the hair behaviour.
/// It will use ComputeShaders in order to do physics calculations on the GPU
/// </summary>
[RequireComponent(typeof(TressFX))]
public class TressFXSimulation : MonoBehaviour
{
	public ComputeShader HairSimulationShader;
	private TressFX master;

	// Kernel ID's
	private int WindSimulationKernelId;
	private int ShapeConstraintsKernelId;

	/// <summary>
	/// This loads the kernel ids from the compute buffer and also sets it's TressFX master.
	/// </summary>
	public void Initialize()
	{
		this.master = this.GetComponent<TressFX>();
		if (this.master == null)
		{
			Debug.LogError ("TressFXSimulation doesnt have a master (TressFX)!");
		}

		// Initialize compute buffer
		this.WindSimulationKernelId = this.HairSimulationShader.FindKernel("WindSimluation");
		this.ShapeConstraintsKernelId = this.HairSimulationShader.FindKernel("ShapeConstraints");
	}

	/// <summary>
	/// This functions dispatches the compute shader functions to simulate the hair behaviour
	/// </summary>
	public void Update()
	{
		// Configure & Dispatch simulation shader (wind force)
		this.SetStrandInfoBuffers(this.WindSimulationKernelId);

		this.HairSimulationShader.SetFloat("timeT", Time.time);
		this.HairSimulationShader.SetFloat("damping", 0.1f);
		this.HairSimulationShader.SetVector("windForce", new Vector4(2.0f,0.0f,0.0f,1.0f));

		this.HairSimulationShader.Dispatch(this.WindSimulationKernelId, this.master.vertexCount, 1, 1);

		// Configure & Dispatch Shape Constraint shader
		this.SetStrandInfoBuffers(this.ShapeConstraintsKernelId);

		this.HairSimulationShader.Dispatch(this.ShapeConstraintsKernelId, this.master.vertexCount, 1, 1);
	}

	/// <summary>
	/// Sets the strand info buffers to a kernel with the given id
	/// </summary>
	private void SetStrandInfoBuffers(int kernelId)
	{
		this.HairSimulationShader.SetBuffer(kernelId, "b_InitialVertexPosition", this.master.InitialVertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(kernelId, "b_CurrentVertexPosition", this.master.VertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(kernelId, "b_LastVertexPosition", this.master.LastVertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(kernelId, "b_StrandIndices", this.master.strandIndicesBuffer);
	}
}
