using System;
using UnityEngine;
using YesChef.Data;

namespace YesChef.Gameplay.Stations
{
    public enum PreparationSlotState
    {
        Empty,
        Processing,
        Ready
    }

    [Serializable]
    public class PreparationSlot
    {
        public PreparationSlotState State { get; private set; } = PreparationSlotState.Empty;
        public IngredientDefinition CurrentIngredient { get; private set; }
        public float TimeRemaining { get; private set; }
        public float TotalDuration { get; private set; }

        public event Action<PreparationSlotState> OnStateChanged;

        public void Place(IngredientDefinition ingredient, float duration)
        {
            if (ingredient == null)
            {
                Debug.LogError("[PreparationSlot] Cannot place a null ingredient.");
                return;
            }

            CurrentIngredient = ingredient;
            TotalDuration = Mathf.Max(0f, duration);
            TimeRemaining = TotalDuration;
            SetState(PreparationSlotState.Processing);
        }

        public IngredientDefinition Pickup()
        {
            if (State != PreparationSlotState.Ready)
            {
                Debug.LogWarning($"[PreparationSlot] Pickup called while in state {State}.");
                return null;
            }

            IngredientDefinition ingredient = CurrentIngredient;
            CurrentIngredient = null;
            TimeRemaining = 0f;
            TotalDuration = 0f;
            SetState(PreparationSlotState.Empty);
            return ingredient;
        }

        public void Tick(float dt)
        {
            if (State == PreparationSlotState.Empty)
            {
                Debug.LogWarning("[PreparationSlot] Tick called while Empty.");
                return;
            }

            if (State != PreparationSlotState.Processing)
            {
                return;
            }

            TimeRemaining = Mathf.Max(0f, TimeRemaining - dt);
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                SetState(PreparationSlotState.Ready);
            }
        }

        private void SetState(PreparationSlotState nextState)
        {
            if (State == nextState)
            {
                return;
            }

            State = nextState;
            OnStateChanged?.Invoke(State);
        }
    }
}
