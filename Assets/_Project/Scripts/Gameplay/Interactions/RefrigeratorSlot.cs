using UnityEngine;
using YesChef.Data;

namespace YesChef.Gameplay.Interactions
{
    [RequireComponent(typeof(BoxCollider))]
    public class RefrigeratorSlot : MonoBehaviour, IInteractable
    {
        [SerializeField] private IngredientDefinition _ingredient;
        [SerializeField] private GameObject _highlight;

        public Transform Anchor => transform;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;

            if (_highlight != null)
                _highlight.SetActive(false);

            if (_ingredient == null)
                Debug.LogError($"[RefrigeratorSlot] '{name}': IngredientDefinition not assigned.", this);
        }

        public bool CanInteract(PlayerHand hand) => hand.IsEmpty;

        public void OnEnterRange(PlayerHand hand)
        {
            if (_highlight != null) _highlight.SetActive(true);
        }

        public void OnExitRange(PlayerHand hand)
        {
            if (_highlight != null) _highlight.SetActive(false);
        }

        public void TryInteract(PlayerHand hand)
        {
            if (_ingredient == null)
            {
                Debug.LogError($"[RefrigeratorSlot] '{name}': Cannot interact — IngredientDefinition not assigned.", this);
                return;
            }
            bool alreadyPrepared = _ingredient.RequiredPreparation == PreparationType.None;
            hand.Pickup(_ingredient, alreadyPrepared);
        }
    }
}
