using UnityEngine;
using System.Collections;

/// <summary>
/// This struct gets passed to the shaders for indexing every vertex to hair strand and hair ids.
/// </summary>
public struct StrandIndex
{
	public int vertexId;
	public int hairId;
	public int vertexCountInStrand;
}
