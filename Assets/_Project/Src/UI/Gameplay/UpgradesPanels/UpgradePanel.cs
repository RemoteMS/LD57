using Reflex.Attributes;
using Services.Gameplay.Economic;
using Services.Gameplay.Upgrading;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI.Gameplay.UpgradesPanels
{
    public class UpgradePanel : MonoBehaviour
    {
        [SerializeField] TMP_Text coinsCountText;
        [SerializeField] private BaseUpgrade upgradeFireRate;
        [SerializeField] private BaseUpgrade reloadTime;
        [SerializeField] private BaseUpgrade maxAmmo;
        private UpgradeSystem _system;
        private EconomicSystem _economicSystem;

        [Inject]
        public void Inject(UpgradeSystem system, EconomicSystem economicSystem)
        {
            _system = system;
            _economicSystem = economicSystem;


            _economicSystem.coinsCount
                .Subscribe(x =>
                {
                    coinsCountText.text = x.ToString();
                })
                .AddTo(this);
        }

        private void Start()
        {
            upgradeFireRate.Bind(
                _system.fireRate,
                _economicSystem,
                "Fire Rate",
                _system.UpdateFireRate
            );

            reloadTime.Bind(
                _system.reloadTime,
                _economicSystem,
                "Reload Time",
                onButtonClick: _system.UpdateReloadTime
            );

            maxAmmo.Bind(
                _system.maxAmmo,
                _economicSystem,
                "Max Ammo",
                onButtonClick: _system.UpdateMaxAmmo
            );
        }
    }
}