using UnityEngine;
using YesChef.Core;
using YesChef.Core.Channels;
using YesChef.Data;

namespace YesChef.Gameplay.Orders
{
    public class OrderService : MonoBehaviour
    {
        [SerializeField] private GameStateChannel _gameStateChannel;
        [SerializeField] private CustomerWindow[] _windows = new CustomerWindow[4];

        private OrderGenerator _generator;
        private bool _acceptingRequests;

        public static OrderService Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[OrderService] Duplicate OrderService found, destroying newest instance.", this);
                Destroy(this);
                return;
            }

            Instance = this;
            BindWindows();
            EnsureGenerator();
            ClearAllWindows();
        }

        private void OnEnable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised += HandleGameStateChanged;
            }
        }

        private void OnDisable()
        {
            if (_gameStateChannel != null)
            {
                _gameStateChannel.OnRaised -= HandleGameStateChanged;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void RequestNextOrder(CustomerWindow window)
        {
            if (!_acceptingRequests || window == null)
            {
                return;
            }

            if (!EnsureGenerator())
            {
                return;
            }

            window.SetOrder(_generator.GenerateRandom(Time.time));
        }

        private void HandleGameStateChanged(GameState previousState, GameState nextState)
        {
            if (nextState == GameState.Running && previousState != GameState.Paused)
            {
                _acceptingRequests = true;
                AssignFreshOrdersToAllWindows();
                return;
            }

            if (nextState == GameState.Menu || nextState == GameState.GameOver)
            {
                _acceptingRequests = false;
                ClearAllWindows();
            }
        }

        private void AssignFreshOrdersToAllWindows()
        {
            if (!EnsureGenerator())
            {
                return;
            }

            foreach (CustomerWindow window in _windows)
            {
                if (window == null)
                {
                    continue;
                }

                window.SetOrder(_generator.GenerateRandom(Time.time));
            }
        }

        private void ClearAllWindows()
        {
            foreach (CustomerWindow window in _windows)
            {
                if (window == null)
                {
                    continue;
                }

                window.ClearOrder();
            }
        }

        private void BindWindows()
        {
            foreach (CustomerWindow window in _windows)
            {
                if (window != null)
                {
                    window.BindOrderService(this);
                }
            }
        }

        private bool EnsureGenerator()
        {
            if (_generator != null)
            {
                return true;
            }

            OrderSettings settings = ResolveSettings();
            if (settings == null)
            {
                Debug.LogError("[OrderService] Could not resolve OrderSettings from the configured windows.", this);
                return false;
            }

            _generator = new OrderGenerator(settings);
            return true;
        }

        private OrderSettings ResolveSettings()
        {
            foreach (CustomerWindow window in _windows)
            {
                if (window != null && window.Settings != null)
                {
                    return window.Settings;
                }
            }

            return null;
        }
    }
}
