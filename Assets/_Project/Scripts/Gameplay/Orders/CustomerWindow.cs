using UnityEngine;
using YesChef.Core.Channels;
using YesChef.Data;
using YesChef.Gameplay.Interactions;

namespace YesChef.Gameplay.Orders
{
    [RequireComponent(typeof(BoxCollider))]
    public class CustomerWindow : MonoBehaviour, IInteractable
    {
        [SerializeField] private OrderSettings _settings;
        [SerializeField] private IntChannel _scoreChannel;
        [SerializeField] private OrderCompletedChannel _completionChannel;
        [SerializeField] private OrderChangedChannel _orderChangedChannel;
        [SerializeField] private Transform _anchor;

        private OrderService _orderService;
        private Order _currentOrder;
        private float _currentElapsedTime;
        private bool _awaitingRespawn;
        private float _respawnAtTime;

        public Transform Anchor => _anchor != null ? _anchor : transform;
        public OrderSettings Settings => _settings;
        public Order CurrentOrder => _currentOrder;
        public float CurrentElapsedTime => _currentElapsedTime;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;
        }

        public void BindOrderService(OrderService orderService)
        {
            _orderService = orderService;
        }

        public bool CanInteract(PlayerHand hand)
        {
            if (hand == null || hand.IsEmpty || !hand.IsHeldPrepared || _currentOrder == null)
            {
                return false;
            }

            return _currentOrder.CanFulfill(hand.HeldIngredient);
        }

        public void OnEnterRange(PlayerHand hand)
        {
        }

        public void OnExitRange(PlayerHand hand)
        {
        }

        public void TryInteract(PlayerHand hand)
        {
            if (!CanInteract(hand))
            {
                return;
            }

            IngredientDefinition preparedIngredient = hand.HeldIngredient;
            if (preparedIngredient == null || !_currentOrder.TryFulfill(preparedIngredient))
            {
                return;
            }

            hand.Drop();
            _orderChangedChannel?.Raise(_currentOrder);

            if (!_currentOrder.IsComplete)
            {
                return;
            }

            int scoreDelta = CalculateScoreDelta(_currentOrder);
            _scoreChannel?.Raise(scoreDelta);
            _completionChannel?.Raise(new OrderCompletedData
            {
                scoreDelta = scoreDelta,
                worldPos = Anchor.position,
                wasNegative = scoreDelta < 0
            });

            BeginRespawnCountdown();
        }

        public void SetOrder(Order order)
        {
            _awaitingRespawn = false;
            _respawnAtTime = 0f;
            _currentOrder = order;
            _currentElapsedTime = _currentOrder == null ? 0f : Mathf.Max(0f, Time.time - _currentOrder.TimeOpenedAt);
            _orderChangedChannel?.Raise(_currentOrder);
        }

        public void ClearOrder()
        {
            _awaitingRespawn = false;
            _respawnAtTime = 0f;
            _currentOrder = null;
            _currentElapsedTime = 0f;
            _orderChangedChannel?.Raise(null);
        }

        private void Update()
        {
            if (_currentOrder != null)
            {
                _currentElapsedTime = Mathf.Max(0f, Time.time - _currentOrder.TimeOpenedAt);
            }

            if (!_awaitingRespawn || Time.time < _respawnAtTime)
            {
                return;
            }

            _awaitingRespawn = false;
            _respawnAtTime = 0f;
            _orderService?.RequestNextOrder(this);
        }

        private int CalculateScoreDelta(Order completedOrder)
        {
            int ingredientValueTotal = 0;
            for (int i = 0; i < completedOrder.RequiredIngredients.Count; i++)
            {
                IngredientDefinition ingredient = completedOrder.RequiredIngredients[i];
                if (ingredient != null)
                {
                    ingredientValueTotal += ingredient.ScoreValue;
                }
            }

            return ingredientValueTotal - Mathf.FloorToInt(Time.time - completedOrder.TimeOpenedAt);
        }

        private void BeginRespawnCountdown()
        {
            float delay = _settings != null ? _settings.OrderRespawnDelay : 5f;
            _awaitingRespawn = true;
            _respawnAtTime = Time.time + delay;
            _currentOrder = null;
            _currentElapsedTime = 0f;
            _orderChangedChannel?.Raise(null);
        }
    }
}
