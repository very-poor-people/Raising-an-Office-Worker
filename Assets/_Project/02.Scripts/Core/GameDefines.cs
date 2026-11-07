using System;
using OfficeWorker.Gameplay.Career;

namespace OfficeWorker.Core
{
    public static class GameDefines
    {
        public const float WorkHoursStart = 9f;    // 09:00 출근
        public const float WorkHoursEnd = 18f;     // 18:00 정규 퇴근
        public const float OvertimeMax = 24f;      // 24:00 강제 야근 종료

        public const float DefaultMaxStamina = 100f;
        public const float StaminaFaintChance = 0.30f; // 체력 0 도달 시 퇴사(해고) 확률 30%

        /// <summary>
        /// 퇴직금 산출 공식:
        /// 인턴: 0원 (수습 기간)
        /// 계약직 이상: 직급 기본급 * 근속일수 * (1 + 도감 수집 보너스 비율)
        /// </summary>
        public static long CalculateSeverancePay(CareerRank rank, int daysWorked, float collectionBonusRatio = 0f)
        {
            if (rank == CareerRank.Intern)
            {
                return 0;
            }

            long baseDaily = rank.GetConfig().DailyBaseSalary;
            double total = baseDaily * Math.Max(1, daysWorked) * (1.0 + collectionBonusRatio);
            return (long)total;
        }

        /// <summary>
        /// 실제 승진 확률 계산 공식:
        /// 기본 확률 + 스펙 보너스(처세술) + 도감 수집 보너스
        /// </summary>
        public static float CalculatePromotionChance(CareerRank rank, int eloquenceLevel, float collectionRate)
        {
            float baseChance = rank.GetConfig().BasePromotionChance;
            float eloquenceBonus = eloquenceLevel * 0.01f; // 레벨당 +1%
            float collectionBonus = (float)Math.Floor(collectionRate / 0.10f) * 0.01f; // 10%당 +1%

            return Math.Clamp(baseChance + eloquenceBonus + collectionBonus, 0.001f, 1.0f);
        }
    }
}
