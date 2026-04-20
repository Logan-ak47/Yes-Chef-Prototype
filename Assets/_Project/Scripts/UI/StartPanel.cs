using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Core.Channels;

namespace YesChef.UI
{
    public class StartPanel : MonoBehaviour
    {
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _controlsText;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _tutorialButton;
        [SerializeField] private Button _backButton;
        [SerializeField] private GameStateChannel _gameStateChannel;

        private CanvasGroup _canvasGroup;
        private const string MenuCopy =
            "A fast kitchen score-attack.\n" +
            "Complete orders before the 3-minute shift ends.";
        private const string TutorialCopy =
            "Goal: finish as many customer orders as possible before time runs out.\n" +
            "Each window is an active order. Deliver the ingredients shown on that ticket in any order.\n\n" +
            "Vegetable -> chop at the table and stay nearby.\n" +
            "Meat -> cook on the stove and come back when ready.\n" +
            "Cheese -> ready immediately.\n\n" +
            "WASD to move\n" +
            "Auto-interact at stations\n" +
            "Esc to pause";

        private bool _showingTutorial;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            EnsureRuntimeButtons();
            ApplyScreenState();
        }

        private void OnEnable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised += HandleGameStateChanged;
            }

            if (_startButton != null)
            {
                _startButton.onClick.AddListener(HandleStartClicked);
            }

            if (_tutorialButton != null)
            {
                _tutorialButton.onClick.AddListener(HandleTutorialClicked);
            }

            if (_backButton != null)
            {
                _backButton.onClick.AddListener(HandleBackClicked);
            }

            SyncInitialState();
        }

        private void OnDisable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised -= HandleGameStateChanged;
            }

            if (_startButton != null)
            {
                _startButton.onClick.RemoveListener(HandleStartClicked);
            }

            if (_tutorialButton != null)
            {
                _tutorialButton.onClick.RemoveListener(HandleTutorialClicked);
            }

            if (_backButton != null)
            {
                _backButton.onClick.RemoveListener(HandleBackClicked);
            }
        }

        private void SyncInitialState()
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            _showingTutorial = false;
            ApplyScreenState();
            SetVisible(gameManager == null || gameManager.CurrentState == GameState.Menu);
        }

        private void HandleGameStateChanged(GameState previousState, GameState nextState)
        {
            if (nextState == GameState.Menu)
            {
                _showingTutorial = false;
                ApplyScreenState();
            }

            SetVisible(nextState == GameState.Menu);
        }

        private void HandleStartClicked()
        {
            FindFirstObjectByType<GameManager>()?.StartGame();
        }

        private void HandleTutorialClicked()
        {
            _showingTutorial = true;
            ApplyScreenState();
        }

        private void HandleBackClicked()
        {
            _showingTutorial = false;
            ApplyScreenState();
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

        private void ApplyScreenState()
        {
            if (_titleText != null)
            {
                _titleText.text = _showingTutorial ? "How To Play" : "Yes Chef!";
                _titleText.fontSize = _showingTutorial ? 44f : 54f;
                RectTransform titleRect = _titleText.rectTransform;
                titleRect.anchoredPosition = _showingTutorial ? new Vector2(0f, 210f) : new Vector2(0f, 180f);
            }

            if (_controlsText != null)
            {
                _controlsText.text = _showingTutorial ? TutorialCopy : MenuCopy;
                _controlsText.fontSize = _showingTutorial ? 24f : 28f;
                RectTransform controlsRect = _controlsText.rectTransform;
                controlsRect.anchoredPosition = _showingTutorial ? new Vector2(0f, 20f) : new Vector2(0f, 18f);
                controlsRect.sizeDelta = _showingTutorial ? new Vector2(860f, 320f) : new Vector2(760f, 180f);
            }

            if (_startButton != null)
            {
                _startButton.gameObject.SetActive(true);
                SetButtonLabel(_startButton, _showingTutorial ? "Play Now" : "Start Cooking");
                SetButtonPosition(_startButton, _showingTutorial ? new Vector2(135f, -210f) : new Vector2(0f, -130f));
            }

            if (_tutorialButton != null)
            {
                _tutorialButton.gameObject.SetActive(!_showingTutorial);
                SetButtonLabel(_tutorialButton, "How To Play");
                SetButtonPosition(_tutorialButton, new Vector2(0f, -210f));
            }

            if (_backButton != null)
            {
                _backButton.gameObject.SetActive(_showingTutorial);
                SetButtonLabel(_backButton, "Back");
                SetButtonPosition(_backButton, new Vector2(-135f, -210f));
            }
        }

        private void EnsureRuntimeButtons()
        {
            if (_startButton == null)
            {
                return;
            }

            if (_tutorialButton == null)
            {
                _tutorialButton = CreateButtonFromTemplate(_startButton, "TutorialButton", "How To Play");
            }

            if (_backButton == null)
            {
                _backButton = CreateButtonFromTemplate(_startButton, "BackButton", "Back");
            }
        }

        private Button CreateButtonFromTemplate(Button template, string objectName, string label)
        {
            Button clone = Instantiate(template, template.transform.parent);
            clone.name = objectName;
            SetButtonLabel(clone, label);
            return clone;
        }

        private static void SetButtonLabel(Button button, string label)
        {
            TMP_Text text = button != null ? button.GetComponentInChildren<TMP_Text>(true) : null;
            if (text != null)
            {
                text.text = label;
            }
        }

        private static void SetButtonPosition(Button button, Vector2 anchoredPosition)
        {
            if (button == null)
            {
                return;
            }

            RectTransform rectTransform = button.transform as RectTransform;
            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = anchoredPosition;
            }
        }
    }
}
