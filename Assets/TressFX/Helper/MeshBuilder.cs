using UnityEngine;
using System.Collections.Generic;

namespace TressFX
{
	/// <summary>
	/// Mesh builder.
	/// Helper class for procedurally generated meshes
	/// </summary>
	public class MeshBuilder
	{
		private List<Mesh> meshes;
		
		/// <summary>
		/// The current vertices.
		/// </summary>
		private List<Vector3> currentVertices;
		
		/// <summary>
		/// The current indices.
		/// </summary>
		private List<int> currentIndices;
		private List<Vector2> currentUv;
		private List<Vector3> currentNormals;
		
		private MeshTopology topology;
		
		public MeshBuilder(MeshTopology topology)
		{
			this.meshes = new List<Mesh> ();
			this.currentIndices = new List<int> ();
			this.currentVertices = new List<Vector3> ();
			this.currentUv = new List<Vector2> ();
			this.currentNormals = new List<Vector3> ();
			this.topology = topology;
		}
		
		public bool HasSpace(int count)
		{
			// Enough space in this mesh?
			if (this.currentVertices.Count + count < 65000)
				return true;
			else
				return false;
		}
		
		/// <summary>
		/// Adds vertices to the mesh generation.
		/// </summary>
		/// <param name="vertices">Vertices.</param>
		/// <returns>true for added to new mesh, false for added to current mesh.</returns>
		public bool AddVertices(Vector3[] vertices, int[] indices, Vector2[] uvs, Vector3[] normals)
		{
			// Enough space in this mesh?
			if (this.currentVertices.Count + vertices.Length < 65000)
			{
				this.currentVertices.AddRange (vertices);
				this.currentIndices.AddRange (indices);
				this.currentUv.AddRange(uvs);
				this.currentNormals.AddRange(normals);

				return false;
			}
			else
			{
				this.GenerateCurrentMesh();
				this.AddVertices(vertices, indices, uvs, normals);
				return true;
			}
		}
		
		public Mesh[] GetMeshes()
		{
			if (this.currentVertices.Count > 0)
			{
				this.GenerateCurrentMesh ();
			}
			
			return this.meshes.ToArray ();
		}
		
		/// <summary>
		/// Generates the current mesh.
		/// </summary>
		private void GenerateCurrentMesh()
		{
			// Initialize mesh
			Mesh mesh = new Mesh ();
			
			// Set vertices and indices
			mesh.vertices = this.currentVertices.ToArray ();
			mesh.uv = this.currentUv.ToArray ();
			mesh.SetIndices (this.currentIndices.ToArray (), this.topology, 0);
			// mesh.normals = this.currentNormals.ToArray ();

			// Add mesh to the list
			this.meshes.Add (mesh);
			
			// Flush lists
			this.currentVertices.Clear ();
			this.currentIndices.Clear ();
			this.currentUv.Clear ();
			this.currentNormals.Clear ();
		}
	}
}