using Cysharp.Threading.Tasks;
using Reflex.Attributes;
using Services.Gameplay.BulletSystem;
using Services.Gameplay.Economic;
using Services.Gameplay.Enemies;
using Services.Storages.Gameplay;
using UniRx;
using UnityEngine;

namespace Services.Gameplay.GameProcessManagement
{
    public class GameLauncher : MonoBehaviour
    {
        private GameProcessManager _gameProcessManager;
        private EconomicSystem _economicSystem;
        private GameplayStorage _gameplayStorage;

        [Inject]
        private void Inject(GameProcessManager manager, EconomicSystem economicSystem, GameplayStorage gameplayStorage)
        {
            _gameplayStorage = gameplayStorage;
            _economicSystem = economicSystem;
            _gameProcessManager = manager;

            _gameProcessManager.currentState.Subscribe(state => Debug.Log($"Current State: {state}")).AddTo(this);
            _gameProcessManager.stateTimeRemaining.Subscribe(time => Debug.Log($"Time Remaining: {time:F1}"))
                .AddTo(this);
            _gameProcessManager.stateTimeElapsed.Subscribe(time => Debug.Log($"Time Elapsed: {time:F1}")).AddTo(this);
            _gameProcessManager.remainingEnemies.Subscribe(count => Debug.Log($"Enemies Remaining: {count}"))
                .AddTo(this);
            _gameProcessManager.hasLost.Subscribe(lost => Debug.Log($"Game Lost: {lost}")).AddTo(this);


            StartGame().Forget();
        }

        private async UniTask StartGame()
        {
            Debug.Log("Starting game...");
            await _gameProcessManager.RunGameAsync();

            // Игра запущена, ждем действий пользователя или автоматического завершения
            Debug.Log("Game process started. Waiting for wave start command.");
        }

        private void Update()
        {
            if (_gameProcessManager.currentState.Value == GameState.WaitingForEnemies)
            {
                if (_gameplayStorage.enemiesCount.Value <= 0)
                {
                    _gameProcessManager.ForceStartCalm();
                }
                // if (_economicSystem.)
            }

            if (Input.GetKeyDown(KeyCode.Z))
            {
                Debug.Log("Z pressed - Starting Wave Approaching");
                _gameProcessManager.ForceStartWaveApproaching(); // Запускаем волну
            }

            if (Input.GetKeyDown(KeyCode.X))
            {
                // todo:
                Debug.Log("X pressed - Forcing Calm State");
                _gameProcessManager.ForceStartCalm(); // Принудительно возвращаемся в Calm
            }

            if (Input.GetKeyDown(KeyCode.C))
            {
                Debug.Log("C pressed - Simulating enemy defeat");
                _gameProcessManager.RegisterEnemyDefeat(); // Уменьшаем количество врагов
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                Debug.Log("V pressed - Dealing damage to simulate game over");
                _economicSystem.DamageToPlayer(new TestDamager(1000)); // Наносим урон для проверки проигрыша
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                Debug.Log("B pressed - Dealing damage to simulate game over");
                _economicSystem.GetPriceForEnemy(new TestPricer(1000));
                ;
            }
        }

        private void OnDestroy()
        {
            // _gameProcessManager?.Dispose(); // Очищаем ресурсы при уничтожении
        }
    }


    public class TestPricer : IEffectable
    {
        public TestPricer(int price)
        {
            Price = price;
        }

        public int DamageToPlayer { get; }
        public int Price { get; }
        public Health Health { get; }
        public GameObject GameObject { get; }
    }

    public class TestDamager : IEffectable
    {
        public TestDamager(int damageToPlayer)
        {
            DamageToPlayer = damageToPlayer;

            Health = new Health(10, 10);
        }

        public int DamageToPlayer { get; }
        public int Price { get; }
        public Health Health { get; }
        public GameObject GameObject { get; }
    }
}