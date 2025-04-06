using System;
using System.Collections.Generic;
using UniRx;

namespace Services.Gameplay.Upgrading
{
    public struct LevelData<T>
    {
        public int Level { get; private set; }
        public T Value { get; private set; }
        public int Cost { get; private set; }

        public LevelData(int level, T value, int cost)
        {
            Level = level;
            Value = value;
            Cost = cost;
        }
    }

    public class UpgradeValue<T> : IDisposable
    {
        private readonly ReactiveProperty<int> _currentLevel;
        private readonly int _maxLevel;
        private readonly List<LevelData<T>> _levels;

        public IReadOnlyReactiveProperty<int> currentLevel => _currentLevel;
        public IReadOnlyReactiveProperty<T> currentValue { get; }
        public IReadOnlyReactiveProperty<T> nextValue { get; } // Добавлено nextValue
        public IReadOnlyReactiveProperty<int> currentCost { get; }
        public IReadOnlyReactiveProperty<int> nextCost { get; }
        public int MaxLevel => _maxLevel;
        public IReadOnlyReactiveProperty<bool> isMaxLevel { get; }

        public UpgradeValue(List<LevelData<T>> levels)
        {
            if (levels == null || levels.Count == 0)
                throw new ArgumentException("Levels list cannot be null or empty.");

            _levels = levels;
            _maxLevel = levels.Count;
            
            _currentLevel = new ReactiveProperty<int>(1);
            currentValue = _currentLevel.Select(level => _levels[level - 1].Value).ToReactiveProperty();
            nextValue = _currentLevel.Select(level => level < _maxLevel ? _levels[level].Value : default(T)).ToReactiveProperty(); // Реализация nextValue
            currentCost = _currentLevel.Select(level => _levels[level - 1].Cost).ToReactiveProperty();
            nextCost = _currentLevel.Select(level => level < _maxLevel ? _levels[level].Cost : -1).ToReactiveProperty();
            isMaxLevel = _currentLevel.Select(level => level >= _maxLevel).ToReactiveProperty();
        }
        
        public bool NextValue()
        {
            if (_currentLevel.Value < _maxLevel)
            {
                _currentLevel.Value++;
                return true;
            }

            return false;
        }

        // Получить значение для конкретного уровня (нереактивный метод)
        public T GetValueAtLevel(int level)
        {
            if (level < 1 || level > _maxLevel)
                throw new ArgumentOutOfRangeException(nameof(level), "Level is out of range.");
            return _levels[level - 1].Value;
        }

        public void Dispose()
        {
            _currentLevel?.Dispose();
            _levels?.Clear();
        }
    }
}