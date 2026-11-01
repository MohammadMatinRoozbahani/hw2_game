using Moonpop.Core;
using UnityEngine;

namespace Moonpop
{
    [CreateAssetMenu(menuName = "Moonpop/Game settings")]
    public sealed class GameSettings : ScriptableObject
    {
        public RunRules rules = new RunRules();
        [Header("World / performance")]
        [Range(8, 12)] public int roadSegments = 9;
        public float segmentLength = 48f;
        public float spawnHorizon = 235f;
        public int parkedCapacity = 24;
        public int movingCapacity = 20;
        public int coinCapacity = 128;
        public int fuelCapacity = 12;
        public uint seed = 402531387;
        [Header("Presentation")]
        public Color cyan = new Color(.3f, 1f, .79f);
        public Color pink = new Color(1f, .36f, .56f);
        [Range(0, 1)] public float musicVolume = 0.45f;
        [Range(0, 1)] public float effectsVolume = 0.8f;
    }
}
