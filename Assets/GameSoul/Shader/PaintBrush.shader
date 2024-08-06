// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader"Unlit/PaintBrush"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BrushTex("Brush Texture",2D)= "white" {}
		_Color("Color",Color)=(1,1,1,1)
		//_Size("Size",Range(1,1000))=1
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
            matrix _Matrix;
			//float _Size;
			fixed4 _Color;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				return o;
			}


            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = float2(0, 0);
				uv = mul(_Matrix, float4(i.uv, 0, 1)).xy;
				fixed4 col = tex2D(_BrushTex, uv);
                return col;
            }

			ENDCG
		}
	}
}

//参考
//https://discussions.unity.com/t/replicating-matrix4x4-trs-function-on-the-gpu/926210/5
//https://docs.unity.cn/cn/current/ScriptReference/Material.SetMatrix.html