using UnityEngine;
using System.Collections;

namespace TressFX
{
	public abstract class ATressFXRender : MonoBehaviour
	{
        [Header("Shadows")]
        /// <summary>
        /// If this is set to true an additional rendering pass for shadows is rendered.
        /// </summary>
        public bool castShadows = true;

        /// <summary>
        /// The shadow shader.
        /// </summary>
        public Shader shadowShader;
		
		/// <summary>
		/// The shadow material.
		/// </summary>
		protected Material shadowMaterial;

		/// <summary>
		/// The TressFX master class.
		/// </summary>
		protected TressFX master;
		
		/// <summary>
		/// The triangle indices buffer.
		/// </summary>
		protected ComputeBuffer g_TriangleIndicesBuffer;

        /// <summary>
        /// The line indices buffer.
        /// </summary>
        public ComputeBuffer g_LineIndicesBuffer;

        /// <summary>
        /// The triangle meshes.
        /// Meshes are built of indices. Every vertices x-position will contain a triangleindex buffer index.
        /// </summary>
        protected Mesh[] triangleMeshes;
		
		/// <summary>
		/// The line meshes.
		/// </summary>
		protected Mesh[] lineMeshes;

        [Header("Debug")]
        /// <summary>
        /// The debug bounding box flag.
        /// </summary>
        public bool debugBoundingBox = false;

        /// <summary>
        /// Rendering bounds in worldspace.
        /// </summary>
        public Bounds worldspaceBounds
        {
            get
            {
                return new Bounds(this.renderingBounds.center, this.renderingBounds.size);
            }
        }

        /// <summary>
        /// The rendering bounds.
        /// In local space!
        /// </summary>
        [HideInInspector]
		public Bounds renderingBounds
        {
            get
            {
                return new Bounds(Vector3.Scale(this._renderingBounds.center, this.transform.localScale), Vector3.Scale(this._renderingBounds.size, this.transform.localScale));
            }
        }
        private Bounds _renderingBounds;

        /// <summary>
        /// Submits draw calls to unitys rendering system to render hair shadows.
        /// </summary>
        protected virtual void RenderShadows()
        {
            if (this.castShadows)
            {
                this.shadowMaterial.SetBuffer("g_HairVertexPositions", this.master.g_HairVertexPositions);

                foreach (var cam in Camera.allCameras)
                {
                    for (int i = 0; i < this.lineMeshes.Length; i++)
                    {
                        this.lineMeshes[i].bounds = renderingBounds;
                        Graphics.DrawMesh(this.lineMeshes[i], Matrix4x4.identity, this.shadowMaterial, this.gameObject.layer, cam);
                    }
                }
            }
        }

		
		public virtual void Awake()
		{
			// Get TressFX master
			this.master = this.GetComponent<TressFX> ();
			
			// Set triangle indices buffer
			this.g_TriangleIndicesBuffer = new ComputeBuffer (this.master.hairData.m_TriangleIndices.Length, 4);
			this.g_TriangleIndicesBuffer.SetData (this.master.hairData.m_TriangleIndices);

            // Set line indices buffer
            this.g_LineIndicesBuffer = new ComputeBuffer(this.master.hairData.m_LineIndices.Length, 4);
            this.g_LineIndicesBuffer.SetData(this.master.hairData.m_LineIndices);

            // Generate meshes
            this.triangleMeshes = this.GenerateTriangleMeshes ();
			this.lineMeshes = this.GenerateLineMeshes ();
			
			// Initialize shadow material
			this.shadowMaterial = new Material (this.shadowShader);
			
			// Create render bounds
			this._renderingBounds = new Bounds (this.master.hairData.m_bSphere.center, new Vector3(this.master.hairData.m_bSphere.radius, this.master.hairData.m_bSphere.radius, this.master.hairData.m_bSphere.radius));
		}
		
		public virtual void Update()
		{
			if (!this.debugBoundingBox)
				return;
			
			Color color = Color.green;
			// Render bounding box
			Vector3 v3Center = this.renderingBounds.center;
			Vector3 v3Extents = this.renderingBounds.extents;
			
			Vector3 v3FrontTopLeft     = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top left corner
			Vector3 v3FrontTopRight    = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z - v3Extents.z);  // Front top right corner
			Vector3 v3FrontBottomLeft  = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom left corner
			Vector3 v3FrontBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z - v3Extents.z);  // Front bottom right corner
			Vector3 v3BackTopLeft      = new Vector3(v3Center.x - v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top left corner
			Vector3 v3BackTopRight        = new Vector3(v3Center.x + v3Extents.x, v3Center.y + v3Extents.y, v3Center.z + v3Extents.z);  // Back top right corner
			Vector3 v3BackBottomLeft   = new Vector3(v3Center.x - v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom left corner
			Vector3 v3BackBottomRight = new Vector3(v3Center.x + v3Extents.x, v3Center.y - v3Extents.y, v3Center.z + v3Extents.z);  // Back bottom right corner
			
			v3FrontTopLeft     = transform.TransformPoint(v3FrontTopLeft);
			v3FrontTopRight    = transform.TransformPoint(v3FrontTopRight);
			v3FrontBottomLeft  = transform.TransformPoint(v3FrontBottomLeft);
			v3FrontBottomRight = transform.TransformPoint(v3FrontBottomRight);
			v3BackTopLeft      = transform.TransformPoint(v3BackTopLeft);
			v3BackTopRight     = transform.TransformPoint(v3BackTopRight);
			v3BackBottomLeft   = transform.TransformPoint(v3BackBottomLeft);
			v3BackBottomRight  = transform.TransformPoint(v3BackBottomRight);  
			
			Debug.DrawLine (v3FrontTopLeft, v3FrontTopRight, color);
			Debug.DrawLine (v3FrontTopRight, v3FrontBottomRight, color);
			Debug.DrawLine (v3FrontBottomRight, v3FrontBottomLeft, color);
			Debug.DrawLine (v3FrontBottomLeft, v3FrontTopLeft, color);
			
			Debug.DrawLine (v3BackTopLeft, v3BackTopRight, color);
			Debug.DrawLine (v3BackTopRight, v3BackBottomRight, color);
			Debug.DrawLine (v3BackBottomRight, v3BackBottomLeft, color);
			Debug.DrawLine (v3BackBottomLeft, v3BackTopLeft, color);
			
			Debug.DrawLine (v3FrontTopLeft, v3BackTopLeft, color);
			Debug.DrawLine (v3FrontTopRight, v3BackTopRight, color);
			Debug.DrawLine (v3FrontBottomRight, v3BackBottomRight, color);
			Debug.DrawLine (v3FrontBottomLeft, v3BackBottomLeft, color);
			
		}
		
		/// <summary>
		/// Generates the triangle meshes.
		/// Meshes are built of indices. Every vertices x-position will contain a triangleindex buffer index.
		/// </summary>
		/// <returns>The triangle meshes.</returns>
		protected virtual Mesh[] GenerateTriangleMeshes()
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
		protected virtual Mesh[] GenerateLineMeshes()
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
        /// Sets the simulation upscaling correction paramters to the given material.
        /// </summary>
        /// <param name="mat"></param>
        protected void SetSimulationTransformCorrection(Material mat)
        {
            mat.SetMatrix("_TFX_World2Object", this.transform.worldToLocalMatrix);
            mat.SetMatrix("_TFX_ScaleMatrix", Matrix4x4.Scale(this.transform.localScale));
            mat.SetMatrix("_TFX_Object2World", this.transform.localToWorldMatrix);

            Vector3 scale = new Vector3
            (
                1 / this.transform.lossyScale.x,
                1 / this.transform.lossyScale.y,
                1 / this.transform.lossyScale.z
            );
            mat.SetVector("_TFX_PositionOffset", Vector3.Scale(this.transform.position, scale) - this.transform.position);
        }
		
		/// <summary>
		/// Raises the destroy event.
		/// Releases all resources not needed any more.
		/// </summary>
		public virtual void OnDestroy()
		{
			this.g_TriangleIndicesBuffer.Release ();
            this.g_LineIndicesBuffer.Release();
		}
		
		/// <summary>
		/// Convertes a Matrix4x4 to a float array.
		/// </summary>
		/// <returns>The to float array.</returns>
		/// <param name="matrix">Matrix.</param>
		protected static float[] MatrixToFloatArray(Matrix4x4 matrix)
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
}
