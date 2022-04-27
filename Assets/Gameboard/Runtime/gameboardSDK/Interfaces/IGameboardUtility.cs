using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gameboard
{
    public interface IGameboardUtility
    {
        void ProcessUpdate();
        void ProcessLateUpdate();
    }
}