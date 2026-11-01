using Moonpop.Core;
using UnityEngine;
namespace Moonpop
{
    public static class VehicleArt
    {
        public static Transform Build(Transform parent,ArtPalette art,int variant,bool player,out MeshRenderer underglow)
        {
            var root=new GameObject(player?"Mochi + roof duck":"Festival traffic buddy").transform;root.SetParent(parent,false);
            var batch=new GeometryBatch();batch.Recipe(ArtRecipe.Car(variant,player),art);batch.Finish(root);
            underglow=GeometryBatch.Single("Speed neon",root,Shapes.RoundBox,art.Cyan,new Vector3(0,.14f,0),new Vector3(1.63f,.028f,2.8f));
            return root;
        }
    }
}
