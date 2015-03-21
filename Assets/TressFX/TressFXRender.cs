using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct PPLL
{
	public uint tangentAndCoverage;
	public float depth;
	public uint uNext;
	public uint ammountLight;
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
	/// The fragment sorting shader.
	/// </summary>
	public ComputeShader fragmentSortingShader;
	
	/// <summary>
	/// The fullscreen quad material.
	/// </summary>
	public Material fullscreenQuadMaterial;

	/// <summary>
	/// The shadow shader.
	/// </summary>
	public Shader shadowShader;

	/// <summary>
	/// The hair material.
	/// </summary>
	private Material hairMaterial;

	/// <summary>
	/// The shadow material.
	/// </summary>
	private Material shadowMaterial;

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
	/// The line meshes.
	/// </summary>
	private Mesh[] lineMeshes;

	/// <summary>
	/// The rendering bounds.
	/// </summary>
	private Bounds renderingBounds;

	/// <summary>
	/// The final render texture.
	/// </summary>
	private RenderTexture finalRenderTexture;

	/// <summary>
	/// The sort fragments kernel identifier.
	/// </summary>
	private int SortFragmentsKernelId;

	/// <summary>
	/// If this is set to true an additional rendering pass for shadows is rendered.
	/// </summary>
	public bool castShadows = true;
	
	/// <summary>
	/// The color of the hair.
	/// </summary>
	public Color hairColor = new Color(0.647f, 0.419f, 0.274f, 1);

	// Rendering/material variables
	public float fiberAlpha = 1.0f;
	public float fiberRadius = 0.14f;
	public bool expandPixels = true;
	public bool thinTip = true;
	public float alphaThreshold = 0.1f;
	public float g_MatKd = 0.4f;
	public float g_MatKa = 0;
	public float g_MatKs1 = 0.14f;
	public float g_MatEx1 = 80;
	public float g_MatKs2 = 0.5f;
	public float g_MatEx2 = 8;

	/// <summary>
	/// Gets the VP matrix.
	/// </summary>
	/// <value>The VP matrix.</value>
	protected Matrix4x4 VPMatrix
	{
		get { return this.GetVPMatrix (); }
	}

	/// <summary>
	/// Start this instance.
	/// Initializes the hair material and all other resources.
	/// </summary>
	public void Start()
	{
		Camera.main.hdr = false;
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

		this.LinkedList = new ComputeBuffer (this.totalHairLayers * Screen.width * Screen.height, 16, ComputeBufferType.Counter);

		// Generate triangle meshes
		this.triangleMeshes = this.GenerateTriangleMeshes ();

		// Create render bounds
		this.renderingBounds = new Bounds (this.master.hairData.m_bSphere.center, new Vector3(this.master.hairData.m_bSphere.radius, this.master.hairData.m_bSphere.radius, this.master.hairData.m_bSphere.radius));

		// Initialize fragment sorter
		this.finalRenderTexture = new RenderTexture (Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32);
		this.finalRenderTexture.filterMode = FilterMode.Point;
		this.finalRenderTexture.enableRandomWrite = true;
		this.finalRenderTexture.hideFlags = HideFlags.HideAndDontSave;
		this.finalRenderTexture.Create ();

		this.SortFragmentsKernelId = this.fragmentSortingShader.FindKernel ("SortFragments");

		// Initialize shadow material
		this.shadowMaterial = new Material (this.shadowShader);

		this.lineMeshes = this.GenerateLineMeshes ();
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
				normals[j] = Vector3.up;
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
	/// Generates the line meshes.
	/// Meshes are built of indices. Every vertices x-position will contain a vertex list index.
	/// </summary>
	/// <returns>The line meshes.</returns>
	protected Mesh[] GenerateLineMeshes()
	{
		// Counter
		int indexCounter = 0;
		MeshBuilder meshBuilder = new MeshBuilder (MeshTopology.Lines);
		
		// Write all indices to the meshes
		for (int i = 0; i < this.master.hairData.m_pVertices.Length; i+=2)
		{
			// Check for space
			if (!meshBuilder.HasSpace(2))
			{
				// Reset index counter
				indexCounter = 0;
			}
			
			Vector3[] vertices = new Vector3[2];
			Vector3[] normals = new Vector3[2];
			int[] indices = new int[2];
			Vector2[] uvs = new Vector2[2];
			
			// Add vertices
			for (int j = 0; j < 2; j++)
			{
				// Prepare data
				vertices[j] = new Vector3(this.master.hairData.m_LineIndices[i+j],0,0);
				normals[j] = Vector3.up;
				indices[j] = indexCounter+j;
				uvs[j] = Vector2.one;
			}
			
			// Add mesh data to builder
			meshBuilder.AddVertices(vertices, indices, uvs, normals);
			
			indexCounter += 2;
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
	/// Gets the VP matrix.
	/// </summary>
	/// <returns>The VP matrix.</returns>
	private Matrix4x4 GetVPMatrix()
	{
		bool d3d = true; // SystemInfo.graphicsDeviceVersion.IndexOf("Direct3D") > -1;
		Matrix4x4 V = Camera.main.worldToCameraMatrix;
		Matrix4x4 P = Camera.main.projectionMatrix;
		if (d3d)
		{
			// Scale and bias from OpenGL -> D3D depth range
			for ( int i = 0; i < 4; i++) { P[2,i] = P[2,i]*0.5f + P[3,i]*0.5f;}
		}

		return (P * V);
	}

	/// <summary>
	/// Raises the render object event.
	/// </summary>
	public void Update()
	{
		// Debug.Log (Camera.main.WorldToScreenPoint (new Vector3 (50, 100, 100)));
		// Set random write targets
		Graphics.ClearRandomWriteTargets ();
		Graphics.SetRandomWriteTarget (1, this.LinkedList);
		Graphics.SetRandomWriteTarget (2, this.LinkedListHead);

		// Set shader buffers
		this.hairMaterial.SetBuffer ("g_HairVertexTangents", this.master.g_HairVertexTangents);
		this.hairMaterial.SetBuffer ("g_HairVertexPositions", this.master.g_HairVertexPositions);
		this.hairMaterial.SetBuffer ("g_TriangleIndicesBuffer", this.g_TriangleIndicesBuffer);
		this.hairMaterial.SetBuffer ("g_HairThicknessCoeffs", this.master.g_HairVertexTangents);
		
		this.hairMaterial.SetMatrix ("VPMatrix", this.VPMatrix);
		this.hairMaterial.SetMatrix ("MMatrix", this.transform.localToWorldMatrix);
		this.hairMaterial.SetMatrix ("InvVPMatrix", this.VPMatrix.inverse);
		
		// Set rendering variables
		this.hairMaterial.SetInt ("g_bExpandPixels", this.expandPixels ? 1 : 0);
		this.hairMaterial.SetFloat ("g_FiberRadius", this.fiberRadius);
		this.hairMaterial.SetFloat ("g_FiberAlpha", this.fiberAlpha);
		this.hairMaterial.SetFloat ("g_bThinTip", this.thinTip ? 1 : 0);
		this.hairMaterial.SetFloat ("g_alphaThreshold", this.alphaThreshold);
		this.hairMaterial.SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
		
		// Update rendering bounds
		Bounds renderingBounds = new Bounds (this.transform.position + this.renderingBounds.center, this.renderingBounds.size);

		// Render meshes
		for (int i = 0; i < this.triangleMeshes.Length; i++)
		{
			this.triangleMeshes[i].bounds = renderingBounds;
			Graphics.DrawMesh (this.triangleMeshes [i], Vector3.zero, Quaternion.identity, this.hairMaterial, 8, this.GetComponent<Camera>());
		}

		// Render shadows
		if (this.castShadows)
		{
			this.shadowMaterial.SetBuffer("g_HairVertexPositions", this.master.g_HairVertexPositions);

			for (int i = 0; i < this.lineMeshes.Length; i++)
			{
				this.lineMeshes[i].bounds = renderingBounds;
				Graphics.DrawMesh (this.lineMeshes [i], Vector3.zero, Quaternion.identity, this.shadowMaterial, 8);
			}
		}
	}

	/// <summary>
	/// Sorts the fragments.
	/// </summary>
	protected void SortFragments()
	{
		this.fragmentSortingShader.SetVector("screenSize", new Vector4(Screen.width, Screen.height, 0, 0));
		this.fragmentSortingShader.SetVector ("g_MatBaseColor", new Vector4 (this.hairColor.r, this.hairColor.g, this.hairColor.b, this.hairColor.a));
		this.fragmentSortingShader.SetVector("g_vEye", new Vector4(Camera.main.transform.position.x, Camera.main.transform.position.y, Camera.main.transform.position.z, 1));
		this.fragmentSortingShader.SetFloats ("InvVPMatrix", this.MatrixToFloatArray(this.VPMatrix.inverse));
		this.fragmentSortingShader.SetFloat ("g_MatKd", this.g_MatKd);
		this.fragmentSortingShader.SetFloat ("g_MatKa", this.g_MatKa);
		this.fragmentSortingShader.SetFloat ("g_MatKs1", this.g_MatKs1);
		this.fragmentSortingShader.SetFloat ("g_MatEx1", this.g_MatEx1);
		this.fragmentSortingShader.SetFloat ("g_MatKs2", this.g_MatKs2);
		this.fragmentSortingShader.SetFloat ("g_MatEx2", this.g_MatEx2);

		this.fragmentSortingShader.SetTexture (this.SortFragmentsKernelId, "LinkedListHead", this.LinkedListHead);
		this.fragmentSortingShader.SetBuffer (this.SortFragmentsKernelId, "LinkedList", this.LinkedList);
		this.fragmentSortingShader.SetTexture (this.SortFragmentsKernelId, "Result", this.finalRenderTexture);

		this.fragmentSortingShader.Dispatch (this.SortFragmentsKernelId, Mathf.CeilToInt ((float)Screen.width / 8.0f), Mathf.CeilToInt ((float)Screen.height / 8.0f), 1);
	}

	/// <summary>
	/// Raises the render object event.
	/// Applies the fullscreen quad after dispatching the fragment sorting compute shader.
	/// </summary>
	public void OnRenderObject()
	{
		/*if (Camera.current != Camera.main)
			return;*/

		this.SortFragments ();

		// Apply fullscreen quad
		GL.PushMatrix();
		{
			GL.LoadOrtho();
			
			this.fullscreenQuadMaterial.SetPass(0);
			this.fullscreenQuadMaterial.SetTexture("_Texture", this.finalRenderTexture);

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
		
		// Clear linked list
		Graphics.SetRenderTarget (this.LinkedListHead);
		GL.Clear (false, true, Color.white);
		Graphics.SetRenderTarget (null);
		Graphics.SetRenderTarget (this.finalRenderTexture);
		GL.Clear (false, true, Color.white);
		Graphics.SetRenderTarget (null);
	}
	
	/// <summary>
	/// Convertes a Matrix4x4 to a float array.
	/// </summary>
	/// <returns>The to float array.</returns>
	/// <param name="matrix">Matrix.</param>
	private float[] MatrixToFloatArray(Matrix4x4 matrix)
	{
		return new float[] 
		{
			matrix.m00, matrix.m01, matrix.m02, matrix.m03,
			matrix.m10, matrix.m11, matrix.m12, matrix.m13,
			matrix.m20, matrix.m21, matrix.m22, matrix.m23,
			matrix.m30, matrix.m31, matrix.m32, matrix.m33
		};
	}
}
