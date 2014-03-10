using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// This class handles the rendering of the hairs.
/// It uses Unity's Graphics.DrawProcedural function to draw the hair mesh procedurally.
/// Currently it can only draw LineStrip hairs, a version with quads / triangles to simulate more exact and detailed hair behaviour.
/// </summary>
[RequireComponent(typeof(TressFX))]
public class TressFXRender : MonoBehaviour
{
	public static List<TressFXRender> instances;

	private static void AddInstance(TressFXRender instance)
	{
		if (instances == null)
		{
			instances = new List<TressFXRender>();
		}
		instances.Add (instance);
	}

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
	/// Holds the time the renderer needed to render in milliseconds.
	/// </summary>
	[HideInInspector]
	public float renderTime;

	/// <summary>
	/// The fiber radius (aka hair thickness)
	/// </summary>
	public float fiberRadius = 0.01f;

	/// <summary>
	/// The expand pixels.
	/// </summary>
	public bool expandPixels = true;

	/// <summary>
	/// Use thin tip?
	/// </summary>
	public bool thinTip = false;

	private ComputeBuffer LinkedListUAV;
	private int oldWidth, oldHeight;

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
		this.hairMaterial = new Material (this.hairRenderingShader);
		AddInstance (this);

		// Linked list uav
		this.LinkedListUAV = new ComputeBuffer (8 * Screen.width, Screen.height, 12);

		// Initialize old screen size
		this.oldWidth = Screen.width;
		this.oldHeight = Screen.height;
	}

	public void Update()
	{
		if (this.oldWidth != Screen.width || this.oldHeight != Screen.height)
		{
			// Re-create buffer
			this.LinkedListUAV = new ComputeBuffer (8 * Screen.width, Screen.height, 12);
		}
	}

	/// <summary>
	/// Raises the destroy event.
	/// This will free the hair rendering material.
	/// </summary>
	public void OnDestroy()
	{
		UnityEngine.Object.DestroyImmediate(this.hairMaterial);
	}

	/// <summary>
	/// Raises the render object event.
	/// This will render the hairs with the shader hairRenderingShader.
	/// </summary>
	public void RenderHair()
	{
		long ticks = DateTime.Now.Ticks;

		// Hair material initialized?
		if (this.hairMaterial != null)
		{
			/*this.hairMaterial.SetPass (0);
			this.hairMaterial.SetColor ("_HairColor", this.HairColor);
			this.hairMaterial.SetBuffer ("_VertexPositionBuffer", this.master.VertexPositionBuffer);
			this.hairMaterial.SetBuffer ("_StrandIndicesBuffer", this.master.StrandIndicesBuffer);
			this.hairMaterial.SetFloat ("_HairThickness", this.hairThickness);
			this.hairMaterial.SetVector("_CameraDirection", new Vector4(Camera.main.transform.forward.x, Camera.main.transform.forward.y, Camera.main.transform.forward.z, 0));

			// Graphics.SetRenderTarget(this.postRender.hairRenderingTexture);
			Graphics.DrawProcedural(MeshTopology.LineStrip, this.master.vertexCount);*/
			// Graphics.SetRenderTarget(null);
			RenderTexture LinkedListHeadUAV = RenderTexture.GetTemporary(Screen.width, Screen.height, 8, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			LinkedListHeadUAV.DiscardContents();

			this.hairMaterial.SetPass(1);
			this.hairMaterial.SetColor("_HairColor", this.HairColor);
			this.hairMaterial.SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
			this.hairMaterial.SetBuffer("g_HairVertexTangents", this.master.TangentsBuffer);
			this.hairMaterial.SetBuffer("g_TriangleIndicesBuffer", this.master.TriangleIndicesBuffer);
			this.hairMaterial.SetVector("g_vEye", Camera.main.transform.position);
			this.hairMaterial.SetVector("g_WinSize", new Vector4(Screen.width, Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
			this.hairMaterial.SetFloat("g_FiberRadius", this.fiberRadius);
			this.hairMaterial.SetFloat("g_bExpandPixels", this.expandPixels ? 0 : 1);
			this.hairMaterial.SetFloat("g_bThinTip", this.thinTip ? 0 : 1);
			// this.hairMaterial.("LinkedListHeadUAV", LinkedListHeadUAV);
			Graphics.SetRandomWriteTarget(0, LinkedListHeadUAV);
			Graphics.SetRandomWriteTarget(1, this.LinkedListUAV);

			Graphics.DrawProcedural(MeshTopology.Triangles, this.master.triangleIndexCount);

			Graphics.ClearRandomWriteTargets();
		}

		this.renderTime = ((float) (DateTime.Now.Ticks - ticks) / 10.0f) / 1000.0f;
	}

	public void OnRenderObject()
	{
		this.RenderHair ();
	}
}
