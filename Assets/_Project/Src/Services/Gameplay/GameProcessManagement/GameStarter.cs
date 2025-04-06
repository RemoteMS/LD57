using Reflex.Attributes;
using UnityEngine;

namespace Services.Gameplay.GameProcessManagement
{
    public class GameStarter : MonoBehaviour
    {
        public KeyCode keyRunState = KeyCode.L;
        public KeyCode nextWave = KeyCode.KeypadEnter;
        public KeyCode nextCalm = KeyCode.K;
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
                Debug.Log("Game started");
            }

            if (Input.GetKey(nextCalm))
            {
                _manager.ForceStartCalm();
                Debug.Log("ForceStartCalm");
            }


            if (Input.GetKey(nextWave))
            {
                _manager.ForceStartCalm();
                Debug.Log("ForceStartCalm");
            }
        }
    }
}