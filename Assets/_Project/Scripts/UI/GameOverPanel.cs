using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Core.Channels;
using YesChef.Services;

namespace YesChef.UI
{
    public class GameOverPanel : MonoBehaviour
    {
        private const float FadeDurationSeconds = 0.3f;

        [SerializeField] private TMP_Text _finalScoreText;
        [SerializeField] private TMP_Text _highScoreText;
        [SerializeField] private GameObject _newHighScoreLabel;
        [SerializeField] private Button _restartButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private IntChannel _scoreChangedChannel;
        [SerializeField] private IntChannel _highScoreChannel;
        [SerializeField] private GameStateChannel _gameStateChannel;

        private CanvasGroup _canvasGroup;
        private int _sessionScore;
        private int _highScore;
        private GameState _currentState;
        private Coroutine _fadeRoutine;

        private void OnEnable()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_scoreChangedChannel != null)
            {
                _scoreChangedChannel.OnRaised += HandleScoreChanged;
            }

            if (_highScoreChannel != null)
            {
                _highScoreChannel.OnRaised += HandleHighScoreChanged;
            }

            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised += HandleGameStateChanged;
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.AddListener(HandleRestartClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(HandleQuitClicked);
            }

            SyncInitialState();
            RefreshTexts();
        }

        private void OnDisable()
        {
            if (_scoreChangedChannel != null)
            {
                _scoreChangedChannel.OnRaised -= HandleScoreChanged;
            }

            if (_highScoreChannel != null)
            {
                _highScoreChannel.OnRaised -= HandleHighScoreChanged;
            }

            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised -= HandleGameStateChanged;
            }

            if (_restartButton != null)
            {
                _restartButton.onClick.RemoveListener(HandleRestartClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(HandleQuitClicked);
            }
        }

        private void SyncInitialState()
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            _currentState = gameManager != null ? gameManager.CurrentState : GameState.Menu;

            HighScoreService highScoreService = FindFirstObjectByType<HighScoreService>();
            if (highScoreService != null)
            {
                _highScore = highScoreService.HighScore;
            }

            SetVisible(_currentState == GameState.GameOver);
        }

        public void SetIsNewHighScore(bool value)
        {
            if (_newHighScoreLabel != null)
            {
                _newHighScoreLabel.SetActive(value);
            }
        }

        private void HandleScoreChanged(int score)
        {
            _sessionScore = score;
            RefreshTexts();
        }

        private void HandleHighScoreChanged(int score)
        {
            _highScore = score;
            RefreshTexts();
        }

        private void HandleGameStateChanged(GameState previousState, GameState nextState)
        {
            _currentState = nextState;
            if (nextState != GameState.GameOver)
            {
                SetIsNewHighScore(false);
                SetVisible(false);
                return;
            }

            RefreshTexts();
            SetVisible(true);
        }

        private void RefreshTexts()
        {
            if (_finalScoreText != null)
            {
                _finalScoreText.text = $"Final Score: {_sessionScore}";
            }

            if (_highScoreText != null)
            {
                _highScoreText.text = $"High Score: {_highScore}";
            }
        }

        private void HandleRestartClicked()
        {
            FindFirstObjectByType<GameManager>()?.StartGame();
        }

        private void HandleQuitClicked()
        {
            FindFirstObjectByType<GameManager>()?.QuitGame();
        }

        private void SetVisible(bool isVisible)
        {
            if (_canvasGroup == null)
            {
                return;
            }

            if (_fadeRoutine != null)
            {
                StopCoroutine(_fadeRoutine);
                _fadeRoutine = null;
            }

            if (!isVisible)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
                return;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _fadeRoutine = StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < FadeDurationSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                _canvasGroup.alpha = Mathf.Clamp01(elapsed / FadeDurationSeconds);
                yield return null;
            }

            _canvasGroup.alpha = 1f;
            _fadeRoutine = null;
        }
    }
}
