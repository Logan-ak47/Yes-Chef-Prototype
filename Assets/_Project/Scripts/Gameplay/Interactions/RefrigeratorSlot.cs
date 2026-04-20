using UnityEngine;
using YesChef.Data;
using YesChef.Gameplay.Visuals;

namespace YesChef.Gameplay.Interactions
{
    [RequireComponent(typeof(BoxCollider))]
    public class RefrigeratorSlot : MonoBehaviour, IInteractable
    {
        [SerializeField] private IngredientDefinition _ingredient;
        [SerializeField] private GameObject _highlight;

        private EmissionHighlight _highlightController;

        public Transform Anchor => transform;

        private void Awake()
        {
            GetComponent<BoxCollider>().isTrigger = true;

            if (_highlight != null)
            {
                _highlightController = _highlight.GetComponent<EmissionHighlight>();
                if (_highlightController == null)
                {
                    _highlightController = _highlight.AddComponent<EmissionHighlight>();
                }
            }

            if (_ingredient == null)
                Debug.LogError($"[RefrigeratorSlot] '{name}': IngredientDefinition not assigned.", this);
        }

        public bool CanInteract(PlayerHand hand) => hand.IsEmpty;

        public void OnEnterRange(PlayerHand hand)
        {
            _highlightController?.Show();
        }

        public void OnExitRange(PlayerHand hand)
        {
            _highlightController?.Hide();
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
