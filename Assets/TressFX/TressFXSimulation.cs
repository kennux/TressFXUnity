using UnityEngine;
using System.Collections.Generic;

public class TressFXSimulation : MonoBehaviour
{
	public ComputeShader HairSimulationShader;

	private TressFX master;

	public void Initialize()
	{
		this.master = this.GetComponent<TressFX>();
		if (this.master == null)
		{
			Debug.LogError ("TressFXSimulation doesnt have a master (TressFX)!");
		}

		// Initialize compute buffer
	}

	public void Update()
	{
		// Dispatch simulation shader (wind force)
		this.HairSimulationShader.SetBuffer(0, "b_InitialVertexPosition", this.master.InitialVertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(0, "b_CurrentVertexPosition", this.master.VertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(0, "b_LastVertexPosition", this.master.LastVertexPositionBuffer);
		this.HairSimulationShader.SetBuffer(0, "b_StrandIndices", this.master.strandIndicesBuffer);
		this.HairSimulationShader.SetFloat("timeT", Time.time);

		this.HairSimulationShader.Dispatch(0, this.master.vertexCount, 1, 1);
	}
}
