using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Core;
using YesChef.Core.Channels;
using YesChef.Gameplay.Orders;

namespace YesChef.UI
{
    public class OrderTicket : MonoBehaviour
    {
        private const string EmptyElapsed = "--";
        private const float FulfilledHideDuration = 0.16f;
        private const float FulfilledPeakScale = 1.12f;

        [SerializeField] private CustomerWindow _customerWindow;
        [SerializeField] private Image[] _ingredientIconSlots = new Image[3];
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TMP_Text _elapsedText;
        [SerializeField] private GameStateChannel _gameStateChannel;
        [SerializeField] private float _fullAgeSeconds = 30f;
        [SerializeField] private Color _fulfilledColor = new Color(0.35f, 1f, 0.45f, 1f);
        [SerializeField] private Color _defaultIconColor = Color.white;
        [SerializeField] private Color _emptyTint = new Color(1f, 1f, 1f, 0.35f);

        private CanvasGroup _canvasGroup;
        private Order _currentOrder;
        private Camera _mainCamera;
        private bool _isStateVisible;
        private float _contentAlpha;
        private Vector3[] _iconBaseScales;
        private Coroutine[] _slotHideRoutines;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _mainCamera = Camera.main;
            _iconBaseScales = new Vector3[_ingredientIconSlots.Length];
            _slotHideRoutines = new Coroutine[_ingredientIconSlots.Length];
            for (int i = 0; i < _ingredientIconSlots.Length; i++)
            {
                if (_ingredientIconSlots[i] != null)
                {
                    _iconBaseScales[i] = _ingredientIconSlots[i].rectTransform.localScale;
                }
            }
        }

        private void OnEnable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised += HandleGameStateChanged;
            }

            if (_customerWindow != null)
            {
                _customerWindow.OnOrderAssigned += HandleOrderAssigned;
                _customerWindow.OnOrderCleared += HandleOrderCleared;
                BindOrder(_customerWindow.CurrentOrder);
            }
            else
            {
                ApplyEmptyState();
            }

            SyncInitialState();
        }

        private void OnDisable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised -= HandleGameStateChanged;
            }

            if (_customerWindow != null)
            {
                _customerWindow.OnOrderAssigned -= HandleOrderAssigned;
                _customerWindow.OnOrderCleared -= HandleOrderCleared;
            }

            UnsubscribeFromOrder(_currentOrder);
            _currentOrder = null;
        }

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_mainCamera != null)
            {
                Vector3 cameraForward = _mainCamera.transform.forward;
                transform.forward = cameraForward;
            }

            if (_customerWindow == null || _customerWindow.CurrentOrder == null)
            {
                return;
            }

            float elapsed = _customerWindow.CurrentElapsedTime;
            if (_elapsedText != null)
            {
                _elapsedText.text = Mathf.FloorToInt(elapsed).ToString();
            }

            if (_progressBar != null)
            {
                _progressBar.value = Mathf.Clamp01(elapsed / Mathf.Max(1f, _fullAgeSeconds));
            }
        }

        private void HandleOrderAssigned(Order order)
        {
            BindOrder(order);
        }

        private void HandleOrderCleared()
        {
            BindOrder(null);
        }

        private void SyncInitialState()
        {
            GameManager gameManager = FindFirstObjectByType<GameManager>();
            GameState initialState = gameManager != null ? gameManager.CurrentState : GameState.Menu;
            HandleGameStateChanged(initialState, initialState);
        }

        private void HandleGameStateChanged(GameState previousState, GameState nextState)
        {
            _isStateVisible = nextState == GameState.Running || nextState == GameState.Paused;
            ApplyVisibility();
        }

        private void BindOrder(Order order)
        {
            if (ReferenceEquals(_currentOrder, order))
            {
                RefreshIcons();
                return;
            }

            UnsubscribeFromOrder(_currentOrder);
            _currentOrder = order;

            if (_currentOrder != null)
            {
                _currentOrder.OnIngredientFulfilled += HandleIngredientFulfilled;
                SetContentAlpha(1f);
                RefreshIcons();
                return;
            }

            ApplyEmptyState();
        }

        private void UnsubscribeFromOrder(Order order)
        {
            if (order == null)
            {
                return;
            }

            order.OnIngredientFulfilled -= HandleIngredientFulfilled;
        }

        private void HandleIngredientFulfilled(int fulfilledIndex)
        {
            if (_currentOrder == null)
            {
                return;
            }

            int visibleSlotIndex = 0;
            for (int ingredientIndex = 0; ingredientIndex < _currentOrder.RequiredIngredients.Count; ingredientIndex++)
            {
                if (_currentOrder.FulfilledIndices.Contains(ingredientIndex))
                {
                    continue;
                }

                if (ingredientIndex == fulfilledIndex)
                {
                    break;
                }

                visibleSlotIndex++;
            }

            if (visibleSlotIndex >= 0 && visibleSlotIndex < _ingredientIconSlots.Length && _ingredientIconSlots[visibleSlotIndex] != null)
            {
                RestartHideAnimation(visibleSlotIndex);
                return;
            }

            RefreshIcons();
        }

        private void RefreshIcons()
        {
            if (_currentOrder == null)
            {
                ApplyEmptyState();
                return;
            }

            int visibleIngredientCount = 0;
            for (int ingredientIndex = 0; ingredientIndex < _currentOrder.RequiredIngredients.Count; ingredientIndex++)
            {
                if (_currentOrder.FulfilledIndices.Contains(ingredientIndex))
                {
                    continue;
                }

                if (visibleIngredientCount >= _ingredientIconSlots.Length)
                {
                    break;
                }

                Image slot = _ingredientIconSlots[visibleIngredientCount];
                if (slot == null)
                {
                    visibleIngredientCount++;
                    continue;
                }

                slot.gameObject.SetActive(true);
                slot.sprite = _currentOrder.RequiredIngredients[ingredientIndex] != null ? _currentOrder.RequiredIngredients[ingredientIndex].Icon : null;
                slot.color = _defaultIconColor;
                slot.rectTransform.localScale = _iconBaseScales[visibleIngredientCount];
                visibleIngredientCount++;
            }

            for (int i = visibleIngredientCount; i < _ingredientIconSlots.Length; i++)
            {
                Image slot = _ingredientIconSlots[i];
                if (slot == null)
                {
                    continue;
                }

                slot.gameObject.SetActive(false);
                slot.color = _emptyTint;
                slot.rectTransform.localScale = _iconBaseScales[i];
            }

            if (_elapsedText != null)
            {
                _elapsedText.text = Mathf.FloorToInt(_customerWindow.CurrentElapsedTime).ToString();
            }

            if (_progressBar != null)
            {
                _progressBar.value = Mathf.Clamp01(_customerWindow.CurrentElapsedTime / Mathf.Max(1f, _fullAgeSeconds));
            }

            SetContentAlpha(1f);
        }

        private void ApplyEmptyState()
        {
            for (int i = 0; i < _ingredientIconSlots.Length; i++)
            {
                if (_slotHideRoutines != null && i < _slotHideRoutines.Length && _slotHideRoutines[i] != null)
                {
                    StopCoroutine(_slotHideRoutines[i]);
                    _slotHideRoutines[i] = null;
                }

                Image slot = _ingredientIconSlots[i];
                if (slot != null)
                {
                    slot.gameObject.SetActive(false);
                    slot.color = _emptyTint;
                    slot.rectTransform.localScale = _iconBaseScales[i];
                }
            }

            if (_progressBar != null)
            {
                _progressBar.value = 0f;
            }

            if (_elapsedText != null)
            {
                _elapsedText.text = EmptyElapsed;
            }

            SetContentAlpha(0.45f);
        }

        private void SetContentAlpha(float alpha)
        {
            _contentAlpha = alpha;
            ApplyVisibility();
        }

        private void ApplyVisibility()
        {
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = _isStateVisible ? _contentAlpha : 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
        }

        private void RestartHideAnimation(int slotIndex)
        {
            if (_slotHideRoutines[slotIndex] != null)
            {
                StopCoroutine(_slotHideRoutines[slotIndex]);
            }

            _slotHideRoutines[slotIndex] = StartCoroutine(AnimateFulfilledSlot(slotIndex));
        }

        private IEnumerator AnimateFulfilledSlot(int slotIndex)
        {
            Image slot = _ingredientIconSlots[slotIndex];
            if (slot == null)
            {
                RefreshIcons();
                yield break;
            }

            RectTransform target = slot.rectTransform;
            Vector3 baseScale = _iconBaseScales[slotIndex];
            Color startColor = _fulfilledColor;
            float elapsed = 0f;

            slot.color = startColor;

            while (elapsed < FulfilledHideDuration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / FulfilledHideDuration);
                float scale = normalized < 0.45f
                    ? Mathf.Lerp(1f, FulfilledPeakScale, normalized / 0.45f)
                    : Mathf.Lerp(FulfilledPeakScale, 0f, (normalized - 0.45f) / 0.55f);

                target.localScale = baseScale * scale;
                slot.color = Color.Lerp(startColor, new Color(startColor.r, startColor.g, startColor.b, 0f), normalized);
                yield return null;
            }

            target.localScale = baseScale;
            slot.color = _defaultIconColor;
            _slotHideRoutines[slotIndex] = null;
            RefreshIcons();
        }
    }
}
