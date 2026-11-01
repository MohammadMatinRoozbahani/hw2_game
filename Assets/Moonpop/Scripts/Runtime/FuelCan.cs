using UnityEngine;

namespace Moonpop
{
    public sealed class FuelCan : Interactable
    {
        protected override float HalfWidth => .65f;
        protected override float HalfLength => .65f;
        protected override void OnInteract(PlayerController player)
        {
            if(Game.Session.CollectFuel())
            {
                Game.Effects.Pickup(transform.position+Vector3.up,true);
                Game.Audio.PlayFuel();
                Game.UI.Notify("FUEL +"+Game.Session.Rules.FuelPickup.ToString("0"),new Color(.3f,1f,.6f));
            }
        }
        protected override void Animate(float dt)
        {
            Visual.localPosition=new Vector3(0,.15f+Mathf.Sin(Age*3)*.10f,0);
            Visual.localRotation=Quaternion.Euler(0,Mathf.Sin(Age*2)*15,0);
        }
        public override void OnSpawn(){base.OnSpawn();Animate(0);}
        public static FuelCan Create(Transform parent,GameManager game,ArtPalette a,int index)
        {
            var go=new GameObject("Fuel can "+index);go.transform.SetParent(parent,false);
            var fuel=go.AddComponent<FuelCan>();
            var visual=new GameObject("Floating jerrycan").transform;visual.SetParent(go.transform,false);
            var b=new GeometryBatch();
            b.Box(a.Green,new Vector3(0,.72f,0),new Vector3(.82f,.92f,.56f));
            b.Box(a.Get("Cream"),new Vector3(.24f,1.27f,0),new Vector3(.25f,.15f,.28f));
            b.Box(a.Green,new Vector3(-.12f,1.33f,0),new Vector3(.33f,.10f,.15f));
            b.Box(a.Green,new Vector3(-.30f,1.2f,0),new Vector3(.09f,.25f,.15f));
            for(int side=-1;side<=1;side+=2)
            {
                b.Box(a.White,new Vector3(0,.72f,side*.25f),new Vector3(.13f,.47f,.025f));
                b.Box(a.White,new Vector3(0,.72f,side*.25f),new Vector3(.45f,.13f,.025f));
            }
            b.Finish(visual);fuel.Configure(game,visual);return fuel;
        }
    }
}
