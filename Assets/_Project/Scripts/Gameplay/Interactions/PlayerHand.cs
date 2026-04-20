using UnityEngine;
using YesChef.Data;

namespace YesChef.Gameplay.Interactions
{
    public class PlayerHand : MonoBehaviour
    {
        private const float PunchDurationSeconds = 0.14f;
        private const float PunchScaleMultiplier = 1.12f;

        [SerializeField] private Transform _handSocket;

        private IngredientDefinition _heldIngredient;
        private bool _isHeldPrepared;
        private GameObject _heldVisual;
        private Coroutine _socketPunchRoutine;
        private Vector3 _handSocketBaseScale = Vector3.one;

        public bool IsEmpty => _heldIngredient == null;
        public IngredientDefinition HeldIngredient => _heldIngredient;
        public bool IsHeldPrepared => _isHeldPrepared;
        public Transform HandSocket => _handSocket;

        private void Awake()
        {
            if (_handSocket != null)
            {
                _handSocketBaseScale = _handSocket.localScale;
            }
        }

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
            {
                _heldVisual = Instantiate(prefab, _handSocket);
                StartPunch(_heldVisual.transform, Vector3.one);
            }
            else
                Debug.LogWarning($"[PlayerHand] No visual prefab assigned on '{def.DisplayName}' (prepared={prepared}).");
        }

        public void Drop()
        {
            if (_handSocket != null)
            {
                RestartSocketPunch();
            }

            DestroyHeldVisual();
            _heldIngredient = null;
            _isHeldPrepared = false;
        }

        public void DestroyHeld() => Drop();

        private void DestroyHeldVisual()
        {
            if (_heldVisual == null) return;
            Destroy(_heldVisual);
            _heldVisual = null;
        }

        private void RestartSocketPunch()
        {
            if (_socketPunchRoutine != null)
            {
                StopCoroutine(_socketPunchRoutine);
            }

            _socketPunchRoutine = StartCoroutine(PunchScale(_handSocket, _handSocketBaseScale));
        }

        private void StartPunch(Transform target, Vector3 baseScale)
        {
            StartCoroutine(PunchScale(target, baseScale));
        }

        private System.Collections.IEnumerator PunchScale(Transform target, Vector3 baseScale)
        {
            float elapsed = 0f;
            while (elapsed < PunchDurationSeconds)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / PunchDurationSeconds);
                float curve = normalized < 0.5f
                    ? Mathf.Lerp(1f, PunchScaleMultiplier, normalized / 0.5f)
                    : Mathf.Lerp(PunchScaleMultiplier, 1f, (normalized - 0.5f) / 0.5f);
                if (target != null)
                {
                    target.localScale = baseScale * curve;
                }

                yield return null;
            }

            if (target != null)
            {
                target.localScale = baseScale;
            }

            if (target == _handSocket)
            {
                _socketPunchRoutine = null;
            }
        }
    }
}
