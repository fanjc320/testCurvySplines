// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader"Unlit/PaintBrush"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BrushTex("Brush Texture",2D)= "white" {}
		_Color("Color",Color)=(1,1,1,1)
		_UV("UV",Vector)=(0,0,0,0)
		_Size("Size",Range(1,1000))=1

		//_ScreenWidth("ScreenWidth", float) = 0
		//_ScreenHeight("ScreenHeight", float) = 0
		_BrushUVWidth("BrushUVWidth", float) = 0
		_BrushUVHeight("BrushUVHeight", float) = 0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" }
		LOD 100
		ZTest Always Cull Off ZWrite Off Fog{ Mode Off }
		Blend SrcAlpha OneMinusSrcAlpha
		//Blend One DstColor
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			sampler2D _BrushTex;
			fixed4 _UV;
			float _Size;
			float _BrushUVWidth;
			float _BrushUVHeight;
			fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}
			
			//fixed4 frag (v2f i) : SV_Target
			//{
			//	// sample the texture
			//	float size = _Size;
			//	float2 uv = i.uv + (0.5f/size);
			//	uv = uv - _UV.xy;
			//	uv *= size;
			//	fixed4 col = tex2D(_BrushTex,uv);
			//	col.rgb = 1;
			//	col *= _Color;
			//	return col;
			//}

			fixed4 frag(v2f i) : SV_Target
			{
			    float size = _Size;
			    //float2 uv = i.uv + (0.5f / size);
			    //uv = uv - _UV.xy; 
			    //uv *= size;
				float2 uv = float2(0,0);
				
				float xInBrush = i.uv.x - (_UV.x - _BrushUVWidth / 2);
				float yInBrush = i.uv.y - (_UV.y - _BrushUVHeight / 2);
				uv.x = xInBrush / _BrushUVWidth;
				uv.y = yInBrush / _BrushUVHeight;
			    fixed4 col = tex2D(_BrushTex, uv);
			    //col.rgb = 1;
			    //col *= _Color;
			    return col;
			}
			ENDCG
		}
	}
}
