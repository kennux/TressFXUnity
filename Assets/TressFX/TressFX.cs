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
	public ComputeBuffer strandIndicesBuffer;
	public ComputeBuffer strandIndicesIntBuffer;

	/// <summary>
	/// Holds the vertex count.
	/// </summary>
	[HideInInspector]
	public int vertexCount;
	
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
		this.strandIndicesBuffer = new ComputeBuffer(strandIndices.Length, 12);

		this.InitialVertexPositionBuffer.SetData(vertices);
		this.LastVertexPositionBuffer.SetData (vertices);
		this.VertexPositionBuffer.SetData (vertices);
		this.strandIndicesBuffer.SetData(strandIndices);

		this.vertexCount = vertices.Length;
		
		TressFXSimulation simulation = this.gameObject.GetComponent<TressFXSimulation>();
		if (simulation != null)
		{
			// Initialize Simulation
			simulation.Initialize();
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
