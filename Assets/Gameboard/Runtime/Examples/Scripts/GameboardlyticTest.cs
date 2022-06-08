using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    public class GameboardlyticTest : MonoBehaviour
    {
        private bool setupCompleted;

        void Update()
        {
            if (!setupCompleted)
            {
                if (Gameboard.Instance != null)
                {
                    setupCompleted = true;
                    Gameboard.Instance.Gameboardlytics_SendPlayEvent("");
                }
            }
        }
    }
}