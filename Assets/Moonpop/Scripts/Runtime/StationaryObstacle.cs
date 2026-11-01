using UnityEngine;

namespace Moonpop
{
    public class StationaryObstacle : Interactable
    {
        protected override bool IsTraffic=>true;
        protected override void OnInteract(PlayerController player) { Game.HitTraffic(transform.position); }
        public static T Create<T>(Transform parent, GameManager game, ArtPalette art, int index) where T : StationaryObstacle
        {
            var go=new GameObject(typeof(T).Name+" "+index); go.transform.SetParent(parent,false);
            var obstacle=go.AddComponent<T>();
            Transform visual=VehicleArt.Build(go.transform,art,index,false,out _);
            obstacle.Configure(game,visual);
            return obstacle;
        }
    }
}
