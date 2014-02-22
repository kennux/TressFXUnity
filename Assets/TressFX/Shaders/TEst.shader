Shader "Water/Tessellated Water" {
Properties {
    _Color ("Color", color) = (1,1,1,0)
    _SpecColor ("Spec color", color) = (0.5,0.5,0.5,0.5)
    _Gloss ("Spec gloss", Range(0,2)) = 0.5
    _Shininess ("Spec shininess", Range(0,2)) = 0.5
    
    _FresnelTex ("Fresnel color (RGB)", 2D) = "white" {}
    _DispTex ("Disp Texture", 2D) = "black" {}
    _NormalMap ("Normalmap", 2D) = "bump" {}
    _ReflectMap ("Reflection Cubemap", Cube) = "white" {}
    
    _ReflectStrength ("Reflection strength", Range(0,1)) = 0.5
    
    _Displacement ("Displacement", Range(0, 10.0)) = 0.3
    _DisplacementMul ("Displacement multiplier", Range(0, 4.0)) = 0.3
	DispSpeed ("Displacement speed (map1: x=x,y=y, map2: x=z, y=w)", Vector) = (1,1,1,1)
	
	_WaveScale ("Wave scale", Range (0.02,1.0)) = .07
	WaveSpeed ("Wave speed (map1 x,y; map2 x,y)", Vector) = (2,1,-2,-1)
	
    _EdgeLength ("Edge length (1-50)", Range(1,50)) = 4
    _MinTessDist ("Tess min distance", float) = 0
    _MaxTessDist ("Tess max distance", float) = 100
    _Tessellation ("Static tesselation", Range(1,32)) = 1
	_Phong ("Phong Strengh", Range(0,1)) = 0.5
	
	_CausticTex ("Caustics texture", 2D) = "black" {}
	_CausticStrength ("Caustics strength", Range(0,1)) = 0
	
	_PermutationTable ("Permutation table", 2D) = "white" {}
	_NoiseStrength ("Noise strength", Range(0, 1)) = 0.5
}
SubShader {
    Tags { "RenderType"="Opaque" "Quene"="Transparent" }
    LOD 500
	Blend SrcAlpha OneMinusSrcAlpha
    ZWrite On

    CGPROGRAM
    #pragma surface surf BlinnPhong addshadow fullforwardshadows vertex:disp tessellate:tessEdge tessphong:_Phong approxview nolightmap
    #pragma target 5.0
    #include "Tessellation.cginc"
    #include "UnityCG.cginc"

    struct appdata {
        float4 vertex : POSITION;
        float4 tangent : TANGENT;
        float3 normal : NORMAL;
        float2 texcoord : TEXCOORD0;
    };

    uniform float _EdgeLength;
    uniform float _Tessellation;
    uniform float _MinTessDist;
    uniform float _MaxTessDist;
	uniform float _WaveScale;
	uniform float4 _WaveOffset;
	uniform float4 _DispOffset;
    
    uniform float _Displacement;
    uniform float _DisplacementMul;
    
    uniform float _Phong;

	// Samplers
    uniform sampler2D _DispTex;
    uniform sampler2D _FresnelTex;
    uniform sampler2D _NormalMap;
    uniform sampler2D _CausticTex;
    uniform sampler2D _PermutationTable;
    uniform samplerCUBE _ReflectMap;
    
    uniform fixed4 _Color;
    uniform fixed _Gloss;
    uniform fixed _Shininess;
    uniform fixed _CausticStrength;
    uniform float _ReflectStrength;
    uniform float _NoiseStrength;
    
    // Used by TRANSFORM_TEX
    uniform float4 _DispTex_ST;

	struct Input {
		float3 worldRefl;
		float3 viewDir;
		float2 uv_Underground;
		float3 worldPos;
		float displacement;
		INTERNAL_DATA
	};
    
    // Perlin noise lib
	// Noise is currently not done! I'll implement noise animation later.
	float FADE(float t) { return t * t * t * ( t * ( t * 6.0f - 15.0f ) + 10.0f ); }
	
	float LERP(float t, float a, float b) { return (a) + (t)*((b)-(a)); }
		
	int PERM(int i)
	{
		return tex2Dlod(_PermutationTable, float4((float)i, 0.0, 0, 0)).a * 255.0f;
	}
	
	float GRAD2(int hash, float x, float y)
	{
		int h = hash % 16;
    	float u = h<4 ? x : y;
    	float v = h<4 ? y : x;
		int hn = h % 2;
		int hm = (h/2) % 2;
    	return ((hn != 0) ? -u : u) + ((hm != 0) ? -2.0f*v : 2.0f*v);
	}
	
	float NOISE2D(float x, float y)
	{
		//returns a noise value between -0.75 and 0.75
		int ix0, iy0, ix1, iy1;
	    float fx0, fy0, fx1, fy1, s, t, nx0, nx1, n0, n1;
	    
	    ix0 = floor(x); 		// Integer part of x
	    iy0 = floor(y); 		// Integer part of y
	    fx0 = x - ix0;        	// Fractional part of x
	    fy0 = y - iy0;        	// Fractional part of y
	    fx1 = fx0 - 1.0f;
	    fy1 = fy0 - 1.0f;
	    ix1 = (ix0 + 1) % 256; 	// Wrap to 0..255
	    iy1 = (iy0 + 1) % 256;
	    ix0 = ix0 % 256;
	    iy0 = iy0 % 256;
	    
	   	t = FADE( fy0 );
		s = FADE( fx0 );
		
		nx0 = GRAD2(PERM(ix0 + PERM(iy0)), fx0, fy0);
	    nx1 = GRAD2(PERM(ix0 + PERM(iy1)), fx0, fy1);
		
	    n0 = lerp(nx0, nx1, t);
	
	    nx0 = GRAD2(PERM(ix1 + PERM(iy0)), fx1, fy0);
	    nx1 = GRAD2(PERM(ix1 + PERM(iy1)), fx1, fy1);
		
	    n1 = lerp(nx0, nx1, t);
	
	    return 0.507f * lerp(n0, n1, s);
	    
	}
    // Perlin noise lib end!

	float4 tessEdge (appdata v0, appdata v1, appdata v2)
    {
    	float tess = UnityEdgeLengthBasedTess (v0.vertex, v1.vertex, v2.vertex, _EdgeLength) + _Tessellation;
    	return UnityDistanceBasedTess(v0.vertex, v1.vertex, v2.vertex, _MinTessDist, _MaxTessDist, tess);
    }

    void disp (inout appdata v)
    {
    	// Displacement map
    	fixed2 texcoords = TRANSFORM_TEX(v.texcoord, _DispTex);
    	fixed4 displacementCoords = float4(texcoords,texcoords) / unity_Scale.w + _DispOffset;

	    float d = tex2Dlod(_DispTex, float4(displacementCoords.xy  * float2(.4, .45),0,0)).r;
	    float d2 = tex2Dlod(_DispTex, float4(displacementCoords.zw,0,0)).r;
	    float displacement = 1;
	    
	    
	    // If there is no noise, don't calculate it
	    if (_NoiseStrength > 0)
	    {
	    	displacement = (d+d2) * ((_Displacement * _DisplacementMul) + ((NOISE2D(v.vertex.x, v.vertex.z) * _NoiseStrength) * sin(_Time.y))) ;
	    }
	    else
	    {
	    	displacement = (d+d2) * (_Displacement * _DisplacementMul);
	    }
	    
	    // Displace
	    v.vertex.y += v.normal.y * displacement;
    }

    void surf (Input IN, inout SurfaceOutput o)
    {
		// scroll bump waves
		fixed4 temp;
		temp.xyzw = IN.worldPos.xzxz * _WaveScale / unity_Scale.w + _WaveOffset;
		fixed3 bump1 = UnpackNormal(tex2D( _NormalMap, temp.xy * float2(.4, .45) )).rgb;
		fixed3 bump2 = UnpackNormal(tex2D( _NormalMap, temp.wz )).rgb;
		o.Normal = (bump1 + bump2) * 0.5;
        
		// Reflection
		fixed3 R = IN.viewDir + ( 2 * dot(IN.viewDir, o.Normal )); 
		fixed4 reflectionColor = texCUBE(_ReflectMap, R);
		
        // Fresnel
        fixed fresnel = saturate(dot(IN.viewDir, o.Normal));
        fixed4 c = tex2D (_FresnelTex, float2(fresnel, fresnel)) * _Color;
        
        o.Albedo = c.rgb;
        
        // Calculate emission
        fixed3 emissionColor = float3(reflectionColor.rgb * _ReflectStrength);
        emissionColor.rgb = lerp(emissionColor.rgb, tex2D(_CausticTex, o.Normal), _CausticStrength);
        
        o.Emission = emissionColor.rgb;
        o.Alpha = _Color.a;
        
        // Specular 
        o.Specular = _Shininess;
        o.Gloss = _Gloss;
    }
	ENDCG
}

FallBack "Diffuse"

}