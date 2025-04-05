using Reflex.Attributes;
using UnityEngine;

namespace Services.Gameplay.GameProcessManagement
{
    public class GameStarter : MonoBehaviour
    {
        public KeyCode keyRunState = KeyCode.L;
        private GameProcessManager _manager;

        [Inject]
        public void Inject(GameProcessManager manager)
        {
            _manager = manager;
        }

        private int i = 0;

        private void Update()
        {
            if (Input.GetKey(keyRunState) && i == 0)
            {
                i++;
                _manager.RunGameAsync();
            }
        }
    }
}