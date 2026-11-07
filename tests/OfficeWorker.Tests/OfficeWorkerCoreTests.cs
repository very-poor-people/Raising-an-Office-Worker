using Xunit;
using OfficeWorker.Core;
using OfficeWorker.Gameplay.Career;
using OfficeWorker.Gameplay.Dismissal;
using OfficeWorker.Gameplay.Economy;
using OfficeWorker.Gameplay.Work;

namespace OfficeWorker.Tests
{
    public class OfficeWorkerCoreTests
    {
        [Fact]
        public void Career_Promotion_Success_And_RankUp()
        {
            var career = new PlayerCareer(CareerRank.Intern);
            career.AddExp(30f); // 인턴 요구치 달성

            Assert.True(career.IsEligibleForPromotion());

            // 주사위 0.1 (승진 확률 50%이므로 성공)
            bool result = career.TryPromotion(0.1f);

            Assert.True(result);
            Assert.Equal(CareerRank.ContractWorker, career.CurrentRank);
            Assert.Equal(0f, career.CurrentExp);
            Assert.Equal(0, career.ConsecutiveFailures);
        }

        [Fact]
        public void Career_Intern_OneFail_Triggers_Dismissal()
        {
            var career = new PlayerCareer(CareerRank.Intern);
            career.AddExp(30f);

            bool dismissed = false;
            career.OnDismissalTriggered += rank => dismissed = true;

            // 주사위 0.9 (승진 확률 50%이므로 실패)
            bool result = career.TryPromotion(0.9f);

            Assert.False(result);
            Assert.True(dismissed);
            Assert.Equal(1, career.ConsecutiveFailures);
        }

        [Fact]
        public void Career_ContractWorker_TwoFail_Allowance()
        {
            var career = new PlayerCareer(CareerRank.ContractWorker);
            career.AddExp(60f);

            int dismissedCount = 0;
            career.OnDismissalTriggered += rank => dismissedCount++;

            // 첫 번째 실패: 계약직은 2회까지 생존
            career.TryPromotion(0.9f);
            Assert.Equal(0, dismissedCount);
            Assert.Equal(1, career.ConsecutiveFailures);

            // 다시 경력 쌓고 두 번째 실패
            career.AddExp(60f);
            career.TryPromotion(0.9f);
            Assert.Equal(1, dismissedCount);
            Assert.Equal(2, career.ConsecutiveFailures);
        }

        [Fact]
        public void Stamina_BurnOut_Triggers_Dismissal()
        {
            var stamina = new PlayerStamina(100f);
            bool dismissed = false;
            stamina.OnExhaustedDismissal += () => dismissed = true;

            // 체력 100 소모, 주사위 0.2 (FaintChance 0.3 이하이므로 퇴사 발동)
            stamina.Consume(100f, 0.2f);

            Assert.True(stamina.IsExhausted);
            Assert.True(dismissed);
        }

        [Fact]
        public void Task_Accept_And_TapWork_CompletesTask_And_AwardsExpMoney()
        {
            var gm = new GameManager();
            gm.StartOfficeWork();

            var task = new TaskItem(TaskType.SimpleDocument, "기본 서류", 20f, 15f, 5000, 1f);
            gm.Tasks.SpawnSuperiorTask(task);

            Assert.Single(gm.Tasks.PendingSuperiorTasks);
            bool accepted = gm.Tasks.AcceptTask(task);
            Assert.True(accepted);
            Assert.Empty(gm.Tasks.PendingSuperiorTasks);
            Assert.Single(gm.Tasks.ActiveTasks);

            // 탭 2회 (기본 탭 파워 10f * 2 = 20f -> 완료)
            gm.HandleTap();
            Assert.False(task.IsCompleted);
            gm.HandleTap();

            Assert.True(task.IsCompleted);
            Assert.Empty(gm.Tasks.ActiveTasks);
            Assert.Equal(15f, gm.Career.CurrentExp);
            Assert.Equal(5000, gm.Money);
        }

        [Fact]
        public void Task_Overflow_Triggers_DeskOverflowDismissal()
        {
            var gm = new GameManager();
            gm.StartOfficeWork();

            DismissalInfo firedInfo = null;
            gm.Dismissal.OnDismissalOccurred += (info, severance) => firedInfo = info;

            // 5개까지는 책상에 수용 가능
            for (int i = 0; i < 5; i++)
            {
                var t = new TaskItem(TaskType.SimpleDocument, $"Task {i}", 100f, 10f, 1000, 1f);
                gm.Tasks.SpawnSuperiorTask(t);
                Assert.True(gm.Tasks.AcceptTask(t));
            }

            // 6번째 수락 시 책상 과적으로 D05 해고 트리거
            var sixthTask = new TaskItem(TaskType.SimpleDocument, "Task 6", 100f, 10f, 1000, 1f);
            gm.Tasks.SpawnSuperiorTask(sixthTask);
            bool accepted = gm.Tasks.AcceptTask(sixthTask);

            Assert.False(accepted);
            Assert.NotNull(firedInfo);
            Assert.Equal(DismissalId.D05_DeskOverflow, firedInfo.Id);
            Assert.Equal(GameState.DismissalGameOver, gm.CurrentState);
        }

        [Fact]
        public void Task_Timeout_Triggers_BossIgnoredDismissal()
        {
            var gm = new GameManager();
            gm.StartOfficeWork();

            DismissalInfo firedInfo = null;
            gm.Dismissal.OnDismissalOccurred += (info, severance) => firedInfo = info;

            var task = new TaskItem(TaskType.SimpleDocument, "방치될 서류", 100f, 10f, 1000, 1f, timeout: 5f);
            gm.Tasks.SpawnSuperiorTask(task);

            // 6초 경과 -> 타임아웃 발동
            gm.Tick(6.0f);

            Assert.NotNull(firedInfo);
            Assert.Equal(DismissalId.D06_BossIgnored, firedInfo.Id);
            Assert.Equal(GameState.DismissalGameOver, gm.CurrentState);
        }

        [Fact]
        public void SeverancePay_Intern_Is_Zero_And_Contract_Is_Positive()
        {
            long internPay = GameDefines.CalculateSeverancePay(CareerRank.Intern, 10);
            Assert.Equal(0, internPay);

            long contractPay = GameDefines.CalculateSeverancePay(CareerRank.ContractWorker, 10);
            // 50,000 * 10 = 500,000
            Assert.Equal(500000, contractPay);
        }

        [Fact]
        public void SpecUpgrade_IncreasesTapPower_And_DecreasesMoney()
        {
            var gm = new GameManager();
            gm.AddMoney(20000); // 20,000원 충전

            float initialTapPower = gm.Specs.GetTapPower();
            Assert.Equal(10f, initialTapPower);

            // 타자 속도 1레벨 업그레이드 비용: 10,000원
            bool upgraded = gm.BuySpecUpgrade(SpecType.TypingSpeed);

            Assert.True(upgraded);
            Assert.Equal(10000, gm.Money);
            Assert.Equal(11.5f, gm.Specs.GetTapPower()); // 10f + 1.5f
        }

        [Fact]
        public void GameManager_Rehire_Resets_Rank_And_Keeps_Money()
        {
            var gm = new GameManager();
            gm.StartOfficeWork();
            gm.AddMoney(50000);

            // 해고 트리거
            gm.TriggerDismissal(DismissalId.D01_InternFail);
            Assert.Equal(GameState.DismissalGameOver, gm.CurrentState);

            // 재입사
            gm.Rehire();

            Assert.Equal(CareerRank.Intern, gm.Career.CurrentRank);
            Assert.Equal(0f, gm.Career.CurrentExp);
            Assert.Equal(50000, gm.Money); // 자금 유지
            Assert.Equal(GameState.OfficeLoop, gm.CurrentState);
        }
    }
}
