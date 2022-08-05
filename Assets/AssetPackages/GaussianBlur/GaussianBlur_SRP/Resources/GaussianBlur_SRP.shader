/*
GaussianBlur_SRP.shader
uses properties and the global textures to generate a blurred effect on the screen.

*/


Shader "Custom/GaussianBlur_SRP"
{
	Properties
	{
		[PerRendererData] _MainTex ("_MainTex", 2D) = "white" {}
		
		[Normal] _DetailTex ("_DetailTex", 2D) = "white" {}
		
		_Distance("_Distance", Range(0,200)) = 25

		_Clearness ("_Clearness", Range(0,1)) = 1
		// _Lightness ("_Lightness", Range(0,1)) = 1

        // _Saturation ("_Saturation", Range(-10,10)) = 1

		 // required for UI.Mask
         [HideInInspector] _StencilComp ("Stencil Comparison", Float) = 8
         [HideInInspector] _Stencil ("Stencil ID", Float) = 0
         [HideInInspector] _StencilOp ("Stencil Operation", Float) = 0
         [HideInInspector] _StencilWriteMask ("Stencil Write Mask", Float) = 255
         [HideInInspector] _StencilReadMask ("Stencil Read Mask", Float) = 255
         [HideInInspector] _ColorMask ("Color Mask", Float) = 15
		 //
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
			"PreviewType" = "Plane"
			"DisableBatching" = "True"
		}

		// required for UI.Mask
        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp] 
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }
        ColorMask [_ColorMask]
		//

		Pass
		{
			ZWrite Off
			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"


			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				half4 color : COLOR;
				half4 screenpos : TEXCOORD2;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 screenuv : TEXCOORD1;
				half4 color : COLOR;
				float2 screenpos : TEXCOORD2;
			};

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				o.screenuv = ((o.vertex.xy / o.vertex.w) + 1) * 0.5;
				o.color = v.color;
				o.screenpos = ComputeScreenPos(o.vertex);
				return o;
			}

			float2 safemul(float4x4 M, float4 v)
			{
				float2 r;

				r.x = dot(M._m00_m01_m02, v);
				r.y = dot(M._m10_m11_m12, v);

				return r;
			}

			sampler2D _MainTex;
			float4 _MainTex_TexelSize;

			// x contains 1.0/width
			// y contains 1.0/height
			// z contains width
			// w contains height

			sampler2D _DetailTex;

            static const int LOOP_ITERATION = 5;

			uniform int _Distance;
			// uniform float _Lightness;
            uniform float  _Clearness;
            // uniform float _Saturation;

            uniform sampler2D _CameraOpaqueTexture;

			float4 frag(v2f i) : SV_Target
			{

				float4 color = float4(0,0,0,0);
				//color += tex2D(_MainTex, i.uv).xyz;

				float4 m = tex2D(_MainTex, i.uv);

				//calculate the white-ness of the _MainTex
				float w = (m.r + m.b + m.g) / 3;

				float2 uvWH = float2((_MainTex_TexelSize.z / _ScreenParams.x) * _MainTex_TexelSize.x, (_MainTex_TexelSize.w / _ScreenParams.y) * _MainTex_TexelSize.x);

				float2 uvBlur = float2(i.screenpos.x - (uvWH.x / 2), i.screenpos.y - (uvWH.y / 2));
                half3 noise =  UnpackNormal(tex2D(_DetailTex, i.screenpos / _MainTex_TexelSize));
                half2 normalBlur = noise.xy;
                uvBlur += normalBlur * 0.01 * w;
				
				float4 BlurColor = float4(0, 0, 0, 0);

				float px = 1 / _ScreenParams.x;
				float py = 1 / _ScreenParams.y;

				//distance from current pixel
				float d = 0.0;

				//current weight
				float cw = 0.0;

				float totalWeight = 0.0;

				float2 offset = float2(0, 0);


				/*
				feel free to change the -10 and 10s for the loops below to a lower number to optimize for mobile
				*/
                [loop]
                for (int y = -LOOP_ITERATION; y <= LOOP_ITERATION; y++)
                {
                    [loop]
                    for (int x = -LOOP_ITERATION; x <= LOOP_ITERATION; x++)
                    {
						d = sqrt(pow(x, 2) + pow(y, 2));

						if (d == 0)
						{
							cw = 0;
						}
						else
						{
							cw = 1 / d;
						}

						/*
						these sections below can be enabled/disabled to optimize.
						disabled to optimize for mobile.
						enable for smoother blur (at higher cost)
						*/

						totalWeight += cw;
						offset = float2(x * px, y * py) * (_Distance / 10.00) * w; //base on pixel whiteness
						BlurColor += tex2D(_CameraOpaqueTexture, uvBlur + offset) * cw;

						// totalWeight += cw;
						// offset = float2(x * px, y * py) * (_Distance / 5.00) * w;
						// BlurColor += tex2D(_CameraOpaqueTexture, uvBlur + offset) * cw;

						// totalWeight += cw;
						// offset = float2(x * px, y * py) * (_Distance / 2.50) * w;
						// BlurColor += tex2D(_CameraOpaqueTexture, uvBlur + offset) * cw;

						//totalWeight += cw;
						//offset = float2(x * px, y * py) * (_Distance / 1.25) * w;
						//BlurColor += tex2D(_CameraOpaqueTexture, uvBlur + offset) * cw;

						//totalWeight += cw;
						//offset = float2(x * px, y * py) * (_Distance / 0.75) * w;
						//BlurColor += tex2D(_CameraOpaqueTexture, uvBlur + offset) * cw;

						//totalWeight += cw;
						//offset = float2(x * px, y * py) * (_Distance / 0.375) * w;
						//BlurColor += tex2D(_CameraOpaqueTexture, uvBlur + offset) * cw;

						//totalWeight += cw;
						//offset = float2(x * px, y * py) * (_Distance / 0.1875) * w;
						//BlurColor += tex2D(_CameraOpaqueTexture, uvBlur + offset) * cw;

					}
				}

				BlurColor = BlurColor / totalWeight;

                BlurColor = normalize(BlurColor) *  _Clearness + i.color * (1-_Clearness);

                BlurColor = lerp(i.color, BlurColor, max(0.3,w));

                // BlurColor = lerp(i.color, BlurColor, 1-_Lightness);

				// float4 intensity = dot(BlurColor, float3(0.299, 0.587, 0.114));
				// float4 sat = lerp(intensity, BlurColor, _Saturation);
				// BlurColor = lerp(BlurColor, sat, w);

				BlurColor.a = 1 * m.a * i.color.a;

				return BlurColor;
				
			}
			ENDCG
		}
	}

	Fallback "Sprites/Default"
}