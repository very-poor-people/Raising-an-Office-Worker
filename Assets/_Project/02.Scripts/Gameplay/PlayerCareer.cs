using System;

namespace OfficeWorker.Gameplay.Career
{
    public class PlayerCareer
    {
        public CareerRank CurrentRank { get; private set; }
        public float CurrentExp { get; private set; }
        public int ConsecutiveFailures { get; private set; }
        public int DaysWorked { get; private set; }

        public event Action<CareerRank> OnPromoted;
        public event Action<int, int> OnPromotionFailed; // currentFailures, maxAllowance
        public event Action<CareerRank> OnDismissalTriggered; // rank when fired

        public PlayerCareer(CareerRank initialRank = CareerRank.Intern)
        {
            CurrentRank = initialRank;
            CurrentExp = 0f;
            ConsecutiveFailures = 0;
            DaysWorked = 1;
        }

        public void AddExp(float amount)
        {
            if (amount <= 0f) return;
            CurrentExp += amount;
        }

        public bool IsEligibleForPromotion()
        {
            if (CurrentRank == CareerRank.President) return false;
            return CurrentExp >= CurrentRank.GetConfig().RequiredExp;
        }

        /// <summary>
        /// 승진 심사 시도
        /// </summary>
        /// <param name="diceRoll">0.0f ~ 1.0f 사이 주사위 값</param>
        /// <param name="eloquenceLevel">처세술 스펙 레벨</param>
        /// <param name="collectionRate">도감 수집률 (0.0f ~ 1.0f)</param>
        public bool TryPromotion(float diceRoll, int eloquenceLevel = 0, float collectionRate = 0f)
        {
            if (!IsEligibleForPromotion()) return false;

            float winChance = Core.GameDefines.CalculatePromotionChance(CurrentRank, eloquenceLevel, collectionRate);

            if (diceRoll <= winChance)
            {
                // 승진 성공
                CurrentRank++;
                CurrentExp = 0f;
                ConsecutiveFailures = 0;
                OnPromoted?.Invoke(CurrentRank);
                return true;
            }
            else
            {
                // 승진 실패
                CurrentExp = 0f;
                ConsecutiveFailures++;
                int maxAllowance = CurrentRank.GetConfig().MaxFailAllowance;
                OnPromotionFailed?.Invoke(ConsecutiveFailures, maxAllowance);

                if (ConsecutiveFailures >= maxAllowance)
                {
                    OnDismissalTriggered?.Invoke(CurrentRank);
                }
                return false;
            }
        }

        public void IncrementDay()
        {
            DaysWorked++;
        }

        public void ResetForRehire()
        {
            CurrentRank = CareerRank.Intern;
            CurrentExp = 0f;
            ConsecutiveFailures = 0;
            DaysWorked = 1;
        }
    }
}
