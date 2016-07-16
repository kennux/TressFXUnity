using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace TressFX
{
    public class TressFXOITRenderer : ATressFXRender
    {
        #region Instances

        public static List<TressFXOITRenderer> renderers = new List<TressFXOITRenderer>();

        #endregion

        [Header("Configuration")]
        /// <summary>
        /// The hair material used for rendering the fill pass.
        /// </summary>
        public Material hairMaterial;

        /// <summary>
        /// How many lights do this hair pickup max?
        /// </summary>
        public int maxLightCount;

        [Range(0,1)]
        public float selfShadowStrength = 0.75f;

        public override void Awake()
        {
            base.Awake();
            renderers.Add(this);
        }

        /// <summary>
        /// Sets the shader parameters of this renderer to the specified material.
        /// </summary>
        /// <param name="material"></param>
        public void SetShaderParams(Material material)
        {
            // Set all properties
            material.SetBuffer("HairVertexTangents", this._master.g_HairVertexTangents);
            material.SetBuffer("HairVertexPositions", this._master.g_HairVertexPositions);
            material.SetBuffer("TriangleIndicesBuffer", this.g_TriangleIndicesBuffer);
            material.SetBuffer("LineIndicesBuffer", this.g_LineIndicesBuffer);
            material.SetBuffer("GlobalRotations", this._master.g_GlobalRotations);
            material.SetBuffer("HairThicknessCoeffs", this._master.g_HairThicknessCoeffs);
            material.SetBuffer("TexCoords", this._master.g_TexCoords);
            material.SetFloat("_ThinTip", this.hairMaterial.GetFloat("_ThinTip"));
            material.SetFloat("_HairWidth", this.hairMaterial.GetFloat("_HairWidth"));
            material.SetFloat("_HairWidthMultiplier", this.hairMaterial.GetFloat("_HairWidthMultiplier"));

            SetSimulationTransformCorrection(material);
        }

        public void RenderFillPass()
        {
            // Prepare all properties
            SetShaderParams(this.hairMaterial);
            SetShaderParams(this.shadowMaterial);

            // Render
            int renderCount = (int)(this._master.hairData.m_TriangleIndices.Length * 1);
            this.hairMaterial.SetPass(0);
            Graphics.DrawProcedural(MeshTopology.Triangles, renderCount);
        }
    }
}