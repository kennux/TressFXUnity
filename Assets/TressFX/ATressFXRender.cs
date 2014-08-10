using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Abstract tressfx render class.
/// Implement this for implementing your own rendering techniques.
/// 
/// This base class handles things like mesh generation.
/// </summary>
[RequireComponent(typeof(TressFX))]
public abstract class ATressFXRender : MonoBehaviour
{
	// Resources
	protected TressFX master;

	/// <summary>
	/// The hair material.
	/// Contains materials for the meshes.
	/// 
	/// This array index is the first index for the meshes Mesh[] list.
	/// </summary>
	protected Material[] hairMaterial;
	
	// Bouding boxes
	protected List<Mesh[]> meshes;
	protected Mesh[] lineMeshes;

	/// <summary>
	/// The hair mesh bounds.
	/// </summary>
	protected Bounds meshBounds;

	/// <summary>
	/// Initialize this instance.
	/// Generates the l
	/// </summary>
	public virtual void Initialize()
	{
		this.master = this.gameObject.GetComponent<TressFX> ();
		
		// Get materials
		this.hairMaterial = new Material[this.master.hairData.Length];
		for (int i = 0; i < this.hairMaterial.Length; i++)
		{
			this.hairMaterial[i] = this.master.hairData[i].hairMaterial;
		}
		
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
		
		// Initialize meshes list
		List<Mesh[]> meshList = new List<Mesh[]>();
		
		int lastHairId = 0;
		
		// Add all vertices to a vector for calculating the center point
		for (int sI = 0; sI < this.master.strands.Length; sI++)
		{
			List<Vector3> meshVertices = new List<Vector3>();
			List<Vector2> meshUvs = new List<Vector2>();
			List<int> meshIndices = new List<int>();
			List<Vector3> lineMeshVertices = new List<Vector3>();
			List<int> lineMeshIndices = new List<int>();
			List<Vector2> lineMeshUvs = new List<Vector2>();
			
			// Reset index counter?
			if (!meshBuilder.HasSpace(this.master.strands[sI].vertices.Length * 6))
			{
				indexCounter = 0;
			}
			if (!lineMeshBuilder.HasSpace(this.master.strands[sI].vertices.Length))
			{
				lineIndexCounter = 0;
			}
			
			if (lastHairId != this.master.strands[sI].hairId)
			{
				indexCounter = 0;
				meshList.Add (meshBuilder.GetMeshes());
				meshBuilder = new MeshBuilder(MeshTopology.Triangles);
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
				Vector3[] triangleVertices = new Vector3[] { new Vector3(vertexCounter,0,0), new Vector3(vertexCounter + 1,0,0), new Vector3(vertexCounter + 2,0,0), new Vector3(vertexCounter + 3,0,0), new Vector3(vertexCounter + 4,0,0), new Vector3(vertexCounter + 5,0,0) };
				
				for (int i = 0; i < triangleVertices.Length; i++)
				{
					int localVertexId = this.master.MapTriangleIndexIdToStrandVertexId((int)triangleVertices[i].x);
					
					// ... No comment
					if (localVertexId >= this.master.strands[sI].vertices.Length)
						localVertexId = this.master.strands[sI].vertices.Length-1;
					
					meshUvs.Add (new Vector2(this.master.strands[sI].vertices[localVertexId].texcoords.x, this.master.strands[sI].vertices[localVertexId].texcoords.y));
				}
				
				meshVertices.AddRange (triangleVertices);
				meshIndices.AddRange (new int[] { indexCounter, indexCounter + 1, indexCounter + 2, indexCounter + 3, indexCounter + 4, indexCounter + 5 });
				
				lineMeshVertices.Add (new Vector3(vertices, 0,0));
				lineMeshUvs.Add (new Vector2(this.master.strands[sI].vertices[vI].texcoords.x, this.master.strands[sI].vertices[vI].texcoords.y));
				
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
				lastHairId = this.master.strands[sI].hairId;
				
				lineIndexCounter++;
				vertices++;
			}
			
			// Add to mesh builder
			meshBuilder.AddVertices(meshVertices.ToArray(), meshIndices.ToArray(), meshUvs.ToArray());
			lineMeshBuilder.AddVertices(lineMeshVertices.ToArray(), lineMeshIndices.ToArray(), lineMeshUvs.ToArray());
		}
		
		this.meshBounds = new Bounds ((addedVertices / vertices), new Vector3 ((highestXDistance-lowestXDistance), (highestYDistance-lowestYDistance), (highestZDistance-lowestZDistance)));
		
		BoxCollider c = this.gameObject.AddComponent<BoxCollider> ();
		c.size = new Vector3 ((highestXDistance-lowestXDistance)*2, (highestYDistance-lowestYDistance)*2, (highestZDistance-lowestZDistance)*2);
		c.center = (addedVertices / vertices);
		
		// Initialize mesh rendering
		meshList.Add(meshBuilder.GetMeshes());
		this.lineMeshes = lineMeshBuilder.GetMeshes ();
		this.meshes = meshList;
		
		for (int i = 0; i < this.meshes.Count; i++)
		{
			for (int j = 0; j < this.meshes[i].Length; j++)
			{
				this.meshes[i][j].bounds = this.meshBounds;
			}
		}
		
		for (int i = 0; i < this.lineMeshes.Length; i++)
		{
			this.lineMeshes[i].bounds = this.meshBounds;
		}
	}
}
