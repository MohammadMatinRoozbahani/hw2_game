Shader "Moonpop/Toy Enamel"
{
 Properties {
  _Color ("Toy enamel", Color) = (1,1,1,1)
  [HDR] _EmissionColor ("Festival glow", Color) = (0,0,0,1)
  _Metallic ("Metallic", Range(0,1)) = 0.02
  _Glossiness ("Smoothness", Range(0,1)) = 0.42
  _RimStrength ("Soft rim", Range(0,0.4)) = 0.1
 }
 SubShader {
  Tags {"RenderType"="Opaque"}
  LOD 200
  CGPROGRAM
  #pragma surface surf Standard fullforwardshadows addshadow
  #pragma target 3.0
  #pragma multi_compile_instancing
  fixed4 _Color;half3 _EmissionColor;half _Metallic,_Glossiness,_RimStrength;
  struct Input {float3 viewDir;float3 worldPos;};
  void surf(Input IN,inout SurfaceOutputStandard o) {
   o.Albedo=_Color.rgb;o.Metallic=_Metallic;o.Smoothness=_Glossiness;
   half rim=pow(1-saturate(dot(normalize(IN.viewDir),o.Normal)),3);
   o.Emission=_EmissionColor+_Color.rgb*(0.025+rim*_RimStrength);o.Alpha=1;
  }
  ENDCG
 }
 FallBack "Diffuse"
}
