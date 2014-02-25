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

public struct TressFXHairConfig
{
	public float globalStiffness;
	public float globalStiffnessMatchingRange;
	public float localStiffness;
	public float damping;
}