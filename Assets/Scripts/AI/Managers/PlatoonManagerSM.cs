using System;
using System.Linq;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>PlatoonManagerSM</c> used to manage a whole platoon.
    /// </summary>
    [Serializable]
    public class PlatoonManagerSM : PlatoonManager
    {
        [Range(1, 100)] public int Size = 3; // Size of the platoon.

        /// <summary>
        /// Method <c>Initialize</c> used to set up the tank based on the the player's number and player's color.
        /// </summary>
        public new void Initialize()
        {
            base.Initialize();

            NumTanksLeft = NumTanks = Size;

            Tanks.AddRange(Enumerable.Range(0, NumTanks).Select(i => new TankManager()).ToArray());

            Spawn();
        }
    }
}
