using UnityEngine;
using System.Collections.Generic;

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

	// Hair indices buffer.
	// They will index every hair strand with it's hair id.
	[HideInInspector]
	public ComputeBuffer HairIndicesBuffer;

	[HideInInspector]
	public ComputeBuffer TriangleIndicesBuffer;

	[HideInInspector]
	public ComputeBuffer TangentsBuffer;

	[HideInInspector]
	public ComputeBuffer ThicknessCoeffsBuffer;

	/// <summary>
	/// Holds the vertex count.
	/// </summary>
	[HideInInspector]
	public int vertexCount;

	/// <summary>
	/// The hair strand count.
	/// </summary>
	[HideInInspector]
	public int strandCount;

	public int triangleIndexCount
	{
		get { return this.triangleIndices.Length; }
	}

	// TressFX Data
	private Vector4[] positionVectors;
	private Vector3[] referenceVectors;
	private Vector3[] vectorTangents;
	private float[] hairRestLengths;
	private int[] strandIndices;
	private int[] offsets;
	private int[] hairIndices;
	private Quaternion[] localRotations;
	private Quaternion[] globalRotations;
	private TressFXTransform[] localTransforms;
	private TressFXTransform[] globalTransforms;
	public TressFXStrand[] strands;
	private int[] triangleIndices;
	private float[] thicknessCoeffs;
	private Dictionary<int, int> globalToLocalVertexIndexMappings;

	[HideInInspector]
	public int hairCount;

	[HideInInspector]
	public TressFXHairData[] hairData;

	/// <summary>
	/// This initializes tressfx and all of it's components.
	/// This function gets called from the TressFX Loader.
	/// </summary>
	/// <param name="vertices">Vertices.</param>
	/// <param name="strandIndices">Strand indices.</param>
	public void Initialize (TressFXStrand[] strands, int numVertices, int hairCount)
	{
		// Initialize data
		this.vertexCount = numVertices;
		this.strandCount = strands.Length;
		this.strands = strands;
		this.hairCount = hairCount;
		this.globalToLocalVertexIndexMappings = new Dictionary<int, int> ();
		this.hairData = this.GetComponent<TressFXLoader> ().hairs;

		// Buffer resources
		positionVectors = new Vector4[numVertices];
		Vector4[] initialPositionVectors = new Vector4[numVertices];
		referenceVectors = new Vector3[numVertices];
		vectorTangents = new Vector3[numVertices];
		hairRestLengths = new float[numVertices];
		strandIndices = new int[numVertices]; 
		offsets = new int[strands.Length];
		hairIndices = new int[strands.Length];

		// Rotations
		localRotations = new Quaternion[numVertices];
		globalRotations = new Quaternion[numVertices];

		// Transforms
		localTransforms = new TressFXTransform[numVertices];
		globalTransforms = new TressFXTransform[numVertices];

		List<int> triangleIndicesList = new List<int> ();
		thicknessCoeffs = new float[numVertices];

		// Initialize transforms and fill hair and strand indices, hair rest lengths and position vectors
		int index = 0;

		for (int i = 0; i < strands.Length; i++)
		{
			hairIndices[i] = strands[i].hairId;

			for (int j = 0; j < strands[i].vertices.Length; j++)
			{
				this.globalToLocalVertexIndexMappings.Add (index, j);
				// Load position of the strand
				positionVectors[index] = /*new Vector4(this.transform.position.x, this.transform.position.y, this.transform.position.z, 0) + */this.transform.localToWorldMatrix * strands[i].GetTressFXVector(j);
				initialPositionVectors[index] = strands[i].GetTressFXVector(j);

				// Get rest length
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

				// Set triangle indices
				if (j < strands[i].vertices.Length - 1)
				{
					triangleIndicesList.Add(2*index);
					triangleIndicesList.Add(2*index+1);
					triangleIndicesList.Add(2*index+2);
					triangleIndicesList.Add(2*index+2);
					triangleIndicesList.Add(2*index+1);
					triangleIndicesList.Add(2*index+3);
				}

				float tVal = strands[i].vertices[j].texcoords.z;
				thicknessCoeffs[index] = Mathf.Sqrt(1.0f - tVal * tVal);

				// Set strand indices currently used for cutting linestrips by a geometry shader
				strandIndices[index] = j;
				index++;
			}

			// Set strand offsets
			offsets[i] = index;
		}

		this.ThicknessCoeffsBuffer = new ComputeBuffer (numVertices, 4);
		this.ThicknessCoeffsBuffer.SetData (thicknessCoeffs);

		this.triangleIndices = triangleIndicesList.ToArray ();
		this.TriangleIndicesBuffer = new ComputeBuffer (this.triangleIndices.Length, 4);
		this.TriangleIndicesBuffer.SetData (this.triangleIndices);

		// Initialize frames
		this.InitializeLocalGlobalFrame();

		// Compute strand tangents
		this.ComputeStrandTangents();
		this.TangentsBuffer = new ComputeBuffer (this.vectorTangents.Length, 12);
		this.TangentsBuffer.SetData (this.vectorTangents);

		// Initialize compute buffers
		this.InitialVertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.LastVertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.VertexPositionBuffer = new ComputeBuffer(numVertices, 16);
		this.StrandIndicesBuffer = new ComputeBuffer(numVertices, 4);
		this.HairIndicesBuffer = new ComputeBuffer(strands.Length, 4);

		this.InitialVertexPositionBuffer.SetData(initialPositionVectors);
		this.StrandIndicesBuffer.SetData(strandIndices);
		this.HairIndicesBuffer.SetData(hairIndices);

		// Initialize simulation if existing
		TressFXSimulation simulation = this.gameObject.GetComponent<TressFXSimulation>();
		if (simulation != null)
		{
			simulation.Initialize(hairRestLengths, referenceVectors, offsets, localRotations, globalRotations);
		}
		
		// Initialize Rendering if existing
		TressFXRender render = this.gameObject.GetComponent<TressFXRender>();
		if (render != null)
		{
			render.Initialize();
		}
		
		this.LastVertexPositionBuffer.SetData (positionVectors);
		this.VertexPositionBuffer.SetData (positionVectors);

		Debug.Log ("TressFX Loaded! Hair vertices: " + this.vertexCount);
	}

	/// <summary>
	/// Raises the destroy event.
	/// </summary>
	public void OnDestroy()
	{
		this.InitialVertexPositionBuffer.Release ();
		this.VertexPositionBuffer.Release ();
		this.LastVertexPositionBuffer.Release ();
		this.TriangleIndicesBuffer.Release ();
		this.HairIndicesBuffer.Release ();
		this.StrandIndicesBuffer.Release ();
		this.TangentsBuffer.Release ();
		this.ThicknessCoeffsBuffer.Release ();
	}

	/// <summary>
	/// Inititalizes the local and global frame.
	/// </summary>
	private void InitializeLocalGlobalFrame()
	{
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
					Quaternion rot = TressFXUtil.QuaternionFromAngleAxis(angle, rotAxis);
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
	}

	/// <summary>
	/// Computes the strand tangents.
	/// </summary>
	private void ComputeStrandTangents()
	{
		// Calculate the first vertex tangent
		vectorTangents[0] = (positionVectors[1] - positionVectors[0]).normalized;

		// Calculate tangents
		for (int i = 1; i < this.vertexCount - 1; i++)
		{
			Vector3 tangent_pre = (positionVectors[i] - positionVectors[i-1]).normalized;
			Vector3 tangent_next = (positionVectors[i+1] - positionVectors[i]).normalized;
			vectorTangents[i] = (tangent_pre + tangent_next).normalized;
		}

		// Last tangent
		vectorTangents[this.vertexCount-1] = (positionVectors[this.vertexCount-1] - positionVectors[this.vertexCount-2]).normalized;
	}

	/// <summary>
	/// Calculates the parametric distance to the root for each vertex in the strand
	/// </summary>
	private void ComputeDistanceToRoot()
	{
		for (int i = 0; i < strands.Length; i++)
		{
			float strandLength = 0;

			// Iterate over every strand
			for (int j = 1; j < strands[i].vertices.Length; j++)
			{
				// Calculate segment length
				float segmentLength = (strands[i].vertices[j].pos - strands[i].vertices[j-1].pos).magnitude;
				strands[i].vertices[j].texcoords.z = strands[i].vertices[j-1].texcoords.z + segmentLength;

				strandLength += segmentLength;
			}

			// Re-iterate...
			for (int j = 1; j < strands[i].vertices.Length; j++)
			{
				strands[i].vertices[j].texcoords.z /= strandLength;
			}
		}
	}

	/// <summary>
	/// Transforms the vertices in positionVectors according the current gameobject's transform.
	/// </summary>
	private void TransformVertices()
	{
		// Transform vertices
		for (int i = 0; i < positionVectors.Length; i++)
		{
			positionVectors[i] = this.transform.TransformPoint(positionVectors[i]);
		}
	}

	/// <summary>
	/// Maps the triangle index identifier to strand vertex identifier.
	/// TODO: Implement this more... "nicely"..
	/// </summary>
	/// <returns>The triangle index identifier to strand vertex identifier.</returns>
	/// <param name="triangleIndexId">Triangle index identifier.</param>
	public int MapTriangleIndexIdToStrandVertexId(int triangleIndexId)
	{
		if (this.triangleIndices.Length > triangleIndexId)
		{
			int triangleIndex = this.triangleIndices [triangleIndexId];
			int globalVertexId = triangleIndex / 2;
			return globalToLocalVertexIndexMappings [globalVertexId];
		}
		else
		{
			return 0;
		}
	}
}
