using UnityEngine;
using YesChef.Core.Channels;
using YesChef.Data;

namespace YesChef.Core
{
    public class GameManager : MonoBehaviour
    {
        [SerializeField] private GameSettings _settings;
        [SerializeField] private GameStateChannel _stateChannel;
        [SerializeField] private FloatChannel _timeRemainingChannel;

        private GameState _currentState = GameState.Menu;
        private float _timeRemaining;

        public GameState CurrentState => _currentState;
        public float TimeRemaining => _timeRemaining;

        private void TransitionTo(GameState next)
        {
            bool legal = (_currentState, next) switch
            {
                (GameState.Menu,     GameState.Running)  => true,
                (GameState.Running,  GameState.Paused)   => true,
                (GameState.Running,  GameState.GameOver) => true,
                (GameState.Paused,   GameState.Running)  => true,
                (GameState.GameOver, GameState.Menu)     => true,
                _ => false
            };

            if (!legal)
            {
                UnityEngine.Debug.LogWarning(
                    $"[GameManager] Illegal state transition: {_currentState} -> {next}");
                return;
            }

            GameState prev = _currentState;
            _currentState = next;
            _stateChannel?.Raise(prev, next);
        }

        private void Update()
        {
            if (_currentState != GameState.Running) return;

            _timeRemaining -= Time.deltaTime;
            _timeRemainingChannel?.Raise(_timeRemaining);

            if (_timeRemaining <= 0f)
            {
                _timeRemaining = 0f;
                TransitionTo(GameState.GameOver);
            }
        }

        public void StartGame()
        {
            _timeRemaining = _settings != null ? _settings.GameDurationSeconds : 180f;
            TransitionTo(GameState.Running);
        }

        public void PauseGame()
        {
            TransitionTo(GameState.Paused);
        }

        public void ResumeGame()
        {
            TransitionTo(GameState.Running);
        }

        public void EndGame()
        {
            TransitionTo(GameState.GameOver);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
