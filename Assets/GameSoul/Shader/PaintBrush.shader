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

			//可做到只显示一次笔刷的效果
			//fixed4 frag(v2f i) : SV_Target
			//{
			//    float2 uv = float2(0, 0);
			//    uv = mul(_Matrix, float4(i.uv, 0, 1)).xy;
			//	float4 col = tex2D(_BrushTex, uv);
			//	col *= _Color;
			//    if (uv.x < 0.0f || uv.x > 1.0f || uv.y < 0.0f || uv.y > 1.0f)
			//	{
			//		col = float4(1, 1, 1, 1);
			//	}
							
			//	//col.rgb = 1;
			//    return col;
			//}

//也是做到只显示一次笔刷的效果，笔刷显示为白色
//fixed4 frag(v2f i) : SV_Target
//{
//    float2 uv = float2(0, 0);
//    uv = mul(_Matrix, float4(i.uv, 0, 1)).xy;
//    fixed4 col = _Color;
//    if (uv.x > 0.0f && uv.x < 1.0f && uv.y > 0.0f && uv.y < 1.0f)
//        col = tex2D(_BrushTex, uv); 
				
//    col.rgb = 1;
//    col *= _Color;
//    return col;
//}


//这个函数，如果brush边界的像素不是透明的，效果是在整个屏幕上tile
//fixed4 frag(v2f i) : SV_Target
//{
//    float2 uv = float2(0, 0);
//    uv = mul(_Matrix, float4(i.uv, 0, 1)).xy;
//    fixed4 col = tex2D(_BrushTex, uv);
//    col.rgb = 1;
//    col *= _Color;
//    return col;
//}

fixed4 frag(v2f i) : SV_Target
{
    float2 uv = float2(0, 0);
    uv = mul(_Matrix, float4(i.uv, 0, 1)).xy;
    fixed4 col = _Color;
    if (uv.x > 0.0f && uv.x < 1.0f && uv.y > 0.0f && uv.y < 1.0f)
        //col = tex2D(_BrushTex, uv);
        return tex2D(_BrushTex, uv);
	else 
        //return tex2D(_BrushTex, uv);
        return fixed4(0,0,0,0);//这个解决了边缘不是透明的brush会覆盖全屏的问题
    //return col;
}


			ENDCG
		}
	}
}

//参考
//https://discussions.unity.com/t/replicating-matrix4x4-trs-function-on-the-gpu/926210/5
//https://docs.unity.cn/cn/current/ScriptReference/Material.SetMatrix.html