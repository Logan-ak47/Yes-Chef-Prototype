using UnityEngine;

namespace YesChef.Data
{
    [CreateAssetMenu(fileName = "DefaultGameSettings", menuName = "YesChef/Settings/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [SerializeField] private float _gameDurationSeconds = 180f;
        [SerializeField] private OrderSettings _orderSettings;
        [SerializeField] private float _playerMoveSpeed = 5f;

        public float GameDurationSeconds => _gameDurationSeconds;
        public OrderSettings OrderSettings => _orderSettings;
        public float PlayerMoveSpeed => _playerMoveSpeed;
    }
}
