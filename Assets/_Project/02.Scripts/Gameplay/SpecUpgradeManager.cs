using System;
using System.Collections.Generic;

namespace OfficeWorker.Gameplay.Economy
{
    public class SpecUpgradeManager
    {
        private readonly Dictionary<SpecType, SpecUpgradeInfo> _specs = new();

        public IReadOnlyDictionary<SpecType, SpecUpgradeInfo> Specs => _specs;

        public event Action<SpecType, int> OnSpecUpgraded; // type, newLevel

        public SpecUpgradeManager()
        {
            InitializeSpecs();
        }

        private void InitializeSpecs()
        {
            _specs[SpecType.TypingSpeed] = new SpecUpgradeInfo(SpecType.TypingSpeed, "타자 속도", 20, 10000, 1.25f);
            _specs[SpecType.MaxStamina] = new SpecUpgradeInfo(SpecType.MaxStamina, "강철 체력", 30, 15000, 1.20f);
            _specs[SpecType.PromotionEloquence] = new SpecUpgradeInfo(SpecType.PromotionEloquence, "처세술", 15, 30000, 1.35f);
            _specs[SpecType.BossPatience] = new SpecUpgradeInfo(SpecType.BossPatience, "눈치력", 15, 20000, 1.30f);
            _specs[SpecType.Leadership] = new SpecUpgradeInfo(SpecType.Leadership, "부하 조련술", 10, 50000, 1.40f);
            _specs[SpecType.SurvivalInstinct] = new SpecUpgradeInfo(SpecType.SurvivalInstinct, "사직서 방어", 10, 100000, 1.50f);
        }

        public bool TryUpgrade(SpecType type, ref long currentMoney)
        {
            if (!_specs.TryGetValue(type, out var spec)) return false;

            long cost = spec.GetNextUpgradeCost();
            if (cost < 0 || currentMoney < cost) return false;

            currentMoney -= cost;
            spec.CurrentLevel++;
            OnSpecUpgraded?.Invoke(type, spec.CurrentLevel);
            return true;
        }

        public int GetLevel(SpecType type)
        {
            return _specs.TryGetValue(type, out var spec) ? spec.CurrentLevel : 0;
        }

        /// <summary>
        /// 타자 탭 파워 계산: 기본 10f + 레벨당 1.5f (15%)
        /// </summary>
        public float GetTapPower()
        {
            return 10f + (GetLevel(SpecType.TypingSpeed) * 1.5f);
        }

        /// <summary>
        /// 추가 최대 체력 계산: 레벨당 +10
        /// </summary>
        public float GetBonusStamina()
        {
            return GetLevel(SpecType.MaxStamina) * 10f;
        }

        /// <summary>
        /// 처세술 레벨 (승진 확률 보너스 계산용)
        /// </summary>
        public int GetEloquenceLevel()
        {
            return GetLevel(SpecType.PromotionEloquence);
        }

        /// <summary>
        /// 사직서 방어 확률 (0.0f ~ 0.20f)
        /// </summary>
        public float GetSurvivalChance()
        {
            return GetLevel(SpecType.SurvivalInstinct) * 0.02f;
        }
    }
}
