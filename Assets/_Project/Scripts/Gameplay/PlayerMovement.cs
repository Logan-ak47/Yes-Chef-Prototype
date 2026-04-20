using UnityEngine;
using UnityEngine.InputSystem;
using YesChef.Core;
using YesChef.Core.Channels;
using YesChef.Data;

namespace YesChef.Gameplay
{
    public class PlayerMovement : MonoBehaviour
    {
        private const float RotationSpeed = 720f;
        private const float StopRotationDamping = 12f;

        [SerializeField] private Rigidbody _rb;
        [SerializeField] private GameSettings _gameSettings;
        [SerializeField] private InputActionReference _moveAction;
        [SerializeField] private GameStateChannel _stateChannel;

        private bool _canMove;
        private Vector3 _lastMoveDirection = Vector3.forward;

        private void OnEnable()
        {
            _moveAction?.action.Enable();
            if (_stateChannel != null)
                _stateChannel.OnRaised += OnGameStateChanged;
        }

        private void OnDisable()
        {
            _moveAction?.action.Disable();
            if (_stateChannel != null)
                _stateChannel.OnRaised -= OnGameStateChanged;
        }

        private void OnGameStateChanged(GameState oldState, GameState newState)
        {
            _canMove = newState == GameState.Running;
            if (!_canMove && _rb != null)
            {
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
                _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, StopRotationDamping * Time.fixedDeltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (_rb == null) return;

            if (!_canMove)
            {
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
                _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, StopRotationDamping * Time.fixedDeltaTime);
                return;
            }

            Vector2 input = _moveAction?.action.ReadValue<Vector2>() ?? Vector2.zero;
            Vector3 moveDir = new Vector3(input.x, 0f, input.y);
            if (_gameSettings == null)
            {
                Debug.LogError("[PlayerMovement] GameSettings is not assigned.", this);
                _rb.velocity = new Vector3(0f, _rb.velocity.y, 0f);
                return;
            }

            float speed = _gameSettings.PlayerMoveSpeed;

            _rb.velocity = new Vector3(moveDir.x * speed, _rb.velocity.y, moveDir.z * speed);

            if (moveDir.sqrMagnitude > 0.01f)
            {
                _lastMoveDirection = moveDir.normalized;
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                _rb.rotation = Quaternion.RotateTowards(
                    _rb.rotation, targetRotation, RotationSpeed * Time.fixedDeltaTime);
                _rb.angularVelocity = Vector3.zero;
            }
            else
            {
                Quaternion settledRotation = Quaternion.Slerp(
                    _rb.rotation,
                    Quaternion.LookRotation(_lastMoveDirection),
                    StopRotationDamping * Time.fixedDeltaTime);
                _rb.MoveRotation(settledRotation);
                _rb.angularVelocity = Vector3.Lerp(_rb.angularVelocity, Vector3.zero, StopRotationDamping * Time.fixedDeltaTime);
            }
        }
    }
}
