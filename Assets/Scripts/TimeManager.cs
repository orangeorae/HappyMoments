using System;
using UnityEngine;

public class TimeManager : SingletonBase<TimeManager>
{

    private float _elapsedTime = 0f; // 게임 시작 후 흐른 시간(초)

    private bool _isRunning = false;

    public float ElapsedTime
    {
        get { return _elapsedTime; }
    }

    public bool IsRunning
    {
        get { return _isRunning; }
    }

    public event Action<float> OnTimeChanged;

    private void Update()
    {
        if (_isRunning == false) 
        {
            return;
        }

        _elapsedTime += UnityEngine.Time.deltaTime;

        if(OnTimeChanged != null)
        {
            OnTimeChanged(_elapsedTime);
        }

    }

    public void StartTimer()
    {
        _isRunning = true;
    }

    public void StopTimer()
    {
        _isRunning = false;
    }

    public void ResetTimer()
    {
        _elapsedTime = 0;
    }


}
