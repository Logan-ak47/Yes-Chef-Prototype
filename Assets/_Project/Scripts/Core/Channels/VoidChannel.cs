using System;
using UnityEngine;

namespace YesChef.Core.Channels
{
    [CreateAssetMenu(fileName = "VoidChannel", menuName = "YesChef/Channels/Void Channel")]
    public class VoidChannel : ScriptableObject
    {
        public event Action OnRaised;

        public void Raise() => OnRaised?.Invoke();
    }
}
