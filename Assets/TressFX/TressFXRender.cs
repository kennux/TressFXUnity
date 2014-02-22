using UnityEngine;
using System.Collections;

/// <summary>
/// This class handles the rendering of the hairs.
/// It uses Unity's Graphics.DrawProcedural function to draw the hair mesh procedurally.
/// Currently it can only draw LineStrip hairs, a version with quads / triangles to simulate more exact and detailed hair behaviour.
/// </summary>
[RequireComponent(typeof(TressFX))]
public class TressFXRender : MonoBehaviour
{
	/// <summary>
	/// This shader will get used by the Graphcis.DrawProcedural function.
	/// </summary>
	public Shader hairRenderingShader;

	// Private ressources needed for rendering
	private TressFX master;
	private Material hairMaterial;
	
	/// <summary>
	/// This holds the hair color (_HairColor in shader) which will get passed to the rendering shader
	/// </summary>
	public Color HairColor;

	/// <summary>
	/// Initializes the renderer.
	/// </summary>
	public void Initialize()
	{
		this.master = this.GetComponent<TressFX>();
		if (this.master == null)
		{
			Debug.LogError ("TressFXRender doesnt have a master (TressFX)!");
		}

		// Initialize material
		this.hairMaterial = new Material(this.hairRenderingShader);
	}

	/// <summary>
	/// Raises the destroy event.
	/// This will free the hair rendering material.
	/// </summary>
	public void OnDestroy()
	{
		Object.DestroyImmediate(this.hairMaterial);
	}

	/// <summary>
	/// Raises the render object event.
	/// This will render the hairs with the shader hairRenderingShader.
	/// </summary>
	public void OnRenderObject()
	{
		// Hair material initialized?
		if (this.hairMaterial != null)
		{
			this.hairMaterial.SetPass(0);
			this.hairMaterial.SetColor ("_HairColor", this.HairColor);
			this.hairMaterial.SetBuffer ("_VertexPositionBuffer", this.master.VertexPositionBuffer);
			this.hairMaterial.SetBuffer ("_StrandIndicesBuffer", this.master.strandIndicesBuffer);

			Graphics.DrawProcedural(MeshTopology.LineStrip, this.master.vertexCount);
		}
	}
}
