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
        [SerializeField] private IngredientDefinition[] _orderableIngredients;

        public int MaxActiveOrders => _maxActiveOrders;
        public float OrderRespawnDelay => _orderRespawnDelay;
        public int MinIngredientsPerOrder => _minIngredientsPerOrder;
        public int MaxIngredientsPerOrder => _maxIngredientsPerOrder;
        public IngredientDefinition[] OrderableIngredients => _orderableIngredients;
    }
}
