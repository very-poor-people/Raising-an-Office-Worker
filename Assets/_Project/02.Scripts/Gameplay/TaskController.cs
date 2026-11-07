using System;
using System.Collections.Generic;

namespace OfficeWorker.Gameplay.Work
{
    public class TaskController
    {
        public const int MaxDeskTaskCount = 5; // 책상 서류 과적 한도

        private readonly List<TaskItem> _activeTasks = new();
        private readonly List<TaskItem> _pendingSuperiorTasks = new(); // 상사 머리 위에 뜬 업무

        public IReadOnlyList<TaskItem> ActiveTasks => _activeTasks;
        public IReadOnlyList<TaskItem> PendingSuperiorTasks => _pendingSuperiorTasks;

        public event Action<TaskItem> OnTaskCompleted;
        public event Action<string> OnDismissalTriggered; // 해고 사유 전달

        /// <summary>
        /// 상사가 새 업무를 발생시킴
        /// </summary>
        public void SpawnSuperiorTask(TaskItem task)
        {
            if (task == null) return;
            _pendingSuperiorTasks.Add(task);
        }

        /// <summary>
        /// 상사의 업무를 탭하여 주인공 책상으로 가져옴
        /// </summary>
        public bool AcceptTask(TaskItem task)
        {
            if (!_pendingSuperiorTasks.Contains(task)) return false;

            if (_activeTasks.Count >= MaxDeskTaskCount)
            {
                // 책상 과적으로 인한 압사 해고
                OnDismissalTriggered?.Invoke("D05_DeskOverflow");
                return false;
            }

            _pendingSuperiorTasks.Remove(task);
            _activeTasks.Add(task);
            return true;
        }

        /// <summary>
        /// 화면 탭 클릭 시 책상 맨 앞 업무 가속 처리
        /// </summary>
        public void TapWork(float tapPower)
        {
            if (_activeTasks.Count == 0) return;

            var currentTask = _activeTasks[0];
            currentTask.CurrentProgress += tapPower;

            if (currentTask.IsCompleted)
            {
                CompleteCurrentTask(currentTask);
            }
        }

        /// <summary>
        /// 틱/초당 시간 업데이트 (자동 진행 및 상사 방치 타이머)
        /// </summary>
        public void UpdateTime(float deltaTime, float autoWorkSpeed = 5f)
        {
            // 1. 책상 업무 자동 진행
            if (_activeTasks.Count > 0)
            {
                var currentTask = _activeTasks[0];
                currentTask.CurrentProgress += autoWorkSpeed * deltaTime;
                if (currentTask.IsCompleted)
                {
                    CompleteCurrentTask(currentTask);
                }
            }

            // 2. 상사 업무 방치 타이머 체크
            for (int i = _pendingSuperiorTasks.Count - 1; i >= 0; i--)
            {
                var task = _pendingSuperiorTasks[i];
                task.TimeoutSeconds -= deltaTime;
                if (task.TimeoutSeconds <= 0f)
                {
                    _pendingSuperiorTasks.RemoveAt(i);
                    // 30초 이상 방치 시 상사 부름 무시 해고
                    OnDismissalTriggered?.Invoke("D06_BossIgnored");
                    break;
                }
            }
        }

        private void CompleteCurrentTask(TaskItem task)
        {
            _activeTasks.Remove(task);
            OnTaskCompleted?.Invoke(task);
        }

        public void ClearAll()
        {
            _activeTasks.Clear();
            _pendingSuperiorTasks.Clear();
        }
    }
}
