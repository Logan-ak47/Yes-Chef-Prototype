using System;
using System.Collections.Generic;
using YesChef.Data;

namespace YesChef.Gameplay.Orders
{
    public class Order
    {
        private readonly List<IngredientDefinition> _requiredIngredients;
        private readonly HashSet<int> _fulfilledIndices;

        public Order(List<IngredientDefinition> requiredIngredients, float timeOpenedAt)
        {
            _requiredIngredients = requiredIngredients ?? new List<IngredientDefinition>();
            _fulfilledIndices = new HashSet<int>();
            TimeOpenedAt = timeOpenedAt;
        }

        public IReadOnlyList<IngredientDefinition> RequiredIngredients => _requiredIngredients;
        public IReadOnlyCollection<int> FulfilledIndices => _fulfilledIndices;
        public float TimeOpenedAt { get; }
        public bool IsComplete => _fulfilledIndices.Count == _requiredIngredients.Count;

        public event Action<int> OnIngredientFulfilled;
        public event Action OnCompleted;

        public bool CanFulfill(IngredientDefinition prepared)
        {
            return FindMatchingUnfulfilledIndex(prepared) >= 0;
        }

        public bool TryFulfill(IngredientDefinition prepared)
        {
            int index = FindMatchingUnfulfilledIndex(prepared);
            if (index < 0)
            {
                return false;
            }

            _fulfilledIndices.Add(index);
            OnIngredientFulfilled?.Invoke(index);

            if (IsComplete)
            {
                OnCompleted?.Invoke();
            }

            return true;
        }

        private int FindMatchingUnfulfilledIndex(IngredientDefinition prepared)
        {
            if (prepared == null)
            {
                return -1;
            }

            for (int i = 0; i < _requiredIngredients.Count; i++)
            {
                if (_fulfilledIndices.Contains(i))
                {
                    continue;
                }

                if (_requiredIngredients[i] == prepared)
                {
                    return i;
                }
            }

            return -1;
        }
    }
}
