using UnityEngine;
using YesChef.Core;
using YesChef.Core.Channels;
using YesChef.UI;

namespace YesChef.Services
{
    public class HighScoreService : MonoBehaviour
    {
        [SerializeField] private GameStateChannel _gameStateChannel;
        [SerializeField] private IntChannel _scoreChangedChannel;
        [SerializeField] private IntChannel _highScoreChannel;

        private IHighScoreStore _store;
        private GameOverPanel _gameOverPanel;
        private int _sessionScore;
        private int _highScore;

        public int HighScore => _highScore;

        private void Awake()
        {
            _store = new PlayerPrefsHighScoreStore();
            _highScore = _store.Load();
            _gameOverPanel = FindFirstObjectByType<GameOverPanel>();
            _highScoreChannel?.Raise(_highScore);
        }

        private void OnEnable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised += HandleGameStateChanged;
            }

            if (_scoreChangedChannel != null)
            {
                _scoreChangedChannel.OnRaised += HandleScoreChanged;
            }
        }

        private void OnDisable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised -= HandleGameStateChanged;
            }

            if (_scoreChangedChannel != null)
            {
                _scoreChangedChannel.OnRaised -= HandleScoreChanged;
            }
        }

        private void HandleScoreChanged(int score)
        {
            _sessionScore = score;
        }

        private void HandleGameStateChanged(GameState previousState, GameState nextState)
        {
            if (nextState != GameState.GameOver)
            {
                return;
            }

            bool isNewHighScore = _sessionScore > _highScore;
            if (isNewHighScore)
            {
                _highScore = _sessionScore;
                _store.Save(_highScore);
            }

            if (_gameOverPanel == null)
            {
                _gameOverPanel = FindFirstObjectByType<GameOverPanel>();
            }

            _gameOverPanel?.SetIsNewHighScore(isNewHighScore);
            _highScoreChannel?.Raise(_highScore);
        }
    }
}
