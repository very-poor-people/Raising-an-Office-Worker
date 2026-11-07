using System;

namespace OfficeWorker.Gameplay.Work
{
    public enum TaskType
    {
        SimpleDocument = 0, // 기본 서류 정리 (가장 흔함, 낮은 난이도)
        Photocopy,          // 복사 및 제본 (빠른 탭 필요)
        ExcelSheet,         // 엑셀 수치 입력 (중간 난이도, 높은 EXP)
        PresentationDeck,   // PPT 프레젠테이션 (높은 난이도, 체력 소모 큼)
        UrgentBossTask      // 상사의 초긴급 보고서 (타임어택, 실패 시 분노 상승)
    }

    [Serializable]
    public class TaskItem
    {
        public TaskType Type;
        public string TaskName;
        public float RequiredProgress;  // 완료에 필요한 작업량 (예: 100f)
        public float CurrentProgress;   // 현재 진행된 작업량
        public float RewardExp;         // 완료 시 획득 경력치
        public long RewardMoney;        // 완료 시 획득 자금
        public float StaminaCostPerSec; // 초당 체력 소모량
        public float TimeoutSeconds;    // 방치 시 상사 분노 트리거 제한시간 (기본 30초)

        public bool IsCompleted => CurrentProgress >= RequiredProgress;
        public float ProgressNormalized => Math.Clamp(CurrentProgress / RequiredProgress, 0f, 1f);

        public TaskItem(TaskType type, string name, float requiredProgress, float exp, long money, float staminaDrain, float timeout = 30f)
        {
            Type = type;
            TaskName = name;
            RequiredProgress = requiredProgress;
            CurrentProgress = 0f;
            RewardExp = exp;
            RewardMoney = money;
            StaminaCostPerSec = staminaDrain;
            TimeoutSeconds = timeout;
        }
    }
}
