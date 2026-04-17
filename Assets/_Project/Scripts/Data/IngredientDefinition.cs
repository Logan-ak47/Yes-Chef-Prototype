using UnityEngine;

namespace YesChef.Data
{
    [CreateAssetMenu(fileName = "NewIngredient", menuName = "YesChef/Data/Ingredient Definition")]
    public class IngredientDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _debugColor = Color.white;
        [SerializeField] private int _scoreValue;
        [SerializeField] private PreparationType _requiredPreparation;
        [SerializeField] private float _preparationDuration;
        [SerializeField] private GameObject _rawVisualPrefab;
        [SerializeField] private GameObject _preparedVisualPrefab;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public Color DebugColor => _debugColor;
        public int ScoreValue => _scoreValue;
        public PreparationType RequiredPreparation => _requiredPreparation;
        public float PreparationDuration => _preparationDuration;
        public GameObject RawVisualPrefab => _rawVisualPrefab;
        public GameObject PreparedVisualPrefab => _preparedVisualPrefab;
    }
}
