using UnityEngine;

namespace YesChef.Data
{
    [CreateAssetMenu(fileName = "NewStation", menuName = "YesChef/Data/Station Definition")]
    public class StationDefinition : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private PreparationType _stationType;
        [SerializeField] private int _slotCount = 1;

        public string DisplayName => _displayName;
        public PreparationType StationType => _stationType;
        public int SlotCount => _slotCount;
    }
}
