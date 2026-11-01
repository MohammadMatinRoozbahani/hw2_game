Shader "Moonpop/Festival Night"
{
 SubShader {
  Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
  Cull Off ZWrite Off
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   struct v2f {float4 pos:SV_POSITION;float3 direction:TEXCOORD0;};
   v2f vert(float4 p:POSITION){v2f o;o.pos=UnityObjectToClipPos(p);o.direction=p.xyz;return o;}
   float hash(float2 p){return frac(sin(dot(p,float2(127.1,311.7)))*43758.5453);}
   half4 frag(v2f i):SV_Target {
    float3 d=normalize(i.direction);float h=exp(-max(0,d.y)*4.5);
    float3 col=lerp(float3(.027,.043,.115),float3(.12,.095,.225),h);
    float2 sky=float2(atan2(d.z,d.x),asin(clamp(d.y,-1,1)))*110;
    float seed=hash(floor(sky));float2 q=abs(frac(sky)-.5);
    float roundStar=1-smoothstep(.015,.095,length(q));
    float crossStar=(1-smoothstep(.005,.025,min(q.x,q.y)))*(1-smoothstep(.025,.14,max(q.x,q.y)));
    float star=max(roundStar,crossStar*.65)*step(.979,seed)*step(.06,d.y);
    col+=star*(.75+.25*sin(_Time.y*(.5+seed)+seed*80))*float3(1,.87,.63)*1.3;
    return half4(col,1);
   }
   ENDCG
  }
 }
 FallBack Off
}
