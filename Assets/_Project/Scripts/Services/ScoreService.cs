using UnityEngine;
using YesChef.Core;
using YesChef.Core.Channels;

namespace YesChef.Services
{
    public class ScoreService : MonoBehaviour
    {
        [SerializeField] private OrderCompletedChannel _orderCompletedChannel;
        [SerializeField] private GameStateChannel _gameStateChannel;
        [SerializeField] private IntChannel _scoreChangedChannel;

        private int _sessionScore;

        public int SessionScore => _sessionScore;

        private void OnEnable()
        {
            if (_orderCompletedChannel != null)
            {
                _orderCompletedChannel.OnRaised += HandleOrderCompleted;
            }

            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_orderCompletedChannel != null)
            {
                _orderCompletedChannel.OnRaised -= HandleOrderCompleted;
            }

            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised -= HandleGameStateChanged;
            }
        }

        private void HandleOrderCompleted(OrderCompletedData data)
        {
            _sessionScore += data.scoreDelta;
            _scoreChangedChannel?.Raise(_sessionScore);
        }

        private void HandleGameStateChanged(GameState previousState, GameState nextState)
        {
            if (nextState != GameState.Running || previousState == GameState.Paused)
            {
                return;
            }

            _sessionScore = 0;
            _scoreChangedChannel?.Raise(_sessionScore);
        }
    }
}
