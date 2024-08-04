Shader "SkillIndicator/Circle" 
{
    Properties 
    {
        [Header(Base)]
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}
        _Intensity("Intensity", float) = 1

        [Header(Sector)]
        [MaterialToggle] _Sector("Sector", Float) = 1
        _Angle ("Angle", Range(0, 360)) = 60
        _Outline ("Outline", Range(0, 5)) = 0.35
        _OutlineAlpha("Outline Alpha", Range(0,1))=0.5
        [MaterialToggle] _Indicator("Indicator", Float) = 1

        [Header(Flow)]
        _FlowColor("Flow Color", color) = (1,1,1,1)
        _FlowFade("Fade", range(0,1)) = 1
        _Duration("Duration", range(0,1)) = 0

        [Header(Blend)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
    }

    SubShader 
    {
        Tags { "Queue"="Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }

        Pass 
        {
            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag  
            #pragma target 2.5
            #pragma multi_compile __ _INDICATOR_ON
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes 
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings 
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Intensity;
            float _Angle;
            half _Sector;
            half _Outline;
            half _OutlineAlpha;
            half4 _FlowColor;
            half _FlowFade;
            half _Duration;
            CBUFFER_END

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            Varyings vert(Attributes v) 
            {
                Varyings o = (Varyings)0;
                o.uv = v.texcoord;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            float4 frag(Varyings i) : SV_Target 
            {
                half4 col = 0;
                half2 uv = i.uv;

                // 텍스처 샘플링을 통해 픽셀 색상 얻어옴.
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                mainTex *= _Intensity;

                #if _INDICATOR_ON
                    return mainTex.b * 0.6 * _Color;
                #endif

                // 중앙을 기준으로 좌표 재계산 uv (0~1) -> center (-1~1)
                float2 centerUV = (uv * 2 - 1);

                // atan2(y,x) : atan(y/x)에 해당하는 함수로, 
                // atan(x)는 [-π/2, π/2]의 범위의 값을 가지지만, atan2(y,x)는 [-π, π]의 값을 리턴한다.
                // atan2(y,x)는 x≠0 이면 항상 올바른 값을 계산핤 수 있으므로 더 선호한다.
                // 결론 : atan2UV : 0~1 사이의 값 (각도 비율)
                float atan2UV = 1-abs(atan2(centerUV.g, centerUV.r)/3.14);

                // _Sector : 일부분만 그릴지 여부 (0 or 1)
                // ceil(x) : 올림한 정수를 리턴(무조건 올림)
                // lerp(x,y,s) : 선형보간인 x + s(y - x) 를 리턴한다.
                // Angle (0~360) * 0.002778 -> (0~1)
                // 결론 : ceil 결과는 0 or 1. ceil을 통해 비교를 하고, _Sector를 통해 사용 여부 고른다 (전체: 1, 부분: 0 or 1)
                // 즉, 각도 내부라면 sector가 1, 아니면 0.
                half sector = lerp(1.0, 1.0 - ceil(atan2UV - _Angle*0.002777778), _Sector);
                
                // sector랑 비슷하지만 조금 더 큰 각도
                half sectorBig = lerp(1.0, 1.0 - ceil(atan2UV - (_Angle+ _Outline) * 0.002777778), _Sector);

                // 경계선을 판별.
                half outline = (sectorBig - sector) * mainTex.g * _OutlineAlpha;

                // step(x,y) : x≤y 이면 1을 리턴하고, 그렇지 않으면 0을 리턴한다.
                // needOutline : 0 or 1
                half needOutline = 1 - step(359, _Angle);
                outline *= needOutline;

                col = mainTex.r * _Color * sector + outline * _Color;

                // smoothstep(min,max,x) : x가 [min, max] 사이의 값인 경우에 대해서 [0, 1] 사이에서 부드럽게 변하는 Hermite 보간법
                half flowCircleInner = smoothstep(_Duration - _FlowFade, _Duration, length(centerUV));

                // step(x,y) : x≤y 이면 1을 리턴하고, 그렇지 않으면 0을 리턴한다.
                // length(x) : 벡터의 길이를 계산한다.
                half flowCircleMask = step(length(centerUV), _Duration);

                // 결론 : flow는 안쪽 빨간색의 색상을 더해준다.
                half4 flow = flowCircleInner * flowCircleMask * _FlowColor * mainTex.g * sector;

                // 최종 색상 도출
                col += flow;

                return col;
            }
            ENDHLSL
        }
    }

    SubShader 
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        Pass 
        {
            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag  
            #include "UnityCG.cginc"
            #pragma target 2.5
            #pragma multi_compile __ _INDICATOR_ON

            fixed4 _Color;
            sampler2D _MainTex; uniform float4 _MainTex_ST;
            half _Intensity;
            float _Angle;
            fixed _Sector;
            fixed _Outline;
            fixed _OutlineAlpha;

            fixed4 _FlowColor;
            fixed _FlowFade;
            fixed _Duration;

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v) 
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);

                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target 
            {
                fixed4 col = 0;
                fixed2 uv = i.uv;
                fixed4 mainTex = tex2D(_MainTex, uv);
                mainTex *= _Intensity;

                #if _INDICATOR_ON
                    return mainTex.b * 0.6 * _Color;
                #endif

                float2 centerUV = (uv * 2 - 1);
                float atan2UV = 1-abs(atan2(centerUV.g, centerUV.r)/3.14);

                fixed sector = lerp(1.0, 1.0 - ceil(atan2UV - _Angle*0.002777778), _Sector);
                fixed sectorBig = lerp(1.0, 1.0 - ceil(atan2UV - (_Angle+ _Outline) * 0.002777778), _Sector);
                fixed outline = (sectorBig -sector) * mainTex.g * _OutlineAlpha;

                fixed needOutline = 1 - step(359, _Angle);
                outline *= needOutline;
                col = mainTex.r * _Color * sector + outline * _Color;

                fixed flowCircleInner = smoothstep(_Duration - _FlowFade, _Duration, length(centerUV));	//�������Ȧ
                fixed flowCircleMask = step(length(centerUV), _Duration);	//Ӳ������
                fixed4 flow = flowCircleInner * flowCircleMask * _FlowColor *mainTex.g * sector;

                col += flow;
                return col;
            }
            ENDCG
        }
    }
}
