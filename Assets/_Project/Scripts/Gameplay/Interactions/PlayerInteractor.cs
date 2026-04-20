using System.Collections.Generic;
using UnityEngine;

namespace YesChef.Gameplay.Interactions
{
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
