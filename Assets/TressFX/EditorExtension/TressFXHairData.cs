using UnityEngine;
using System.Collections;
using UnityEditor;

/// <summary>
/// Tress FX hair data object.
/// </summary>
public class TressFXHairData : ScriptableObject
{
	// HAIR LOADER PROPERTIES

	/// <summary>
	/// The hair model in tfx format.
	/// </summary>
	public TextAsset hairModel;

	/// <summary>
	/// If this is set to true both ends of the hair will be immovable.
	/// </summary>
	public bool makeBothEndsImmovable;

	// RENDERING PROPERTIES

	/// <summary>
	/// The hair material.
	/// </summary>
	public Material hairMaterial;

	// SIMULATION PROPERTIES

	/// <summary>
	/// The global stiffness.
	/// </summary>
	public float globalStiffness;

	/// <summary>
	/// The global stiffness matching range.
	/// </summary>
	public float globalStiffnessMatchingRange;

	/// <summary>
	/// The local stiffness.
	/// </summary>
	public float localStiffness;

	/// <summary>
	/// The movement damping.
	/// </summary>
	public float damping;

	[MenuItem("Assets/Create/TressFX Hair")]
	public static void CreateAsset()
	{
		ScriptableObjectUtility.CreateAsset<TressFXHairData> ();
	}
}
