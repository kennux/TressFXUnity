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
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("Hair section 0");
		EditorGUILayout.LabelField ("-----------------------------------");
		target.Damping0 = EditorGUILayout.FloatField ("Damping", target.Damping0);
		target.StiffnessForLocalShapeMatching0 = EditorGUILayout.FloatField ("Local shape matching stiffness", target.StiffnessForLocalShapeMatching0);
		target.StiffnessForGlobalShapeMatching0 = EditorGUILayout.FloatField ("Global shape matching stiffness", target.StiffnessForGlobalShapeMatching0);
		target.GlobalShapeMatchingEffectiveRange0 = EditorGUILayout.FloatField ("Global shape matching effective range", target.GlobalShapeMatchingEffectiveRange0);
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("Hair section 1");
		EditorGUILayout.LabelField ("-----------------------------------");
		target.Damping1 = EditorGUILayout.FloatField ("Damping", target.Damping1);
		target.StiffnessForLocalShapeMatching1 = EditorGUILayout.FloatField ("Local shape matching stiffness", target.StiffnessForLocalShapeMatching1);
		target.StiffnessForGlobalShapeMatching1 = EditorGUILayout.FloatField ("Global shape matching stiffness", target.StiffnessForGlobalShapeMatching1);
		target.GlobalShapeMatchingEffectiveRange1 = EditorGUILayout.FloatField ("Global shape matching effective range", target.GlobalShapeMatchingEffectiveRange1);
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("Hair section 2");
		EditorGUILayout.LabelField ("-----------------------------------");
		target.Damping2 = EditorGUILayout.FloatField ("Damping", target.Damping2);
		target.StiffnessForLocalShapeMatching2 = EditorGUILayout.FloatField ("Local shape matching stiffness", target.StiffnessForLocalShapeMatching2);
		target.StiffnessForGlobalShapeMatching2 = EditorGUILayout.FloatField ("Global shape matching stiffness", target.StiffnessForGlobalShapeMatching2);
		target.GlobalShapeMatchingEffectiveRange2 = EditorGUILayout.FloatField ("Global shape matching effective range", target.GlobalShapeMatchingEffectiveRange2);
		
		EditorGUILayout.LabelField ("");
		EditorGUILayout.LabelField ("Hair section 3");
		EditorGUILayout.LabelField ("-----------------------------------");
		target.Damping3 = EditorGUILayout.FloatField ("Damping", target.Damping3);
		target.StiffnessForLocalShapeMatching3 = EditorGUILayout.FloatField ("Local shape matching stiffness", target.StiffnessForLocalShapeMatching3);
		target.StiffnessForGlobalShapeMatching3 = EditorGUILayout.FloatField ("Global shape matching stiffness", target.StiffnessForGlobalShapeMatching3);
		target.GlobalShapeMatchingEffectiveRange3 = EditorGUILayout.FloatField ("Global shape matching effective range", target.GlobalShapeMatchingEffectiveRange3);

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
