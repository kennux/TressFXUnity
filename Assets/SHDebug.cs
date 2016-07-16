using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using System.Collections;

public class SHDebug : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}


#if UNITY_EDITOR

[CustomEditor(typeof(SHDebug))]
public class SHDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var targetGo = ((SHDebug)target).gameObject;
        SphericalHarmonicsL2 sh;
        LightProbes.GetInterpolatedProbe(targetGo.transform.position, null, out sh);
        
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                EditorGUILayout.FloatField("SH_" + i + "_" + j, sh[i, j]);
            }
        }
    }
}

#endif