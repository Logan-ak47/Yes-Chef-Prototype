namespace YesChef.Services
{
    public interface IHighScoreStore
    {
        int Load();
        void Save(int value);
    }
}
