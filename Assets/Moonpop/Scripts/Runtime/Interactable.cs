using Moonpop.Core;
using UnityEngine;

namespace Moonpop
{
                                                                                             
    public abstract class Interactable : PoolObject
    {
        protected GameManager Game;
        protected Transform Visual;
        protected float Age;
        protected virtual float HalfWidth => .83f;
        protected virtual float HalfLength => 1.65f;
        protected virtual bool IsTraffic=>false;
        protected virtual float ForwardSpeed => 0f;
        private bool consumed,nearMissGiven;

        public void Configure(GameManager game, Transform visual) { Game=game; Visual=visual; }
        public override void OnSpawn() { consumed=false;nearMissGiven=false; Age=0; }
        public virtual void Tick(float worldSpeed, float dt)
        {
            if (!Leased) return;
            Vector3 before=transform.position;
            transform.position += Vector3.back * Mathf.Max(0, worldSpeed-ForwardSpeed) * dt;
            Age += dt;
            Animate(dt);
            Vector3 after=transform.position;
            PlayerController player=Game.Player;
            if (!consumed && Game.Session.State==GameState.Playing && SweptCollision.Intersects(
                before.x-player.PreviousPosition.x,before.z-player.PreviousPosition.z,
                after.x-player.transform.position.x,after.z-player.transform.position.z,
                HalfWidth+PlayerController.HalfWidth,HalfLength+PlayerController.HalfLength))
            {
                consumed=true;
                OnInteract(player);
                Recycle();
                return;
            }
            if(IsTraffic&&!nearMissGiven&&before.z>=0&&after.z<0)
            {
                nearMissGiven=true;float dx=Mathf.Abs(after.x-player.transform.position.x);
                if(dx>HalfWidth+PlayerController.HalfWidth&&dx<=Game.Session.Rules.LaneWidth+.15f)Game.AwardNearMiss();
            }
            if (after.z < -16f) Recycle();
        }
        protected virtual void Animate(float dt) { }
        protected abstract void OnInteract(PlayerController player);
    }
}
