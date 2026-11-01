using Moonpop.Core;
using UnityEngine;
namespace Moonpop
{
    public sealed class RoadSegment:PoolObject
    {
        public static RoadSegment Create(Transform parent,ArtPalette art,float length,int index)
        {
            var go=new GameObject("Pooled festival block "+index);go.transform.SetParent(parent,false);
            var segment=go.AddComponent<RoadSegment>();var batch=new GeometryBatch();
            batch.Recipe(ArtRecipe.Road(index,length),art);batch.Finish(go.transform);
            for(int side=-1;side<=1;side+=2)
            {
                var lamp=new GameObject("Warm street light");lamp.transform.SetParent(go.transform,false);
                lamp.transform.localPosition=new Vector3(side*7.1f,5.2f,0);
                Light light=lamp.AddComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.75f,.48f);
                light.intensity=2.8f;light.range=16;light.shadows=LightShadows.None;
            }
            return segment;
        }
    }
}
