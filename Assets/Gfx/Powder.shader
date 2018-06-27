Shader "Unlit/Powder"
{
    Properties
    {
        _Color("Color", Color) = (1, 1, 1, 1)
    }

        SubShader
    {
        Tags{ "RenderType" = "Opaque" }
        LOD 100
        Cull Off
        Pass
    {
        CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#pragma multi_compile_instancing
#include "UnityCG.cginc"

        struct appdata
    {
        float4 vertex : POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        UNITY_VERTEX_INPUT_INSTANCE_ID // necessary only if you want to access instanced properties in fragment Shader.
    };

    UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_DEFINE_INSTANCED_PROP(float4, _Color)
        UNITY_INSTANCING_BUFFER_END(Props)

        v2f vert(appdata v)
    {
        v2f o;

        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_TRANSFER_INSTANCE_ID(v, o); // necessary only if you want to access instanced properties in the fragment Shader.

        o.vertex = UnityObjectToClipPos(v.vertex);
        return o;
    }

    fixed4 frag(v2f i) : SV_Target
    {
        UNITY_SETUP_INSTANCE_ID(i); // necessary only if any instanced properties are going to be accessed in the fragment Shader.
    return UNITY_ACCESS_INSTANCED_PROP(Props, _Color);
    }
        ENDCG
    }
    }
}