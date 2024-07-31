using System;
using System.Collections.Generic;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>PlatoonManager</c> used to manage a whole platoon.
    /// </summary>
    [Serializable]
    public class PlatoonManager
    {
        /// <summary>
        /// Enum <c>Category</c> used to determine if the player is human or AI.
        /// </summary>
        public enum Categories
        {
            Human,  // Platoon is human
            AI      // Platoon is AI
        }

        public Categories Category;                                 // Whether this platoon is controlled by the player or by AI.
        public Color Color = Color.gray;                            // This is the color this platoon will be tinted.
        public GameObject Prefab;                                   // The prefab to be instantiated for this platoon.
        [HideInInspector] public string RGB;                        // A string that represents the category colored to match the corresponding tank.
        [HideInInspector] public string CategoryRGB;                // A string that represents the category and the corresponding RGB string.
        [HideInInspector, Range(0, 99)] public uint NumWins;        // The number of wins this player has so far.
        [HideInInspector] public float[] RoundPoints;               // The points earned by this player in each round.
        [HideInInspector, Range(0, 99)] public float AccPoints;     // The accumulated points earned by this player.
        [HideInInspector] public int NumTanks;                      // The number of tanks spawned.
        [HideInInspector] public int NumTanksLeft;                  // How many tanks are currently left alive.
        public Action NumTanksLeftCalc;                             // A delegate that is called when the number of tanks left has changed.
        [HideInInspector] public List<TankManager> Tanks;           // A collection of TankManagers for enabling and disabling different aspects of the tanks.

        private GameManager m_GameManager;

        /// <summary>
        /// Destructor <c>PlatoonManager</c> used to clear the tanks list.
        /// </summary>
        ~PlatoonManager() => Clear();

        /// <summary>
        /// Method <c>Clear</c> used to clear the tanks list.
        /// </summary>
        public void Clear()
        {
            Tanks.ForEach(tank => tank.DestroyInstance());
            Tanks.Clear();
        }

        /// <summary>
        /// Method <c>NumTanksLeftActive</c> used to calculate the number of tanks left active.
        /// </summary>
        public void NumTanksLeftActive()
        {
            var numTanksLeft = 0;

            foreach (var tank in Tanks)
                if (tank.Instance.activeSelf)
                    numTanksLeft += 1;

            NumTanksLeftCalc?.Invoke();

            NumTanksLeft = numTanksLeft;
        }

        /// <summary>
        /// Method <c>Initialize</c> used to set up the tank based on the the player's number and player's color.
        /// </summary>
        protected virtual void Initialize()
        {
            m_GameManager = GameManager.Instance;

            RoundPoints = new float[m_GameManager.NumOfRounds];

            // Create a string using the correct color.
            RGB = ColorUtility.ToHtmlStringRGB(Color);
            CategoryRGB = $"<color=#{RGB}>{Category}</color>";
        }

        /// <summary>
        /// Method <c>Spawn</c> create all the tanks and set up references between them.
        /// </summary>
        protected void Spawn()
        {
            foreach (var tank in Tanks)
            {
                tank.Instance = GameObject.Instantiate(Prefab, Vector3.zero, Quaternion.identity) as GameObject;
                tank.Setup(Color);
            }
        }

        /// <summary>
        /// Method <c>CameraTargets</c> create a collection of transforms the same size as the number of tanks.
        /// </summary>
        public Transform[] CameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks.
            Transform[] targets = new Transform[Tanks.Count];

            Tanks.ForEach(tank => targets[Tanks.IndexOf(tank)] = tank.Instance.transform);

            // These are the targets the camera should follow.
            return targets;
        }

        /// <summary>
        /// Method <c>ZeroTanksLeft</c> this is used to check if there is one or fewer tanks remaining and thus the 
        /// round should end.
        /// </summary>
        public bool ZeroTanksLeft()
        {
            NumTanksLeftActive();

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return NumTanksLeft < 1;
         }

        /// <summary>
        /// Method <c>ResetAllTanks</c> this function is used to turn all the tanks back on and reset their positions 
        /// and properties.
        /// </summary>
        public void ResetAllTanks() => Tanks.ForEach(tank => tank.Reset());

        /// <summary>
        /// Method <c>EnableTankControls</c> used to enable control for all tanks.
        /// </summary>
        public void EnableTankControls() => Tanks.ForEach(tank => tank.EnableControl());

        /// <summary>
        /// Method <c>DisableTankControls</c> used to disable tank control for all units.
        /// </summary>
        public void DisableTankControls() => Tanks.ForEach(tank => tank.DisableControl());
    }
}
