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
	[HideInInspector]
	public int strandCount;

	public CapsuleCollider headCollider;
	
	/// <summary>
	/// This initializes tressfx and all of it's components.
	/// This function gets called from the TressFX Loader.
	/// </summary>
	/// <param name="vertices">Vertices.</param>
	/// <param name="strandIndices">Strand indices.</param>
	public void Initialize (TressFXStrand[] strands, int numVertices)
	{
		this.strandCount = strandCount;
		
		Vector4[] positionVectors = new Vector4[numVertices];
		Vector3[] referenceVectors = new Vector3[numVertices];
		float[] hairRestLengths = new float[numVertices];
		int[] strandIndices = new int[numVertices];
		int[] offsets = new int[strands.Length];

		float test = (new Vector3(24.233000f,87.055603f,19.246000f) - new Vector3(23.678499f,86.132896f,20.288401f)).magnitude;


		int index = 0;
		for (int i = 0; i < strands.Length; i++)
		{
			for (int j = 0; j < strands[i].vertices.Length; j++)
			{
				positionVectors[index] = strands[i].GetTressFXVector(j);
				if (j < strands[i].vertices.Length - 1)
				{
					hairRestLengths[index] = (strands[i].vertices[j].pos - strands[i].vertices[j+1].pos).magnitude;
				}
				else
				{
					hairRestLengths[index] = 0;
				}
				strandIndices[index] = j;
				index++;
			}
			offsets[i] = index;
		}

		for (int i = 1; i < positionVectors.Length; i++)
		{
			// Calculate reference vector
			if (i > 0 && i < numVertices - 1)
			{
				Vector3 lastVector = new Vector3(positionVectors[i-1].x, positionVectors[i-1].y, positionVectors[i-1].z);
				Vector3 currentVector = new Vector3(positionVectors[i].x, positionVectors[i].y, positionVectors[i].z);
				Vector3 nextVector = new Vector3(positionVectors[i+1].x, positionVectors[i+1].y, positionVectors[i+1].z);

				Vector3 X_i = (currentVector - lastVector);
				Vector3 X_i1 = (nextVector - currentVector);

				Vector3 X_i_norm = X_i.normalized;

				// Rotation matrix
				// X_i_norm.X, X_i_norm.Y, X_i_norm.Z
				// X_i_norm.X, X_i_norm.Y, X_i_norm.Z
				// X_i_norm.X, X_i_norm.Y, X_i_norm.Z
				float[,] matrix = new float[,]
				{
					{ X_i_norm.x, X_i_norm.x, X_i_norm.x },
					{ X_i_norm.y, X_i_norm.y, X_i_norm.y },
					{ X_i_norm.z, X_i_norm.z, X_i_norm.z }
				};

				Matrix4x4 rot = new Matrix4x4();
				rot.m00 = X_i_norm.x;
				rot.m01 = X_i_norm.x;
				rot.m02 = X_i_norm.x;
				rot.m03 = 0;
				rot.m10 = X_i_norm.y;
				rot.m11 = X_i_norm.y;
				rot.m12 = X_i_norm.y;
				rot.m13 = 0;
				rot.m20 = X_i_norm.z;
				rot.m21 = X_i_norm.z;
				rot.m22 = X_i_norm.z;
				rot.m23 = 0;
				rot.m30 = 0;
				rot.m31 = 0;
				rot.m32 = 0;
				rot.m33 = 0;

				rot = rot.inverse;
				rot.m03 = 0;
				rot.m13 = 0;
				rot.m23 = 0;
				rot.m30 = 0;
				rot.m31 = 0;
				rot.m32 = 0;
				rot.m33 = 0;

				referenceVectors[i] = rot * X_i1;
			}
		}

		// Initialize compute buffers
		this.InitialVertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.LastVertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.VertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.StrandIndicesBuffer = new ComputeBuffer(numVertices, 4);

		this.InitialVertexPositionBuffer.SetData(positionVectors);
		this.LastVertexPositionBuffer.SetData (positionVectors);
		this.VertexPositionBuffer.SetData (positionVectors);
		this.StrandIndicesBuffer.SetData(strandIndices);

		this.vertexCount = numVertices;
		this.strandCount = strands.Length;

		// Generate headcollider
		TressFXCapsuleCollider headCollider = new TressFXCapsuleCollider();
		
		headCollider.point1 = new Vector4(-0.095f, 92.000f, -9.899f, 26.5f);
		headCollider.point2 = new Vector4(-0.405f, 93.707f, 5.111f, 24.113f);

		TressFXSimulation simulation = this.gameObject.GetComponent<TressFXSimulation>();
		if (simulation != null)
		{
			// Initialize Simulation
			simulation.Initialize(headCollider, hairRestLengths, referenceVectors, offsets);
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
