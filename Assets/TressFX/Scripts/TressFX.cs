using UnityEngine;
using System.Collections;

namespace TressFX
{
	/// <summary>
	/// Tress FX main class.
	/// </summary>
	public class TressFX : MonoBehaviour
	{
		public TressFXHair hairData;

		/// <summary>
		/// The hair vertex positions buffer.
		/// </summary>
		public ComputeBuffer g_HairVertexPositions;
		public ComputeBuffer g_HairVertexPositionsPrev;
		public ComputeBuffer g_HairVertexTangents;
		public ComputeBuffer g_InitialHairPositions;
		public ComputeBuffer g_GlobalRotations;
		public ComputeBuffer g_LocalRotations;

		public ComputeBuffer g_HairThicknessCoeffs;
		public ComputeBuffer g_HairRestLengthSRV;
		public ComputeBuffer g_HairStrandType;
		public ComputeBuffer g_HairRefVecsInLocalFrame;
		public ComputeBuffer g_FollowHairRootOffset;
		public ComputeBuffer g_TexCoords;

		/// <summary>
		/// Start this instance.
		/// Initializes all buffers and other resources needed by tressfx simulation and rendering.
		/// </summary>
		public void Awake()
		{
			if (this.hairData == null)
			{
				Debug.LogError("No hair data assigned to TressFX :(");
			}

			// Vertex buffers
			this.g_HairVertexPositions = this.InitializeBuffer (this.hairData.m_pVertices, 16);
			this.g_HairVertexPositionsPrev = this.InitializeBuffer (this.hairData.m_pVertices, 16);
			this.g_InitialHairPositions = this.InitializeBuffer (this.hairData.m_pVertices, 16);

			// Tangents and rotations
			this.g_HairVertexTangents = this.InitializeBuffer (this.hairData.m_pTangents, 16);
			this.g_GlobalRotations = this.InitializeBuffer (this.hairData.m_pGlobalRotations, 16);
			this.g_LocalRotations = this.InitializeBuffer (this.hairData.m_pLocalRotations, 16);

			// Others
			this.g_HairRestLengthSRV = this.InitializeBuffer (this.hairData.m_pRestLengths, 4);
			this.g_HairStrandType = this.InitializeBuffer (this.hairData.m_pHairStrandType, 4);
			this.g_HairRefVecsInLocalFrame = this.InitializeBuffer (this.hairData.m_pRefVectors, 16);
			this.g_FollowHairRootOffset = this.InitializeBuffer (this.hairData.m_pFollowRootOffset, 16);
			this.g_HairThicknessCoeffs = this.InitializeBuffer (this.hairData.m_pThicknessCoeffs, 4);
			this.g_TexCoords = this.InitializeBuffer(this.hairData.m_TexCoords, 16);
		}

		/// <summary>
		/// Initializes the a new ComputeBuffer.
		/// </summary>
		/// <returns>The buffer.</returns>
		/// <param name="data">Data.</param>
		/// <param name="stride">Stride.</param>
		private ComputeBuffer InitializeBuffer(System.Array data, int stride)
		{
			ComputeBuffer returnBuffer = new ComputeBuffer (data.Length, stride);
			returnBuffer.SetData (data);
			return returnBuffer;
		}

		/// <summary>
		/// Raises the destroy event.
		/// Cleans up all used resources.
		/// </summary>
		public void OnDestroy()
		{
			this.g_HairVertexPositions.Release ();
			this.g_HairVertexPositionsPrev.Release ();
			this.g_InitialHairPositions.Release ();

			this.g_HairVertexTangents.Release ();
			this.g_GlobalRotations.Release ();
			this.g_LocalRotations.Release ();

			this.g_HairThicknessCoeffs.Release ();
			this.g_HairRestLengthSRV.Release ();
			this.g_HairStrandType.Release ();
			this.g_HairRefVecsInLocalFrame.Release ();
			this.g_FollowHairRootOffset.Release ();
			this.g_TexCoords.Release ();
        }
	}
}