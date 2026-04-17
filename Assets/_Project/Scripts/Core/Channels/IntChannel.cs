using System;
using UnityEngine;

namespace YesChef.Core.Channels
{
    [CreateAssetMenu(fileName = "IntChannel", menuName = "YesChef/Channels/Int Channel")]
    public class IntChannel : ScriptableObject
    {
        public event Action<int> OnRaised;

        public void Raise(int value) => OnRaised?.Invoke(value);
    }
}
