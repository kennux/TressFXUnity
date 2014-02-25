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

	// Strand indices buffer.
	// They will index every hair strand with it's hair id.
	[HideInInspector]
	public ComputeBuffer HairIndicesBuffer;

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
		int[] hairIndices = new int[strands.Length];

		// Rotations
		Quaternion[] localRotations = new Quaternion[numVertices];
		Quaternion[] globalRotations = new Quaternion[numVertices];

		// Transforms
		TressFXTransform[] localTransforms = new TressFXTransform[numVertices];
		TressFXTransform[] globalTransforms = new TressFXTransform[numVertices];

		int index = 0;
		for (int i = 0; i < strands.Length; i++)
		{
			hairIndices[i] = strands[i].hairId;

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

				// Init transforms
				globalTransforms[index] = new TressFXTransform();
				localTransforms[index] = new TressFXTransform();
				strands[i].localTransforms[j] = localTransforms[index];
				strands[i].globalTransforms[j] = globalTransforms[index];

				strandIndices[index] = j;
				index++;
			}

			offsets[i] = index;
		}

		// Init global / local frame
		for (int i = 0; i < positionVectors.Length; i++)
		{
			// Calculate rotations for vertex 0
			if (i == 0)
			{
				Vector3 currentVector = new Vector3(positionVectors[i].x, positionVectors[i].y, positionVectors[i].z);
				Vector3 nextVector = new Vector3(positionVectors[i+1].x, positionVectors[i+1].y, positionVectors[i+1].z);
				
				Vector3 vec = nextVector - currentVector;
				// Rotations for vertex 0 local = global
				Vector3 vecX = vec.normalized;
				
				Vector3 vecZ = Vector3.Cross(vecX, new Vector3(1, 0, 0));
				
				if (vecZ.magnitude * vecZ.magnitude < 0.0001f)
				{
					vecZ = Vector3.Cross(vecX, new Vector3(0, 1, 0));
				}
				
				vecZ.Normalize();
				
				Vector3 vecY = Vector3.Cross(vecZ, vecX).normalized;
				
				// Construct rotation matrix
				Matrix3x3 rotL2W = new Matrix3x3();
				rotL2W.matrixData[0,0] = vecX.x; rotL2W.matrixData[0,1] = vecY.x; rotL2W.matrixData[0,2] = vecZ.x;
				rotL2W.matrixData[1,0] = vecX.y; rotL2W.matrixData[1,1] = vecY.y; rotL2W.matrixData[1,2] = vecZ.y;
				rotL2W.matrixData[2,0] = vecX.z; rotL2W.matrixData[2,1] = vecY.z; rotL2W.matrixData[2,2] = vecZ.z;
				
				localTransforms[i].translation = currentVector;
				localTransforms[i].rotation = rotL2W.ToQuaternion();
				globalTransforms[i] = localTransforms[i];
			}
			else
			{
				// Normal rotation calculation
				Vector3 lastVector = new Vector3(positionVectors[i-1].x, positionVectors[i-1].y, positionVectors[i-1].z);
				Vector3 currentVector = new Vector3(positionVectors[i].x, positionVectors[i].y, positionVectors[i].z);
				
				Vector3 vec = Quaternion.Inverse(globalRotations[i-1]) * (currentVector - lastVector);
				
				Vector3 vecX = vec.normalized;
				
				Vector3 X = new Vector3(1.0f, 0, 0);
				Vector3 rotAxis = Vector3.Cross(X, vecX);
				float angle = Mathf.Acos(Vector3.Dot(X, vecX));
				
				
				if ( Mathf.Abs(angle) < 0.001 || rotAxis.magnitude < 0.001 )
				{
					localRotations[i] = Quaternion.identity;
				}
				else
				{
					rotAxis.Normalize();
					Quaternion rot = TressFXUtil.QuaternionFromAngleAxis(angle, rotAxis); // new Quaternion(rotAxis.x, rotAxis.y, rotAxis.z, angle);
					localRotations[i] = rot;
				}

				localTransforms[i].translation = vec;
				globalTransforms[i] = TressFXTransform.Multiply(globalTransforms[i-1], localTransforms[i]);
			}
		}

		// Generate rotations and reference vectors
		for (int i = 0; i < positionVectors.Length; i++)
		{
			referenceVectors[i] = localTransforms[i].translation;
			localRotations[i] = localTransforms[i].rotation;
			globalRotations[i] = localTransforms[i].rotation;
		}

		// Initialize compute buffers
		this.InitialVertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.LastVertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.VertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.StrandIndicesBuffer = new ComputeBuffer(numVertices, 4);
		this.HairIndicesBuffer = new ComputeBuffer(strands.Length, 4);

		this.InitialVertexPositionBuffer.SetData(positionVectors);
		this.LastVertexPositionBuffer.SetData (positionVectors);
		this.VertexPositionBuffer.SetData (positionVectors);
		this.StrandIndicesBuffer.SetData(strandIndices);
		this.HairIndicesBuffer.SetData(hairIndices);

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
			simulation.Initialize(headCollider, hairRestLengths, referenceVectors, offsets, localRotations, globalRotations);
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
