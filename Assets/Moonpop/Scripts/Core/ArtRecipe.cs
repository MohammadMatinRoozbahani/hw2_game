using System;
using System.Collections.Generic;

namespace Moonpop.Core
{
    [Serializable] public struct V3
    {
        public float x,y,z;
        public V3(float x,float y,float z){this.x=x;this.y=y;this.z=z;}
    }
    [Serializable] public sealed class ArtPart
    {
        public string shape,material;
        public V3 position,scale,rotation;
        public bool shadow=true;
    }
    [Serializable] public sealed class ArtMaterial
    {
        public string name;public V3 color;public float glow,smoothness=.42f,metallic=.02f;
        public ArtMaterial(string name,float r,float g,float b,float glow=0){this.name=name;color=new V3(r,g,b);this.glow=glow;}
    }

                                                                                                            
    public static class ArtRecipe
    {
        public static readonly string[] Skins={"Mint Mochi","Berry Buddy","Lavender Puff","Lemon Drop"};
        public static V3 CameraPosition(int mode)=>mode==0?new V3(0,5.7f,-11.6f):mode==1?new V3(0,4.8f,-9.8f):new V3(0,8.4f,-14.5f);
        public static V3 CameraLook()=>new V3(0,1.3f,16);
        public static ArtMaterial[] Materials()=>new[]
        {
            new ArtMaterial("Mint",.20f,.82f,.70f),new ArtMaterial("Berry",1f,.34f,.49f),
            new ArtMaterial("Lavender",.65f,.48f,.93f),new ArtMaterial("Lemon",1f,.78f,.28f),
            new ArtMaterial("Sky",.35f,.69f,.97f),new ArtMaterial("Peach",1f,.59f,.42f),
            new ArtMaterial("Cream",1f,.91f,.75f),new ArtMaterial("White",.95f,.98f,1),
            new ArtMaterial("Ink",.055f,.075f,.15f),new ArtMaterial("Tire",.10f,.12f,.20f),
            new ArtMaterial("Glass",.22f,.37f,.55f),new ArtMaterial("Road",.20f,.23f,.35f){smoothness=.24f},
            new ArtMaterial("Ground",.20f,.20f,.33f){smoothness=.12f},new ArtMaterial("Wood",.46f,.28f,.32f),
            new ArtMaterial("Garden",.25f,.43f,.38f){smoothness=.10f},
            new ArtMaterial("Leaf",.34f,.67f,.62f),new ArtMaterial("Blossom",1f,.54f,.72f),
            new ArtMaterial("Cloud",.43f,.41f,.65f){smoothness=.08f},new ArtMaterial("Hill",.29f,.31f,.49f){smoothness=.06f},
            new ArtMaterial("WarmGlow",1f,.76f,.40f,1.15f),new ArtMaterial("MintGlow",.20f,1f,.78f,1.65f),
            new ArtMaterial("PinkGlow",1f,.27f,.53f,1.65f),new ArtMaterial("CoinGlow",1f,.77f,.25f,.9f),
            new ArtMaterial("RedGlow",1f,.18f,.28f,1.4f),new ArtMaterial("FuelGlow",.39f,1f,.64f,.9f),
            new ArtMaterial("MoonGlow",1f,.89f,.63f,.28f)
        };
        public static string SkinMaterial(int index)=>new[]{"Mint","Berry","Lavender","Lemon"}[Math.Max(0,Math.Min(3,index))];
        private static void P(List<ArtPart> list,string shape,string mat,float x,float y,float z,float sx,float sy,float sz,float rx=0,float ry=0,float rz=0,bool shadow=true)
        {
            list.Add(new ArtPart{shape=shape,material=mat,position=new V3(x,y,z),scale=new V3(sx,sy,sz),rotation=new V3(rx,ry,rz),shadow=shadow});
        }
        private static void B(List<ArtPart> l,string m,float x,float y,float z,float sx,float sy,float sz,float rz=0)=>P(l,"RoundBox",m,x,y,z,sx,sy,sz,0,0,rz);
        private static void S(List<ArtPart> l,string m,float x,float y,float z,float sx,float sy,float sz)=>P(l,"Sphere",m,x,y,z,sx,sy,sz);

        public static List<ArtPart> Car(int variant,bool player)
        {
            var l=new List<ArtPart>();string paint=SkinMaterial(player?0:variant%4);
            B(l,"Tire",0,.42f,0,1.65f,.27f,3.10f);
            B(l,"Cream",0,.57f,0,1.91f,.36f,3.48f);
            B(l,paint,0,.88f,0,1.88f,.67f,3.38f);
            B(l,paint,0,1.11f,1.03f,1.74f,.25f,1.18f);
            B(l,"Cream",0,1.34f,-.35f,1.63f,.82f,1.82f);
            B(l,"Glass",0,1.39f,.59f,1.39f,.52f,.17f);
            B(l,"Glass",0,1.40f,-1.28f,1.38f,.48f,.16f);
            for(int side=-1;side<=1;side+=2)
            {
                B(l,"Glass",side*.84f,1.40f,-.34f,.09f,.47f,1.39f);
                B(l,"Cream",side*.89f,1.38f,-.34f,.07f,.54f,.07f);
                for(int axle=-1;axle<=1;axle+=2)
                {
                    P(l,"Cylinder","Tire",side*.92f,.43f,axle*1.04f,.83f,.30f,.83f,0,0,90);
                    P(l,"Cylinder","Cream",side*1.08f,.43f,axle*1.04f,.50f,.025f,.50f,0,0,90);
                    S(l,paint,side*1.105f,.43f,axle*1.04f,.19f,.19f,.19f);
                }
                S(l,"WarmGlow",side*.64f,.97f,1.69f,.40f,.42f,.16f);
                S(l,"RedGlow",side*.65f,.91f,-1.73f,.35f,.25f,.13f);
                B(l,paint,side*1.01f,1.27f,.27f,.26f,.17f,.26f);
                S(l,"Ink",side*.31f,1.44f,.702f,.17f,.20f,.075f);
                S(l,"White",side*.31f-.025f,1.485f,.745f,.055f,.065f,.02f);
                S(l,"Berry",side*.59f,1.27f,.704f,.23f,.10f,.04f);
                S(l,"Ink",side*.30f,1.44f,-1.38f,.15f,.19f,.055f);
                S(l,"White",side*.30f-.025f,1.48f,-1.414f,.05f,.06f,.018f);
                S(l,"Berry",side*.57f,1.28f,-1.38f,.18f,.08f,.035f);
                B(l,"Cream",side*.60f,.46f,-1.76f,.18f,.15f,.19f);
            }
            B(l,"Cream",0,.63f,1.74f,1.20f,.16f,.15f);
            B(l,"Cream",0,.64f,-1.77f,1.16f,.16f,.15f);
            for(int j=0;j<9;j++)
            {
                float x=(j-4)*.047f;float y=.76f- .085f*(1-x*x/.045f);
                S(l,"Ink",x,y,1.845f,.052f,.045f,.04f);
            }
            if(player)
            {
                B(l,"Cream",0,1.79f,-.35f,1.5f,.18f,1.25f);
                                                                                    
                S(l,"Lemon",0,2.00f,-.36f,.47f,.36f,.52f);
                S(l,"Lemon",0,2.24f,-.20f,.34f,.34f,.33f);
                B(l,"Peach",0,2.20f,-.005f,.22f,.10f,.17f);
                for(int side=-1;side<=1;side+=2)
                {
                    S(l,"Ink",side*.105f,2.29f,-.08f,.047f,.055f,.04f);
                    S(l,"Lemon",side*.24f,2.00f,-.34f,.12f,.19f,.29f);
                }
            }
            else if(variant%3==1)
            {
                for(int side=-1;side<=1;side+=2)
                {
                    S(l,"Cream",side*.43f,1.98f,-.35f,.26f,.74f,.28f);
                    S(l,"Berry",side*.43f,2.01f,-.19f,.12f,.47f,.05f);
                }
            }
            else if(variant%3==2)B(l,"Peach",0,1.86f,-.4f,1.05f,.35f,.85f);
            return l;
        }

        public static List<ArtPart> Road(int index,float length=48)
        {
            var l=new List<ArtPart>();var r=new Random(4103+index*293);
            P(l,"Box","Ground",0,-.55f,0,320,.8f,length);
            P(l,"Box","Road",0,-.14f,0,10.3f,.28f,length);
            for(int side=-1;side<=1;side+=2)
            {
                P(l,"Box","Garden",side*84.25f,-.06f,0,151.5f,.4f,length);
                P(l,"Box","Cream",side*9.0f,.07f,0,1.0f,.18f,length);
                P(l,"Box","Leaf",side*20.6f,.5f,0,1.2f,.72f,length);
                P(l,"Box","Lavender",side*6.9f,-.03f,0,3.3f,.25f,length);
                for(float z=-length/2+1;z<length/2;z+=2)
                {
                    B(l,((int)(z+length/2)/2)%2==0?"Cream":"Berry",side*5.35f,.16f,z,.48f,.40f,1.92f);
                    if(((int)z+100)%6==0)S(l,"WarmGlow",side*5.35f,.40f,z,.13f,.10f,.13f);
                }
                for(float z=-length/2+3;z<length/2;z+=6)
                    B(l,"Cream",side*1.6f,.025f,z,.11f,.035f,2.35f);
                for(int lamp=0;lamp<2;lamp++)
                {
                    float z=-12+lamp*24;
                    P(l,"Cylinder","Mint",side*7.1f,2.7f,z,.14f,5.4f,.14f);
                    B(l,"Cream",side*7.1f,.28f,z,.48f,.56f,.48f);
                    S(l,"WarmGlow",side*7.1f,5.5f,z,.62f,.72f,.62f);
                    P(l,"Cone","Lavender",side*7.1f,6.03f,z,.88f,.37f,.88f);
                }
                for(int h=0;h<3;h++)
                {
                    float z=-16+h*16;float height=4.3f+(float)r.NextDouble()*2;
                    string color=new[]{"Peach","Lavender","Mint","Sky","Berry"}[(h+index+(side+1))%5];
                    float x=side*(12.7f+(h%2)*2f);
                    B(l,"Cream",x,.13f,z,6.6f,.28f,6.9f);
                    P(l,"Box","Cream",side*10.8f,.07f,z,3.6f,.18f,1.2f);
                    S(l,"Leaf",side*18.9f,.69f,z+4.6f,2.2f,1.1f,1.7f);
                    B(l,color,x,height*.5f+.2f,z,5.4f,height,5.8f);
                    B(l,"Cream",x,height+.28f,z,5.75f,.5f,6.1f);
                    B(l,color,x,height+.68f,z,5.0f,.6f,5.4f);
                                                                                            
                    for(int floor=0;floor<2;floor++)for(int window=-1;window<=1;window++)
                    {
                        float yy=1.55f+floor*2.25f;
                        B(l,"Cream",x-side*2.76f,yy,z+window*1.53f,.16f,1.34f,1.07f);
                        B(l,"WarmGlow",x-side*2.86f,yy,z+window*1.53f,.035f,1.10f,.84f);
                    }
                    for(int awning=0;awning<7;awning++)
                        B(l,awning%2==0?"Cream":color,x-side*3.12f,3.0f,z+(awning-3)*.78f,1.0f,.30f,.78f);
                    Tree(l,side*9.2f,z+5.8f,(h+index)%2==0);
                    B(l,"Wood",side*7.6f,.70f,z-5.7f,.9f,.18f,2.3f);
                    B(l,"Wood",side*7.9f,1.05f,z-5.7f,.16f,.65f,2.3f);
                    for(int end=-1;end<=1;end+=2)B(l,"Cream",side*7.6f,.30f,z-5.7f+end*.8f,.6f,.60f,.13f);
                }
            }
                                                                                        
            for(int side=-1;side<=1;side+=2)P(l,"Cylinder","Cream",side*7.8f,3.5f,0,.13f,7,.13f);
            for(int j=0;j<=36;j++)
            {
                float x=-7.8f+j*15.6f/36;float y=5.9f+x*x*.014f;
                S(l,"Wood",x,y,0,.085f,.085f,.085f);
                if(j%4==0)
                {
                    S(l,j%8==0?"PinkGlow":"WarmGlow",x,y-.27f,0,.36f,.43f,.36f);
                    B(l,"Cream",x,y-.56f,0,.16f,.06f,.16f);
                }
            }
            return l;
        }
        private static void Tree(List<ArtPart> l,float x,float z,bool blossom)
        {
            P(l,"Cylinder","Wood",x,1.23f,z,.40f,2.46f,.40f);
            string c=blossom?"Blossom":"Leaf";
            S(l,c,x,3.0f,z,2.7f,2.5f,2.7f);
            S(l,c,x-.65f,2.65f,z+.3f,1.9f,1.9f,2);
            S(l,c,x+.65f,2.8f,z-.25f,1.9f,1.9f,2);
            B(l,"Cream",x,.10f,z,1.3f,.18f,1.3f);
        }
        public static List<ArtPart> Backdrop()
        {
            var l=new List<ArtPart>();
            for(int i=0;i<9;i++)S(l,"Hill",(i-4)*27,5+(i%3)*4,220+(i%2)*12,48,33+(i%3)*14,45);
            S(l,"MoonGlow",-46,41,170,13,13,3.0f);
            for(int side=-1;side<=1;side+=2)
            {
                S(l,"Ink",-46+side*2.0f,42,168.45f,.6f,.8f,.13f);
                S(l,"Berry",-46+side*3.5f,39.8f,168.4f,1.5f,.65f,.12f);
            }
            for(int i=0;i<9;i++)S(l,"Ink",-46+(i-4)*.36f,39.5f-.35f*(1-(i-4)*(i-4)/16f),168.4f,.38f,.24f,.12f);
            for(int i=0;i<6;i++)
            {
                float x=-80+i*31,z=130+(i%2)*36,y=31+(i%3)*9;
                if(i==1)x-=18;
                for(int c=-1;c<=1;c++)S(l,"Cloud",x+c*4,y-Math.Abs(c),z,11,5+(c==0?2:0),6);
            }
            for(int i=0;i<3;i++)
            {
                float x=26+i*15,y=19+i*5,z=108+i*22;
                S(l,i%2==0?"Peach":"Lavender",x,y,z,6,7.5f,6);
                S(l,"Cream",x,y,z-2.65f,2.8f,6.7f,.75f);
                B(l,"Wood",x,y-5.6f,z,2.0f,1.2f,1.5f);
                for(int side=-1;side<=1;side+=2)P(l,"Cylinder","Cream",x+side*.8f,y-4.4f,z,.06f,1.8f,.06f);
            }
            return l;
        }
        public static List<ArtPart> FerrisWheel()
        {
            var l=new List<ArtPart>();
            P(l,"Torus","Cream",0,0,0,20,20,2.4f);
            P(l,"Torus","PinkGlow",0,0,-.21f,19.4f,19.4f,1.6f);
            S(l,"Lemon",0,0,-.6f,2.3f,2.3f,1.2f);
            for(int i=0;i<12;i++)
            {
                float angle=i*30*(float)Math.PI/180,x=(float)Math.Sin(angle)*9,y=(float)Math.Cos(angle)*9;
                B(l,"Cream",0,0,0,.14f,18.6f,.16f,i*30);
                S(l,SkinMaterial(i%4),x,y,-.45f,2.4f,2.3f,1.9f);
                S(l,"WarmGlow",x,y+.8f,-1.35f,.32f,.32f,.12f);
            }
            return l;
        }
    }
}
