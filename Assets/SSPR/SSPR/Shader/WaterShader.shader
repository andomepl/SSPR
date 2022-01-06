Shader "Custom/WaterShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _CubeMap("Sky",CUBE)=""{}
        _Edge("Edge",Range(0,1))=0.5
        _Intensity("Intensity",Range(-1,1))=0.2
    }
    SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200
        Pass{
            

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 5.0
            #include"UnityCG.cginc"


            sampler2D _MainTex;
            samplerCUBE _CubeMap;
            sampler2D _ReflectTex;
            float _Intensity;
            float _Edge;
            fixed4 _Color;



            struct input {
                float4 pos:POSITION;
                float2 uv:TEXCOORD0;
                float3 normal:NORMAL;

            };
            struct inpolation {
                float4 pos:SV_POSITION;
                float2 uv:TEXCOORD0;
                float4 screenuv:TEXCOORD1;
                float3 worldPos:TEXCOORD2;
                float3 worldNormal:TEXCOORD3;
            };

            inpolation vert(input i) {

                inpolation o;

                o.pos = UnityObjectToClipPos(i.pos);

                o.worldPos = mul(unity_ObjectToWorld, i.pos);
                o.worldNormal = mul(unity_ObjectToWorld, i.normal);

                o.screenuv = ComputeScreenPos(o.pos);
                o.uv = i.uv;

                return o;
            }

            float SDF(float2 uv) {

                float2 distance = abs(uv) - float2(0.0,1.0);


                distance =-pow(distance, _Intensity);

                        
                //return  length(distance);

                return length(max(distance, 0.0)) - min(max(distance.x, distance.y), 0.0);

            }




            float4 frag(inpolation i) :SV_Target{

                i.worldNormal = normalize(i.worldNormal);

                float3 viewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

                float3 refDir = reflect(-viewDir, i.worldNormal);
                float3 skycol = texCUBE(_CubeMap, refDir).rgb;

                float2 screenUV = i.screenuv.xy / i.screenuv.w;

               // float3 Color = tex2D(_MainTex,i.uv).rgb* _Color;

                float4 refColor = tex2D(_ReflectTex, screenUV);


                float mask =1-SDF(screenUV*2-1);

                mask = mask / (1 + mask);
                mask = smoothstep(0, _Edge, abs(mask));
                mask *= refColor.a;

                float3 sky_add_ref = skycol *(1-mask) + refColor.rgb * mask;

                return float4(sky_add_ref.rgb, 1.0f);
            }    
            ENDCG
            }
        }
    FallBack "Diffuse"
}
