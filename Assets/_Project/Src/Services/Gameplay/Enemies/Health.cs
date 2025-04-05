using System;
using UniRx;
using UnityEngine;

namespace Services.Gameplay.Enemies
{
    [Serializable]
    public class Health : IDisposable
    {
        public IReadOnlyReactiveProperty<int> Value => _value;
        
        [SerializeField] private ReactiveProperty<int> _value;
        private readonly int _maxValue;
        
        public bool IsAlive => _value.Value > 0;
        
        public Health(int maxValue, int initialValue)
        {
            if (initialValue < 0 || maxValue <= 0)
                throw new ArgumentException("Initial value and max value must be greater than zero.");

            _maxValue = maxValue;
            _value = new ReactiveProperty<int>(Math.Min(initialValue, maxValue));
        }

        public void Damage(int val)
        {
            if (val < 0)
                throw new ArgumentOutOfRangeException(nameof(val), "Damage value cannot be negative.");

            _value.Value = Math.Max(0, _value.Value - val);
        }

        public void Dispose()
        {
            _value?.Dispose();
        }
    }
}