using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using System.Linq;
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

        public void Reimport()
        {
            // Export hair vertices
            /*int i = 0;
            Dictionary <int, HairMesh> meshes = new Dictionary<int, HairMesh>();
            for (i = 0; i < this.hairPartConfig.Length; i++)
                meshes.Add(i, new HairMesh());

            foreach (var index in this.m_pHairStrandType)
            {
                int offset = (i * this.m_NumOfVerticesPerStrand);
                HairStrand hs = new HairStrand()
                {
                    isGuidanceStrand = this.m_pFollowRootOffset[i].x == 0 && this.m_pFollowRootOffset[i].y == 0 && this.m_pFollowRootOffset[i].z == 0;
                };

                for (int j = 0; j < this.m_NumOfVerticesPerStrand; j++)
                {
                    hs.strands.Add(index, this.m_pVertices[offset+j])
                }
                meshes[offset + j].strands.Add(new )
            }*/
        }

        /// <summary>
        /// Merges other into this tressfx hair.
        /// </summary>
        /// <param name="other"></param>
        public void MergeIntoThis(TressFXHair other)
        {
            if (this.hairPartConfig.Length + other.hairPartConfig.Length > 4)
                throw new Exception("Tressfx merge error! Hair cannot have more than 4 parts");

            // Head info check
            if (this.m_NumOfVerticesPerStrand != other.m_NumOfVerticesPerStrand)
                throw new Exception("Tressfx merge error! Hair vertex count per strand must match!");

            /*int newPartCount = this.hairPartConfig.Length + other.hairPartConfig.Length;
            int otherPartCount = other.hairPartConfig.Length;*/
            int thisPartCount = this.hairPartConfig.Length;

            // Create lists
            List<int> pHairStrandType = new List<int>(this.m_pHairStrandType);
            List<Vector4> pRefVectors = new List<Vector4>(this.m_pRefVectors);
            List<Vector4> pGlobalRotations = new List<Vector4>(this.m_pGlobalRotations);
            List<Vector4> pLocalRotations = new List<Vector4>(this.m_pLocalRotations);
            List<Vector4> pVertices = new List<Vector4>(this.m_pVertices);
            List<Vector4> pTangents = new List<Vector4>(this.m_pTangents);
            List<float> pThicknessCoeffs = new List<float>(this.m_pThicknessCoeffs);
            List<Vector4> pFollowRootOffset = new List<Vector4>(this.m_pFollowRootOffset);
            List<float> pRestLengths = new List<float>(this.m_pRestLengths);
            List<int> triangleIndices = new List<int>(this.m_TriangleIndices);
            List<int> lineIndices = new List<int>(this.m_LineIndices);
            List<Vector4> texCoords = new List<Vector4>(this.m_TexCoords);

            // Prepare new strand types
            List<int> tmpIntList = new List<int>();
            for (int i = 0; i < other.m_pHairStrandType.Length; i++)
            {
                tmpIntList.Add(other.m_pHairStrandType[i] + thisPartCount);
            }

            // Add up new strand types
            pHairStrandType.AddRange(tmpIntList);
            tmpIntList.Clear();

            // Prepare indices
            // TRIANGLES
            int indexOffset = this.m_pVertices.Length;
            for (int i = 0; i < other.m_TriangleIndices.Length; i++)
            {
                tmpIntList.Add(other.m_TriangleIndices[i] + indexOffset);
            }

            // Add new triangle indices
            triangleIndices.AddRange(tmpIntList);
            tmpIntList.Clear();

            // LINES
            for (int i = 0; i < other.m_LineIndices.Length; i++)
            {
                tmpIntList.Add(other.m_LineIndices[i] + indexOffset);
            }

            // Add new triangle indices
            lineIndices.AddRange(tmpIntList);
            tmpIntList.Clear();

            // Add up static stuff
            pRefVectors.AddRange(other.m_pRefVectors);
            pGlobalRotations.AddRange(other.m_pGlobalRotations);
            pLocalRotations.AddRange(other.m_pLocalRotations);
            pVertices.AddRange(other.m_pVertices);
            pTangents.AddRange(other.m_pTangents);
            pThicknessCoeffs.AddRange(other.m_pThicknessCoeffs);
            pFollowRootOffset.AddRange(other.m_pFollowRootOffset);
            pRestLengths.AddRange(other.m_pRestLengths);
            texCoords.AddRange(other.m_TexCoords);

            // Write back to this
            this.m_pHairStrandType = pHairStrandType.ToArray();
            this.m_pRefVectors = pRefVectors.ToArray();
            this.m_pGlobalRotations = pGlobalRotations.ToArray();
            this.m_pLocalRotations = pLocalRotations.ToArray();
            this.m_pVertices = pVertices.ToArray();
            this.m_pTangents = pTangents.ToArray();
            this.m_pThicknessCoeffs = pThicknessCoeffs.ToArray();
            this.m_pFollowRootOffset = pFollowRootOffset.ToArray();
            this.m_pRestLengths = pRestLengths.ToArray();
            this.m_TriangleIndices = triangleIndices.ToArray();
            this.m_LineIndices = lineIndices.ToArray();
            this.m_TexCoords = texCoords.ToArray();

            // Part configs
            List<HairPartConfig> partConfigs = new List<HairPartConfig>();
            partConfigs.AddRange(this.hairPartConfig);
            partConfigs.AddRange(other.hairPartConfig);
            this.hairPartConfig = partConfigs.ToArray();

            // Recalc header
            this.m_NumTotalHairVertices = pVertices.Count;
            this.m_NumTotalHairStrands = this.m_NumTotalHairStrands + other.m_NumTotalHairStrands;
            this.m_NumGuideHairVertices = this.m_NumGuideHairVertices + other.m_NumGuideHairVertices;
            this.m_NumGuideHairStrands = this.m_NumGuideHairStrands + other.m_NumGuideHairStrands;

            // Bounds
            Bounds newBounds = new Bounds(this.m_bSphere.center, Vector3.one * this.m_bSphere.radius);
            newBounds.Encapsulate(new Bounds(other.m_bSphere.center, Vector3.one * this.m_bSphere.radius));

            this.m_bSphere.center = newBounds.center;
            this.m_bSphere.radius = Mathf.Max(newBounds.extents.x, newBounds.extents.y, newBounds.extents.z);
        }
	}
}
