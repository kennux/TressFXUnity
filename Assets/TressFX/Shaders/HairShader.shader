Shader "TressFX/HairShader" {
	Properties
	{
		_HairColor ("Hair Color (RGB)", Color) = (1,1,1,1)
		_PrimaryShift ("Primary Shift", Float) = 0.5
		_SecondaryShift ("Secondary Shift", Float) = 0.5
		_SpecularColor1 ("Specular color 1", Color) = (1,1,1,1)
		_SpecularColor2 ("Specular color 2", Color) = (1,1,1,1)
		_Roughness1 ("Roughness1", Range(0,1)) = 0.5
		_Roughness2 ("Roughness2", Range(0,1)) = 0.5
		_MainTex ("Base (RGB)", 2D) = "white" {}
		_SpecularTex ("Specular texture (RGB)", 2D) = "white" {}
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			Tags {"LightMode" = "ForwardBase" } 
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#pragma target 5.0
			#pragma multi_compile_fwdbase
			#define OneOnLN2_x6 8.656170
          	#define Pi 3.14159265358979323846
          	
			struct v2f {
			  float4 pos : SV_POSITION;
			  float4 normal : NORMAL;
			  fixed2 texcoords : TEXCOORD4;
			  float3 lightDir : TEXCOORD3;
			  float3 viewDir : TEXCOORD2;
			  float3 Tangent : TANGENT;
			  LIGHTING_COORDS(0,1)
			};
			
			// Buffers
			StructuredBuffer<float3> g_HairVertexTangents;
			StructuredBuffer<float3> g_HairVertexPositions;
			StructuredBuffer<float3> g_HairInitialVertexPositions;
			StructuredBuffer<int> g_TriangleIndicesBuffer;
			StructuredBuffer<int> g_HairThicknessCoeffs;
			
			uniform sampler2D _MainTex;
			uniform sampler2D _SpecularTex;
			
			// Vertex shader props
			uniform float4 g_WinSize;
			uniform float g_FiberRadius;
			uniform float g_bExpandPixels;
			uniform float g_bThinTip;
			uniform float usePreprocessedData; 
			uniform float4x4 inverseModelMatrix;
			
			// Main props
			uniform fixed4 _HairColor;
			
			// Lighting
        	uniform float _SpecShift;
        	uniform float _PrimaryShift;
        	uniform float _SecondaryShift;
			uniform fixed4 _SpecularColor1;
          	uniform fixed4 _SpecularColor2;
			uniform float _Roughness1;
          	uniform float _Roughness2;
          	
        	
        	// --------------------------------------
        	// TressFX Antialias shader written by AMD
        	// License is included in Readme.md
        	// 
        	// Modified by KennuX
        	// --------------------------------------
			v2f vert (appdata_base v)
	        {
	            v2f o;
	            
			    // Index id
				uint vertexId = g_TriangleIndicesBuffer[(int)v.vertex.x];
				
			    // Access the current line segment
			    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used
			    
			    // Get tangent
			    float3 t = g_HairVertexTangents[index].xyz;
				float3 vert = g_HairVertexPositions[index].xyz;
				
			    // Get updated positions and tangents from simulation result
			    float ratio = ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0f;

			    // Calculate right and projected right vectors
			    float3 right      = normalize( cross( t, normalize(vert - _WorldSpaceCameraPos) ) );
			    float2 proj_right = normalize( mul( UNITY_MATRIX_MVP, float4(right, 0) ).xy );
			    
			    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
			    float expandPixels = (g_bExpandPixels < 0 ) ? 0.0 : 0.71;
			    
			    // Declare position variables
			    fixed fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;
			    
			    // Calculate final vertex position
			    float4 edgePosition = mul(UNITY_MATRIX_MVP,float4(vert +  fDirIndex * right * ratio * g_FiberRadius, 1.0));
			    edgePosition = edgePosition / edgePosition.w;
			    float4 position = edgePosition + fDirIndex * float4(proj_right * expandPixels / g_WinSize.y, 0.0, 0.0);
				
			    // Vertex data
				o.pos = position;
				o.normal = float4(normalize(mul(inverseModelMatrix, float4(vert,1)).xyz), 1);
				
				// Lighting data
				o.lightDir = WorldSpaceLightDir( float4(vert,1) );
				o.viewDir = WorldSpaceViewDir( float4(vert,1) );
				o.Tangent = t;
				
				o.texcoords = v.texcoord.xy;
				v.vertex = position;
				
    			TRANSFER_VERTEX_TO_FRAGMENT(o);
    			
	            return o;
	        }
	        
	        // Kajiya-Kay implementation from Lux shaders
	        // https://github.com/larsbertram69/Lux/blob/master/Lux%20Shader/Human/Hair/Lux%20Hair.shader
			inline float3 KajiyaKay (float3 N, float3 T, float3 H, float specNoise) 
			{
				// float3 B = normalize(T + N * specNoise);
				float dotBH = dot(normalize(T + N * specNoise),H);
				return sqrt(1-dotBH*dotBH);
			}
			
	        fixed4 frag (v2f i) : COLOR
	        {
				// Albedo calculation
				fixed3 Albedo = UNITY_LIGHTMODEL_AMBIENT.rgb * _HairColor.rgb * tex2D(_MainTex, i.texcoords).rgb;
				
				// Lighting precalculations
				fixed3 spec = tex2D(_SpecularTex, i.texcoords).rgb;
				half SpecShift = spec.r * 2 - 1;
				float atten = LIGHT_ATTENUATION(i);
				
				float dotNL = max(0,dot(i.normal, i.lightDir));

				//  Spec
				float2 specPower = exp2(10 * float2(spec.g * _Roughness1, spec.g * _Roughness2) + 1) - 1.75;

				// First specular Highlight / Do not add specNoise here 
				float3 H = normalize(i.lightDir + i.viewDir);
				float3 spec1 = specPower.x * pow( KajiyaKay(i.normal, i.Tangent.xyz * SpecShift, H, _PrimaryShift), specPower.x);
				// Add 2nd specular Highlight
				float3 spec2 = specPower.y * pow( KajiyaKay(i.normal, i.Tangent.xyz * SpecShift, H, _SecondaryShift ), specPower.y) * spec.b;

				//  Fresnel
				fixed fresnel = exp2(-OneOnLN2_x6 * dot(normalize(normalize(i.lightDir) + normalize(i.viewDir)), i.lightDir));
				spec1 *= _SpecularColor1 + ( 1.0 - _SpecularColor1 ) * fresnel;
				spec2 *= _SpecularColor2 + ( 1.0 - _SpecularColor2 ) * fresnel;    
				spec1 += spec2;

				// Normalize
				spec1 *= 0.125 * dotNL;
				
				// Diffuse Lighting: Lerp shifts the shadow boundrary for a softer look
				float3 diffuse = saturate (lerp (0.25, 1.0, dotNL));
				
				// Combine
				return fixed4((Albedo * diffuse + spec1) * _LightColor0.rgb  * (atten * 2), 1);
	        }
	        
			ENDCG
		}
		
		// Pass to render object as a shadow collector
	    Pass
	    {
	        Name "ShadowCollector"
	        Tags { "LightMode" = "ShadowCollector" }
	 
	        Fog {Mode Off}
			ZWrite On ZTest LEqual
	 
	        CGPROGRAM
	        #pragma vertex vert
	        #pragma fragment frag
	        #pragma multi_compile_shadowcollector
			#pragma target 5.0

	        #define SHADOW_COLLECTOR_PASS
	        #include "UnityCG.cginc"
			
			StructuredBuffer<float3> g_HairVertexTangents;
			StructuredBuffer<float3> g_HairVertexPositions;
			StructuredBuffer<int> g_TriangleIndicesBuffer;
			StructuredBuffer<float> g_HairThicknessCoeffs;
			uniform float4 g_WinSize;
			uniform float g_FiberRadius;
			uniform float g_bExpandPixels;
			uniform float g_bThinTip;

	        struct v2f {
	            V2F_SHADOW_COLLECTOR;
	        };

        	// --------------------------------------
        	// TressFX Antialias shader written by AMD
        	// 
        	// Modified by KennuX
        	// --------------------------------------
	        v2f vert (appdata_base v)
	        { 
	        	// Access the current line segment
				uint vertexId = g_TriangleIndicesBuffer[(int)v.vertex.x];
				
			    // Access the current line segment
			    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used
				
			    float3 vert = g_HairVertexPositions[index].xyz;
			    // Get updated positions and tangents from simulation result
			    float3 t = g_HairVertexTangents[index].xyz;
			    float ratio = ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0f;

			    // Calculate right and projected right vectors
			    float3 right      = normalize( cross( t, normalize(vert - _WorldSpaceCameraPos) ) );
			    
			    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
			    float expandPixels = (g_bExpandPixels < 0 ) ? 0.0 : 0.71;
			    
			    // Which direction to expand?
			    fixed fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;
			    
			    // Calculate the edge position
			    v.vertex = float4(vert +  fDirIndex * right * ratio * g_FiberRadius, 1.0);
	            
	            v2f o;
	            TRANSFER_SHADOW_COLLECTOR(o)
	            return o;
	        }

	        half4 frag (v2f i) : COLOR
	        {
	            SHADOW_COLLECTOR_FRAGMENT(i)
	        }
	        ENDCG
	    }
	} 
}