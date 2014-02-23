using UnityEngine;
using System.Collections;

/// <summary>
/// This struct gets passed to the shaders for indexing every vertex to hair strand and hair ids.
/// </summary>
public struct StrandIndex
{
	public int vertexInStrandId;
	public int hairId;
	public int vertexCountInStrand;
}

public struct TressFXCapsuleCollider
{
	public Vector4 point1; // xyz = top sphere center, w = radius
	public Vector4 point2; // xyz = bottom sphere center, w = radius * radius
}
