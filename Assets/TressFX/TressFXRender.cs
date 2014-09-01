using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct PPLL
{
	public uint tangentAndDepth;
	public uint depth;
	public uint uNext;
}

public class TressFXRender : MonoBehaviour
{
	/// <summary>
	/// The hair shader.
	/// </summary>
	public Shader hairShader;

	/// <summary>
	/// The total hair layers count.
	/// Normally it is not needed to change this.
	/// </summary>
	public int totalHairLayers = 32;

	/// <summary>
	/// The hair material.
	/// </summary>
	private Material hairMaterial;

	/// <summary>
	/// The main camera the game is using for rendering.
	/// </summary>
	private TressFXCamera myCamera;

	/// <summary>
	/// The TressFX master class.
	/// </summary>
	private TressFX master;

	/// <summary>
	/// The triangle indices buffer.
	/// </summary>
	private ComputeBuffer g_TriangleIndicesBuffer;

	/// <summary>
	/// The linked list head texture.
	/// </summary>
	private RenderTexture LinkedListHead;

	/// <summary>
	/// The linked list compute buffer.
	/// </summary>
	private ComputeBuffer LinkedList;

	/// <summary>
	/// Start this instance.
	/// Initializes the hair material and all other resources.
	/// </summary>
	public void Start()
	{
		this.hairMaterial = new Material (this.hairShader);

		// Get TressFX master
		this.master = this.GetComponent<TressFX> ();

		// Attach camera script to main camera
		this.myCamera = Camera.main.gameObject.AddComponent<TressFXCamera> ();

		// Set triangle indices buffer
		this.g_TriangleIndicesBuffer = new ComputeBuffer (this.master.hairData.m_TriangleIndices.Length, 4);
		this.g_TriangleIndicesBuffer.SetData (this.master.hairData.m_TriangleIndices);

		// Initialize linked list
		this.LinkedListHead = new RenderTexture (Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
		this.LinkedListHead.filterMode = FilterMode.Point;
		this.LinkedListHead.enableRandomWrite = true;
		this.LinkedListHead.hideFlags = HideFlags.HideAndDontSave;
		this.LinkedListHead.Create ();

		this.LinkedList = new ComputeBuffer (this.totalHairLayers * Screen.width * Screen.height, 12, ComputeBufferType.Counter);
	}

	/// <summary>
	/// Raises the destroy event.
	/// Releases all resources not needed any more.
	/// </summary>
	public void OnDestroy()
	{
		this.g_TriangleIndicesBuffer.Release ();
		this.LinkedListHead.Release ();
		this.LinkedList.Release ();
	}

	/// <summary>
	/// Raises the render object event.
	/// </summary>
	public void OnRenderObject()
	{
		// Clear linked list
		Graphics.SetRenderTarget (this.LinkedListHead);
		GL.Clear (false, true, Color.white);
		Graphics.SetRenderTarget (null);

		/*PPLL[] t = new PPLL[this.totalHairLayers * Screen.width * Screen.height];
		this.LinkedList.GetData (t);*/

		// Set random write targets
		// Graphics.ClearRandomWriteTargets ();
		Graphics.SetRandomWriteTarget (1, this.LinkedListHead);
		Graphics.SetRandomWriteTarget (2, this.LinkedList);

		// Set shader buffers
		this.hairMaterial.SetBuffer ("g_HairVertexTangents", this.master.g_HairVertexTangents);
		this.hairMaterial.SetBuffer ("g_HairVertexPositions", this.master.g_HairVertexPositions);
		this.hairMaterial.SetBuffer ("g_TriangleIndicesBuffer", this.g_TriangleIndicesBuffer);
		this.hairMaterial.SetBuffer ("g_HairThicknessCoeffs", this.master.g_HairVertexTangents);

		// Set rendering variables
		this.hairMaterial.SetInt ("g_bExpandPixels", 1);
		this.hairMaterial.SetFloat ("g_FiberRadius", 0.01f);
		this.hairMaterial.SetFloat ("g_FiberAlpha", 1.0f);
		this.hairMaterial.SetFloat ("g_alphaThreshold", 0.1f);
		this.hairMaterial.SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));

		this.hairMaterial.SetPass (0);
		Graphics.DrawProcedural (MeshTopology.Triangles, this.master.hairData.m_TriangleIndices.Length);

		// Graphics.ClearRandomWriteTargets ();
	}
}
