using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.IO;
using System;
using TressFXLib;

namespace TressFX
{
	[Serializable]
	public struct HairPartConfig
	{
		[SerializeField]
		public float Damping;
		
		[SerializeField]
		public float StiffnessForLocalShapeMatching;
		
		[SerializeField]
		public float StiffnessForGlobalShapeMatching;
		
		[SerializeField]
		public float GlobalShapeMatchingEffectiveRange;
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
		public int m_NumOfVerticesPerStrand;

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
		/// The texcoords of the hair.
		/// </summary>
		[SerializeField]
		[HideInInspector]
		public Vector4[] m_TexCoords;
		
		/// <summary>
		/// The hair parts simulation configuration.
		/// </summary>
		[SerializeField]
		public HairPartConfig[] hairPartConfig;
	}
}
