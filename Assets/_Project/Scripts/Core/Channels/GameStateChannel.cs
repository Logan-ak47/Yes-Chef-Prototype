using System;
using UnityEngine;

namespace YesChef.Core.Channels
{
    [CreateAssetMenu(fileName = "GameStateChannel", menuName = "YesChef/Channels/Game State Channel")]
    public class GameStateChannel : ScriptableObject
    {
        public event Action<GameState, GameState> OnRaised;

        public void Raise(GameState oldState, GameState newState) => OnRaised?.Invoke(oldState, newState);
    }
}
