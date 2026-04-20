using System.Collections;
using TMPro;
using UnityEngine;

namespace YesChef.UI
{
    public class ScoreFloater : MonoBehaviour
    {
        private const float LifetimeSeconds = 1.5f;
        private const float RiseDistance = 1f;

        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _positiveColor = new Color(0.35f, 1f, 0.45f, 1f);
        [SerializeField] private Color _negativeColor = new Color(1f, 0.3f, 0.3f, 1f);

        private Camera _mainCamera;
        private Coroutine _lifetimeRoutine;

        private void LateUpdate()
        {
            if (_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }

            if (_mainCamera != null)
            {
                transform.forward = _mainCamera.transform.forward;
            }
        }

        public void Play(int scoreDelta, bool wasNegative)
        {
            if (_label == null)
            {
                return;
            }

            _label.text = scoreDelta >= 0 ? $"+{scoreDelta}" : scoreDelta.ToString();
            _label.color = wasNegative ? _negativeColor : _positiveColor;

            if (_lifetimeRoutine != null)
            {
                StopCoroutine(_lifetimeRoutine);
            }

            _lifetimeRoutine = StartCoroutine(AnimateFloater());
        }

        private IEnumerator AnimateFloater()
        {
            Vector3 start = transform.position;
            Vector3 end = start + Vector3.up * RiseDistance;
            Color baseColor = _label.color;
            float elapsed = 0f;

            while (elapsed < LifetimeSeconds)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / LifetimeSeconds);
                transform.position = Vector3.Lerp(start, end, t);

                Color color = baseColor;
                color.a = 1f - t;
                _label.color = color;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
