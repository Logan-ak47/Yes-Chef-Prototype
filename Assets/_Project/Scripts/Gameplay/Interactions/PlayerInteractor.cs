using System.Collections.Generic;
using UnityEngine;

namespace YesChef.Gameplay.Interactions
{
    // Sits on the InteractionRange child GameObject (SphereCollider trigger).
    // Auto-interact fires ONLY on trigger entry. If hand state changes while the player
    // is already overlapping a station (e.g. stove finishes cooking), the station must
    // call ReEvaluate() — typically via a UnityEvent — to re-check CanInteract.
    public class PlayerInteractor : MonoBehaviour
    {
        private PlayerHand _hand;
        private readonly HashSet<IInteractable> _inRange = new HashSet<IInteractable>();

        private void Awake()
        {
            _hand = GetComponentInParent<PlayerHand>();
            if (_hand == null)
                Debug.LogError("[PlayerInteractor] No PlayerHand found on parent hierarchy.", this);
        }

        private void OnTriggerEnter(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable == null) return;

            _inRange.Add(interactable);
            interactable.OnEnterRange(_hand);

            if (interactable.CanInteract(_hand))
                interactable.TryInteract(_hand);
        }

        private void OnTriggerExit(Collider other)
        {
            var interactable = other.GetComponent<IInteractable>();
            if (interactable == null) return;

            _inRange.Remove(interactable);
            interactable.OnExitRange(_hand);
        }

        // Stations call this (via UnityEvent or direct reference) when their internal
        // state changes while the player is standing in range, so auto-interact can
        // fire without requiring the player to re-enter the trigger.
        public void ReEvaluate()
        {
            foreach (var interactable in _inRange)
            {
                if (interactable.CanInteract(_hand))
                    interactable.TryInteract(_hand);
            }
        }
    }
}
