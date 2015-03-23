using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TressFXHair))]
public class TressFXHairEditor : Editor
{
	public override void OnInspectorGUI()
	{
		// Get the tressfx hair instance
		TressFXHair target = (TressFXHair)this.target;

		EditorGUILayout.LabelField ("Hair Stats");
		EditorGUILayout.LabelField ("-----------------------------------");
		EditorGUILayout.LabelField ("Total Vertices: ", target.m_NumTotalHairVertices+"");
		EditorGUILayout.LabelField ("Total Strands: ", target.m_NumTotalHairStrands+"");
		EditorGUILayout.LabelField ("Guidance Vertices: ", target.m_NumGuideHairVertices+"");
		EditorGUILayout.LabelField ("Guidance Strands: ", target.m_NumGuideHairStrands+"");

		for (int i = 0; i < target.hairPartConfig.Length; i++)
		{
			EditorGUILayout.LabelField ("");
			EditorGUILayout.LabelField ("Hair section "+i);
			EditorGUILayout.LabelField ("-----------------------------------");
			target.hairPartConfig[i].Damping = EditorGUILayout.FloatField ("Damping", target.hairPartConfig[i].Damping);
			target.hairPartConfig[i].StiffnessForLocalShapeMatching = EditorGUILayout.FloatField ("Local shape matching stiffness", target.hairPartConfig[i].StiffnessForLocalShapeMatching);
			target.hairPartConfig[i].StiffnessForGlobalShapeMatching = EditorGUILayout.FloatField ("Global shape matching stiffness", target.hairPartConfig[i].StiffnessForGlobalShapeMatching);
			target.hairPartConfig[i].GlobalShapeMatchingEffectiveRange = EditorGUILayout.FloatField ("Global shape matching effective range", target.hairPartConfig[i].GlobalShapeMatchingEffectiveRange);
		}

		// Save hair data
		if (GUILayout.Button ("Save"))
		{
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
		}

		// Load new hair data
		if (GUILayout.Button("Load new Hairdata"))
		{
			// Load new hair file
			string hairfilePath = EditorUtility.OpenFilePanel ("Open TressFX Hair data", "", "tfxb");
			target.OpenHairData (hairfilePath);

			// Save
			EditorUtility.SetDirty(target);
			AssetDatabase.SaveAssets();
		}
	}
}