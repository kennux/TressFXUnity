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
	public Material hairMaterial;

	// Bouding boxes
	private Mesh[] meshes;
	private Bounds meshBounds;

	// Config
	public bool expandPixels = true;
	public float fiberRadius = 0.14f;
	public bool thinTip = true;

	public void Initialize(Mesh[] meshes)
	{
		this.master = this.gameObject.GetComponent<TressFX> ();
		this.hairMaterial = new Material (this.hairMaterial);

		// Calculate mesh bounds
		Vector3 addedVertices = Vector3.zero;
		float highestXDistance = 0;
		float highestYDistance = 0;
		float highestZDistance = 0;
		float lowestXDistance = 100;
		float lowestYDistance = 100;
		float lowestZDistance = 100;
		int vertices = 0;

		// Add all vertices to a vector for calculating the center point
		for (int sI = 0; sI < this.master.strands.Length; sI++)
		{
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

				vertices++;
			}
		}

		this.meshBounds = new Bounds ((addedVertices / vertices), new Vector3 ((highestXDistance-lowestXDistance)*2, (highestYDistance-lowestYDistance)*2, (highestZDistance-lowestZDistance)*2));

		BoxCollider c = this.gameObject.AddComponent<BoxCollider> ();
		c.size = new Vector3 ((highestXDistance-lowestXDistance)*2, (highestYDistance-lowestYDistance)*2, (highestZDistance-lowestZDistance)*2);
		c.center = (addedVertices / vertices);

		// Initialize mesh rendering
		this.meshes = meshes;
		for (int i = 0; i < meshes.Length; i++)
		{
			this.meshes[i].bounds = this.meshBounds;
		}

	}

	public void LateUpdate()
	{
		this.hairMaterial.SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
		this.hairMaterial.SetBuffer("g_HairVertexTangents", this.master.TangentsBuffer);
		this.hairMaterial.SetBuffer("g_TriangleIndicesBuffer", this.master.TriangleIndicesBuffer);
		this.hairMaterial.SetVector("g_vEye", Camera.main.transform.position);
		this.hairMaterial.SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
		this.hairMaterial.SetFloat("g_FiberRadius", this.fiberRadius);
		this.hairMaterial.SetFloat("g_bExpandPixels", this.expandPixels ? 0 : 1);
		this.hairMaterial.SetFloat("g_bThinTip", this.thinTip ? 0 : 1);

		for (int i = 0; i < this.meshes.Length; i++)
		{
			Graphics.DrawMesh(this.meshes[i], Vector3.zero, this.transform.rotation, this.hairMaterial, 8);
		}
	}
}