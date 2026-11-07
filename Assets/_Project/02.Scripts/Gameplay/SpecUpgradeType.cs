using System;

namespace OfficeWorker.Gameplay.Economy
{
    public enum SpecType
    {
        TypingSpeed = 0,     // 타자 속도 (탭당 업무 진행도 증가)
        MaxStamina,          // 강철 체력 (최대 체력 증가)
        PromotionEloquence,  // 처세술 (모든 직급 기본 승진 확률 증가)
        BossPatience,        // 눈치력 (상사 업무 방치 제한시간 증가)
        Leadership,          // 부하 조련술 (부하직원 업무 효율 및 스트레스 저항)
        SurvivalInstinct     // 사직서 방어 (해고 위기 시 시말서 제출로 1회 생존 확률)
    }

    [Serializable]
    public class SpecUpgradeInfo
    {
        public SpecType Type;
        public string SpecName;
        public int CurrentLevel;
        public int MaxLevel;
        public long BaseUpgradeCost;
        public float CostMultiplier;

        public SpecUpgradeInfo(SpecType type, string name, int maxLevel, long baseCost, float multiplier)
        {
            Type = type;
            SpecName = name;
            CurrentLevel = 0;
            MaxLevel = maxLevel;
            BaseUpgradeCost = baseCost;
            CostMultiplier = multiplier;
        }

        public long GetNextUpgradeCost()
        {
            if (CurrentLevel >= MaxLevel) return -1;
            return (long)(BaseUpgradeCost * Math.Pow(CostMultiplier, CurrentLevel));
        }
    }
}
