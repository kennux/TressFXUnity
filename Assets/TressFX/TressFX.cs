using UnityEngine;
using System.Collections;

/// <summary>
/// This class is the TressFX main class.
/// It will initialize all other components and stores the compute buffers used for simulation and drawing.
/// </summary>
public class TressFX : MonoBehaviour
{
	// Vertex position buffers used for instanced drawing

	/// <summary>
	/// This holds the initial vertex positions.
	/// This will only get used for simulation
	/// </summary>
	[HideInInspector]
	public ComputeBuffer InitialVertexPositionBuffer;

	/// <summary>
	/// The last vertex position buffer will hold the positions from the LAST frame.
	/// It will only get used for simulation.
	/// </summary>
	[HideInInspector]
	public ComputeBuffer LastVertexPositionBuffer;

	/// <summary>
	/// The vertex position buffer.
	/// This buffer holds the vertices CURRENT positions which will get used for drawing and simulation.
	/// </summary>
	[HideInInspector]
	public ComputeBuffer VertexPositionBuffer;

	// Strand indices buffer.
	// They will index every vertex in the hair strands, so the first vertex will have the index 0, the last for ex. 11
	// This way weighted animation / simulation can be done.
	[HideInInspector]
	public ComputeBuffer StrandIndicesBuffer;

	/// <summary>
	/// Holds the vertex count.
	/// </summary>
	[HideInInspector]
	public int vertexCount;

	public CapsuleCollider headCollider;
	
	/// <summary>
	/// This initializes tressfx and all of it's components.
	/// This function gets called from the TressFX Loader.
	/// </summary>
	/// <param name="vertices">Vertices.</param>
	/// <param name="strandIndices">Strand indices.</param>
	public void Initialize (Vector3[] vertices, StrandIndex[] strandIndices)
	{
		// Initialize compute buffers
		this.InitialVertexPositionBuffer = new ComputeBuffer(vertices.Length, 12);
		this.LastVertexPositionBuffer = new ComputeBuffer(vertices.Length, 12);
		this.VertexPositionBuffer = new ComputeBuffer(vertices.Length, 12);
		this.StrandIndicesBuffer = new ComputeBuffer(strandIndices.Length, 12);

		this.InitialVertexPositionBuffer.SetData(vertices);
		this.LastVertexPositionBuffer.SetData (vertices);
		this.VertexPositionBuffer.SetData (vertices);
		this.StrandIndicesBuffer.SetData(strandIndices);

		this.vertexCount = vertices.Length;

		// Generate headcollider
		TressFXCapsuleCollider headCollider = new TressFXCapsuleCollider();

		/*Vector3 topSphereCenter = this.headCollider.transform.localPosition + new Vector3(0, this.headCollider.radius / 2,0);
		Vector3 bottomSphereCenter = this.headCollider.transform.localPosition - new Vector3(0, this.headCollider.radius / 2,0);*/
		
		headCollider.point1 = new Vector4(-0.095f, 92.000f, -9.899f, 26.5f); // new Vector4(topSphereCenter.x, topSphereCenter.y, topSphereCenter.z, this.headCollider.radius);
		headCollider.point2 = new Vector4(-0.405f, 93.707f, 5.111f, 24.113f); // new Vector4(bottomSphereCenter.x, bottomSphereCenter.y, bottomSphereCenter.z, this.headCollider.radius*this.headCollider.radius);

		TressFXSimulation simulation = this.gameObject.GetComponent<TressFXSimulation>();
		if (simulation != null)
		{
			// Initialize Simulation
			simulation.Initialize(headCollider);
		}
		
		TressFXRender render = this.gameObject.GetComponent<TressFXRender>();
		if (render != null)
		{
			// Initialize Rendering
			render.Initialize();
		}

		Debug.Log ("TressFX Loaded! Hair vertices: " + this.vertexCount);
	}
}
