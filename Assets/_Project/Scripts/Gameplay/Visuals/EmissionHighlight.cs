using UnityEngine;

namespace YesChef.Gameplay.Visuals
{
    public class EmissionHighlight : MonoBehaviour
    {
        private const string EmissionColorProperty = "_EmissionColor";
        private const float HighlightIntensity = 1.6f;

        [SerializeField] private Renderer[] _renderers;

        private Color[] _baseEmissionColors;
        private bool _isInitialized;

        private void Awake()
        {
            TryEnsureInitialized();
            SetHighlighted(false);
        }

        public void Show()
        {
            SetHighlighted(true);
        }

        public void Hide()
        {
            SetHighlighted(false);
        }

        private void SetHighlighted(bool isHighlighted)
        {
            if (!TryEnsureInitialized())
            {
                return;
            }

            float intensity = isHighlighted ? HighlightIntensity : 0f;
            int count = Mathf.Min(_renderers.Length, _baseEmissionColors.Length);
            for (int i = 0; i < count; i++)
            {
                Renderer renderer = _renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.enabled = isHighlighted;

                Material material = renderer.material;
                if (material.HasProperty(EmissionColorProperty))
                {
                    material.SetColor(EmissionColorProperty, _baseEmissionColors[i] * intensity);
                }
            }
        }

        private bool TryEnsureInitialized()
        {
            if (_isInitialized)
            {
                return true;
            }

            if (_renderers == null || _renderers.Length == 0)
            {
                _renderers = GetComponentsInChildren<Renderer>(true);
            }

            if (_renderers == null)
            {
                _renderers = System.Array.Empty<Renderer>();
            }

            _baseEmissionColors = new Color[_renderers.Length];
            for (int i = 0; i < _renderers.Length; i++)
            {
                Renderer renderer = _renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                Material material = renderer.material;
                material.EnableKeyword("_EMISSION");
                _baseEmissionColors[i] = material.HasProperty(EmissionColorProperty)
                    ? material.GetColor(EmissionColorProperty)
                    : material.color;
            }

            _isInitialized = true;
            return true;
        }
    }
}
