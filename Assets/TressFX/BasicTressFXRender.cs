using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

/// <summary>
/// This class handles the rendering of the hairs.
/// 
/// Basic TressFX render implementation using a simple kajiya-kay rendering shader.
/// </summary>
[RequireComponent(typeof(TressFX))]
public class BasicTressFXRender : ATressFXRender
{
	public Shader hairShadowShader;
	private Material hairShadowMaterial;

	// Config
	public bool expandPixels = true;
	public float fiberRadius = 0.14f;
	public bool thinTip = true;

	public override void Initialize()
	{
		base.Initialize ();
		this.hairShadowMaterial = new Material (this.hairShadowShader);
	}

	public void LateUpdate()
	{
		for (int i = 0; i < this.hairMaterial.Length; i++)
		{
			this.hairMaterial[i].SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
			this.hairMaterial[i].SetBuffer("g_HairVertexTangents", this.master.TangentsBuffer);
			this.hairMaterial[i].SetBuffer("g_TriangleIndicesBuffer", this.master.TriangleIndicesBuffer);
			this.hairMaterial[i].SetVector("g_vEye", Camera.main.transform.position);
			this.hairMaterial[i].SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
			this.hairMaterial[i].SetFloat("g_FiberRadius", this.fiberRadius);
			this.hairMaterial[i].SetFloat("g_bExpandPixels", this.expandPixels ? 0 : 1);
			this.hairMaterial[i].SetFloat("g_bThinTip", this.thinTip ? 0 : 1);
			this.hairMaterial[i].SetBuffer ("g_HairInitialVertexPositions", this.master.InitialVertexPositionBuffer);
			this.hairMaterial[i].SetVector ("modelTransform", new Vector4 (this.transform.position.x, this.transform.position.y, this.transform.position.z, 1));
		}
		
		for (int i = 0; i < this.meshes.Count; i++)
		{
			for (int j = 0; j < this.meshes[i].Length; j++)
			{
				Graphics.DrawMesh(this.meshes[i][j], Vector3.zero, Quaternion.identity, this.hairMaterial[i], 8);
			}
		}

		// Render shadows
		this.hairShadowMaterial.SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
		for (int i = 0; i < this.lineMeshes.Length; i++)
		{
			Graphics.DrawMesh(this.lineMeshes[i], Vector3.zero, Quaternion.identity, this.hairShadowMaterial, 8);
		}
	}
}