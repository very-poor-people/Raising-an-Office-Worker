using System;

namespace OfficeWorker.Gameplay.Work
{
    public class PlayerStamina
    {
        public float MaxStamina { get; private set; }
        public float CurrentStamina { get; private set; }
        public bool IsExhausted => CurrentStamina <= 0f;

        public event Action<float, float> OnStaminaChanged; // current, max
        public event Action OnExhaustedDismissal; // 체력 고갈로 인한 자진 퇴사 발생

        public PlayerStamina(float maxStamina = Core.GameDefines.DefaultMaxStamina)
        {
            MaxStamina = Math.Max(10f, maxStamina);
            CurrentStamina = MaxStamina;
        }

        public void Consume(float amount, float faintDiceRoll = 1.0f)
        {
            if (amount <= 0f) return;

            CurrentStamina = Math.Max(0f, CurrentStamina - amount);
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);

            if (CurrentStamina <= 0f)
            {
                // 체력 0 도달 시 30% 확률로 자진 퇴사 해고 발생
                if (faintDiceRoll <= Core.GameDefines.StaminaFaintChance)
                {
                    OnExhaustedDismissal?.Invoke();
                }
            }
        }

        public void Recover(float amount)
        {
            if (amount <= 0f) return;
            CurrentStamina = Math.Min(MaxStamina, CurrentStamina + amount);
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }

        public void UpgradeMaxStamina(float newMax)
        {
            MaxStamina = Math.Max(10f, newMax);
            CurrentStamina = MaxStamina;
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }

        public void Reset()
        {
            CurrentStamina = MaxStamina;
            OnStaminaChanged?.Invoke(CurrentStamina, MaxStamina);
        }
    }
}
