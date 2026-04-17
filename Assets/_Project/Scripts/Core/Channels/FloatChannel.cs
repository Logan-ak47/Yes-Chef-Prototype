using System;
using UnityEngine;

namespace YesChef.Core.Channels
{
    [CreateAssetMenu(fileName = "FloatChannel", menuName = "YesChef/Channels/Float Channel")]
    public class FloatChannel : ScriptableObject
    {
        public event Action<float> OnRaised;

        public void Raise(float value) => OnRaised?.Invoke(value);
    }
}
