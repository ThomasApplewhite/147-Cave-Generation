Shader "Custom/SimpleTwinkleRockShader"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}

        _twinkleColor("Twinkle Color", Color) = (1,1,1,1)

        //_twinkleWidth("Twinkle Width", Range(0.0, 1.0)) = 0.1
        //_twinkleHeight("Twinkle Height", Range(0.0, 1.0)) = 0.1
    }

    SubShader
    {
        Pass
        {
            CGPROGRAM
            #include "UnityCG.cginc"

            #pragma vertex vert
            #pragma fragment frag

            sampler2D _MainTex;
            
            float4 _twinkleColor;

            //float _twinkleWidth;
            //float _twinkleHeight;

            struct VertexShaderInput
            {
                float4 vertex   : POSITION;
                float4 normal   : NORMAL;
                float2 uv       : TEXCOORD0;
            };

            struct VertexShaderOutput
            {
                float4 pos  : SV_POSITION;
                float2 uv   : TEXCOORD0;
                float4 objpos: TEXCOORD1;
                float4 world: TEXCOORD2;
            };

            /*//can you tell me where the wang hash is?
            //Thomas Wang's assembly hash is nice and fast
            uint wang_hash(uint seed)
            {
                seed = (seed ^ 61) ^ (seed >> 16);
                seed *= 9;
                seed = seed ^ (seed >> 4);
                seed *= 0x27d4eb2d;
                seed = seed ^ (seed >> 15);
                return seed;
            }*/

            //not as random as wang_hash, but faster
            /*uint rand_lcg(uint rng_state)
            {
                // LCG values from Numerical Recipes
                rng_state = 1664525 * rng_state + 1013904223;
                return rng_state;
            }*/

            //both of these hashes are from David Hoskins
            //  1 out, 1 in...
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
            bool isInTwinkle(float2 uv, float4 pos)
            {
                float twinkleSlope = 500;
                float twinkleSeed = hash11(71.3);
                float centerX = twinkleSeed;
                float centerY = twinkleSeed;

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

            VertexShaderOutput vert (VertexShaderInput v)
            {
                VertexShaderOutput o;

                o.uv = v.uv;
                o.pos = UnityObjectToClipPos(v.vertex);// + float4(0, wave, 0, 1));
                o.objpos = v.vertex;

                o.world = mul(unity_ObjectToWorld, v.vertex);

                return o;
            };

            float4 frag(VertexShaderOutput i):SV_TARGET
            {
                float4 trueOut;

                if(isInTwinkle(i.uv, i.world))
                {
                    trueOut = _twinkleColor;
                }
                else
                {
                    trueOut = tex2D(_MainTex, i.uv);
                }

                return trueOut;
            };

            ENDCG
        }
    }
}
