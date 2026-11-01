using System;

namespace Moonpop.Core
{
    public enum GameState { Menu, Playing, Paused, GameOver }
    public enum EndReason { None, Collision, FuelEmpty }

                                                                                                
    [Serializable]
    public sealed class RunRules
    {
        public int StartingLives = 3;
        public float MaximumFuel = 100f;
        public float FuelPerSecond = 1.85f;
        public float FuelPickup = 30f;
        public float InitialSpeed = 26f;
        public float MaximumSpeed = 62f;
        public float DamageGraceSeconds = 1.5f;
        public float LaneWidth = 3.2f;
        public float LaneChangeSeconds = 0.23f;
        public int CoinValue = 25;
        public float DistanceScoreFactor = 0.15f;
        public float NitroMultiplier=1.38f;
        public float NitroDrainPerSecond=38f;
        public float NitroRechargePerSecond=12f;
        public float NitroExtraFuelPerSecond=1.3f;

        public void Validate()
        {
            foreach(float value in new[]{MaximumFuel,FuelPerSecond,FuelPickup,InitialSpeed,MaximumSpeed,DamageGraceSeconds,LaneWidth,LaneChangeSeconds,DistanceScoreFactor,NitroMultiplier,NitroDrainPerSecond,NitroRechargePerSecond,NitroExtraFuelPerSecond})
                if(float.IsNaN(value)||float.IsInfinity(value))throw new ArgumentException("Rules must be finite.");
            if (StartingLives < 1 || MaximumFuel <= 0 || FuelPerSecond <= 0 ||
                FuelPickup <= 0 || InitialSpeed <= 0 || MaximumSpeed < InitialSpeed ||
                DamageGraceSeconds < 0 || LaneWidth <= 0 || LaneChangeSeconds <= 0 ||
                CoinValue < 0 || DistanceScoreFactor < 0 || NitroMultiplier<1 || NitroDrainPerSecond<=0 || NitroRechargePerSecond<0 || NitroExtraFuelPerSecond<0)
                throw new ArgumentException("Invalid driving rules.");
        }
    }

    public sealed class RunSession
    {
        public readonly RunRules Rules;
        public GameState State { get; private set; } = GameState.Menu;
        public EndReason Reason { get; private set; }
        public int Lives { get; private set; }
        public int Coins { get; private set; }
        public float Fuel { get; private set; }
        public double Distance { get; private set; }
        public double Elapsed { get; private set; }
        public float GraceRemaining { get; private set; }
        public int BestScore { get; private set; }
        public float Nitro { get; private set; }=100;
        public int BonusScore { get; private set; }
        public int NearMisses { get; private set; }
        public int Combo { get; private set; }
        public int BestCombo { get; private set; }
        public int Badges { get; private set; }
        public float ComboRemaining { get; private set; }
        public double BoostSeconds { get; private set; }
        private bool boostHeld,boostBlocked;
        public bool Boosting=>State==GameState.Playing&&boostHeld&&!boostBlocked&&Nitro>0;
        public float Difficulty => Clamp01((float)(Elapsed / 180.0 * 0.45 + Distance / 8000.0 * 0.55));
        public float NormalSpeed => Rules.InitialSpeed + (Rules.MaximumSpeed - Rules.InitialSpeed) * Difficulty;
        public float Speed => NormalSpeed*(Boosting?Rules.NitroMultiplier:1);
        public int Score => (int)Math.Min(int.MaxValue, Math.Floor(Distance * Rules.DistanceScoreFactor) + (double)Coins * Rules.CoinValue+BonusScore);
        public event Action<GameState> StateChanged;

        public RunSession(RunRules rules, int bestScore = 0)
        {
            rules.Validate();
            Rules = rules;
            BestScore = Math.Max(0, bestScore);
            Lives = rules.StartingLives;
            Fuel = rules.MaximumFuel;
        }

        public void Start()
        {
            CommitBest();
            Lives = Rules.StartingLives;
            Fuel = Rules.MaximumFuel;
            Coins = 0;
            Distance = 0;
            Elapsed = 0;
            GraceRemaining = 0;
            Nitro=100;BonusScore=0;NearMisses=0;Combo=0;BestCombo=0;Badges=0;ComboRemaining=0;BoostSeconds=0;
            boostHeld=false;boostBlocked=false;
            Reason = EndReason.None;
            SetState(GameState.Playing);
        }

        public void Tick(float deltaTime)
        {
            if (State != GameState.Playing || deltaTime <= 0 || float.IsNaN(deltaTime) || float.IsInfinity(deltaTime)) return;
                                                                                      
            bool boosting=Boosting;
            float boostedBudget=boosting?Math.Min(deltaTime,Nitro/Rules.NitroDrainPerSecond):0;
            float boostedRate=Rules.FuelPerSecond+Rules.NitroExtraFuelPerSecond;
            float boostedTime=Math.Min(boostedBudget,Fuel/boostedRate);
            bool emptyDuringBoost=boosting&&boostedBudget>=Fuel/boostedRate;
            float afterBoost=emptyDuringBoost?0:Math.Max(0,Fuel-boostedTime*boostedRate);
            float normalBudget=emptyDuringBoost?0:deltaTime-boostedTime;
            float normalTime=Math.Min(normalBudget,afterBoost/Rules.FuelPerSecond);
            bool emptyDuringNormal=normalBudget>0&&normalBudget>=afterBoost/Rules.FuelPerSecond;
            float activeTime=boostedTime+normalTime;
            float oldSpeed=NormalSpeed;
            Distance+=oldSpeed*(boostedTime*Rules.NitroMultiplier+normalTime);
            Elapsed+=activeTime;
            GraceRemaining = Math.Max(0, GraceRemaining - activeTime);
            Fuel=emptyDuringBoost||emptyDuringNormal?0:Math.Max(0,afterBoost-normalTime*Rules.FuelPerSecond);
            float afterNitro=Math.Max(0,Nitro-boostedTime*Rules.NitroDrainPerSecond);
            if(boosting&&afterNitro<=.0001f)boostBlocked=true;
            Nitro=Math.Min(100,afterNitro+normalTime*Rules.NitroRechargePerSecond);BoostSeconds+=boostedTime;
            ComboRemaining=Math.Max(0,ComboRemaining-activeTime);
            if(ComboRemaining<=0)Combo=0;
            CheckBadges();
            CommitBest();
            if (Fuel <= 0) Finish(EndReason.FuelEmpty);
        }

        public bool HitObstacle()
        {
            if (State != GameState.Playing || GraceRemaining > 0) return false;
            Lives = Math.Max(0, Lives - 1);
            GraceRemaining = Rules.DamageGraceSeconds;
            Combo=0;ComboRemaining=0;
            if (Lives == 0) Finish(EndReason.Collision);
            return true;
        }

        public bool CollectCoin()
        {
            if (State != GameState.Playing) return false;
            if (Coins < int.MaxValue) Coins++;
            Nitro=Math.Min(100,Nitro+1.8f);CheckBadges();
            CommitBest();
            return true;
        }

        public bool CollectFuel()
        {
            if (State != GameState.Playing) return false;
            Fuel = Math.Min(Rules.MaximumFuel, Fuel + Rules.FuelPickup);
            return true;
        }

        public void SetBoostHeld(bool held)
        {
            if(!held){boostHeld=false;boostBlocked=false;return;}
            if(State!=GameState.Playing)return;
            boostHeld=true;
            if(Nitro<8)boostBlocked=true;
        }
        public int RegisterNearMiss()
        {
            if(State!=GameState.Playing||GraceRemaining>0)return 0;
            NearMisses++;Combo=ComboRemaining>0?Math.Min(5,Combo+1):1;
            ComboRemaining=3.2f;BestCombo=Math.Max(BestCombo,Combo);
            int award=100*Combo;AddBonus(award);CheckBadges();CommitBest();return award;
        }
        private void AddBonus(int value){BonusScore=(int)Math.Min(int.MaxValue,(long)BonusScore+value);}
        private void CheckBadges()
        {
            if(Coins>=20&&(Badges&1)==0){Badges|=1;AddBonus(500);}
            if(NearMisses>=8&&(Badges&2)==0){Badges|=2;AddBonus(500);}
            if(Distance>=1500&&(Badges&4)==0){Badges|=4;AddBonus(500);}
        }

        public void Pause() { if (State == GameState.Playing) SetState(GameState.Paused); }
        public void Resume() { if (State == GameState.Paused) SetState(GameState.Playing); }
        public void ReturnToMenu() { CommitBest(); SetState(GameState.Menu); }
        public void CommitBest() { BestScore = Math.Max(BestScore, Score); }

        private void Finish(EndReason reason)
        {
            Reason = reason;
            CommitBest();
            SetState(GameState.GameOver);
        }

        private void SetState(GameState state)
        {
            if (state == State) return;
            State = state;
            StateChanged?.Invoke(state);
        }

        private static float Clamp01(float n) => Math.Max(0, Math.Min(1, n));
    }
}
