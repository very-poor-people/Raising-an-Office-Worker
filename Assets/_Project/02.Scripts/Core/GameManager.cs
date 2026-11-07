using System;
using OfficeWorker.Gameplay.Career;
using OfficeWorker.Gameplay.Dismissal;
using OfficeWorker.Gameplay.Economy;
using OfficeWorker.Gameplay.Work;

namespace OfficeWorker.Core
{
    public enum GameState
    {
        Title,
        OfficeLoop,
        PromotionWait,
        DismissalGameOver,
        MetaUpgradeShop
    }

    public class GameManager
    {
        public GameState CurrentState { get; private set; }
        public long Money { get; private set; }

        public PlayerCareer Career { get; }
        public PlayerStamina Stamina { get; }
        public TaskController Tasks { get; }
        public DismissalController Dismissal { get; }
        public SpecUpgradeManager Specs { get; }

        public event Action<GameState> OnStateChanged;
        public event Action<long> OnMoneyChanged;

        public GameManager()
        {
            Career = new PlayerCareer();
            Stamina = new PlayerStamina();
            Tasks = new TaskController();
            Dismissal = new DismissalController();
            Specs = new SpecUpgradeManager();

            CurrentState = GameState.Title;
            Money = 0;

            BindEvents();
        }

        private void BindEvents()
        {
            // 1. 업무 완료 시 보상
            Tasks.OnTaskCompleted += task =>
            {
                Career.AddExp(task.RewardExp);
                AddMoney(task.RewardMoney);
            };

            // 2. 업무 컨트롤러 해고 사유 (서류 과적, 상사 무시)
            Tasks.OnDismissalTriggered += reasonKey =>
            {
                if (reasonKey == "D05_DeskOverflow")
                    TriggerDismissal(DismissalId.D05_DeskOverflow);
                else if (reasonKey == "D06_BossIgnored")
                    TriggerDismissal(DismissalId.D06_BossIgnored);
            };

            // 3. 체력 고갈 해고
            Stamina.OnExhaustedDismissal += () =>
            {
                TriggerDismissal(DismissalId.D04_BurnOut);
            };

            // 4. 승진 실패 누적 한도 초과 해고
            Career.OnDismissalTriggered += rank =>
            {
                DismissalId id = rank switch
                {
                    CareerRank.Intern => DismissalId.D01_InternFail,
                    CareerRank.ContractWorker => DismissalId.D02_ContractFail,
                    _ => DismissalId.D03_FullTimeFail
                };
                TriggerDismissal(id);
            };

            // 5. 해고 발생 시 퇴직금 입금
            Dismissal.OnDismissalOccurred += (info, severance) =>
            {
                AddMoney(severance);
                SetState(GameState.DismissalGameOver);
            };
        }

        public void StartOfficeWork()
        {
            Tasks.ClearAll();
            Stamina.UpgradeMaxStamina(GameDefines.DefaultMaxStamina + Specs.GetBonusStamina());
            Stamina.Reset();
            SetState(GameState.OfficeLoop);
        }

        /// <summary>
        /// 화면 탭 이벤트: 작업 진행 가속 및 체력 소모
        /// </summary>
        public void HandleTap(float faintDiceRoll = 1.0f)
        {
            if (CurrentState != GameState.OfficeLoop) return;

            float tapPower = Specs.GetTapPower();
            Tasks.TapWork(tapPower);

            // 탭당 체력 소모 (1f)
            Stamina.Consume(1.0f, faintDiceRoll);
        }

        public void Tick(float deltaTime)
        {
            if (CurrentState != GameState.OfficeLoop) return;
            Tasks.UpdateTime(deltaTime);
        }

        public bool TryPromotion(float diceRoll)
        {
            if (!Career.IsEligibleForPromotion()) return false;

            int eloquence = Specs.GetEloquenceLevel();
            float collectionRate = Dismissal.GetCollectionRate();

            return Career.TryPromotion(diceRoll, eloquence, collectionRate);
        }

        public void TriggerDismissal(DismissalId id)
        {
            // 사직서 방어 스펙 체크
            // shortcut: 간단한 확률 주사위는 인자로 전달받지 않고 내부 난수 또는 0 방어로 처리, upgrade: DiceProvider DI 주입
            float survivalChance = Specs.GetSurvivalChance();
            if (survivalChance > 0f)
            {
                // 생존 성공 로직이 확장될 수 있음
            }

            Tasks.ClearAll();
            Dismissal.TriggerDismissal(id, Career.CurrentRank, Career.DaysWorked);
        }

        public void Rehire()
        {
            Career.ResetForRehire();
            StartOfficeWork();
        }

        public void AddMoney(long amount)
        {
            if (amount <= 0) return;
            Money += amount;
            OnMoneyChanged?.Invoke(Money);
        }

        public bool BuySpecUpgrade(SpecType type)
        {
            long currentMoney = Money;
            if (Specs.TryUpgrade(type, ref currentMoney))
            {
                Money = currentMoney;
                OnMoneyChanged?.Invoke(Money);
                return true;
            }
            return false;
        }

        public void SetState(GameState newState)
        {
            if (CurrentState == newState) return;
            CurrentState = newState;
            OnStateChanged?.Invoke(CurrentState);
        }
    }
}
