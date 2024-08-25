/*
 * TankManager.cs
 * 
 * This script manages the spawning, resetting, and destruction of tanks in a Unity game. It handles the initialization of tanks, their colors, and the game state when tanks are destroyed.
 * 
 * Features:
 * - Spawns tanks at designated spawn points.
 * - Manages the player count and tracks the remaining tanks.
 * - Handles tank destruction and announces the last tank standing.
 * - Resets tanks to their initial positions and states.
 * 
 * Components:
 * - GameObject spawnPointContainer: The parent object containing all spawn points.
 * - GameObject tankPrefab: The prefab used to instantiate tanks.
 * - Action<Tank> OneTankLeft: Delegate called when only one tank is left.
 * - Color[] playerColors: Array of colors assigned to each tank.
 * - int playerCount: The number of players in the game.
 * - List<Tank> tanks: List of all tanks in the game.
 * - List<Transform> spawnPoints: List of all spawn points in the game.
 * 
 * Methods:
 * - Awake: Initializes spawn points and spawns tanks.
 * - OnTankDestroy: Handles the destruction of a tank and checks for the last tank standing.
 * - Restart: Resets all tanks to their initial positions and states.
 * - SpawnTanks: Spawns tanks and assigns colors to them.
 * - GetTanksTransform: Returns an array of transforms for all tanks.
 * - NumberOfPlayers (Property): Gets the number of players based on spawn points.
 */

using System;
using System.Collections.Generic;
using UnityEngine;

namespace CE6127.Tanks.Advanced
{
    public class TankManager : MonoBehaviour
    {
        public GameObject spawnPointContainer;  // This is the parent of all spawn points.
        public GameObject tankPrefab;           // This is the prefab of the tank.

        public Action<Tank> OneTankLeft;        // This will be called when only one tank left in the scene.

        // This is the list of colors for the tanks.
        protected Color[] playerColors =
        {
            Color.red,
            Color.blue,
            Color.green
        };

        protected int playerCount;                        // This is the number of players in the scene.
        protected List<Tank> tanks = new();               // This is the list of tanks in the scene.
        protected List<Transform> spawnPoints = new();    // This is the list of spawn points in the scene.

        private void Awake()
        {
            // Setup the spawn points from spawn parent.
            var spawnTrans = spawnPointContainer.transform;
            // Loop through all the children and add them to the list.
            for (var i = 0; i < spawnTrans.childCount; ++i)
                spawnPoints.Add(spawnTrans.GetChild(i));

            SpawnTanks();
        }

        public void OnTankDestroy(Tank target)
        {
            // Reduce the player count and put the dead tank to the back of the list.
            playerCount--;
            tanks.Remove(target);
            tanks.Add(target);

            // If it is the last tank standing, call delegate to announce the winner.
            if (playerCount == 1)
            {
                // Call the delegate
                OneTankLeft?.Invoke(tanks[0]); // First tank is always the winner.

                // Disable the input of the winner.
                tanks[0].inputIsEnabled = false;
            }
        }

        public void Restart()
        {
            // Reset all tanks.
            foreach (var tank in tanks)
                tank.Restart(spawnPoints[tank.playerNum].position, spawnPoints[tank.playerNum].rotation);

            // Reset the player count.
            playerCount = tanks.Count;
        }

        // Spawn and set up the tank's color.
        public void SpawnTanks()
        {
            playerCount = spawnPoints.Count;

            for (var i = 0; i < playerCount; ++i)
            {
                // Spawn Tank and store it.
                GameObject tank = Instantiate(tankPrefab, spawnPoints[i].position, spawnPoints[i].rotation);
                tanks.Add(tank.GetComponent<Tank>());
                tanks[i].playerNum = i;
                // Subscribe to the destroy event.
                tanks[i].DestroyTank += OnTankDestroy;

                // Set up color.
                var renderers = tanks[i].GetComponentsInChildren<MeshRenderer>();
                foreach (var rend in renderers)
                    rend.material.color = playerColors[i];
            }
        }

        public Transform[] GetTanksTransform()
        {
            var count = tanks.Count;
            var tanksTrans = new Transform[count];
            for (var i = 0; i < count; ++i)
                tanksTrans[i] = tanks[i].transform;

            return tanksTrans;
        }

        public int NumberOfPlayers
        {
            get { return spawnPoints.Count; }
        }
    }
}
