using UnityEngine;

namespace YesChef.Gameplay.Interactions
{
    public interface IInteractable
    {
        bool CanInteract(PlayerHand hand);
        void OnEnterRange(PlayerHand hand);
        void OnExitRange(PlayerHand hand);
        void TryInteract(PlayerHand hand);
        Transform Anchor { get; }
    }
}
