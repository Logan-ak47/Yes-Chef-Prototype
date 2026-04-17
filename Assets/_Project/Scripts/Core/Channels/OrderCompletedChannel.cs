using System;
using UnityEngine;

namespace YesChef.Core.Channels
{
    public struct OrderCompletedData
    {
        public int scoreDelta;
        public Vector3 worldPos;
        public bool wasNegative;
    }

    [CreateAssetMenu(fileName = "OrderCompletedChannel", menuName = "YesChef/Channels/Order Completed Channel")]
    public class OrderCompletedChannel : ScriptableObject
    {
        public event Action<OrderCompletedData> OnRaised;

        public void Raise(OrderCompletedData data) => OnRaised?.Invoke(data);
    }
}
