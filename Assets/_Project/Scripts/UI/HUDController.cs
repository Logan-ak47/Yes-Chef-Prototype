using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Core.Channels;
using YesChef.Services;

namespace YesChef.UI
{
    public class HUDController : MonoBehaviour
    {
        private const float WarningThresholdSeconds = 10f;
        private const float PunchDuration = 0.18f;
        private const float PulseDuration = 0.45f;

        [SerializeField] private TMP_Text _scoreText;
        [SerializeField] private TMP_Text _highScoreText;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private IntChannel _scoreChangedChannel;
        [SerializeField] private IntChannel _highScoreChannel;
        [SerializeField] private FloatChannel _timeRemainingChannel;
        [SerializeField] private GameStateChannel _gameStateChannel;

        private CanvasGroup _canvasGroup;
        private Color _defaultTimerColor = Color.white;
        private Coroutine _scorePunchRoutine;
        private Coroutine _timerPulseRoutine;
        private Vector3 _scoreBaseScale = Vector3.one;
        private Vector3 _timerBaseScale = Vector3.one;
        private GameState _currentState = GameState.Menu;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_scoreText != null)
            {
                _scoreBaseScale = _scoreText.rectTransform.localScale;
            }

            if (_timerText != null)
            {
                _timerBaseScale = _timerText.rectTransform.localScale;
                _defaultTimerColor = _timerText.color;
            }
        }

        private void OnEnable()
        {
            if (_scoreChangedChannel != null)
            {
                _scoreChangedChannel.OnRaised += HandleScoreChanged;
            }

            if (_highScoreChannel != null)
            {
                _highScoreChannel.OnRaised += HandleHighScoreChanged;
            }

            if (_timeRemainingChannel != null)
            {
                _timeRemainingChannel.OnRaised += HandleTimeRemainingChanged;
            }

            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised += HandleGameStateChanged;
            }

            if (_pauseButton != null)
            {
                _pauseButton.onClick.AddListener(HandlePauseClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(HandleQuitClicked);
            }

            RefreshInitialState();
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

            if (_timeRemainingChannel != null)
            {
                _timeRemainingChannel.OnRaised -= HandleTimeRemainingChanged;
            }

            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised -= HandleGameStateChanged;
            }

            if (_pauseButton != null)
            {
                _pauseButton.onClick.RemoveListener(HandlePauseClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(HandleQuitClicked);
            }
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                return;
            }

            if (_currentState == GameState.Running)
            {
                HandlePauseClicked();
            }
        }

        private void RefreshInitialState()
        {
            HandleScoreChanged(0);
            HandleTimeRemainingChanged(0f);

            HighScoreService highScoreService = FindFirstObjectByType<HighScoreService>();
            if (highScoreService != null)
            {
                HandleHighScoreChanged(highScoreService.HighScore);
            }

            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                HandleTimeRemainingChanged(gameManager.TimeRemaining);
                HandleGameStateChanged(gameManager.CurrentState, gameManager.CurrentState);
                return;
            }

            SetVisible(false);
        }

        private void HandleScoreChanged(int value)
        {
            if (_scoreText != null)
            {
                _scoreText.text = $"Score: {value}";
                RestartRoutine(ref _scorePunchRoutine, PunchScale(_scoreText.rectTransform, _scoreBaseScale, 1.12f, PunchDuration));
            }
        }

        private void HandleHighScoreChanged(int value)
        {
            if (_highScoreText != null)
            {
                _highScoreText.text = $"High: {value}";
            }
        }

        private void HandleTimeRemainingChanged(float secondsRemaining)
        {
            if (_timerText == null)
            {
                return;
            }

            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(secondsRemaining));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            _timerText.text = $"{minutes:00}:{seconds:00}";

            bool isWarning = secondsRemaining <= WarningThresholdSeconds && _currentState == GameState.Running;
            _timerText.color = isWarning ? new Color(0.9f, 0.2f, 0.2f, 1f) : _defaultTimerColor;

            if (isWarning)
            {
                if (_timerPulseRoutine == null)
                {
                    _timerPulseRoutine = StartCoroutine(PulseTimer());
                }
            }
            else
            {
                StopRoutine(ref _timerPulseRoutine);
                _timerText.rectTransform.localScale = _timerBaseScale;
            }
        }

        private void HandleGameStateChanged(GameState previousState, GameState nextState)
        {
            _currentState = nextState;
            SetVisible(nextState == GameState.Running);

            if (nextState != GameState.Running && _timerText != null)
            {
                StopRoutine(ref _timerPulseRoutine);
                _timerText.rectTransform.localScale = _timerBaseScale;
            }
        }

        private void HandlePauseClicked()
        {
            FindFirstObjectByType<GameManager>()?.PauseGame();
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

            _canvasGroup.alpha = isVisible ? 1f : 0f;
            _canvasGroup.interactable = isVisible;
            _canvasGroup.blocksRaycasts = isVisible;
        }

        private IEnumerator PulseTimer()
        {
            RectTransform target = _timerText.rectTransform;

            while (true)
            {
                float elapsed = 0f;
                while (elapsed < PulseDuration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.PingPong(elapsed / PulseDuration, 1f);
                    float scale = Mathf.Lerp(1f, 1.08f, t);
                    target.localScale = _timerBaseScale * scale;
                    yield return null;
                }
            }
        }

        private static IEnumerator PunchScale(RectTransform target, Vector3 baseScale, float peakScale, float duration)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / duration);
                float curve = normalized < 0.5f
                    ? Mathf.Lerp(1f, peakScale, normalized / 0.5f)
                    : Mathf.Lerp(peakScale, 1f, (normalized - 0.5f) / 0.5f);
                target.localScale = baseScale * curve;
                yield return null;
            }

            target.localScale = baseScale;
        }

        private void RestartRoutine(ref Coroutine routine, IEnumerator enumerator)
        {
            StopRoutine(ref routine);
            routine = StartCoroutine(enumerator);
        }

        private void StopRoutine(ref Coroutine routine)
        {
            if (routine == null)
            {
                return;
            }

            StopCoroutine(routine);
            routine = null;
        }
    }
}
