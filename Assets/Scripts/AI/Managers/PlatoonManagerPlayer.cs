using System;
using System.Linq;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>PlatoonManagerPlayer</c> used to manage a whole platoon.
    /// </summary>
    [Serializable]
    public class PlatoonManagerPlayer : PlatoonManager
    {
        /// <summary>
        /// Struct <c>TankPlayer</c> used to store the player's input action map.
        /// </summary>
        [Serializable]
        public struct TankPlayer
        {
            public InputActionMaps InputActionMap; // The player's input action map.
        }

        public TankPlayer[] Players; // Array of players.

        /// <summary>
        /// Method <c>Initialize</c> used to set up the tank based on the the player's number and player's color.
        /// </summary>
        public new void Initialize()
        {
            base.Initialize();

            NumTanksLeft = NumTanks = Players.Length;

            Tanks.AddRange(Enumerable.Range(0, NumTanks).Select(i => new TankManager()).ToArray());

            Tanks.ForEach(tank => tank.InputActionMapName = Players[Tanks.IndexOf(tank)].InputActionMap.ToString());

            Spawn();
        }
    }
}
