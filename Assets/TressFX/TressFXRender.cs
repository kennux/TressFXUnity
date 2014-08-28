using UnityEngine;
using System.Collections;

/// <summary>
/// Tress FX render.
/// ONLY FOR TESTING PURPOSES!
/// THIS RENDERING DOES NOT SUPPORT LIGHTING OR SHADOWS OR ANYTHING ELSE!
/// JUST FOR TESTING!!!
/// </summary>
[RequireComponent(typeof(TressFX))]
public class TressFXRender : MonoBehaviour
{
	public Shader shader;

	private Material mat;
	private TressFX master;

	public void Start()
	{
		this.master = this.GetComponent<TressFX> ();
		this.mat = new Material (this.shader);
	}

	public void OnRenderObject()
	{
		this.mat.SetPass (0);
		this.mat.SetBuffer ("_VertexPositionBuffer", this.master.m_HairVertexPositions);
		Graphics.DrawProcedural (MeshTopology.Lines, this.master.hairData.m_NumGuideHairVertices);
	}
}
