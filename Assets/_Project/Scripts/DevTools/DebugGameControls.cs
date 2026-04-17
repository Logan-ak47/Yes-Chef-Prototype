using UnityEngine;
using YesChef.Core;

namespace YesChef.DevTools
{
    /// <summary>
    /// Disposable OnGUI debug panel — drop on any GameObject alongside GameManager.
    /// </summary>
    public class DebugGameControls : MonoBehaviour
    {
        [SerializeField] private GameManager _gameManager;

        private void OnGUI()
        {
            if (_gameManager == null) return;

            GUILayout.BeginArea(new Rect(10, 10, 180, 160));
            GUILayout.Label($"State : {_gameManager.CurrentState}");
            GUILayout.Label($"Time  : {_gameManager.TimeRemaining:F1}s");
            GUILayout.Space(6);
            if (GUILayout.Button("Start Game"))  _gameManager.StartGame();
            if (GUILayout.Button("Pause Game"))  _gameManager.PauseGame();
            if (GUILayout.Button("Resume Game")) _gameManager.ResumeGame();
            if (GUILayout.Button("End Game"))    _gameManager.EndGame();
            if (GUILayout.Button("Quit"))        _gameManager.QuitGame();
            GUILayout.EndArea();
        }
    }
}
