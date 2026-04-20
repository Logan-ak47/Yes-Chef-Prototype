using System;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Data;
using YesChef.Gameplay.Interactions;
using YesChef.Gameplay.Visuals;

namespace YesChef.Gameplay.Stations
{
    [RequireComponent(typeof(BoxCollider))]
    public class ChoppingTable : MonoBehaviour, IInteractable
    {
        [SerializeField] private StationDefinition _definition;
        [SerializeField] private Transform _slotAnchor;
        [SerializeField] private Canvas _timerCanvas;
        [SerializeField] private Slider _timerSlider;
        [SerializeField] private Image _timerFillImage;
        [SerializeField] private GameObject _highlight;

        private readonly PreparationSlot _slot = new PreparationSlot();
        private bool _playerPresent;
        private GameObject _slotVisual;
        private EmissionHighlight _highlightController;

        public Transform Anchor => transform;
        public event Action SlotStateChanged;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;

            if (_highlight != null)
            {
                _highlightController = _highlight.GetComponent<EmissionHighlight>();
                if (_highlightController == null)
                {
                    _highlightController = _highlight.AddComponent<EmissionHighlight>();
                }
            }

            ConfigureSlider();
            RefreshTimerDisplay();

            if (_definition == null)
            {
                Debug.LogError($"[ChoppingTable] '{name}': StationDefinition not assigned.", this);
                return;
            }

            if (_definition.StationType != PreparationType.Chopping)
            {
                Debug.LogWarning($"[ChoppingTable] '{name}': StationDefinition type is not Chopping.", this);
            }

            _slot.OnStateChanged += HandleSlotStateChanged;
        }

        private void OnDestroy()
        {
            _slot.OnStateChanged -= HandleSlotStateChanged;
        }

        public bool CanInteract(PlayerHand hand)
        {
            if (hand.IsEmpty)
            {
                return _slot.State == PreparationSlotState.Ready;
            }

            return hand.HeldIngredient != null
                && hand.HeldIngredient.RequiredPreparation == PreparationType.Chopping
                && !hand.IsHeldPrepared
                && _slot.State == PreparationSlotState.Empty;
        }

        public void OnEnterRange(PlayerHand hand)
        {
            _playerPresent = true;
            _highlightController?.Show();
        }

        public void OnExitRange(PlayerHand hand)
        {
            _playerPresent = false;
            _highlightController?.Hide();
        }

        public void TryInteract(PlayerHand hand)
        {
            if (hand.IsEmpty && _slot.State == PreparationSlotState.Ready)
            {
                PickupFromSlot(hand);
                return;
            }

            if (!hand.IsEmpty
                && hand.HeldIngredient != null
                && hand.HeldIngredient.RequiredPreparation == PreparationType.Chopping
                && !hand.IsHeldPrepared
                && _slot.State == PreparationSlotState.Empty)
            {
                PlaceOnSlot(hand);
            }
        }

        private void Update()
        {
            if (!_playerPresent || _slot.State != PreparationSlotState.Processing)
            {
                return;
            }

            _slot.Tick(Time.deltaTime);
            RefreshTimerDisplay();
        }

        private void PlaceOnSlot(PlayerHand hand)
        {
            IngredientDefinition ingredient = hand.HeldIngredient;
            if (ingredient == null)
            {
                return;
            }

            hand.Drop();
            _slot.Place(ingredient, ingredient.PreparationDuration);

            DestroySlotVisual();
            if (_slotAnchor != null && ingredient.RawVisualPrefab != null)
            {
                _slotVisual = Instantiate(ingredient.RawVisualPrefab, _slotAnchor);
            }
        }

        private void PickupFromSlot(PlayerHand hand)
        {
            IngredientDefinition ingredient = _slot.Pickup();
            if (ingredient == null)
            {
                return;
            }

            hand.Pickup(ingredient, prepared: true);
            DestroySlotVisual();
        }

        private void HandleSlotStateChanged(PreparationSlotState state)
        {
            if (state == PreparationSlotState.Ready)
            {
                SwapVisualToPrepared();
            }

            RefreshTimerDisplay();
            SlotStateChanged?.Invoke();
        }

        private void ConfigureSlider()
        {
            if (_timerSlider == null)
            {
                return;
            }

            _timerSlider.minValue = 0f;
            _timerSlider.maxValue = 1f;
            _timerSlider.wholeNumbers = false;
        }

        private void RefreshTimerDisplay()
        {
            bool isProcessing = _slot.State == PreparationSlotState.Processing && _slot.TotalDuration > 0f;

            if (_timerCanvas != null)
            {
                _timerCanvas.gameObject.SetActive(isProcessing);
            }

            if (!isProcessing || _timerSlider == null)
            {
                return;
            }

            float ratio = _slot.TimeRemaining / _slot.TotalDuration;
            _timerSlider.SetValueWithoutNotify(ratio);

            if (_timerFillImage != null)
            {
                float elapsed = 1f - ratio;
                _timerFillImage.color = elapsed < 0.5f
                    ? Color.Lerp(Color.green, Color.yellow, elapsed * 2f)
                    : Color.Lerp(Color.yellow, Color.red, (elapsed - 0.5f) * 2f);
            }
        }

        private void SwapVisualToPrepared()
        {
            DestroySlotVisual();

            GameObject prefab = _slot.CurrentIngredient?.PreparedVisualPrefab;
            if (_slotAnchor != null && prefab != null)
            {
                _slotVisual = Instantiate(prefab, _slotAnchor);
            }
        }

        private void DestroySlotVisual()
        {
            if (_slotVisual != null)
            {
                Destroy(_slotVisual);
                _slotVisual = null;
            }
        }
    }
}
