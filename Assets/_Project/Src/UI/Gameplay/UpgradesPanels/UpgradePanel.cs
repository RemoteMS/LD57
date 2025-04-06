using Reflex.Attributes;
using Services.Gameplay.Economic;
using Services.Gameplay.Upgrading;
using UnityEngine;

namespace UI.Gameplay.UpgradesPanels
{
    public class UpgradePanel : MonoBehaviour
    {
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