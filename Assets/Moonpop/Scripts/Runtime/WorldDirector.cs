using Moonpop.Core;
using UnityEngine;

namespace Moonpop
{
    public sealed class WorldDirector
    {
        private readonly GameManager game;
        private readonly FixedPool<RoadSegment> road;
        private readonly FixedPool<StationaryObstacle> parked;
        private readonly FixedPool<MovingObstacle> moving;
        private readonly FixedPool<Coin> coins;
        private readonly FixedPool<FuelCan> fuel;
        private TrafficPlanner planner;
        private float nextRowZ;
        private uint runNumber;
        public int PoolMisses => road.Misses+parked.Misses+moving.Misses+coins.Misses+fuel.Misses;

        public WorldDirector(GameManager gm,ArtPalette art,Transform parent)
        {
            game=gm;GameSettings s=gm.Settings;
            road=new FixedPool<RoadSegment>(s.roadSegments,i=>RoadSegment.Create(parent,art,s.segmentLength,i));
            parked=new FixedPool<StationaryObstacle>(s.parkedCapacity,i=>StationaryObstacle.Create<StationaryObstacle>(parent,gm,art,i));
            moving=new FixedPool<MovingObstacle>(s.movingCapacity,i=>StationaryObstacle.Create<MovingObstacle>(parent,gm,art,i));
            coins=new FixedPool<Coin>(s.coinCapacity,i=>Coin.Create(parent,gm,art,i));
            fuel=new FixedPool<FuelCan>(s.fuelCapacity,i=>FuelCan.Create(parent,gm,art,i));
            ResetRoad();
        }
        private void ResetRoad()
        {
            road.ReturnAll();
            for(int i=0;i<road.Capacity;i++)road.Rent(new Vector3(0,0,(i-.5f)*game.Settings.segmentLength));
        }
        public void ShowMenu(){ClearTraffic();ResetRoad();}
        public void ResetRun()
        {
            ClearTraffic();ResetRoad();
            planner=new TrafficPlanner(game.Settings.seed+runNumber++*7919u);
            nextRowZ=62;
            FillAhead();
        }
        private void ClearTraffic(){parked.ReturnAll();moving.ReturnAll();coins.ReturnAll();fuel.ReturnAll();}
        public void AdvanceRoad(float travel)
        {
            float length=game.Settings.segmentLength;
            float span=road.Capacity*length;
            float minimum=-length*1.5f,maximum=minimum+span;
            for(int i=0;i<road.Active.Count;i++)
            {
                RoadSegment tile=road.Active[i];tile.transform.position+=Vector3.back*travel;
            }
            for(int i=road.Active.Count-1;i>=0;i--)
            {
                RoadSegment tile=road.Active[i];
                Vector3 position=tile.transform.position;
                if(position.z<minimum||position.z>=maximum)
                {
                    position.z=minimum+Mathf.Repeat(position.z-minimum,span);
                    tile.Recycle();road.Rent(position);
                }
            }
        }
        public void Tick(float speed,float dt)
        {
            AdvanceRoad(speed*dt);
            TickPool(parked,speed,dt);TickPool(moving,speed,dt);TickPool(coins,speed,dt);TickPool(fuel,speed,dt);
            nextRowZ-=speed*dt;
            if(game.Session.State==GameState.Playing)FillAhead();
        }
        private void TickPool<T>(FixedPool<T> pool,float speed,float dt) where T:Interactable
        {
            for(int i=pool.Active.Count-1;i>=0;i--)
            {
                if(game.Session.State!=GameState.Playing)break;
                pool.Active[i].Tick(speed,dt);
            }
        }
        private void FillAhead()
        {
            while(nextRowZ<game.Settings.spawnHorizon)
            {
                TrafficRow row=planner.Next(game.Session.Difficulty);
                for(int lane=0;lane<3;lane++)
                {
                    if(!row.IsBlocked(lane))continue;
                    Vector3 p=new Vector3((lane-1)*game.Session.Rules.LaneWidth,0,nextRowZ);
                    if(row.IsMoving(lane))moving.Rent(p);else parked.Rent(p);
                }
                float x=(row.SafeLane-1)*game.Session.Rules.LaneWidth;
                if(row.HasFuel)fuel.Rent(new Vector3(x,0,nextRowZ));
                for(int i=0;i<row.CoinCount;i++)coins.Rent(new Vector3(x,0,nextRowZ+8+i*3));
                nextRowZ+=TrafficPlanner.RowSpacing(game.Session.Speed,game.Session.Difficulty);
            }
        }
    }
}
