using System;
using UnityEngine;
using YesChef.Gameplay.Orders;

namespace YesChef.Core.Channels
{
    [CreateAssetMenu(fileName = "OrderChangedChannel", menuName = "YesChef/Channels/Order Changed Channel")]
    public class OrderChangedChannel : ScriptableObject
    {
        public event Action<Order> OnRaised;

        public void Raise(Order order) => OnRaised?.Invoke(order);
    }
}
