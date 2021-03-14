// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Custom/TwinkleRockShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0

        _twinkleColor("Twinkle Color", Color) = (1,1,1,1)
        _twinkleSeedColor("Twinkle Seed", Color) = (4, 3, 2, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        float4 _twinkleColor;
        float4 _twinkleSeedColor;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        float hash11(float p)
        {
            p = frac(p * .1031);
            p *= p + 33.33;
            p *= p + p;
            return frac(p);
        }

        //  1 out, 3 in...
        float hash13(float3 p3)
        {
	        p3  = frac(p3 * .1031);
            p3 += dot(p3, p3.zyx + 31.32);
            return frac((p3.x + p3.y) * p3.z);
        }

        //not super efficeint, which is bad for a shader. Will get worse once it actually gets a twinkle shape
        //we'll figure it out
        bool isInTwinkle(float2 uv, float4 seed)
        {
            float twinkleSlope = 500;
            float twinkleSeed = hash13(seed.xyz);
            float centerX = twinkleSeed;
            float centerY = hash11(twinkleSeed);

            float twinkleSpeed = 100;
            float twinkleMaxSize = 10;
            float twinkleOffset = twinkleSeed / 10;
            float _twinkleWidth = (sin(_Time * twinkleSpeed + twinkleOffset) + 1)/(2 * twinkleMaxSize);
            float _twinkleHeight = (sin(_Time * twinkleSpeed + twinkleOffset) + 1)/(2 * twinkleMaxSize);

            bool goodGraph = uv.y <= abs(1/(twinkleSlope * (uv.x - centerX))) + centerY && uv.y >= -1 * (abs(1/(twinkleSlope * (uv.x - centerX)))) + centerY;
            bool goodX = uv.x > centerX - _twinkleWidth && uv.x < centerX + _twinkleWidth;
            bool goodY = uv.y > centerY - _twinkleHeight && uv.y < centerY + _twinkleHeight;

            return goodGraph && goodX && goodY;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c;
            if(isInTwinkle(IN.uv_MainTex, mul(unity_ObjectToWorld, _twinkleSeedColor)))
            {
                c = _twinkleColor;        
            }
            else
            {
                c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            }
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
