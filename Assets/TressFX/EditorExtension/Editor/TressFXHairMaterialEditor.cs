using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TressFXHairMaterial))]
public class TressFXHairMaterialEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// Get the tressfx hair instance
		TressFXHairMaterial target = (TressFXHairMaterial)this.target;

		target.hairColor = EditorGUILayout.ColorField ("Hair Color", target.hairColor);
		target.alphaThreshold = EditorGUILayout.Slider ("Alpha threshold", target.alphaThreshold, 0.0f, 1.0f);
		target.fiberAlpha = EditorGUILayout.Slider ("Fiber alpha", target.fiberAlpha, 0.0f, 1.0f);
		target.fiberRadius = EditorGUILayout.Slider ("Fiber radius", target.fiberRadius, 0.0f, 1.0f);
		target.expandPixels = EditorGUILayout.Toggle ("Expand pixels", target.expandPixels);
		target.thinTip = EditorGUILayout.Toggle ("Thin tip", target.thinTip);
		target.g_MatEx1 = EditorGUILayout.FloatField ("Kajiya-Kay Ex1", target.g_MatEx1);
		target.g_MatEx2 = EditorGUILayout.FloatField ("Kajiya-Kay Ex2", target.g_MatEx2);
		target.g_MatKa = EditorGUILayout.FloatField ("Kajiya-Kay Ka", target.g_MatKa);
		target.g_MatKd = EditorGUILayout.FloatField ("Kajiya-Kay Kd", target.g_MatKd);
		target.g_MatKs1 = EditorGUILayout.FloatField ("Kajiya-Kay Ks1", target.g_MatKs1);
		target.g_MatKs2 = EditorGUILayout.FloatField ("Kajiya-Kay Ks2", target.g_MatKs2);
	}
}
