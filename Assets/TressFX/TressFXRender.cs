using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// This class handles the rendering of the hairs.
/// </summary>
[RequireComponent(typeof(TressFX))]
public class TressFXRender : MonoBehaviour
{
	// Resources
	private TressFX master;
	private Material hairMaterial;
	private Material hairShadowMaterial;

	public Shader standardHairShader;
	public Shader hairShadowShader;

	// Bouding boxes
	private Mesh[] meshes;
	private Mesh[] lineMeshes;
	private Bounds meshBounds;

	// Config
	public bool expandPixels = true;
	public float fiberRadius = 0.14f;
	public bool thinTip = true;
	public Color hairColor;
	public Color specColor;
	public float shininess = 0.5f;
	public float gloss = 0.5f;

	public void Initialize()
	{
		this.master = this.gameObject.GetComponent<TressFX> ();
		this.hairMaterial = new Material (this.standardHairShader);
		this.hairShadowMaterial = new Material (this.hairShadowShader);

		// Calculate mesh bounds
		Vector3 addedVertices = Vector3.zero;
		float highestXDistance = 0;
		float highestYDistance = 0;
		float highestZDistance = 0;
		float lowestXDistance = 100;
		float lowestYDistance = 100;
		float lowestZDistance = 100;
		int vertices = 0;
		int indexCounter = 0;
		int lineIndexCounter = 0;
		int vertexCounter = 0;

		MeshBuilder meshBuilder = new MeshBuilder (MeshTopology.Triangles);
		MeshBuilder lineMeshBuilder = new MeshBuilder (MeshTopology.Lines);

		// Add all vertices to a vector for calculating the center point
		for (int sI = 0; sI < this.master.strands.Length; sI++)
		{
			List<Vector3> meshVertices = new List<Vector3>();
			List<int> meshIndices = new List<int>();
			List<Vector3> lineMeshVertices = new List<Vector3>();
			List<int> lineMeshIndices = new List<int>();
			
			// Reset index counter?
			if (!meshBuilder.HasSpace(this.master.strands[sI].vertices.Length * 6))
			{
				indexCounter = 0;
			}
			if (!lineMeshBuilder.HasSpace(this.master.strands[sI].vertices.Length))
			{
				lineIndexCounter = 0;
			}

			for (int vI = 0; vI < this.master.strands[sI].vertices.Length; vI++)
			{
				Vector3 vertexPos = this.master.strands[sI].vertices[vI].pos;
				addedVertices += vertexPos;

				// Highest distances
				if (Mathf.Abs(vertexPos.x) > highestXDistance)
				{
					highestXDistance = Mathf.Abs(vertexPos.x);
				}
				if (Mathf.Abs(vertexPos.y) > highestYDistance)
				{
					highestYDistance = Mathf.Abs(vertexPos.y);
				}
				if (Mathf.Abs(vertexPos.z) > highestZDistance)
				{
					highestZDistance = Mathf.Abs(vertexPos.z);
				}

				// Lowest dists
				if (Mathf.Abs(vertexPos.x) < lowestXDistance)
				{
					lowestXDistance = Mathf.Abs(vertexPos.x);
				}
				if (Mathf.Abs(vertexPos.y) < lowestYDistance)
				{
					lowestYDistance = Mathf.Abs(vertexPos.y);
				}
				if (Mathf.Abs(vertexPos.z) < lowestZDistance)
				{
					lowestZDistance = Mathf.Abs(vertexPos.z);
				}
				
				// Add mesh data
				meshVertices.AddRange (new Vector3[] { new Vector3(vertexCounter,0,0), new Vector3(vertexCounter + 1,0,0), new Vector3(vertexCounter + 2,0,0), new Vector3(vertexCounter + 3,0,0), new Vector3(vertexCounter + 4,0,0), new Vector3(vertexCounter + 5,0,0) });
				meshIndices.AddRange (new int[] { indexCounter, indexCounter + 1, indexCounter + 2, indexCounter + 3, indexCounter + 4, indexCounter + 5 });

				lineMeshVertices.Add (new Vector3(vertices, 0, 0));

				if (/*vI == 0 || */this.master.strands[sI].vertices.Length-1 == vI)
				{
					// First or last index, so only one index
					lineMeshIndices.AddRange(new int[] {lineIndexCounter-1, lineIndexCounter});
				}
				else
				{
					lineMeshIndices.AddRange(new int[] {lineIndexCounter, lineIndexCounter+1});
				}

				indexCounter += 6;
				vertexCounter += 6;

				lineIndexCounter++;
				vertices++;
			}
			
			// Add to mesh builder
			meshBuilder.AddVertices(meshVertices.ToArray(), meshIndices.ToArray());
			lineMeshBuilder.AddVertices(lineMeshVertices.ToArray(), lineMeshIndices.ToArray());
		}

		this.meshBounds = new Bounds ((addedVertices / vertices), new Vector3 ((highestXDistance-lowestXDistance), (highestYDistance-lowestYDistance), (highestZDistance-lowestZDistance)));

		BoxCollider c = this.gameObject.AddComponent<BoxCollider> ();
		c.size = new Vector3 ((highestXDistance-lowestXDistance)*2, (highestYDistance-lowestYDistance)*2, (highestZDistance-lowestZDistance)*2);
		c.center = (addedVertices / vertices);

		// Initialize mesh rendering
		this.meshes = meshBuilder.GetMeshes();
		this.lineMeshes = lineMeshBuilder.GetMeshes ();
		
		for (int i = 0; i < this.meshes.Length; i++)
		{
			this.meshes[i].bounds = this.meshBounds;
		}

		for (int i = 0; i < this.lineMeshes.Length; i++)
		{
			this.lineMeshes[i].bounds = this.meshBounds;
		}
	}

	public void LateUpdate()
	{
		this.hairMaterial.SetColor("_HairColor", this.hairColor);
		this.hairMaterial.SetColor("_SpecColor", this.specColor);
		this.hairMaterial.SetFloat("_Shininess", this.shininess);
		this.hairMaterial.SetFloat("_Gloss", this.gloss);
		this.hairMaterial.SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
		this.hairMaterial.SetBuffer("g_HairVertexTangents", this.master.TangentsBuffer);
		this.hairMaterial.SetBuffer("g_TriangleIndicesBuffer", this.master.TriangleIndicesBuffer);
		this.hairMaterial.SetVector("g_vEye", Camera.main.transform.position);
		this.hairMaterial.SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
		this.hairMaterial.SetFloat("g_FiberRadius", this.fiberRadius);
		this.hairMaterial.SetFloat("g_bExpandPixels", this.expandPixels ? 0 : 1);
		this.hairMaterial.SetFloat("g_bThinTip", this.thinTip ? 0 : 1);
		this.hairMaterial.SetBuffer ("g_HairInitialVertexPositions", this.master.InitialVertexPositionBuffer);

		for (int i = 0; i < this.meshes.Length; i++)
		{
            Graphics.DrawMesh(this.meshes[i], Vector3.zero, Quaternion.identity, this.hairMaterial, 8);
		}

		// Render shadows
		this.hairShadowMaterial.SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
		for (int i = 0; i < this.lineMeshes.Length; i++)
		{
			Graphics.DrawMesh(this.lineMeshes[i], Vector3.zero, Quaternion.identity, this.hairShadowMaterial, 8);
		}
	}
}