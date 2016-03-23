Shader "TressFX/Surface"
{
	Properties
	{
		_HairColor ( "Material base color", Color) = (0,0,0,1)
		_SpecularColor ( "Specular color", Color) = (0,0,0,1)
		g_MatKd ( "Material Kd", Range(0,1)) = 0.4
		g_MatKa ( "Material Ka", Range(0,1)) = 0
		g_MatKs1 ( "Material Ks1", Range(0,1)) = 0.14
		g_MatEx1 ( "Primary highlight strength (Ex1)", Range(0,100)) = 80
		g_MatEx2 ( "Secondary highlight strength (Ex2)", Range(0,10)) = 8
		g_MatKs2 ( "Material Ks2", Range(0,1)) = 0.5
		g_bThinTip ( "Thin tip (>0 means enabled)", Range(0,1)) = 0
		_HairWidth ( "HairWidth", Range(0,1)) = 0.14
		_HairWidthMultiplier ( "HairWidth Multiplier", Range(0,1)) = 1
		_HairColorTex ( "Hair color texture", 2D) = "white" {}
		_HairSpecularTexEx1 ( "Primary highlight multiplicator texture (Ex1)", 2D) = "white" {}
		_HairSpecularTexEx2 ( "Secondary highlight multiplicator texture (Ex2)", 2D) = "white" {}
		_RandomTex ("Random Texture (Set by TressFX)", 2D) = "white" {}
		_RandomMultiplicator ("Randomization multiplicator (The higher the more random)", Range(0,100)) = 0
	}
	SubShader
	{
		Tags { "RenderType"="TressFX" "Queue" = "Transparent" }
		LOD 200
		
		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#include "TressFX.cginc"
		#pragma surface surf KajiyaKay vertex:VertBase addshadow fullforwardshadows
		
		// Target = DX 11
		#pragma target 5.0
		#pragma only_renderers d3d11

		struct Input {
			float3 iTangent;
			float2 uv_HairColorTex;
		};

		// Color / Material parameters
		uniform float4 _HairColor;
		uniform float4 _SpecularColor;
		uniform float _Reflectivity;
		uniform float _RandomMultiplicator;
		
		// Deformation / Procedural mesh generation parameters
		uniform float g_bThinTip;
		uniform float _HairWidth;
		uniform float _HairWidthMultiplier;
		
		// Textures
		uniform sampler2D _HairColorTex;
		uniform sampler2D _HairSpecularTexEx1;
		uniform sampler2D _HairSpecularTexEx2;
		
		// MISC
		uniform int _VerticesPerStrand;
				
		void VertBase (inout appdata_full v, out Input o)
		{
			UNITY_INITIALIZE_OUTPUT(Input,o);
			#ifdef SHADER_API_D3D11
		    uint indexIndex = v.vertex.x;
			uint vertexId = g_TriangleIndicesBuffer[indexIndex];

			// Access the current line segment
			uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used
			float fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;

			// Get updated positions and tangents from simulation result
			float3 t = g_HairVertexTangents[index].xyz;
			float3 vert = GetVertexPosition(index); //  g_HairVertexPositions[index].xyz;
			float ratio = ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0;
			
			// Calculate right vector
			float3 right = normalize( cross( t, normalize(vert - _WorldSpaceCameraPos) ) );
			float3 left = normalize(cross (t, float3(0,1,0)));
			
			// Calculate normal
			v.normal.xyz = cross(left, t);
			v.vertex.xyz = vert + ((right * ((_HairWidth * _HairWidthMultiplier) * ratio))  * fDirIndex);
			v.texcoord.xy = g_TexCoords[vertexId].xy;
			
			// Set tangent
			o.iTangent = g_HairVertexTangents[index].xyz;
			#endif
		}
		
		void surf (Input IN, inout SurfaceOutputKajiyaKay o)
		{
			// Only "passthru" stuff
			o.Albedo = tex2D(_HairColorTex, IN.uv_HairColorTex) * _HairColor;
			o.SpecularEx1 = tex2D(_HairSpecularTexEx1, IN.uv_HairColorTex).r;
			o.SpecularEx2 = tex2D(_HairSpecularTexEx2, IN.uv_HairColorTex).r;
			o.Alpha = 1;
			o.Specular = _SpecularColor;
			o.iTangent = IN.iTangent;
			o.Random = tex2D(_RandomTex, IN.uv_HairColorTex).x * _RandomMultiplicator;
		}
		
		ENDCG
	}
}