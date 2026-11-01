using System;
using System.Collections.Generic;
using Moonpop.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace Moonpop
{
    public sealed class ArtPalette
    {
        private readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
        public readonly Material Asphalt,Ground,Concrete,Glass,Rubber,Alloy,Cyan,Pink,Gold,White,Red,Green,Building,Window,Particle;
        public readonly Material[] Paint;
        public Material Get(string key)=>materials[key];
        public ArtPalette()
        {
            Shader shader=Resources.Load<Shader>("Rendering/NeonSurface");
            if(!shader)throw new InvalidOperationException("Moonpop surface shader is missing.");
            foreach(ArtMaterial item in ArtRecipe.Materials())
            {
                var mat=new Material(shader){name=item.name,enableInstancing=true};
                Color color=new Color(item.color.x,item.color.y,item.color.z);
                mat.SetColor("_Color",color);mat.SetColor("_EmissionColor",color*item.glow);
                mat.SetFloat("_Metallic",item.metallic);mat.SetFloat("_Glossiness",item.smoothness);
                mat.SetFloat("_RimStrength",.10f);materials.Add(item.name,mat);
            }
            Asphalt=Get("Road");Ground=Get("Ground");Concrete=Get("Cream");Glass=Get("Glass");
            Rubber=Get("Tire");Alloy=Get("Cream");Cyan=Get("MintGlow");Pink=Get("PinkGlow");
            Gold=Get("CoinGlow");White=Get("WarmGlow");Red=Get("RedGlow");Green=Get("FuelGlow");
            Building=Get("Lavender");Window=Get("WarmGlow");
            Paint=new[]{Get("Mint"),Get("Berry"),Get("Lavender"),Get("Lemon")};
            Particle=new Material(Resources.Load<Shader>("Rendering/NeonParticle")){name="Moonpop soft particles"};
        }
    }
    public static class Shapes
    {
        private static Mesh box,cylinder,cabin,ring,roundBox,sphere,cone,star;
        public static Mesh Box=>box?box:(box=CreateBox());
        public static Mesh Cylinder=>cylinder?cylinder:(cylinder=CreateCylinder(20));
        public static Mesh Cabin=>cabin?cabin:(cabin=CreateCabin());
        public static Mesh Ring=>ring?ring:(ring=CreateTorus());
        public static Mesh RoundBox=>roundBox?roundBox:(roundBox=CreateRoundedBox());
        public static Mesh Sphere=>sphere?sphere:(sphere=CreateSphere());
        public static Mesh Cone=>cone?cone:(cone=CreateCone());
        public static Mesh Star=>star?star:(star=CreateStar());
        public static Mesh Get(string shape)
        {
            switch(shape){case "RoundBox":return RoundBox;case "Sphere":return Sphere;case "Cylinder":return Cylinder;case "Torus":return Ring;case "Cone":return Cone;case "Star":return Star;default:return Box;}
        }
        private static Mesh MeshOf(string name,List<Vector3> vertices,List<int> triangles,List<Vector3> normals=null)
        {
            var m=new Mesh{name=name};m.SetVertices(vertices);m.SetTriangles(triangles,0);
            if(normals==null)m.RecalculateNormals();else m.SetNormals(normals);
            m.RecalculateBounds();return m;
        }
        private static void Triangle(List<int> t,List<Vector3> v,int a,int b,int c,Vector3 outward)
        {
            if(Vector3.Dot(Vector3.Cross(v[b]-v[a],v[c]-v[a]),outward)>=0)t.AddRange(new[]{a,b,c});else t.AddRange(new[]{a,c,b});
        }
        private static Mesh CreateRoundedBox()
        {
            const int n=7;const float radius=.17f,inner=.5f-radius;
            float[] grid={-.5f,-.47f,-.405f,-.33f,.33f,.405f,.47f,.5f};
            var v=new List<Vector3>();var normals=new List<Vector3>();var t=new List<int>();
            Vector3[] normal={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
            for(int face=0;face<6;face++)
            {
                int start=v.Count;Vector3 forward=normal[face];
                Vector3 u=face<2?Vector3.forward:Vector3.right;Vector3 w=face<4?(face<2?Vector3.up:Vector3.forward):Vector3.up;
                for(int j=0;j<=n;j++)for(int i=0;i<=n;i++)
                {
                    Vector3 p=forward*.5f+u*grid[i]+w*grid[j];
                    Vector3 q=new Vector3(Mathf.Clamp(p.x,-inner,inner),Mathf.Clamp(p.y,-inner,inner),Mathf.Clamp(p.z,-inner,inner));
                    Vector3 norm=(p-q).normalized;v.Add(q+norm*radius);normals.Add(norm);
                }
                for(int j=0;j<n;j++)for(int i=0;i<n;i++)
                {
                    int a=start+j*(n+1)+i,b=a+1,c=a+n+1,d=c+1;
                    Triangle(t,v,a,b,c,forward);Triangle(t,v,b,d,c,forward);
                }
            }
            return MeshOf("Moonpop rounded cube",v,t,normals);
        }
        private static Mesh CreateSphere()
        {
            const int lon=20,lat=12;var v=new List<Vector3>();var norms=new List<Vector3>();var t=new List<int>();
            for(int j=0;j<=lat;j++)for(int i=0;i<=lon;i++)
            {
                float a=i*Mathf.PI*2/lon,b=j*Mathf.PI/lat;
                Vector3 normal=new Vector3(Mathf.Sin(b)*Mathf.Cos(a),Mathf.Cos(b),Mathf.Sin(b)*Mathf.Sin(a));
                v.Add(normal*.5f);norms.Add(normal);
            }
            for(int j=0;j<lat;j++)for(int i=0;i<lon;i++)
            {
                int a=j*(lon+1)+i,b=a+1,c=a+lon+1,d=c+1;
                Triangle(t,v,a,b,c,(v[a]+v[b]+v[c]).normalized);Triangle(t,v,b,d,c,(v[b]+v[d]+v[c]).normalized);
            }
            return MeshOf("Moonpop sphere",v,t,norms);
        }
        private static Mesh CreateTorus()
        {
            const int rings=40,sides=8;var v=new List<Vector3>();var norms=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<=rings;i++)for(int j=0;j<=sides;j++)
            {
                float a=i*Mathf.PI*2/rings,b=j*Mathf.PI*2/sides;
                var n=new Vector3(Mathf.Cos(a)*Mathf.Cos(b),Mathf.Sin(a)*Mathf.Cos(b),Mathf.Sin(b));
                v.Add(new Vector3(Mathf.Cos(a)*.46f,Mathf.Sin(a)*.46f,0)+n*.04f);norms.Add(n);
            }
            for(int i=0;i<rings;i++)for(int j=0;j<sides;j++)
            {
                int a=i*(sides+1)+j,b=a+1,c=a+sides+1,d=c+1;
                Triangle(t,v,a,b,c,norms[a]);Triangle(t,v,b,d,c,norms[d]);
            }
            return MeshOf("Moonpop torus",v,t,norms);
        }
        private static Mesh CreateCone()
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int i=0;i<24;i++)
            {
                float a=i*Mathf.PI*2/24,b=(i+1)*Mathf.PI*2/24;int k=v.Count;
                v.Add(new Vector3(Mathf.Cos(a)*.5f,-.5f,Mathf.Sin(a)*.5f));v.Add(new Vector3(Mathf.Cos(b)*.5f,-.5f,Mathf.Sin(b)*.5f));
                v.Add(Vector3.up*.5f);v.Add(Vector3.down*.5f);
                Triangle(t,v,k,k+1,k+2,new Vector3(Mathf.Cos(a),.3f,Mathf.Sin(a)));Triangle(t,v,k+3,k+1,k,Vector3.down);
            }
            return MeshOf("Moonpop cone",v,t);
        }
        private static Mesh CreateStar()
        {
            var v=new List<Vector3>();var t=new List<int>();
            for(int side=0;side<2;side++)for(int i=0;i<10;i++)
            {
                float a=Mathf.PI*.5f+i*Mathf.PI/5,r=i%2==0?.5f:.245f;
                v.Add(new Vector3(Mathf.Cos(a)*r,Mathf.Sin(a)*r,side==0?-.12f:.12f));
            }
            v.Add(new Vector3(0,0,-.12f));v.Add(new Vector3(0,0,.12f));
            for(int i=0;i<10;i++)
            {
                int n=(i+1)%10;
                Triangle(t,v,20,i,n,Vector3.back);Triangle(t,v,21,i+10,n+10,Vector3.forward);
                Vector3 outward=new Vector3(v[i].x,v[i].y,0).normalized;
                Triangle(t,v,i,i+10,n,outward);Triangle(t,v,n,i+10,n+10,outward);
            }
            return MeshOf("Lucky star",v,t);
        }
        private static Mesh CreateBox()
        {
            Vector3[] v = { new Vector3(-.5f,-.5f,-.5f),new Vector3(.5f,-.5f,-.5f),new Vector3(.5f,.5f,-.5f),new Vector3(-.5f,.5f,-.5f),
                new Vector3(-.5f,-.5f,.5f),new Vector3(.5f,-.5f,.5f),new Vector3(.5f,.5f,.5f),new Vector3(-.5f,.5f,.5f) };
            return FlatMesh("Unit box", v, new[] {0,2,1,0,3,2, 4,5,6,4,6,7, 0,4,7,0,7,3, 1,2,6,1,6,5, 3,7,6,3,6,2, 0,1,5,0,5,4});
        }
        private static Mesh CreateCabin()
        {
            Vector3[] v = { new Vector3(-.5f,0,-.5f),new Vector3(.5f,0,-.5f),new Vector3(.38f,1,-.25f),new Vector3(-.38f,1,-.25f),
                new Vector3(-.5f,0,.5f),new Vector3(.5f,0,.5f),new Vector3(.38f,1,.15f),new Vector3(-.38f,1,.15f) };
            return FlatMesh("Sport cabin", v, new[] {0,2,1,0,3,2,4,5,6,4,6,7,0,4,7,0,7,3,1,2,6,1,6,5,3,7,6,3,6,2,0,1,5,0,5,4});
        }
        private static Mesh FlatMesh(string name, Vector3[] points, int[] indices)
        {
            var vertices = new Vector3[indices.Length];
            var triangles = new int[indices.Length];
            for (int i = 0; i < indices.Length; i++) { vertices[i] = points[indices[i]]; triangles[i] = i; }
            var mesh = new Mesh { name = name, vertices = vertices, triangles = triangles };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
        private static Mesh CreateCylinder(int sides)
        {
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            for (int i = 0; i < sides; i++)
            {
                float a = i * Mathf.PI * 2 / sides, b = (i + 1) * Mathf.PI * 2 / sides;
                int k = vertices.Count;
                vertices.Add(new Vector3(Mathf.Cos(a)*.5f,-.5f,Mathf.Sin(a)*.5f));
                vertices.Add(new Vector3(Mathf.Cos(b)*.5f,-.5f,Mathf.Sin(b)*.5f));
                vertices.Add(new Vector3(Mathf.Cos(b)*.5f,.5f,Mathf.Sin(b)*.5f));
                vertices.Add(new Vector3(Mathf.Cos(a)*.5f,.5f,Mathf.Sin(a)*.5f));
                vertices.Add(new Vector3(0,-.5f,0)); vertices.Add(new Vector3(0,.5f,0));
                triangles.AddRange(new[] {k,k+2,k+1,k,k+3,k+2,k+4,k,k+1,k+5,k+2,k+3});
            }
            var mesh = new Mesh { name = "Cylinder 16", vertices = vertices.ToArray(), triangles = triangles.ToArray() };
            mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }
    }
    public sealed class GeometryBatch
    {
        private readonly Dictionary<Material,List<CombineInstance>> groups=new Dictionary<Material,List<CombineInstance>>();
        public void Box(Material m,Vector3 p,Vector3 s)=>Add(Shapes.RoundBox,m,p,s,Quaternion.identity);
        public void Add(Mesh mesh,Material material,Vector3 position,Vector3 scale,Quaternion rotation)
        {
            if(!groups.TryGetValue(material,out var list)){list=new List<CombineInstance>();groups.Add(material,list);}
            list.Add(new CombineInstance{mesh=mesh,transform=Matrix4x4.TRS(position,rotation,scale)});
        }
        public void Recipe(List<ArtPart> parts,ArtPalette palette)
        {
            foreach(ArtPart p in parts)Add(Shapes.Get(p.shape),palette.Get(p.material),Vec(p.position),Vec(p.scale),Quaternion.Euler(Vec(p.rotation)));
        }
        public static Vector3 Vec(V3 p)=>new Vector3(p.x,p.y,p.z);
        public void Finish(Transform parent,bool shadows=true)
        {
            foreach(var group in groups)
            {
                var go=new GameObject(group.Key.name);go.transform.SetParent(parent,false);
                var mesh=new Mesh{name=parent.name+" / "+group.Key.name,indexFormat=IndexFormat.UInt32};
                mesh.CombineMeshes(group.Value.ToArray(),true,true);go.AddComponent<MeshFilter>().sharedMesh=mesh;
                var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=group.Key;r.receiveShadows=true;
                r.shadowCastingMode=shadows?ShadowCastingMode.On:ShadowCastingMode.Off;
            }
        }
        public static MeshRenderer Single(string name,Transform parent,Mesh mesh,Material material,Vector3 position,Vector3 scale)
        {
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=scale;
            go.AddComponent<MeshFilter>().sharedMesh=mesh;var r=go.AddComponent<MeshRenderer>();r.sharedMaterial=material;
            r.shadowCastingMode=ShadowCastingMode.Off;return r;
        }
    }
}
