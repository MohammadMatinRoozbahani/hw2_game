using Moonpop.Core;
using UnityEngine;
namespace Moonpop
{
    public sealed class FestivalBackdrop:MonoBehaviour
    {
        private Transform wheel;
        public void Initialize(ArtPalette art)
        {
            var scenery=new GeometryBatch();scenery.Recipe(ArtRecipe.Backdrop(),art);scenery.Finish(transform,false);
            wheel=new GameObject("Slow ferris wheel").transform;wheel.SetParent(transform,false);
            wheel.localPosition=new Vector3(-31,13.5f,118);
            var rotor=new GeometryBatch();rotor.Recipe(ArtRecipe.FerrisWheel(),art);rotor.Finish(wheel,false);
            var stand=new GeometryBatch();
            for(int side=-1;side<=1;side+=2)stand.Add(Shapes.RoundBox,art.Get("Lavender"),new Vector3(-31+side*2.1f,6.3f,119),new Vector3(.65f,14f,.7f),Quaternion.Euler(0,0,side*17));
            stand.Finish(transform,false);
        }
        private void Update(){if(wheel)wheel.Rotate(0,0,Time.deltaTime*4,Space.Self);}
    }
}
