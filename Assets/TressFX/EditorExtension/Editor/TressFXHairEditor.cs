using UnityEngine;
using System.Collections;
using UnityEditor;
using TressFXLib;

namespace TressFX
{
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
			EditorGUILayout.LabelField ("Maximum Verts / strand: ", target.m_NumOfVerticesPerStrand+"");
			EditorGUILayout.LabelField ("Follow strands per strand: ", target.m_NumFollowHairsPerOneGuideHair+"");

			for (int i = 0; i < target.hairPartConfig.Length; i++)
			{
				EditorGUILayout.LabelField ("");
				EditorGUILayout.LabelField ("Hair section "+i);
				EditorGUILayout.LabelField ("-----------------------------------");
				target.hairPartConfig[i].Damping = EditorGUILayout.FloatField ("Damping", target.hairPartConfig[i].Damping);
				target.hairPartConfig[i].StiffnessForLocalShapeMatching = EditorGUILayout.FloatField ("LSM stiffness", target.hairPartConfig[i].StiffnessForLocalShapeMatching);
				target.hairPartConfig[i].StiffnessForGlobalShapeMatching = EditorGUILayout.FloatField ("GSM stiffness", target.hairPartConfig[i].StiffnessForGlobalShapeMatching);
				target.hairPartConfig[i].GlobalShapeMatchingEffectiveRange = EditorGUILayout.FloatField ("GSM effective range", target.hairPartConfig[i].GlobalShapeMatchingEffectiveRange);
			}

            // Merge
            EditorGUILayout.LabelField("Drag tressfx hair here to merge");
            TressFXHair mergeHair = (TressFXHair)EditorGUILayout.ObjectField(null, typeof(TressFXHair), false);

            if (mergeHair != null)
            {
                target.MergeIntoThis(mergeHair);
            }

                // Save hair data
            if (GUILayout.Button ("Save"))
			{
				EditorUtility.SetDirty(target);
				AssetDatabase.SaveAssets();
			}
		}
	}
}