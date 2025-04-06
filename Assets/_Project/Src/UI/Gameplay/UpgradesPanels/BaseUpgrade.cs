using System;
using System.Globalization;
using Services.Gameplay.Economic;
using Services.Gameplay.Upgrading;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Gameplay.UpgradesPanels
{
    public class BaseUpgrade : MonoBehaviour, IDisposable
    {
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TMP_Text currentLevel;
        [SerializeField] private TMP_Text cost;
        [SerializeField] private TMP_Text currentValue;
        [SerializeField] private TMP_Text label;


        private readonly CompositeDisposable _disposables = new();

        public void Bind(UpgradeValue<int> data, EconomicSystem economicSystem, string labelText, Action onButtonClick)
        {
            label.text = labelText;

            data.currentLevel.Subscribe(
                x => { currentLevel.text = x.ToString(); }).AddTo(_disposables);

            data.currentValue.Subscribe(x => { currentValue.text = x.ToString(CultureInfo.InvariantCulture); })
                .AddTo(_disposables);

            data.nextCost
                .Where(x => x != -1)
                .Subscribe(x => { cost.text = x.ToString(); })
                .AddTo(_disposables);

            data.nextCost
                .Where(x => x == -1)
                .Subscribe(x =>
                {
                    cost.text = "MAX";
                    upgradeButton.interactable = false;
                })
                .AddTo(_disposables);

            economicSystem.coinsCount
                .Select(x => x > data.nextCost.Value)
                .Subscribe(x => { upgradeButton.interactable = x; })
                .AddTo(_disposables);

            upgradeButton.OnClickAsObservable()
                .Subscribe(x => { onButtonClick?.Invoke(); })
                .AddTo(_disposables);
        }

        public void Bind(UpgradeValue<float> data, EconomicSystem economicSystem, string labelText,
            Action onButtonClick)
        {
            label.text = labelText;
            data = data;

            data.currentLevel.Subscribe(
                x => { currentLevel.text = x.ToString(); }).AddTo(_disposables);

            data.currentValue.Subscribe(x => { currentValue.text = x.ToString(CultureInfo.InvariantCulture); })
                .AddTo(_disposables);

            data.nextCost
                .Where(x => x != -1)
                .Subscribe(x => { cost.text = x.ToString(); })
                .AddTo(_disposables);

            data.nextCost
                .Where(x => x == -1)
                .Subscribe(x =>
                {
                    cost.text = "MAX";
                    upgradeButton.interactable = false;
                })
                .AddTo(_disposables);

            economicSystem.coinsCount
                .Select(x => x > data.nextCost.Value)
                .Subscribe(x => { upgradeButton.interactable = x; })
                .AddTo(_disposables);

            upgradeButton.OnClickAsObservable()
                .Subscribe(x => { onButtonClick?.Invoke(); })
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