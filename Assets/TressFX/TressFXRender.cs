using UnityEngine;
using System.Collections.Generic;
using System.Collections;
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

	private ComputeBuffer debug;

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
		this.LinkedListUAV = new ComputeBuffer (8 * Screen.width * Screen.height, 12);

		// Initialize old screen size
		this.oldWidth = Screen.width;
		this.oldHeight = Screen.height;

		this.debug = new ComputeBuffer(10, 4);
		this.debug.SetData (new float[] { 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f, 1.0f });
	}

	public void Update()
	{
		if (this.oldWidth != Screen.width || this.oldHeight != Screen.height)
		{
			// Re-create buffer
			this.LinkedListUAV = new ComputeBuffer (8 * Screen.width * Screen.height, 12);
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
			RenderTexture LinkedListHeadUAV = RenderTexture.GetTemporary(Screen.width, Screen.height, 8, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Default);
			RenderTexture.active = LinkedListHeadUAV;
			GL.Clear (true, true, Color.white);
			RenderTexture.active = null;

			this.SetShaderData(LinkedListHeadUAV);
			this.hairMaterial.SetPass(0);

			// A-Buffer Pass
			Graphics.DrawProcedural(MeshTopology.Triangles, this.master.triangleIndexCount);
			
			/*PPL_Struct[] pplData = new PPL_Struct[8*Screen.width*Screen.height];
			this.LinkedListUAV.GetData(pplData);

			foreach (PPL_Struct da in pplData)
			{
				if (da.depth != 0 || da.TangentAndCoverage != 0 || da.uNext != 0)
				{
					int te = 1;
				}
			}*/

			// Graphics.DrawTexture(new Rect(0,0,200,200), LinkedListHeadUAV);

			// K-Buffer Pass

			// Draw fullscreen quad
			/*GL.PushMatrix();
			{
				GL.LoadOrtho();

				this.hairMaterial.SetPass(1);
				this.SetShaderData(LinkedListHeadUAV);

				GL.Begin(GL.TRIANGLES);
				{
					GL.TexCoord2(0.0f, 1.0f);
					GL.Vertex3(-1.0f, -1.0f, 0.0f);
					GL.TexCoord2(0.0f, 0.0f);
					GL.Vertex3(-1.0f, 1.0f, 0.0f);
					GL.TexCoord2(1.0f, 1.0f);
					GL.Vertex3(1.0f, -1.0f, 0.0f);
					
					GL.TexCoord2(1.0f, 1.0f);
					GL.Vertex3(1.0f, -1.0f, 0.0f);
					GL.TexCoord2(0.0f, 0.0f);
					GL.Vertex3(-1.0f, 1.0f, 0.0f);
					GL.TexCoord2(1.0f, 0.0f);
					GL.Vertex3(1.0f, 1.0f, 0.0f);
				}
				GL.End();
			}
			GL.PopMatrix();*/

			// Graphics.ClearRandomWriteTargets();
		}

		this.renderTime = ((float) (DateTime.Now.Ticks - ticks) / 10.0f) / 1000.0f;
	}

	private void SetShaderData(RenderTexture LinkedListHeadUAV)
	{
		Graphics.SetRandomWriteTarget(3, this.debug);
		// this.hairMaterial.SetBuffer("debug", this.debug);
		this.hairMaterial.SetColor("_HairColor", this.HairColor);
		this.hairMaterial.SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
		this.hairMaterial.SetBuffer("g_HairVertexTangents", this.master.TangentsBuffer);
		this.hairMaterial.SetBuffer("g_TriangleIndicesBuffer", this.master.TriangleIndicesBuffer);
		this.hairMaterial.SetVector("g_vEye", Camera.main.transform.position);
		this.hairMaterial.SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
		this.hairMaterial.SetFloat("g_FiberRadius", 0.14f); // this.fiberRadius);
		this.hairMaterial.SetFloat("g_bExpandPixels", this.expandPixels ? 0 : 1);
		this.hairMaterial.SetFloat("g_bThinTip", this.thinTip ? 0 : 1);
		this.hairMaterial.SetMatrix("g_mInvViewProj", (Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix).inverse);
		this.hairMaterial.SetFloat ("g_FiberAlpha", 1.0f); // 0.33f); // this.HairColor.a);
		this.hairMaterial.SetFloat ("g_alphaThreshold", 0.003f); // this.HairColor.a);
		/*this.hairMaterial.SetTexture("LinkedListHeadUAV", LinkedListHeadUAV);
		this.hairMaterial.SetBuffer("LinkedListUAV", this.LinkedListUAV);*/
		Graphics.SetRandomWriteTarget(1, LinkedListHeadUAV);
		Graphics.SetRandomWriteTarget(2, this.LinkedListUAV);
	}
	
	public void OnRenderObject()
	{
		this.RenderHair ();
	}
}

public struct PPL_Struct
{
	public uint TangentAndCoverage;	
	public uint depth;
	public uint uNext;
}
