using UnityEngine;
using System.Collections;
using UnityEditor;
using TressFXLib;

namespace TressFX
{
	[CustomEditor(typeof(TressFXHair))]
	public class TressFXHairEditor : Editor
	{
		private Vector3 scale = new Vector3();

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

			// Save hair data
			if (GUILayout.Button ("Save"))
			{
				EditorUtility.SetDirty(target);
				AssetDatabase.SaveAssets();
			}

			/*
			// Load new hair data TFXB
			if (GUILayout.Button("Load new Hairdata (TFXB)"))
			{
				// Load new hair file
				string hairfilePath = EditorUtility.OpenFilePanel ("Open TressFX Hair data", "", "tfxb");
				target.LoadHairData (Hair.Import(HairFormat.TFXB, hairfilePath));
				
				// Save
				EditorUtility.SetDirty(target);
				AssetDatabase.SaveAssets();
			}
			
			// Load new hair data ASE
			if (GUILayout.Button("Load new Hairdata (ASE)"))
			{
				// Load new hair file
				string hairfilePath = EditorUtility.OpenFilePanel ("Open TressFX Hair data", "", "ase");
				target.LoadHairData (Hair.Import(HairFormat.ASE, hairfilePath));
				
				// Save
				EditorUtility.SetDirty(target);
				AssetDatabase.SaveAssets();
			}
			*/
		}
	}
}