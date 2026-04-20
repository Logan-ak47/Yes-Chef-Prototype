using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using YesChef.Data;
using YesChef.Gameplay.Interactions;
using YesChef.Gameplay.Visuals;

namespace YesChef.Gameplay.Stations
{
    [RequireComponent(typeof(BoxCollider))]
    public class Stove : MonoBehaviour, IInteractable
    {
        [SerializeField] private StationDefinition _definition;
        [SerializeField] private Transform[] _slotAnchors = new Transform[2];
        [SerializeField] private Canvas[] _timerCanvases = new Canvas[2];
        [SerializeField] private Slider[] _timerSliders = new Slider[2];
        [SerializeField] private Image[] _timerFillImages = new Image[2];
        [SerializeField] private GameObject _highlight;

        private readonly List<PreparationSlot> _slots = new List<PreparationSlot>(2);
        private readonly GameObject[] _slotVisuals = new GameObject[2];
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

            _slots.Clear();
            for (int i = 0; i < 2; i++)
            {
                int slotIndex = i;
                PreparationSlot slot = new PreparationSlot();
                slot.OnStateChanged += state => HandleSlotStateChanged(slotIndex, state);
                _slots.Add(slot);
                ConfigureSlider(slotIndex);
                RefreshTimerDisplay(slotIndex);
            }

            if (_definition == null)
            {
                Debug.LogError($"[Stove] '{name}': StationDefinition not assigned.", this);
                return;
            }

            if (_definition.StationType != PreparationType.Cooking)
            {
                Debug.LogWarning($"[Stove] '{name}': StationDefinition type is not Cooking.", this);
            }
            else if (_definition.SlotCount != 2)
            {
                Debug.LogWarning($"[Stove] '{name}': StationDefinition slot count should be 2, found {_definition.SlotCount}.", this);
            }
        }

        public bool CanInteract(PlayerHand hand)
        {
            if (hand.IsEmpty)
            {
                return FirstReadySlot() >= 0;
            }

            return hand.HeldIngredient != null
                && hand.HeldIngredient.RequiredPreparation == PreparationType.Cooking
                && !hand.IsHeldPrepared
                && FirstEmptySlot() >= 0;
        }

        public void OnEnterRange(PlayerHand hand)
        {
            _highlightController?.Show();
        }

        public void OnExitRange(PlayerHand hand)
        {
            _highlightController?.Hide();
        }

        public void TryInteract(PlayerHand hand)
        {
            if (hand.IsEmpty)
            {
                int readySlot = FirstReadySlot();
                if (readySlot >= 0)
                {
                    PickupFromSlot(hand, readySlot);
                }

                return;
            }

            if (hand.HeldIngredient == null
                || hand.HeldIngredient.RequiredPreparation != PreparationType.Cooking
                || hand.IsHeldPrepared)
            {
                return;
            }

            int emptySlot = FirstEmptySlot();
            if (emptySlot >= 0)
            {
                PlaceOnSlot(hand, emptySlot);
            }
        }

        private void Update()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].State != PreparationSlotState.Processing)
                {
                    continue;
                }

                _slots[i].Tick(Time.deltaTime);
                RefreshTimerDisplay(i);
            }
        }

        private void PlaceOnSlot(PlayerHand hand, int slotIndex)
        {
            IngredientDefinition ingredient = hand.HeldIngredient;
            if (ingredient == null)
            {
                return;
            }

            hand.Drop();
            _slots[slotIndex].Place(ingredient, ingredient.PreparationDuration);

            DestroySlotVisual(slotIndex);
            if (slotIndex < _slotAnchors.Length && _slotAnchors[slotIndex] != null && ingredient.RawVisualPrefab != null)
            {
                _slotVisuals[slotIndex] = Instantiate(ingredient.RawVisualPrefab, _slotAnchors[slotIndex]);
            }
        }

        private void PickupFromSlot(PlayerHand hand, int slotIndex)
        {
            IngredientDefinition ingredient = _slots[slotIndex].Pickup();
            if (ingredient == null)
            {
                return;
            }

            hand.Pickup(ingredient, prepared: true);
            DestroySlotVisual(slotIndex);
        }

        private void HandleSlotStateChanged(int slotIndex, PreparationSlotState state)
        {
            if (state == PreparationSlotState.Ready)
            {
                SwapVisualToPrepared(slotIndex);
            }

            RefreshTimerDisplay(slotIndex);
            SlotStateChanged?.Invoke();
        }

        private void ConfigureSlider(int slotIndex)
        {
            if (slotIndex >= _timerSliders.Length || _timerSliders[slotIndex] == null)
            {
                return;
            }

            _timerSliders[slotIndex].minValue = 0f;
            _timerSliders[slotIndex].maxValue = 1f;
            _timerSliders[slotIndex].wholeNumbers = false;
        }

        private void RefreshTimerDisplay(int slotIndex)
        {
            if (!IsValidSlotIndex(slotIndex))
            {
                return;
            }

            bool isProcessing = _slots[slotIndex].State == PreparationSlotState.Processing
                && _slots[slotIndex].TotalDuration > 0f;

            if (slotIndex < _timerCanvases.Length && _timerCanvases[slotIndex] != null)
            {
                _timerCanvases[slotIndex].gameObject.SetActive(isProcessing);
            }

            if (!isProcessing || slotIndex >= _timerSliders.Length || _timerSliders[slotIndex] == null)
            {
                return;
            }

            float ratio = _slots[slotIndex].TimeRemaining / _slots[slotIndex].TotalDuration;
            _timerSliders[slotIndex].SetValueWithoutNotify(ratio);

            if (slotIndex < _timerFillImages.Length && _timerFillImages[slotIndex] != null)
            {
                float elapsed = 1f - ratio;
                _timerFillImages[slotIndex].color = elapsed < 0.5f
                    ? Color.Lerp(Color.green, Color.yellow, elapsed * 2f)
                    : Color.Lerp(Color.yellow, Color.red, (elapsed - 0.5f) * 2f);
            }
        }

        private void SwapVisualToPrepared(int slotIndex)
        {
            DestroySlotVisual(slotIndex);

            GameObject prefab = _slots[slotIndex].CurrentIngredient?.PreparedVisualPrefab;
            if (slotIndex < _slotAnchors.Length && _slotAnchors[slotIndex] != null && prefab != null)
            {
                _slotVisuals[slotIndex] = Instantiate(prefab, _slotAnchors[slotIndex]);
            }
        }

        private void DestroySlotVisual(int slotIndex)
        {
            if (slotIndex >= _slotVisuals.Length || _slotVisuals[slotIndex] == null)
            {
                return;
            }

            Destroy(_slotVisuals[slotIndex]);
            _slotVisuals[slotIndex] = null;
        }

        private bool IsValidSlotIndex(int slotIndex)
        {
            return slotIndex >= 0 && slotIndex < _slots.Count;
        }

        private int FirstReadySlot()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].State == PreparationSlotState.Ready)
                {
                    return i;
                }
            }

            return -1;
        }

        private int FirstEmptySlot()
        {
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i].State == PreparationSlotState.Empty)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
