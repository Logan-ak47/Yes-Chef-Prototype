using System;
using UnityEngine;
using YesChef.Data;

namespace YesChef.Core.Channels
{
    [CreateAssetMenu(fileName = "IngredientChannel", menuName = "YesChef/Channels/Ingredient Channel")]
    public class IngredientChannel : ScriptableObject
    {
        public event Action<IngredientDefinition> OnRaised;

        public void Raise(IngredientDefinition ingredient) => OnRaised?.Invoke(ingredient);
    }
}
