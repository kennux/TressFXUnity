Shader "TressFX/HairShader" {
	Properties
	{
	}
	SubShader
	{
		Tags { "RenderType" = "Opaque" }
		Pass
		{
			Tags {"LightMode" = "ForwardBase"} 
			
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
			  float3 lightDir : TEXCOORD3;
			  float3 viewDir : TEXCOORD2;
			  LIGHTING_COORDS(0,1)
			};
			
			StructuredBuffer<float3> g_HairVertexTangents;
			StructuredBuffer<float3> g_HairVertexPositions;
			StructuredBuffer<float3> g_HairInitialVertexPositions;
			StructuredBuffer<int> g_TriangleIndicesBuffer;
			StructuredBuffer<int> g_HairThicknessCoeffs;
			uniform float3 g_vEye;
			uniform float4 g_WinSize;
			uniform float g_FiberRadius;
			uniform float g_bExpandPixels;
			uniform fixed4 _HairColor;
			uniform fixed _Shininess;
			uniform fixed _Gloss;
			uniform fixed4 _SpecularColor1;
          	uniform fixed4 _SpecularColor2;
			uniform float g_bThinTip;
			uniform float4 modelTransform;
			
			v2f vert (appdata_base input)
	        {
	            v2f o;
	            
				uint vertexId = g_TriangleIndicesBuffer[(int)input.vertex.x];
				
			    // Access the current line segment
			    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used

			    // Get updated positions and tangents from simulation result
			    float3 t = g_HairVertexTangents[index].xyz;
			    float3 vert = g_HairVertexPositions[index].xyz;
			    float3 vertexNormal = normalize(modelTransform.xyz - g_HairVertexPositions[index].xyz);
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
				
				appdata_base v;
				v.vertex = float4(posi, 1);
				
    			TRANSFER_VERTEX_TO_FRAGMENT(o);
    			
	            return o;
	        }
	        
	        /*inline float3 KajiyaKay (float3 N, float3 T, float3 H, float specNoise) 
	        {
	            float3 B = normalize(T + N * specNoise);
	            //return sqrt(1-pow(dot(B,H),2));
	            float dotBH = dot(B,H);
	            return sqrt(1-dotBH*dotBH);
	        }*/

	        half4 frag (v2f i) : COLOR
	        {
	        	/*half2 Specular12 = half2(_Shininess, 1 * 0.5f);
	        	float4 _AnisoDir = float4(0.0,1.0,0.0,0.0);
	        	float SpecShift = 0.8f;
	        	float _PrimaryShift = 1;
	        	float _SecondaryShift = 1;
	        	float _RimStrength = 0.5f;
	        	float atten = LIGHT_ATTENUATION(i);
	        	
		        fixed3 h = normalize(normalize(i.lightDir) + normalize(i.viewDir));
	            float dotNL = max(0,dot(i.normal, i.lightDir));
	            
	        //  Spec
	            float2 specPower = exp2(10 * Specular12 + 1) - 1.75;

	            // First specular Highlight / Do not add specNoise here 
	            float3 H = normalize(i.lightDir + i.viewDir);
	            float3 spec1 = specPower.x * pow( KajiyaKay(i.normal, _AnisoDir * SpecShift, H, _PrimaryShift), specPower.x);
	            // Add 2nd specular Highlight
	            float3 spec2 = specPower.y * pow( KajiyaKay(i.normal, _AnisoDir * SpecShift, H, _SecondaryShift ), specPower.y) * 1;
	        
	        //  Fresnel
	            fixed fresnel = exp2(-OneOnLN2_x6 * dot(h, i.lightDir));
	            spec1 *= _SpecularColor1 + ( 1.0 - _SpecularColor1 ) * fresnel;
	            spec2 *= _SpecularColor2 + ( 1.0 - _SpecularColor2 ) * fresnel;    
	            spec1 += spec2;

	            // Normalize
	            spec1 *= 0.125 * dotNL;

	            // Rim
	            fixed RimPower = saturate (1.0 - dot(i.normal, i.viewDir));
	            fixed Rim = _RimStrength * RimPower*RimPower;

	            fixed4 c;
	            // Diffuse Lighting: Lerp shifts the shadow boundrary for a softer look
	            float3 diffuse = saturate (lerp (0.25, 1.0, dotNL));
	            // Combine
	            c.rgb = ((_HairColor.rgb + Rim) * diffuse + spec1) *  unity_LightColor[0].rgb  * (atten * 2);
	            // c.a = s.Alpha;
	            return c;*/
	            
            	fixed4 c = fixed4(0,0,0,0);
            	if (_WorldSpaceLightPos0.w == 0)
            	{
		        	float atten = LIGHT_ATTENUATION(i);
					half3 h = normalize (i.lightDir + i.viewDir);
		
					fixed diff = max (0, dot (i.normal, i.lightDir));
					
					float nh = max (0, dot (i.normal, h));
					float spec = pow (nh, _Shininess*128.0) * _Gloss;
					
					
					c.rgb = (_HairColor.rgb * _LightColor0.rgb * diff + _LightColor0.rgb * _SpecColor.rgb * spec) * (atten * 2);
				}
				
				// c.a = s.Alpha + _LightColor0.a * _SpecColor.a * spec * atten;
				return c;
	        }
			ENDCG
		}
		
		/*Pass
		{
			Tags {"LightMode" = "ForwardAdd"} 
			Cull Off
			
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"
			#include "AutoLight.cginc"
			#include "Lighting.cginc"
			#pragma target 5.0
			#pragma multi_compile_fwdadd

			struct v2f {
			  float4 pos : SV_POSITION;
			  float4 normal : NORMAL;
			  float3 lightDir : TEXCOORD5;
			  float3 viewDir : TEXCOORD2;
			  float3 posWorld : TEXCOORD3;
			  float3 posLight : TEXCOORD4;
			  LIGHTING_COORDS(0,1)
			};
			
			StructuredBuffer<float3> g_HairVertexTangents;
			StructuredBuffer<float3> g_HairVertexPositions;
			StructuredBuffer<int> g_TriangleIndicesBuffer;
			uniform float3 g_vEye;
			uniform float4 g_WinSize;
			uniform float g_FiberRadius;
			uniform float g_bExpandPixels;
			uniform fixed4 _HairColor;
			uniform fixed _Shininess;
			uniform fixed _Gloss;
			
			v2f vert (appdata_base input)
	        {
	            v2f o;
	            
				uint vertexId = g_TriangleIndicesBuffer[(int)input.vertex.x];
				
			    // Access the current line segment
			    uint index = vertexId / 2;  // vertexId is actually the indexed vertex id when indexed triangles are used

			    // Get updated positions and tangents from simulation result
			    float3 t = g_HairVertexTangents[index].xyz;
			    float3 vert = g_HairVertexPositions[index].xyz;
			    float ratio = 1.0f; // ( g_bThinTip > 0 ) ? g_HairThicknessCoeffs[index] : 1.0f;

			    // Calculate right and projected right vectors
			    float3 right      = normalize( cross( t, normalize(vert - g_vEye) ) );
			    float2 proj_right = normalize( mul( UNITY_MATRIX_VP, float4(right, 0) ).xy );
			    
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
				o.normal = normalize(float4(vert,1));
                o.posWorld = posi;
                o.posLight = mul(_LightMatrix0, o.posWorld);
                
				o.lightDir = ObjSpaceLightDir( float4(input.vertex.xyz, 1) );
				o.viewDir = WorldSpaceViewDir( float4(input.vertex.xyz, 1) );
				
				appdata_base v;
				v.vertex = float4(posi, 1);
				
    			TRANSFER_VERTEX_TO_FRAGMENT(o);
    			
	            return o;
	        }

	        half4 frag (v2f i) : COLOR
	        {
				float3 normalDirection = normalize(i.normal);
	            float3 viewDirection = normalize(
	               _WorldSpaceCameraPos - float3(i.posWorld));
	            float3 lightDirection;
	            float attenuation;
	 
	            if (0.0 == _WorldSpaceLightPos0.w) // directional light?
	            {
	               attenuation = 1.0; // no attenuation
	               lightDirection = 
	                  normalize(_WorldSpaceLightPos0.xyz);
	            } 
	            else // point or spot light
	            {
	               float3 vertexToLightSource = 
	                  float3(_WorldSpaceLightPos0 - i.posWorld);
	               lightDirection = normalize(vertexToLightSource);
	 
	               float dist = i.posLight.z; 
	                  // use z coordinate in light space as signed distance
	               dist = length(vertexToLightSource);
	               attenuation = 1.0 / dist;
	               
	                  // texture lookup for attenuation               
	               // alternative with linear attenuation: 
	               //    float distance = length(vertexToLightSource);
	               //    attenuation = 1.0 / distance;
	            }
	 
	            float3 diffuseReflection = 
	               attenuation * _LightColor0.rgb * _HairColor.rgb
	               * max(0.0, dot(normalDirection, lightDirection));
	 
	            float3 specularReflection;
	            if (dot(normalDirection, lightDirection) < 0.0) 
	               // light source on the wrong side?
	            {
	               specularReflection = float3(0.0, 0.0, 0.0); 
	                  // no specular reflection
	            }
	            else // light source on the right side
	            {
	               specularReflection = attenuation * _LightColor0.rgb
	                  * _SpecColor.rgb * pow(max(0.0, dot(
	                  reflect(-lightDirection, normalDirection), 
	                  viewDirection)), _Shininess);
	            }
	 
	            return float4(diffuseReflection + specularReflection, 1.0);
	        }
			ENDCG
		}*/
		
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
