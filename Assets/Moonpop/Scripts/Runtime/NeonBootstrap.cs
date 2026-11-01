using UnityEngine;

namespace Moonpop
{
                                                                                                                  
    public sealed class NeonBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            if(GameManager.Instance)return;
            GameSettings settings=Resources.Load<GameSettings>("GameSettings");
            if(!settings)throw new System.InvalidOperationException("Missing GameSettings.asset.");
            var root=new GameObject("MOONPOP");
            root.AddComponent<GameManager>().Initialize(settings);
        }
    }
}
