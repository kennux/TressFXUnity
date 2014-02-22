Shader "KennuX/HairShader"
{
	Properties
	{
		_HairColor ("Hair Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200
		Blend SrcAlpha OneMinusSrcAlpha // turn on alpha blending
		
		CGPROGRAM
		// Upgrade NOTE: excluded shader from OpenGL ES 2.0 because it does not contain a surface program or both vertex and fragment programs.
		#pragma exclude_renderers gles d3d9 opengl xbox360 ps3 flash
		#pragma target 5.0
		
		#include "UnityCG.cginc"
        
		// #pragma vertex vert
	    #pragma surface surf Lambert
	    #pragma target 5.0
		
		uniform fixed4 _HairColor;

		// /---------------------------------------------------------------------
		// | Surface Shader
		struct Input
		{
			fixed4 col : COLOR;
		};

		void surf (Input IN, inout SurfaceOutput o)
		{
			o.Albedo = _HairColor.rgb;
			o.Alpha = _HairColor.a;
		}
		
		ENDCG
	}
}
