﻿using UnityEngine;

namespace Gameboard.EventArgs
{
    public class CompanionDiceRollEventArgs : CompanionMessageResponseArgs
    {
        public int[] diceSizesRolledList = new int[0];
        public int addedModifier = 0;
        public string diceNotation;

        public override string ToString()
        {
            return JsonUtility.ToJson(this);
        }
    }
}