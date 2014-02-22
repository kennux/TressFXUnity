using UnityEngine;
using System.Collections;

public class TressFX : MonoBehaviour
{
	// Vertex position buffers used for instanced drawing
	[HideInInspector]
	public ComputeBuffer InitialVertexPositionBuffer;
	[HideInInspector]
	public ComputeBuffer LastVertexPositionBuffer;
	[HideInInspector]
	public ComputeBuffer VertexPositionBuffer;

	// Strand indices buffer.
	// They will index every vertex in the hair strands, so the first vertex will have the index 0, the last for ex. 11
	// This way weighted animation / simulation can be done.
	[HideInInspector]
	public ComputeBuffer strandIndicesBuffer;

	[HideInInspector]
	public int vertexCount;

	public Color HairColor;


	public void Initialize (Vector3[] vertices, int[] strandIndices)
	{
		// Initialize compute buffers
		this.InitialVertexPositionBuffer = new ComputeBuffer(vertices.Length, 12);
		this.LastVertexPositionBuffer = new ComputeBuffer(vertices.Length, 12);
		this.VertexPositionBuffer = new ComputeBuffer(vertices.Length, 12);
		this.strandIndicesBuffer = new ComputeBuffer(vertices.Length, 4);

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
	
	// Update is called once per frame
	void Update ()
	{
	}
}
