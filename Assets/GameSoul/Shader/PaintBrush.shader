// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader"Unlit/PaintBrush"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_BrushTex("Brush Texture",2D)= "white" {}
		_Color("Color",Color)=(1,1,1,1)
		_UV("UV",Vector)=(0,0,0,0)
		//_BrushUV("Brush UV",Vector)=(0,0)
		_Size("Size",Range(1,1000))=1
		//_BrushRotation("BrushRotation",float4x4)
        //_Matrix("Matrix", float4x4)

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
            //fixed2 _BrushUV;
            matrix _Matrix;
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

//float4x4 Scale(float3 vec)
//{
//    float4x4 m;
//    m._m00 = vec.x;
//    m._m01 = 0.0;
//    m._m02 = 0.0;
//    m._m03 = 0.0;
//    m._m10 = 0.0;
//    m._m11 = vec.y;
//    m._m12 = 0.0;
//    m._m13 = 0.0;
//    m._m20 = 0.0;
//    m._m21 = 0.0;
//    m._m22 = vec.z;
//    m._m23 = 0.0;
//    m._m30 = 0.0;
//    m._m31 = 0.0;
//    m._m32 = 0.0;
//    m._m33 = 1.0;
//    return m;
//}

//// Creates a translation matrix.
//float4x4 Translate(float3 vec)
//{
//    float4x4 m;
//    m._m00 = 1.0;
//    m._m01 = 0.0;
//    m._m02 = 0.0;
//    m._m03 = vec.x;
//    m._m10 = 0.0;
//    m._m11 = 1.0;
//    m._m12 = 0.0;
//    m._m13 = vec.y;
//    m._m20 = 0.0;
//    m._m21 = 0.0;
//    m._m22 = 1.0;
//    m._m23 = vec.z;
//    m._m30 = 0.0;
//    m._m31 = 0.0;
//    m._m32 = 0.0;
//    m._m33 = 1.0;
//    return m;
//}

//// Creates a rotation matrix. Note: Assumes unit quaternion
//float4x4 Rotate(float4 q)
//{
//    // Precalculate coordinate products
//    float x = q.x * 2.0;
//    float y = q.y * 2.0;
//    float z = q.z * 2.0;
//    float xx = q.x * x;
//    float yy = q.y * y;
//    float zz = q.z * z;
//    float xy = q.x * y;
//    float xz = q.x * z;
//    float yz = q.y * z;
//    float wx = q.w * x;
//    float wy = q.w * y;
//    float wz = q.w * z;

//    // Calculate 3x3 matrix from orthonormal basis
//    float4x4 m;
//    m._m00 = 1.0f - (yy + zz);
//    m._m10 = xy + wz;
//    m._m20 = xz - wy;
//    m._m30 = 0.0;
//    m._m01 = xy - wz;
//    m._m11 = 1.0f - (xx + zz);
//    m._m21 = yz + wx;
//    m._m31 = 0.0;
//    m._m02 = xz + wy;
//    m._m12 = yz - wx;
//    m._m22 = 1.0f - (xx + yy);
//    m._m32 = 0.0;
//    m._m03 = 0.0;
//    m._m13 = 0.0;
//    m._m23 = 0.0;
//    m._m33 = 1.0;
//    return m;
//}

//float4x4 TRS(float4x4 s, float4x4 r, float4x4 t)
//{
//    float4x4 final = mul(t, mul(s, r));
//    return final;
//}
			
//fixed4 frag(v2f i) : SV_Target
//{
//				// sample the texture
//    float size = _Size;
//    float2 uv = i.uv + (0.5f / size);
//    uv = uv - _UV.xy;
//    uv *= size;
//    fixed4 col = tex2D(_BrushTex, uv);
//    col.rgb = 1;
//    col *= _Color;
//    return col;
//}

//fixed4 frag(v2f i) : SV_Target
//{
//    float size = _Size;
//			    //float2 uv = i.uv + (0.5f / size);
//			    //uv = uv - _UV.xy; 
//			    //uv *= size;
//    float2 uv = float2(0, 0);
				
//    float xInBrush = i.uv.x - (_UV.x - _BrushUVWidth / 2);
//    float yInBrush = i.uv.y - (_UV.y - _BrushUVHeight / 2);
//    uv.x = xInBrush / _BrushUVWidth;
//    uv.y = yInBrush / _BrushUVHeight;
//    fixed4 col = tex2D(_BrushTex, uv);
//			    //col.rgb = 1;
//			    //col *= _Color;
//    return col;
//}

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = float2(0, 0);
            				
    uv = mul(_Matrix, float4(i.uv, 0, 1)).xy;
    fixed4 col = tex2D(_BrushTex, uv);
                //fixed4 col = float4(1,0,0,1);
                //fixed4 col = tex2D(_BrushTex, _BrushUV);
                return col;
            }

			ENDCG
		}
	}
}

//参考
//https://discussions.unity.com/t/replicating-matrix4x4-trs-function-on-the-gpu/926210/5
//https://docs.unity.cn/cn/current/ScriptReference/Material.SetMatrix.html