using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// This class handles the rendering of the hairs.
/// </summary>
[RequireComponent(typeof(TressFX))]
public class TressFXRender : MonoBehaviour
{
	private TressFX master;

	public Material hairMaterial;

	public bool expandPixels = true;

	[HideInInspector]
	public float renderTime;

	private Mesh[] meshes;

	public MeshFilter test;

	public void Initialize(Mesh[] meshes)
	{
		this.master = this.gameObject.GetComponent<TressFX> ();

		// Initialize mesh rendering
		/*for (int i = 0; i < meshes.Length; i++)
		{
			// Init game object
			GameObject g = new GameObject();
			g.transform.name = "Hair Mesh #"+i;
			g.transform.parent = this.transform;
			g.transform.localPosition = Vector3.zero;

			// Add renderers
			MeshFilter meshFilter = g.AddComponent<MeshFilter>();
			MeshRenderer meshRenderer = g.AddComponent<MeshRenderer>();

			meshFilter.sharedMesh = meshes[i];
			meshRenderer.sharedMaterial = this.hairMaterial;
		}*/
		this.meshes = meshes;
	}

	public void LateUpdate()
	{
		this.hairMaterial.SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
		this.hairMaterial.SetBuffer("g_HairVertexTangents", this.master.TangentsBuffer);
		this.hairMaterial.SetBuffer("g_TriangleIndicesBuffer", this.master.TriangleIndicesBuffer);
		this.hairMaterial.SetVector("g_vEye", Camera.main.transform.position);
		this.hairMaterial.SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
		this.hairMaterial.SetFloat("g_FiberRadius", 0.14f); // this.fiberRadius);
		this.hairMaterial.SetFloat("g_bExpandPixels", this.expandPixels ? 0 : 1);

		for (int i = 0; i < this.meshes.Length; i++)
		{
			
			// boundsTarget is the center of the camera's frustum, in world coordinates:
			Vector3 camPosition = Camera.main.transform.position;
			Vector3 normCamForward = Vector3.Normalize(Camera.main.transform.forward);
			float boundsDistance = (Camera.main.farClipPlane - Camera.main.nearClipPlane) / 2 + Camera.main.nearClipPlane;
			Vector3 boundsTarget = camPosition + (normCamForward * boundsDistance);
			
			// The game object's transform will be applied to the mesh's bounds for frustum culling checking.
			// We need to "undo" this transform by making the boundsTarget relative to the game object's transform:
			Vector3 realtiveBoundsTarget = this.transform.InverseTransformPoint(boundsTarget);
			
			// Set the bounds of the mesh to be a 1x1x1 cube (actually doesn't matter what the size is)
			this.meshes[i].bounds = test.sharedMesh.bounds; // new Bounds(realtiveBoundsTarget, Vector3.one);
			
			//Graphics.DrawMesh(this.meshes[i], this.transform.localToWorldMatrix, this.hairMaterial, 1);
			Graphics.DrawMesh(this.meshes[i], Vector3.zero, this.transform.rotation, this.hairMaterial, 1);
		}
	}
}