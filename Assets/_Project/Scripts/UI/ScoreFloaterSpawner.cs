using UnityEngine;
using YesChef.Core.Channels;

namespace YesChef.UI
{
    public class ScoreFloaterSpawner : MonoBehaviour
    {
        [SerializeField] private OrderCompletedChannel _orderCompletedChannel;
        [SerializeField] private ScoreFloater _scoreFloaterPrefab;
        [SerializeField] private Vector3 _spawnOffset = new Vector3(0f, 1.8f, 0f);

        private void OnEnable()
        {
            if (_orderCompletedChannel != null)
            {
                _orderCompletedChannel.OnRaised += HandleOrderCompleted;
            }
        }

        private void OnDisable()
        {
            if (_orderCompletedChannel != null)
            {
                _orderCompletedChannel.OnRaised -= HandleOrderCompleted;
            }
        }

        private void HandleOrderCompleted(OrderCompletedData data)
        {
            if (_scoreFloaterPrefab == null)
            {
                return;
            }

            ScoreFloater floater = Instantiate(_scoreFloaterPrefab, data.worldPos + _spawnOffset, Quaternion.identity);
            floater.Play(data.scoreDelta, data.wasNegative);
        }
    }
}
