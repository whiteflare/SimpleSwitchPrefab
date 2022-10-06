Shader "Custom/SimpleSwitchPrefab/Mirror/Cutout"
{
    Properties
    {
        _MainTex    ("Base (RGB)", 2D) = "white" {}
        _Cutoff     ("Cutoff Threshold", Range(0, 1)) = 0.5
        [HideInInspector] _ReflectionTex0("", 2D) = "white" {}
        [HideInInspector] _ReflectionTex1("", 2D) = "white" {}
    }
    SubShader
    {
        Tags{ "RenderType" = "TransparentCutout" "Queue" = "AlphaTest" }

        Pass
        {
            Cull BACK
            ZWrite ON

            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityInstancing.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            struct appdata 
            {
                float4 vertex   : POSITION;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos      : SV_POSITION;
                float2 uv       : TEXCOORD0;
                float4 refl     : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert(appdata v)
            {
                v2f o;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.refl = ComputeNonStereoScreenPos(o.pos);

                return o;
            }

            sampler2D _ReflectionTex0;
            sampler2D _ReflectionTex1;
            half _Cutoff;

            half4 frag(v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half4 color = tex2D(_MainTex, i.uv);
                color   *= unity_StereoEyeIndex == 0 ? tex2Dproj(_ReflectionTex0, UNITY_PROJ_COORD(i.refl)) : tex2Dproj(_ReflectionTex1, UNITY_PROJ_COORD(i.refl));
                color.a = saturate(color.a);

                clip(color.a - _Cutoff);

                return color;
            }

            ENDCG

        }
    }
}
