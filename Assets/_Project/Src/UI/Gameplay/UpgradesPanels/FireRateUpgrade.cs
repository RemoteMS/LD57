using System;
using Reflex.Attributes;
using Services.Gameplay.Economic;
using Services.Gameplay.Upgrading;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay.UpgradesPanels
{
    public class FireRateUpgrade : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text currentLevel;
        [SerializeField] private TMP_Text cost;

        private UpgradeSystem _system;
        private UpgradeValue<float> _fireRate;

        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Inject(UpgradeSystem system, EconomicSystem economicSystem)
        {
            _system = system;
            _fireRate = system.fireRate;

            _fireRate.currentLevel.Subscribe(
                x => { currentLevel.text = x.ToString(); }).AddTo(_disposables);

            _fireRate.nextCost
                .Where(x => x != -1)
                .Subscribe(x => { cost.text = x.ToString(); })
                .AddTo(_disposables);

            _fireRate.nextCost
                .Where(x => x == -1)
                .Subscribe(x =>
                {
                    cost.text = "MAX";
                    upgradeButton.interactable = false;
                })
                .AddTo(_disposables);

            upgradeButton.OnClickAsObservable()
                .Subscribe(x => { _system.UpdateFireRate(); })
                .AddTo(_disposables);

            economicSystem.coinsCount
                .Select(x => x > _fireRate.nextCost.Value)
                .Subscribe(x => { upgradeButton.interactable = x; })
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables?.Dispose();
        }

        private void OnDestroy()
        {
            Dispose();
        }
    }
}