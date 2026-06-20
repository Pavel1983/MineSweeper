public interface IBoardHud
{
    void ResetHud(int minesCount);
    void UpdateFlaggedCount(int flaggedCount);
    void UpdateElapsedTime(float elapsedSeconds);
}
