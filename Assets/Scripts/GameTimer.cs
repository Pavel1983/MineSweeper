using System;

public class GameTimer
{
    public event Action<float> ElapsedTimeChanged;

    public float ElapsedSeconds { get; private set; }
    public bool IsRunning { get; private set; }

    public void Reset()
    {
        ElapsedSeconds = 0f;
        IsRunning = false;
    }

    public void Start()
    {
        IsRunning = true;
    }

    public void Stop()
    {
        IsRunning = false;
    }

    public void Tick(float deltaTime)
    {
        if (!IsRunning)
        {
            return;
        }

        ElapsedSeconds += deltaTime;
        ElapsedTimeChanged?.Invoke(ElapsedSeconds);
    }
}
