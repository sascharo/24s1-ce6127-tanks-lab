/*
 * TankFiringSystem.cs
 * 
 * This script controls the firing mechanism of a tank in a Unity game. It manages the cooldown between shots, instantiates shells, and applies launch force to them.
 * 
 * Features:
 * - Cooldown management to prevent continuous firing.
 * - Shell instantiation and launch with specified force.
 * - State management to handle firing readiness and cooldown.
 * - Debugging GUI to display the current firing state.
 * 
 * Components:
 * - float cooldown: The time between each shot.
 * - Rigidbody shellPrefabRigidbody: The Prefab's Rigidbody of the shell.
 * - float launchForce: The force given to the shell when firing.
 * - Transform spawnPoint: The position and direction of the shell when firing.
 * - FireState currentState: Enum to represent the firing state (ReadyToFire, OnCooldown).
 * - float cooldownCounter: Counter to track the cooldown time.
 * - Tank TankPrefab: Reference to the tank that owns this firing system.
 * 
 * Methods:
 * - Start(): Initializes the cooldown counter.
 * - Update(): Manages the cooldown state.
 * - Fire(): Fires a shell if the tank is ready and sets the state to cooldown.
 * - CurrentFireState (Property): Gets or sets the current firing state.
 * - OnGUI(): Displays the current firing state for debugging purposes.
 */

using UnityEngine;

namespace CE6127.Tanks.Advanced
{
    public class TankFiringSystem : MonoBehaviour
    {
        public float cooldown = 0.5f;           // The time between each shot.
        public Rigidbody shellPrefabRigidbody;  // The Prefab's Rigidbody of the shell.
        public float launchForce = 15f;         // The force given to the shell when firing.
        public Transform spawnPoint;            // The position and direction of the shell when firing.

        // The state of the firing system.
        public enum FireState
        {
            ReadyToFire,
            OnCooldown
        }
        protected FireState currentState = FireState.ReadyToFire; // The current state of the firing system.

        protected float cooldownCounter;                          // The counter for the cooldown time.

        public FireState CurrentFireState
        {
            get { return currentState; }
            set
            {
                if (currentState != value)
                {
                    switch (currentState)
                    {
                        case FireState.ReadyToFire:
                            break;
                        case FireState.OnCooldown:
                            {
                                cooldownCounter = cooldown;
                                break;
                            }
                        default:
                            break;
                    }

                    currentState = value;
                }
            }
        }

        private Tank TankPrefab { get { return GetComponent<Tank>(); } }    // Reference to the tank that owns this firing system.

        private void Start()
        {
            // Set the cooldown counter to the cooldown time.
            cooldownCounter = cooldown;
        }

        // Update is called once per frame.
        void Update()
        {
            switch (CurrentFireState)
            {
                case FireState.ReadyToFire:
                    break;
                case FireState.OnCooldown:
                    {
                        cooldownCounter -= Time.deltaTime;
                        if (cooldownCounter <= 0)
                            CurrentFireState = FireState.ReadyToFire;
                        break;
                    }
                default:
                    break;
            }
        }

        public Rigidbody Fire()
        {
            if (CurrentFireState == FireState.ReadyToFire)
            {
                // Change state.
                CurrentFireState = FireState.OnCooldown;

                // Spawn shell by creating an instance of the shell and store a reference to it's rigidbody.
                var shell = Instantiate(shellPrefabRigidbody, spawnPoint.position, spawnPoint.rotation) as Rigidbody;

                // Set the shell's velocity to the launch force in the fire position's forward direction.
                shell.velocity = launchForce * spawnPoint.forward;

                return shell;
            }

            return null;
        }

        private void OnGUI()
        {
            var nl = string.Concat(System.Linq.Enumerable.Repeat("\n\n", TankPrefab.playerNum));
            var str = $"{nl}\t\t\t\t\t<color='red'><size=35>{CurrentFireState}</size></color>";
            GUILayout.Label(str);
        }
    }
}
