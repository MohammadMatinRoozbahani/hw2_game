using UnityEngine;

namespace Moonpop
{
                                                                                                                   
    public sealed class MovingObstacle : StationaryObstacle
    {
        protected override float ForwardSpeed => 3f;
        protected override void Animate(float dt)
        {
            Visual.localRotation=Quaternion.Euler(0,Mathf.Sin(Age*1.4f)*1.5f,0);
        }
        public override void OnDespawn() { Visual.localRotation=Quaternion.identity; }
    }
}
