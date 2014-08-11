Shader "TressFX/HairShader" {
	Properties
	{
		_HairColor ("Hair Color (RGB)", Color) = (1,1,1,1)
		_SpecShift ("Specular Shift", Float) = 0.5
		_PrimaryShift ("Primary Shift", Float) = 0.5
		_SecondaryShift ("Secondary Shift", Float) = 0.5
		_RimStrength ("Rim lighting strength", Float) = 0.5
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
			uniform float3 g_vEye;
			uniform float4 g_WinSize;
			uniform float g_FiberRadius;
			uniform float g_bExpandPixels;
			uniform float g_bThinTip;
			uniform float4 modelTransform;
			
			// Main props
			uniform fixed4 _HairColor;
			
			// Lighting
        	uniform float _SpecShift;
        	uniform float _PrimaryShift;
        	uniform float _SecondaryShift;
        	uniform float _RimStrength;
			uniform fixed4 _SpecularColor1;
          	uniform fixed4 _SpecularColor2;
			uniform float _Roughness1;
          	uniform float _Roughness2;
        	
        	// --------------------------------------
        	// TressFX Antialias shader written by AMD
        	// License is included in Readme.md
        	// --------------------------------------
			v2f vert (appdata_base input)
	        {
	            v2f o;
	            
				uint vertexId = g_TriangleIndicesBuffer[(int)input.vertex.x];
				
			    // Access the current line segment
			    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used

			    // Get updated positions and tangents from simulation result
			    float3 t = g_HairVertexTangents[index].xyz;
			    float3 vert = g_HairVertexPositions[index].xyz;
			    float3 vertexNormal = normalize(g_HairInitialVertexPositions[index].xyz);
			    vertexNormal.y = abs(vertexNormal.y);
			    float ratio = ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0f;

			    // Calculate right and projected right vectors
			    float3 right      = normalize( cross( t, normalize(vert - g_vEye) ) );
			    float2 proj_right = normalize( mul( UNITY_MATRIX_MVP, float4(right, 0) ).xy );
			    
			    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
			    float expandPixels = (g_bExpandPixels < 0 ) ? 0.0 : 0.71;

				// Calculate the negative and positive offset screenspace positions
				float4 hairEdgePositions[2]; // 0 is negative, 1 is positive
				float4 hairEdgePositionsNormal[2]; // 0 is negative, 1 is positive
				hairEdgePositions[0] = float4(vert +  -1.0 * right * ratio * g_FiberRadius, 1.0);
				hairEdgePositions[1] = float4(vert +   1.0 * right * ratio * g_FiberRadius, 1.0);
				hairEdgePositionsNormal[0] = hairEdgePositions[0];
				hairEdgePositionsNormal[1] = hairEdgePositions[1];
				hairEdgePositions[0] = mul(UNITY_MATRIX_MVP, hairEdgePositions[0]);
				hairEdgePositions[1] = mul(UNITY_MATRIX_MVP, hairEdgePositions[1]);
				hairEdgePositions[0] = hairEdgePositions[0]/hairEdgePositions[0].w;
				hairEdgePositions[1] = hairEdgePositions[1]/hairEdgePositions[1].w;
				
			    // Write output data
			    float fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;
			    float3 pos = (fDirIndex==-1.0 ? hairEdgePositions[0] : hairEdgePositions[1]) + fDirIndex * float3(proj_right * expandPixels / g_WinSize.y, 0.0f);
			    
			    float3 posi = (fDirIndex==-1.0 ? hairEdgePositionsNormal[0] : hairEdgePositionsNormal[1]) + fDirIndex * float3(proj_right * expandPixels / g_WinSize.y, 0.0f);
			    
				o.pos = float4(pos, 1);
				o.normal = float4(vertexNormal, 1);
                
				o.lightDir = WorldSpaceLightDir( float4(posi,1) );
				o.viewDir = WorldSpaceViewDir( float4(posi,1) );
				
				o.texcoords = input.texcoord.xy;
				o.Tangent = t;
				
				appdata_base v;
				v.vertex = float4(posi, 1);
				
    			TRANSFER_VERTEX_TO_FRAGMENT(o);
    			
	            return o;
	        }
	        
	        // Kajiya-Kay implementation from Lux shaders
	        // https://github.com/larsbertram69/Lux/blob/master/Lux%20Shader/Human/Hair/Lux%20Hair.shader
	        
	        struct FSLightingOutput
	        {
	        	fixed3 Albedo;
	        	fixed Alpha;
	        	float4 Normal;
	        	float3 Tangent;
	        	fixed SpecShift;
	        	fixed Specular;
	        	fixed SpecNoise;
	        	half2 Specular12;
	        	fixed3 SpecularColor;
	        };
	        
			inline float3 KajiyaKay (float3 N, float3 T, float3 H, float specNoise) 
			{
				float3 B = normalize(T + N * specNoise);
				//return sqrt(1-pow(dot(B,H),2));
				float dotBH = dot(B,H);
				return sqrt(1-dotBH*dotBH);
			}

			inline fixed4 LightingLuxHair (FSLightingOutput s, fixed3 lightDir, fixed3 viewDir, fixed atten)
			{
				fixed3 h = normalize(normalize(lightDir) + normalize(viewDir));
				float dotNL = max(0,dot(s.Normal, lightDir));

				//  Spec
				float2 specPower = exp2(10 * s.Specular12 + 1) - 1.75;

				// First specular Highlight / Do not add specNoise here 
				float3 H = normalize(lightDir + viewDir);
				float3 spec1 = specPower.x * pow( KajiyaKay(s.Normal, s.Tangent.xyz * s.SpecShift, H, _PrimaryShift), specPower.x);
				// Add 2nd specular Highlight
				float3 spec2 = specPower.y * pow( KajiyaKay(s.Normal, s.Tangent.xyz * s.SpecShift, H, _SecondaryShift ), specPower.y) * s.SpecNoise;

				//  Fresnel
				fixed fresnel = exp2(-OneOnLN2_x6 * dot(h, lightDir));
				spec1 *= _SpecularColor1 + ( 1.0 - _SpecularColor1 ) * fresnel;
				spec2 *= _SpecularColor2 + ( 1.0 - _SpecularColor2 ) * fresnel;    
				spec1 += spec2;

				// Normalize
				spec1 *= 0.125 * dotNL;

				// Rim
				fixed RimPower = saturate (1.0 - dot(s.Normal, viewDir));
				fixed Rim = _RimStrength * RimPower*RimPower;

				fixed4 c;
				// Diffuse Lighting: Lerp shifts the shadow boundrary for a softer look
				float3 diffuse = saturate (lerp (0.25, 1.0, dotNL));
				
				// Combine
				c.rgb = ((s.Albedo + Rim) * diffuse + spec1) * _LightColor0.rgb  * (atten * 2);
				c.a = s.Alpha;
				return c;
			}
			
	        fixed4 frag (v2f i) : COLOR
	        {
	        	FSLightingOutput o = (FSLightingOutput)0;
	        	
	        	fixed3 textureColor = tex2D(_MainTex, i.texcoords).rgb;
	        	
				o.Albedo = _HairColor.rgb * textureColor;
				o.Alpha = _HairColor.a;
				o.Normal = i.normal;
				
				fixed3 spec = tex2D(_SpecularTex, i.texcoords).rgb;
				
				o.SpecShift = spec.r * 2 - 1;
				
				// Calculate primary per Pixel Roughness * Roughness1
				o.Specular = spec.g * _Roughness1;
				// Store Roughness for direct lighting
				o.Specular12 = half2(o.Specular, spec.g * _Roughness2);
				// store per pixel Spec Noise
				o.SpecNoise = spec.b;

				// Lux Ambient Lighting functions also need o.SpecularColor(rgb) 
				// So we have to make it a bit more complicated here
				// Tweak Roughness for ambient lighting
				o.Specular *= o.Specular; 
				o.SpecularColor = _SpecularColor1.rgb;
				o.Tangent = i.Tangent;
				
				float atten = LIGHT_ATTENUATION(i);
				
				return LightingLuxHair(o, i.lightDir, i.viewDir, atten);
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
			uniform float3 g_vEye;
			uniform float4 g_WinSize;
			uniform float g_FiberRadius;
			uniform float g_bExpandPixels;
			uniform float g_bThinTip;

	        struct v2f {
	            V2F_SHADOW_COLLECTOR;
	        };

        	// --------------------------------------
        	// TressFX Antialias shader written by AMD
        	// --------------------------------------
	        v2f vert (appdata_base input)
	        { 
	        	// Access the current line segment
				uint vertexId = g_TriangleIndicesBuffer[(int)input.vertex.x]; // vertexId is actually the indexed vertex id when indexed triangles are used
				
			    uint index = vertexId / 2;  

			    // Get updated positions and tangents from simulation result
			    float3 t = g_HairVertexTangents[index].xyz;
			    float3 vert = g_HairVertexPositions[index].xyz;
			    float ratio = ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0f;

			    // Calculate right and projected right vectors
			    float3 right      = normalize( cross( t, normalize(vert - g_vEye) ) );
			    float2 proj_right = normalize( mul( UNITY_MATRIX_VP, float4(right, 0) ).xy );
			    
			    // g_bExpandPixels should be set to 0 at minimum from the CPU side; this would avoid the below test
			    float expandPixels = (g_bExpandPixels < 0 ) ? 0.0 : 0.71;

				// Calculate the negative and positive offset screenspace positions
				float4 hairEdgePositions[2]; // 0 is negative, 1 is positive
				hairEdgePositions[0] = float4(vert +  -1.0 * right * ratio * g_FiberRadius, 1.0);
				hairEdgePositions[1] = float4(vert +   1.0 * right * ratio * g_FiberRadius, 1.0);
				hairEdgePositions[0] = hairEdgePositions[0]/hairEdgePositions[0].w;
				hairEdgePositions[1] = hairEdgePositions[1]/hairEdgePositions[1].w;
				
			    // Write output data
			    float fDirIndex = (vertexId & 0x01) ? -1.0 : 1.0;
			    float3 pos = (fDirIndex==-1.0 ? hairEdgePositions[0] : hairEdgePositions[1]) + fDirIndex * float3(proj_right * expandPixels / g_WinSize.y, 0.0f);
		       	
	            
		        appdata_base v;
		        v.vertex = float4(pos.xyz, 1);
		        // v.normal = normalize(float4(vert,1));
	            
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
