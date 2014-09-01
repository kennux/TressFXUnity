using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct PPLL
{
	public uint tangentAndCoverage;
	public float depth;
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
	/// The triangle meshes.
	/// Meshes are built of indices. Every vertices x-position will contain a triangleindex buffer index.
	/// </summary>
	private Mesh[] triangleMeshes;

	/// <summary>
	/// The rendering bounds.
	/// </summary>
	private Bounds renderingBounds;

	public MeshFilter test;

	public Material testMat;

	/// <summary>
	/// Start this instance.
	/// Initializes the hair material and all other resources.
	/// </summary>
	public void Start()
	{
		this.hairMaterial = new Material (this.hairShader);

		// Get TressFX master
		this.master = this.GetComponent<TressFX> ();

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

		// Generate triangle meshes
		this.triangleMeshes = this.GenerateTriangleMeshes ();

		// Create render bounds
		this.renderingBounds = new Bounds (this.master.hairData.m_bSphere.center, new Vector3(this.master.hairData.m_bSphere.radius, this.master.hairData.m_bSphere.radius, this.master.hairData.m_bSphere.radius));

	
	}

	/// <summary>
	/// Generates the triangle meshes.
	/// Meshes are built of indices. Every vertices x-position will contain a triangleindex buffer index.
	/// </summary>
	/// <returns>The triangle meshes.</returns>
	protected Mesh[] GenerateTriangleMeshes()
	{
		// Counter
		int indexCounter = 0;
		MeshBuilder meshBuilder = new MeshBuilder (MeshTopology.Triangles);

		// Write all indices to the meshes
		for (int i = 0; i < this.master.hairData.m_TriangleIndices.Length; i+=6)
		{
			// Check for space
			if (!meshBuilder.HasSpace(6))
			{
				// Reset index counter
				indexCounter = 0;
			}
			
			Vector3[] vertices = new Vector3[6];
			Vector3[] normals = new Vector3[6];
			int[] indices = new int[6];
			Vector2[] uvs = new Vector2[6];

			// Add vertices
			for (int j = 0; j < 6; j++)
			{
				// Prepare data
				vertices[j] = new Vector3(i+j,0,0);
				normals[j] = Vector3.one;
				indices[j] = indexCounter+j;
				uvs[j] = Vector2.one;
			}

			// Add mesh data to builder
			meshBuilder.AddVertices(vertices, indices, uvs, normals);

			indexCounter += 6;
		}

		return meshBuilder.GetMeshes ();
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
	public void Update()
	{

		// Clear linked list
		Graphics.SetRenderTarget (this.LinkedListHead);
		GL.Clear (false, true, Color.white);
		Graphics.SetRenderTarget (null);
		
		// Set random write targets
		Graphics.ClearRandomWriteTargets ();
		Graphics.SetRandomWriteTarget (1, this.LinkedList);
		Graphics.SetRandomWriteTarget (2, this.LinkedListHead);

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
		
		// Update rendering bounds
		Bounds renderingBounds = new Bounds (this.transform.position + this.renderingBounds.center, this.renderingBounds.size);

		// Render meshes
		for (int i = 0; i < this.triangleMeshes.Length; i++)
		{
			this.triangleMeshes[i].bounds = renderingBounds;
			Graphics.DrawMesh (this.triangleMeshes [i], Vector3.zero, this.transform.rotation, this.hairMaterial, 8, Camera.main);
		}
	}

	public void OnRenderObject()
	{
		if (Camera.current != Camera.main)
			return;

		/*PPLL[] test = new PPLL[this.totalHairLayers * Screen.width * Screen.height];
		this.LinkedList.GetData (test);*/

		Graphics.ClearRandomWriteTargets ();

		// Apply fullscreen quad
		GL.PushMatrix();
		{
			GL.LoadOrtho();
			
			this.testMat.SetPass(0);
			this.testMat.SetTexture("_TestTex", this.LinkedListHead);
			
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
}
