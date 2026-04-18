using UnityEngine;
using YesChef.Data;

namespace YesChef.Gameplay.Interactions
{
    public class PlayerHand : MonoBehaviour
    {
        [SerializeField] private Transform _handSocket;

        private IngredientDefinition _heldIngredient;
        private bool _isHeldPrepared;
        private GameObject _heldVisual;

        public bool IsEmpty => _heldIngredient == null;
        public IngredientDefinition HeldIngredient => _heldIngredient;
        public bool IsHeldPrepared => _isHeldPrepared;
        public Transform HandSocket => _handSocket;

        public void Pickup(IngredientDefinition def, bool prepared)
        {
            if (!IsEmpty)
            {
                Debug.LogWarning("[PlayerHand] Tried to pick up while already holding something.");
                return;
            }
            if (def == null)
            {
                Debug.LogError("[PlayerHand] Pickup called with null IngredientDefinition.");
                return;
            }

            _heldIngredient = def;
            _isHeldPrepared = prepared;

            var prefab = prepared ? def.PreparedVisualPrefab : def.RawVisualPrefab;
            if (prefab != null)
                _heldVisual = Instantiate(prefab, _handSocket);
            else
                Debug.LogWarning($"[PlayerHand] No visual prefab assigned on '{def.DisplayName}' (prepared={prepared}).");
        }

        // Clears the hand and destroys the held visual.
        // Stations should read HeldIngredient/IsHeldPrepared BEFORE calling Drop().
        public void Drop()
        {
            DestroyHeldVisual();
            _heldIngredient = null;
            _isHeldPrepared = false;
        }

        // Alias used by TrashCan; semantically "discard without giving to a station".
        public void DestroyHeld() => Drop();

        private void DestroyHeldVisual()
        {
            if (_heldVisual == null) return;
            Destroy(_heldVisual);
            _heldVisual = null;
        }
    }
}
