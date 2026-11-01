using UnityEngine;

namespace Moonpop
{
    public sealed class Coin : Interactable
    {
        protected override float HalfWidth => .52f;
        protected override float HalfLength => .52f;
        protected override void OnInteract(PlayerController player)
        {
            if (Game.Session.CollectCoin())
            {
                Game.Effects.Pickup(transform.position+Vector3.up, false);
                Game.Audio.PlayCoin();
            }
        }
        protected override void Animate(float dt)
        {
            Visual.localRotation=Quaternion.Euler(0,Age*120f,0);
            Visual.localPosition=new Vector3(0,1.05f+Mathf.Sin(Age*4)*.12f,0);
        }
        public override void OnSpawn() { base.OnSpawn(); Animate(0); }
        public static Coin Create(Transform parent,GameManager game,ArtPalette a,int index)
        {
            var go=new GameObject("Coin "+index);go.transform.SetParent(parent,false);
            var coin=go.AddComponent<Coin>();
            var visual=new GameObject("Spinning coin").transform;visual.SetParent(go.transform,false);
            GeometryBatch.Single("Lucky gold star",visual,Shapes.Star,a.Gold,Vector3.zero,Vector3.one*1.05f);
            GeometryBatch.Single("Gold coin rim",visual,Shapes.Ring,a.Gold,Vector3.zero,Vector3.one*1.20f);
            var b=new GeometryBatch();
            for(int side=-1;side<=1;side+=2)
            {
                b.Add(Shapes.Sphere,a.Get("Ink"),new Vector3(side*.12f,.03f,-.135f),new Vector3(.065f,.09f,.02f),Quaternion.identity);
                b.Add(Shapes.Sphere,a.Get("White"),new Vector3(side*.12f-.01f,.05f,-.15f),Vector3.one*.022f,Quaternion.identity);
            }
            b.Finish(visual); coin.Configure(game,visual);return coin;
        }
    }
}
