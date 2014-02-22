using UnityEngine;
using System.Collections;

public class TressFXRender : MonoBehaviour
{
	public Shader hairRenderingShader;
	
	private TressFX master;
	private Material hairMaterial;
	private ComputeBuffer drawArguments;

	
	public void Initialize()
	{
		this.master = this.GetComponent<TressFX>();
		if (this.master == null)
		{
			Debug.LogError ("TressFXRender doesnt have a master (TressFX)!");
		}

		// Initialize material
		this.hairMaterial = new Material(this.hairRenderingShader);

		// Initialize arguments
		this.drawArguments = new ComputeBuffer(1, 16);
		int[] args = new int[4];
		args[0] = 230668;
		args[1] = 1;
		args[2] = 0;
		args[3] = 0;
		this.drawArguments.SetData (args);
	}
	
	public void OnRenderObject()
	{
		if (this.hairMaterial != null)
		{
			this.hairMaterial.SetPass(0);
			this.hairMaterial.SetColor ("_HairColor", this.master.HairColor);
			this.hairMaterial.SetBuffer ("_VertexPositionBuffer", this.master.VertexPositionBuffer);
			this.hairMaterial.SetBuffer ("_StrandIndicesBuffer", this.master.strandIndicesBuffer);

			// Graphics.DrawProceduralIndirect(MeshTopology.LineStrip, this.drawArguments);
			Graphics.DrawProcedural(MeshTopology.LineStrip, this.master.vertexCount);
		}
	}
}
