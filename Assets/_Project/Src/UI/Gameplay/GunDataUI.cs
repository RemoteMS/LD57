using System;
using Reflex.Attributes;
using Services.Storages.Gameplay;
using TMPro;
using UniRx;
using UnityEngine;

namespace UI.Gameplay
{
    public class GunDataUI : MonoBehaviour, IDisposable
    {
        [SerializeField] private TMP_Text maxAmmo;
        [SerializeField] private TMP_Text currentAmmo;
        [SerializeField] private TMP_Text isReloading;

        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public void Inject(GameplayStorage storage)
        {
            var gunData = storage.gunData;

            gunData.maxAmmo.Subscribe(x => { maxAmmo.text = x.ToString(); }).AddTo(_disposables);
            gunData.currentAmmo.Subscribe(x => { currentAmmo.text = x.ToString(); }).AddTo(_disposables);
            gunData.isReloading.Subscribe(x => { isReloading.text = x.ToString(); }).AddTo(_disposables);
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