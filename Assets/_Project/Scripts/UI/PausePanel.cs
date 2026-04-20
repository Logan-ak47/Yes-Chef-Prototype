using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Core.Channels;

namespace YesChef.UI
{
    public class PausePanel : MonoBehaviour
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private GameStateChannel _gameStateChannel;

        private CanvasGroup _canvasGroup;
        private GameState _currentState;

        private void OnEnable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised += HandleGameStateChanged;
            }

            if (_resumeButton != null)
            {
                _resumeButton.onClick.AddListener(HandleResumeClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.AddListener(HandleQuitClicked);
            }

            SyncInitialState();
        }

        private void OnDisable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised -= HandleGameStateChanged;
            }

            if (_resumeButton != null)
            {
                _resumeButton.onClick.RemoveListener(HandleResumeClicked);
            }

            if (_quitButton != null)
            {
                _quitButton.onClick.RemoveListener(HandleQuitClicked);
            }
        }

        private void Update()
        {
            if (_currentState == GameState.Paused && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                HandleResumeClicked();
            }
        }

        private void SyncInitialState()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            _currentState = gameManager != null ? gameManager.CurrentState : GameState.Menu;
            SetVisible(_currentState == GameState.Paused);
        }

        private void HandleGameStateChanged(GameState previousState, GameState nextState)
        {
            _currentState = nextState;
            SetVisible(nextState == GameState.Paused);
        }

        private void HandleResumeClicked()
        {
            FindFirstObjectByType<GameManager>()?.ResumeGame();
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
    }
}
