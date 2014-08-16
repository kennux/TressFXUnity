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

	protected override void Render()
	{
		// Get MVP Matrix
		/*Matrix4x4 M = Matrix4x4.TRS (Vector3.zero, Quaternion.identity, Vector3.one);
		Matrix4x4 V = Camera.main.worldToCameraMatrix;
		Matrix4x4 P = GL.GetGPUProjectionMatrix (Camera.main.projectionMatrix, false);

		Matrix4x4 MVP = P*V*M;*/

		for (int i = 0; i < this.hairMaterial.Length; i++)
		{
			this.hairMaterial[i].SetBuffer("g_HairVertexPositions", this.master.VertexPositionBuffer);
			this.hairMaterial[i].SetBuffer("g_HairVertexTangents", this.master.TangentsBuffer);
			this.hairMaterial[i].SetBuffer("g_TriangleIndicesBuffer", this.master.TriangleIndicesBuffer);
			this.hairMaterial[i].SetBuffer("g_HairThicknessCoeffs", this.master.ThicknessCoeffsBuffer);
			this.hairMaterial[i].SetVector("g_WinSize", new Vector4((float) Screen.width, (float) Screen.height, 1.0f / (float) Screen.width, 1.0f / (float) Screen.height));
			this.hairMaterial[i].SetFloat("g_FiberRadius", this.fiberRadius);
			this.hairMaterial[i].SetFloat("g_bExpandPixels", this.expandPixels ? 0 : 1);
			this.hairMaterial[i].SetFloat("g_bThinTip", this.thinTip ? 0 : 1);
			this.hairMaterial[i].SetBuffer ("g_HairInitialVertexPositions", this.master.InitialVertexPositionBuffer);
			this.hairMaterial[i].SetMatrix ("inverseModelMatrix", Matrix4x4.TRS (this.transform.position, this.transform.rotation, Vector3.one).inverse);
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
	
	/// <summary>
	/// Convertes a Matrix4x4 to a float array.
	/// </summary>
	/// <returns>The to float array.</returns>
	/// <param name="matrix">Matrix.</param>
	private float[] MatrixToFloatArray(Matrix4x4 matrix)
	{
		return new float[] 
		{
			matrix.m00, matrix.m01, matrix.m02, matrix.m03,
			matrix.m10, matrix.m11, matrix.m12, matrix.m13,
			matrix.m20, matrix.m21, matrix.m22, matrix.m23,
			matrix.m30, matrix.m31, matrix.m32, matrix.m33
		};
	}
}