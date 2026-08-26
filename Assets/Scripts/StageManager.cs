using System;
using UnityEngine;

public class StageManager : SingletonBase<StageManager>
{
    [Header("스테이지 설정")] // 각 스테이지가 시작 되는 시간을 0초 60초 150초
    [SerializeField] private float[] _stageStartTimes = { 0f, 60f, 150f };

    private int _currentStage = 0;

    public int CurrentStage
    {
        get { return _currentStage; }
    }

    public event Action<int> OnStageChanged;

    private void OnEnable()
    {
        TimeManager.Instance.OnTimeChanged += HandleTimeChanged;
    }

    private void OnDisable()
    {
        TimeManager.Instance.OnTimeChanged -= HandleTimeChanged;
    }

    private void HandleTimeChanged(float elapsedTIme)
    {
        int nextStage = _currentStage;


        // 프레임 드랍등으로 인해 오류가 생길 것을 방지하기 위해 
        // 추후 스테이지가 더 많아지면 수정할 것
        for (int i = 0; i < _stageStartTimes.Length; i++) //지금 시간 기준으로 도달 가능한 가장 높은 스테이지 찾기
        {
            if (elapsedTIme >= _stageStartTimes[i])
            {
                nextStage = i;
            }
        }

        if (nextStage != _currentStage)
        {
            _currentStage = nextStage;
            Debug.Log($"[StageManager] 스테이지 {_currentStage}로 변경 (경과시간: {elapsedTIme}초");
            OnStageChanged?.Invoke(_currentStage);
        }
    }
}
