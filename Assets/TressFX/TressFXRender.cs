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
	private RenderTexture LinkedListHeadUAV;
	private ComputeBuffer LinkedListHeadUAVBuffer;
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

		// Initialize old screen size
		this.oldWidth = Screen.width;
		this.oldHeight = Screen.height;

		this.debug = new ComputeBuffer(10, 4);
		this.debug.SetData (new float[] { 1 });

		this.CreateResources();
	}

	public void Update()
	{
		if (this.oldWidth != Screen.width || this.oldHeight != Screen.height)
		{
			// Re-create resources
			this.CreateResources();
		}
	}

	private void CreateResources()
	{
		this.LinkedListHeadUAV = new RenderTexture(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
		this.LinkedListHeadUAV.enableRandomWrite = true;
		this.LinkedListHeadUAV.Create();

		this.LinkedListUAV = new ComputeBuffer (8 * Screen.width * Screen.height, 12, ComputeBufferType.Counter);
		this.LinkedListHeadUAVBuffer = new ComputeBuffer(Screen.width * Screen.height, 4);

		Graphics.ClearRandomWriteTargets();
		Graphics.SetRandomWriteTarget(1, this.LinkedListHeadUAV);
		Graphics.SetRandomWriteTarget(2, this.LinkedListUAV);
		Graphics.SetRandomWriteTarget(3, this.debug);

		// Initialize
		/*PPL_Struct[] initialData = new PPL_Struct[8 * Screen.width * Screen.height];
		for (int i = 0; i < 8 * Screen.width * Screen.height; i++)
		{
			initialData[i] = new PPL_Struct();
		}
		this.LinkedListUAV.SetData(initialData);*/
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
			// Clear render texture
			RenderTexture.active = this.LinkedListHeadUAV;
			GL.Clear(true, true, Color.white);
			RenderTexture.active = null;

			this.SetShaderData();
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

			// K-Buffer Pass
			/*int[] test2 = new int[Screen.width * Screen.height];
			this.LinkedListHeadUAVBuffer.GetData(test2);
			foreach (int da in test2)
			{
				if (da != 0)
				{
					int te = 1;
				}
			}*/

			// Draw fullscreen quad
			GL.PushMatrix();
			{
				GL.LoadOrtho();

				this.hairMaterial.SetPass(1);
				this.hairMaterial.SetTexture("LinkedListHeadSRV", this.LinkedListHeadUAV);
				this.hairMaterial.SetBuffer("LinkedListSRV", this.LinkedListUAV);
				this.SetShaderData();

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
			GL.PopMatrix();
		}

		Graphics.DrawTexture(new Rect(0,0,100,100), this.LinkedListHeadUAV);

		Debug.Log (Screen.width + " " + Screen.height);
		this.renderTime = ((float) (DateTime.Now.Ticks - ticks) / 10.0f) / 1000.0f;
	}

	private void SetShaderData()
	{
		Matrix4x4 g_mInvViewProjViewport = new Matrix4x4();
		Matrix4x4 mViewport = new Matrix4x4();
		
		mViewport[0,0] = 2.0f / Screen.width;
		mViewport[0,1] = 0.0f;
		mViewport[0,2] = 0.0f;
		mViewport[0,3] = 0.0f;
		mViewport[1,0] = 0.0f;
		mViewport[1,1] = 2.0f / Screen.height;
		mViewport[1,2] = 0.0f;
		mViewport[1,3] = 0.0f;
		mViewport[2,0] = 0.0f;
		mViewport[2,1] = 0.0f;
		mViewport[2,2] = 1.0f;
		mViewport[2,3] = 0.0f;
		mViewport[3,0] = -1.0f;
		mViewport[3,1] = 1.0f;
		mViewport[3,2] = 0.0f;
		mViewport[3,3] = 1.0f;

		g_mInvViewProjViewport = (Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix).inverse * mViewport;
		
		this.hairMaterial.SetColor("_HairColor", this.HairColor);
		this.hairMaterial.SetMatrix("_VPMatrix", Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix);
		this.hairMaterial.SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
		this.hairMaterial.SetBuffer("g_HairVertexTangents", this.master.TangentsBuffer);
		this.hairMaterial.SetBuffer("g_TriangleIndicesBuffer", this.master.TriangleIndicesBuffer);
		this.hairMaterial.SetVector("g_vEye", Camera.main.transform.position);
		this.hairMaterial.SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
		this.hairMaterial.SetFloat("g_FiberRadius", 0.14f); // this.fiberRadius);
		this.hairMaterial.SetFloat("g_bExpandPixels", this.expandPixels ? 0 : 1);
		this.hairMaterial.SetFloat("g_bThinTip", this.thinTip ? 0 : 1);
		this.hairMaterial.SetMatrix("g_mInvViewProj", (Camera.main.projectionMatrix * Camera.main.worldToCameraMatrix).inverse);
		this.hairMaterial.SetMatrix("g_mInvViewProjViewport", g_mInvViewProjViewport);
		this.hairMaterial.SetFloat ("g_FiberAlpha", 1.0f); // 0.33f); // this.HairColor.a);
		this.hairMaterial.SetFloat ("g_alphaThreshold", 0.003f); // this.HairColor.a);
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
