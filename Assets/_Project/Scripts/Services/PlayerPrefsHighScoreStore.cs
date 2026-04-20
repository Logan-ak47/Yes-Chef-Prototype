using UnityEngine;

namespace YesChef.Services
{
    public sealed class PlayerPrefsHighScoreStore : IHighScoreStore
    {
        private const string HighScoreKey = "YesChef.HighScore";

        public int Load()
        {
            return PlayerPrefs.GetInt(HighScoreKey, 0);
        }

        public void Save(int value)
        {
            PlayerPrefs.SetInt(HighScoreKey, Mathf.Max(0, value));
            PlayerPrefs.Save();
        }
    }
}
