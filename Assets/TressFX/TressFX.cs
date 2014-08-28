using UnityEngine;
using System.Collections;

/// <summary>
/// Tress FX main class.
/// </summary>
public class TressFX : MonoBehaviour
{
	public TressFXHair hairData;

	/// <summary>
	/// The hair vertex positions buffer.
	/// </summary>
	public ComputeBuffer m_HairVertexPositions;

	/// <summary>
	/// Start this instance.
	/// Initializes all buffers and other resources needed by tressfx simulation and rendering.
	/// </summary>
	public void Start()
	{
		this.m_HairVertexPositions = new ComputeBuffer (this.hairData.m_NumGuideHairVertices, 16);
		this.m_HairVertexPositions.SetData (this.hairData.m_pVertices);
	}

	/// <summary>
	/// Raises the destroy event.
	/// Cleans up all used resources.
	/// </summary>
	public void OnDestroy()
	{
		this.m_HairVertexPositions.Release ();
	}
}
