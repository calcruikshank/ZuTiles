using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard.Tools
{
    public class GameboardToolFoundation : MonoBehaviour
    {
        void OnDisable()
        {
            PerformCleanup();
        }

        void OnDestroy()
        {
            PerformCleanup();
        }

        void OnApplicationQuit()
        {
            PerformCleanup();
        }

        protected virtual void PerformCleanup() { }
    }
}