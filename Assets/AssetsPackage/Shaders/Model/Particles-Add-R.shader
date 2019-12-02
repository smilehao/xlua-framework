// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

// Simplified Additive Particle shader. Differences from regular Additive Particle one:
// - no Tint color
// - no Smooth particle support
// - no AlphaTest
// - no ColorMask

// replacement of "HOG/Particles/Additive"
// merge GRB and Alpha channel
Shader "HOG/Particles/Additive_R" {
	Properties {
		_MainTex ("Particle RGB Texture", 2D) = "white" {}
		_EffectColor("EffectColor", Color) = (1,1,1,1)
	}

    SubShader {  
		Tags { "Queue"="Transparent+403" "IgnoreProjector"="True" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off Lighting Off ZWrite Off Fog { Mode Off }
          
		CGINCLUDE
		#pragma multi_compile LIGHTMAP_OFF LIGHTMAP_ON
		
		#include "UnityCG.cginc" 
		sampler2D _MainTex;

		float4 _MainTex_ST;
		uniform fixed4 _EffectColor;
  
		
			
		struct v2f {
			fixed4 pos : SV_POSITION;
			fixed2 uv : TEXCOORD0;
			fixed4 color : TEXCOORD1;
		};

	
		v2f vert (appdata_full v)
		{
			v2f o;
			o.pos = UnityObjectToClipPos(v.vertex);
			o.uv.xy = TRANSFORM_TEX(v.texcoord.xy,_MainTex);
			o.color = _EffectColor*v.color;
			return o;
		}
		ENDCG

		
		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag	
			fixed4 frag (v2f i) : COLOR
			{
				fixed r = tex2D(_MainTex, i.uv.xy).r;
				fixed4 o;
				o.rgb = r * i.color;
				o.a = i.color.a;
										
				return o;
			}
			ENDCG 
		}	
    }   

}