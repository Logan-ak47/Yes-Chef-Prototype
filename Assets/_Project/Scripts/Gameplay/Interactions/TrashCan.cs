using UnityEngine;

namespace YesChef.Gameplay.Interactions
{
    [RequireComponent(typeof(BoxCollider))]
    public class TrashCan : MonoBehaviour, IInteractable
    {
        [SerializeField] private GameObject _highlight;

        public Transform Anchor => transform;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;

            if (_highlight != null)
                _highlight.SetActive(false);
        }

        public bool CanInteract(PlayerHand hand) => !hand.IsEmpty;

        public void OnEnterRange(PlayerHand hand)
        {
            if (_highlight != null) _highlight.SetActive(true);
        }

        public void OnExitRange(PlayerHand hand)
        {
            if (_highlight != null) _highlight.SetActive(false);
        }

        public void TryInteract(PlayerHand hand) => hand.DestroyHeld();
    }
}
