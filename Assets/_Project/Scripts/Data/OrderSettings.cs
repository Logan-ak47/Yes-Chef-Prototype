using UnityEngine;

namespace YesChef.Data
{
    [CreateAssetMenu(fileName = "DefaultOrderSettings", menuName = "YesChef/Settings/Order Settings")]
    public class OrderSettings : ScriptableObject
    {
        [SerializeField] private int _maxActiveOrders = 4;
        [SerializeField] private float _orderRespawnDelay = 5f;
        [SerializeField] private int _minIngredientsPerOrder = 2;
        [SerializeField] private int _maxIngredientsPerOrder = 3;
        [SerializeField, Range(0f, 3f), Tooltip("Multiplier applied to the elapsed-seconds penalty when scoring a completed order. 1.0 = spec default (1 point lost per second). 0.0 = no time penalty. Values above 1 make time pressure harsher.")]
        private float _scorePenaltyMultiplier = 1f;
        [SerializeField] private IngredientDefinition[] _orderableIngredients;

        public int MaxActiveOrders => _maxActiveOrders;
        public float OrderRespawnDelay => _orderRespawnDelay;
        public int MinIngredientsPerOrder => _minIngredientsPerOrder;
        public int MaxIngredientsPerOrder => _maxIngredientsPerOrder;
        public float ScorePenaltyMultiplier => _scorePenaltyMultiplier;
        public IngredientDefinition[] OrderableIngredients => _orderableIngredients;
    }
}
