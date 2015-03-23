using UnityEngine;
using System.Collections;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public class TressFXHairMaterial : ScriptableObject
{
	/// <summary>
	/// The color of the hair.
	/// </summary>
	[SerializeField]
	public Color hairColor = new Color(0.647f, 0.419f, 0.274f, 1);
	
	// Rendering/material variables
	[SerializeField]
	public float fiberAlpha = 1.0f;
	[SerializeField]
	public float fiberRadius = 0.14f;
	[SerializeField]
	public bool expandPixels = true;
	[SerializeField]
	public bool thinTip = true;
	[SerializeField]
	public float alphaThreshold = 0.1f;
	[SerializeField]
	public float g_MatKd = 0.4f;
	[SerializeField]
	public float g_MatKa = 0;
	[SerializeField]
	public float g_MatKs1 = 0.14f;
	[SerializeField]
	public float g_MatEx1 = 80;
	[SerializeField]
	public float g_MatKs2 = 0.5f;
	[SerializeField]
	public float g_MatEx2 = 8;

	#if UNITY_EDITOR
	
	/// <summary>
	/// Creates a new asset.
	/// </summary>
	[MenuItem("Assets/Create/TressFX/Hair Material")]
	public static void CreateAsset()
	{
		// Create new hair asset
		TressFXHairMaterial newHairData = ScriptableObjectUtility.CreateAsset<TressFXHairMaterial> ("New Hair Material");
		
		EditorUtility.SetDirty (newHairData);
		AssetDatabase.SaveAssets ();
	}
	#endif
}
