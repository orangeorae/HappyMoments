using System;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    private const float Wave1TimeLimit = 30f;
    private const float Wave2TimeLimit = 45f;
    private const float BossWaveTImeLimit = 60f;
    private const int LastStage = 4;

    private int _currentStage = 1;
    private int _currentWave = 1;
    private float _lastSpendTime;
    private float _waveStartTime;
    private bool _isGameEnd;

    public int CurrentStage { get { return _currentStage; } }
    public int CurrentWave { get { return _currentWave; } }

    public event Action<int> OnStageChanged;
    public event Action<int> OnWaveChanged;
    public event Action OnBossTimeOver;

    private void Start()
    {
        TimeManager.Instance.OnTimeChanged += HandleTimeChanged;
    }

    private void OnDisable()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.OnTimeChanged -= HandleTimeChanged;
        }
    }

    private void HandleTimeChanged(float elapsedTIme)
    {
        if (_isGameEnd)
        {
            return;
        }

        _lastSpendTime = elapsedTIme;
        float waveTime = _lastSpendTime - _waveStartTime;

        if (_currentWave == 1 && waveTime >= Wave1TimeLimit)
        {
            MoveNextWave();
        }

        else if (_currentWave == 2 && waveTime >= Wave2TimeLimit)
        {
            MoveNextWave();
        }

        else if (_currentWave ==3 && waveTime >= BossWaveTImeLimit)
        {
            _isGameEnd = true;
            Debug.Log("[StageManager] 보스 제한 시간 초과");

            if(OnBossTimeOver != null)
            {
                OnBossTimeOver();
            }
        }
    }
    private void MoveNextWave()
    {
        _currentWave++;
        _waveStartTime = _lastSpendTime;

        if(OnWaveChanged != null)
        {
            OnWaveChanged(_currentWave);
        }

        Debug.Log("[StageManager] Stage " + _currentStage + " Wave " + _currentWave);
    }

    public void BossDefeated()
    {
        if(_isGameEnd || _currentWave!= 3)
        {
            return;
        }

        if(_currentStage >= LastStage)
        {
            _isGameEnd = true;
            Debug.Log("[StageManager] 모든 Stage 완료");
            return;
        }

        _currentStage++;
        _currentWave = 1;
        _waveStartTime = _lastSpendTime;

        if(OnStageChanged != null)
        {
            OnStageChanged(_currentStage);
        }
        Debug.Log("[StageManager] Stage " + _currentStage + " Wave 1");
    }
}
