#if UNITY_5_3_OR_NEWER
using UnityEngine;
using OfficeWorker.Gameplay.Career;
using OfficeWorker.Gameplay.Dismissal;
using OfficeWorker.Gameplay.Economy;
using OfficeWorker.Gameplay.Work;

namespace OfficeWorker.Core
{
    /// <summary>
    /// Unity 씬에서 게임 루프를 구동하고 OnGUI 디버그 화면을 제공하는 러너 컴포넌트
    /// </summary>
    public class OfficeGameRunner : MonoBehaviour
    {
        public GameManager Game { get; private set; }

        [Header("Spawn Settings")]
        [SerializeField] private float taskSpawnInterval = 5f;
        private float _spawnTimer;

        private void Awake()
        {
            Game = new GameManager();
            Game.StartOfficeWork();
        }

        private void Update()
        {
            if (Game == null || Game.CurrentState != GameState.OfficeLoop) return;

            Game.Tick(Time.deltaTime);

            // 주기적으로 상사 업무 스폰
            _spawnTimer += Time.deltaTime;
            if (_spawnTimer >= taskSpawnInterval)
            {
                _spawnTimer = 0f;
                var newTask = new TaskItem(TaskType.SimpleDocument, "일반 업무 서류", 50f, 10f, 5000, 1f, timeout: 25f);
                Game.Tasks.SpawnSuperiorTask(newTask);
            }
        }

        private void OnGUI()
        {
            if (Game == null) return;

            GUILayout.BeginArea(new Rect(20, 20, 360, 600), GUI.skin.box);
            GUILayout.Label($"<b>[내꿈은 정규직!] - 개발자 디버그 콘솔</b>");
            GUILayout.Label($"상태: {Game.CurrentState}");
            GUILayout.Label($"직급: {Game.Career.CurrentRank.GetConfig().TitleKorean} (실패: {Game.Career.ConsecutiveFailures}/{Game.Career.CurrentRank.GetConfig().MaxFailAllowance})");
            GUILayout.Label($"경력(EXP): {Game.Career.CurrentExp:F0} / {Game.Career.CurrentRank.GetConfig().RequiredExp}");
            GUILayout.Label($"체력: {Game.Stamina.CurrentStamina:F0} / {Game.Stamina.MaxStamina:F0}");
            GUILayout.Label($"소지금: {Game.Money:N0} 원");

            GUILayout.Space(10);

            if (Game.CurrentState == GameState.OfficeLoop)
            {
                if (GUILayout.Button("💻 업무 연타하기 (Tap Work)", GUILayout.Height(40)))
                {
                    Game.HandleTap(Random.value);
                }

                GUILayout.Space(5);
                GUILayout.Label($"상사 대기 업무: {Game.Tasks.PendingSuperiorTasks.Count}개");
                if (Game.Tasks.PendingSuperiorTasks.Count > 0)
                {
                    if (GUILayout.Button("📥 상사 업무 수락 (책상으로 가져오기)"))
                    {
                        Game.Tasks.AcceptTask(Game.Tasks.PendingSuperiorTasks[0]);
                    }
                }

                GUILayout.Label($"책상 작업 서류: {Game.Tasks.ActiveTasks.Count} / {TaskController.MaxDeskTaskCount}");
                if (Game.Tasks.ActiveTasks.Count > 0)
                {
                    var t = Game.Tasks.ActiveTasks[0];
                    GUILayout.Label($"진행도: {t.CurrentProgress:F0} / {t.RequiredProgress:F0} ({t.ProgressNormalized * 100:F0}%)");
                }

                GUILayout.Space(5);
                if (Game.Career.IsEligibleForPromotion())
                {
                    GUI.backgroundColor = Color.green;
                    if (GUILayout.Button("⭐ 승진 심사 도전!", GUILayout.Height(40)))
                    {
                        Game.TryPromotion(Random.value);
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
            else if (Game.CurrentState == GameState.DismissalGameOver)
            {
                GUI.backgroundColor = Color.red;
                GUILayout.Label("<b>[권 고 사 직]</b>", GUI.skin.label);
                GUI.backgroundColor = Color.white;
                if (GUILayout.Button("🔄 재입사하기 (새 인턴으로 시작)", GUILayout.Height(40)))
                {
                    Game.Rehire();
                }
            }

            GUILayout.Space(10);
            GUILayout.Label("<b>스펙 업그레이드</b>");
            foreach (var pair in Game.Specs.Specs)
            {
                var spec = pair.Value;
                long cost = spec.GetNextUpgradeCost();
                string costText = cost > 0 ? $"{cost:N0}원" : "MAX";
                if (GUILayout.Button($"{spec.SpecName} (Lv.{spec.CurrentLevel}) - {costText}"))
                {
                    Game.BuySpecUpgrade(spec.Type);
                }
            }

            GUILayout.EndArea();
        }
    }
}
#endif
