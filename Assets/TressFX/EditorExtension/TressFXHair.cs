using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System;

/// <summary>
/// Struct which represents one vertex in a strand.
/// </summary>
[Serializable]
public struct TressFXStrandVertex
{
	/// <summary>
	/// The position of the vertex.
	/// </summary>
	[SerializeField]
	public Vector3 position;

	/// <summary>
	/// The tangent of the vertex.
	/// </summary>
	[SerializeField]
	public Vector3 tangent;

	/// <summary>
	/// The texcoord of the vertex.
	/// </summary>
	[SerializeField]
	public Vector4 texcoord;

	/// <summary>
	/// Initializes a new instance of the <see cref="TressFXStrandVertex"/> struct.
	/// </summary>
	/// <param name="position">Position.</param>
	/// <param name="tangent">Tangent.</param>
	/// <param name="texcoord">Texcoord.</param>
	public TressFXStrandVertex(Vector3 position, Vector3 tangent, Vector4 texcoord)
	{
		this.position = position;
		this.tangent = tangent;
		this.texcoord = texcoord;
	}
}

/// <summary>
/// Tress FX bounding sphere.
/// </summary>
[Serializable]
public struct TressFXBoundingSphere
{
	/// <summary>
	/// The center position of the sphere.
	/// </summary>
	[SerializeField]
	public Vector3 center;

	/// <summary>
	/// The sphere radius.
	/// </summary>
	[SerializeField]
	public float radius;

	/// <summary>
	/// Initializes a new instance of the <see cref="TressFXBoundingSphere"/> struct.
	/// </summary>
	/// <param name="center">Center.</param>
	/// <param name="radius">Radius.</param>
	public TressFXBoundingSphere(Vector3 center, float radius)
	{
		this.center = center;
		this.radius = radius;
	}
}

/// <summary>
/// Tress FX hair asset type.
/// </summary>
public class TressFXHair : ScriptableObject
{
	/// <summary>
	/// The number of total hair vertices.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int m_NumTotalHairVertices;

	/// <summary>
	/// The number of total hair strands.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int m_NumTotalHairStrands;

	/// <summary>
	/// The max number of vertices in one strand.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int m_MaxNumOfVerticesInStrand;

	/// <summary>
	/// The number of guide hair vertices.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int m_NumGuideHairVertices;

	/// <summary>
	/// The number of guide hair strands.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int m_NumGuideHairStrands;

	/// <summary>
	/// The number of following hairs per one guide hair.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int m_NumFollowHairsPerOneGuideHair;

	/// <summary>
	/// An array which gets used for indexing the strands.
	/// As index you use the hair strand index, the value will be the hair file where it was loaded from.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int[] m_pHairStrandType;

	/// <summary>
	/// Initial reference vector of edges in their local frame for each hair segment.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public Vector4[] m_pRefVectors;

	/// <summary>
	/// The global rotations quaternions for each hair segment.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public Vector4[] m_pGlobalRotations;

	/// <summary>
	/// The local rotations quaternions for each hair segment.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public Vector4[] m_pLocalRotations;
	
	/// <summary>
	/// The hair strand vertices.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public Vector4[] m_pVertices;
	
	/// <summary>
	/// The hair strand tangents used by the lighting model (Kajiya-Kay).
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public Vector4[] m_pTangents;

	/// <summary>
	/// The preprocessed triangle vertices.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public TressFXStrandVertex[] m_pTriangleVertices;

	/// <summary>
	/// The hair thickness coefficients.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public float[] m_pThicknessCoeffs;

	/// <summary>
	/// The offsets from following hairs to their guidance hair.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public Vector4[] m_pFollowRootOffset;

	/// <summary>
	/// The distances between hair segments when they are resting.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public float[] m_pRestLengths;

	/// <summary>
	/// The hair bounding sphere.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public TressFXBoundingSphere m_bSphere;

	/// <summary>
	/// The triangle indices.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int[] m_TriangleIndices;

	/// <summary>
	/// The line indices.
	/// </summary>
	[SerializeField]
	[HideInInspector]
	public int[] m_LineIndices;

	/// <summary>
	/// Opens the hair data (tfxb) at the given path.
	/// </summary>
	/// <param name="path">Path.</param>
	public void OpenHairData(string path)
	{
		// Open file
		BinaryReader reader = null;
		try
		{
			reader = new BinaryReader (File.Open (path, FileMode.Open));

			// Load information variables
			this.m_NumTotalHairVertices = reader.ReadInt32 ();
			this.m_NumTotalHairStrands = reader.ReadInt32 ();
			this.m_MaxNumOfVerticesInStrand = reader.ReadInt32 ();
			this.m_NumGuideHairVertices = reader.ReadInt32 ();
			this.m_NumGuideHairStrands = reader.ReadInt32 ();
			this.m_NumFollowHairsPerOneGuideHair = reader.ReadInt32 ();

			// Load actual hair data
			this.m_pHairStrandType = TressFXLoader.ReadIntegerArray (reader, this.m_NumTotalHairStrands);
			this.m_pRefVectors = TressFXLoader.ReadVector4Array (reader, this.m_NumTotalHairVertices);
			this.m_pGlobalRotations = TressFXLoader.ReadVector4Array (reader, this.m_NumTotalHairVertices);
			this.m_pLocalRotations = TressFXLoader.ReadVector4Array (reader, this.m_NumTotalHairVertices);
			this.m_pVertices = TressFXLoader.ReadVector4Array (reader, this.m_NumTotalHairVertices);
			this.m_pTangents = TressFXLoader.ReadVector4Array (reader, this.m_NumTotalHairVertices);
			this.m_pTriangleVertices = TressFXLoader.ReadStrandVertexArray (reader, this.m_NumTotalHairVertices);
			this.m_pThicknessCoeffs = TressFXLoader.ReadFloatArray (reader, this.m_NumTotalHairVertices);
			this.m_pFollowRootOffset = TressFXLoader.ReadVector4Array (reader, this.m_NumTotalHairStrands);
			this.m_pRestLengths = TressFXLoader.ReadFloatArray (reader, this.m_NumTotalHairVertices);

			// Load bounding sphere
			this.m_bSphere = new TressFXBoundingSphere (TressFXLoader.ReadVector3 (reader), reader.ReadSingle ());

			// Load indices
			int triangleIndicesCount = TressFXLoader.ReadStringInteger(reader);
			this.m_TriangleIndices = TressFXLoader.ReadIntegerArray (reader, triangleIndicesCount);

			int lineIndicesCount = TressFXLoader.ReadStringInteger(reader);
			this.m_LineIndices = TressFXLoader.ReadIntegerArray (reader, lineIndicesCount);
			
			// We are ready!
			Debug.Log ("Hair loaded. Vertices loaded: " + this.m_NumTotalHairVertices + ", Strands: " + this.m_NumTotalHairStrands + ", Triangle Indices: " + triangleIndicesCount + ", Line Indices: " + lineIndicesCount);
		}
		finally
		{
			// Free the file
			reader.Close ();
		}
	}

	#if UNITY_EDITOR

	/// <summary>
	/// Creates a new asset.
	/// </summary>
	[MenuItem("Assets/Create/TressFX Hair")]
	public static void CreateAsset()
	{
		// Create new hair asset
		TressFXHair newHairData = ScriptableObjectUtility.CreateAsset<TressFXHair> ();

		// Open hair data
		string hairfilePath = EditorUtility.OpenFilePanel ("Open TressFX Hair data", "", "tfxb");
		newHairData.OpenHairData (hairfilePath);

		EditorUtility.SetDirty (newHairData);
		AssetDatabase.SaveAssets ();
	}
	#endif
}
