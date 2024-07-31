using System;
using System.Collections.Generic;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Advanced
{
    public class TankManager : MonoBehaviour
    {
        public GameObject SpawnPointContainer;  // This is the parent of all spawn points.
        public GameObject TankPrefab;           // This is the prefab of the tank.

        public Action<Tank> OneTankLeft;        // This will be called when only one tank left in the scene.

        // This is the list of colors for the tanks.
        protected Color[] m_PlayerColors =
        {
            Color.red,
            Color.blue,
            Color.green
        };

        protected int m_PlayerCount;                        // This is the number of players in the scene.
        protected List<Tank> m_Tanks = new();               // This is the list of tanks in the scene.
        protected List<Transform> m_SpawnPoints = new();    // This is the list of spawn points in the scene.

        private void Awake()
        {
            // Setup the spawn points from spawn parent.
            var spawnTrans = SpawnPointContainer.transform;
            // Loop through all the children and add them to the list.
            for (var i = 0; i < spawnTrans.childCount; ++i)
                m_SpawnPoints.Add(spawnTrans.GetChild(i));

            SpawnTanks();
        }

        public void OnTankDestroy(Tank target)
        {
            // Reduce the player count and put the dead tank to the back of the list.
            m_PlayerCount--;
            m_Tanks.Remove(target);
            m_Tanks.Add(target);

            // If it is the last tank standing, call delegate to announce the winner.
            if (m_PlayerCount == 1)
            {
                // Call the delegate
                OneTankLeft?.Invoke(m_Tanks[0]); // First tank is always the winner.

                // Disable the input of the winner.
                m_Tanks[0].InputIsEnabled = false;
            }
        }

        public void Restart()
        {
            // Reset all tanks.
            foreach (var tank in m_Tanks)
                tank.Restart(m_SpawnPoints[tank.PlayerNum].position, m_SpawnPoints[tank.PlayerNum].rotation);

            // Reset the player count.
            m_PlayerCount = m_Tanks.Count;
        }

        // Spawn and set up the tank's color.
        public void SpawnTanks()
        {
            m_PlayerCount = m_SpawnPoints.Count;

            for (var i = 0; i < m_PlayerCount; ++i)
            {
                // Spawn Tank and store it.
                GameObject tank = Instantiate(TankPrefab, m_SpawnPoints[i].position, m_SpawnPoints[i].rotation);
                m_Tanks.Add(tank.GetComponent<Tank>());
                m_Tanks[i].PlayerNum = i;
                // Subscribe to the destroy event.
                m_Tanks[i].DestroyTank += OnTankDestroy;

                // Set up color.
                var renderers = m_Tanks[i].GetComponentsInChildren<MeshRenderer>();
                foreach (var rend in renderers)
                    rend.material.color = m_PlayerColors[i];
            }
        }

        public Transform[] GetTanksTransform()
        {
            var count = m_Tanks.Count;
            var tanksTrans = new Transform[count];
            for (var i = 0; i < count; ++i)
                tanksTrans[i] = m_Tanks[i].transform;

            return tanksTrans;
        }

        public int NumberOfPlayers
        {
            get { return m_SpawnPoints.Count; }
        }
    }
}
