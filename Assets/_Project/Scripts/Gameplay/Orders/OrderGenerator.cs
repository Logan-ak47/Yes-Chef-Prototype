using System.Collections.Generic;
using UnityEngine;
using YesChef.Data;

namespace YesChef.Gameplay.Orders
{
    public class OrderGenerator
    {
        private readonly OrderSettings _settings;

        public OrderGenerator(OrderSettings settings)
        {
            _settings = settings;
        }

        public Order GenerateRandom(float currentTime)
        {
            var requiredIngredients = new List<IngredientDefinition>();

            if (_settings == null)
            {
                Debug.LogError("[OrderGenerator] Cannot generate an order without OrderSettings.");
                return new Order(requiredIngredients, currentTime);
            }

            IngredientDefinition[] orderableIngredients = _settings.OrderableIngredients;
            if (orderableIngredients == null || orderableIngredients.Length == 0)
            {
                Debug.LogError("[OrderGenerator] OrderSettings has no orderable ingredients assigned.");
                return new Order(requiredIngredients, currentTime);
            }

            int minCount = Mathf.Min(_settings.MinIngredientsPerOrder, _settings.MaxIngredientsPerOrder);
            int maxCount = Mathf.Max(_settings.MinIngredientsPerOrder, _settings.MaxIngredientsPerOrder);
            int ingredientCount = minCount == maxCount
                ? minCount
                : (Random.value < 0.5f ? minCount : maxCount);

            for (int i = 0; i < ingredientCount; i++)
            {
                int ingredientIndex = Random.Range(0, orderableIngredients.Length);
                requiredIngredients.Add(orderableIngredients[ingredientIndex]);
            }

            return new Order(requiredIngredients, currentTime);
        }
    }
}
