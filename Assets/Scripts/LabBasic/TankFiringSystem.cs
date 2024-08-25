/*
 * TankFiringSystem.cs
 * 
 * Controls the firing mechanism of a tank in a Unity game. It manages the cooldown between shots, instantiates shells, and applies launch force to them.
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
 * 
 * Methods:
 * - Start: Initializes the cooldown counter.
 * - Update: Manages the cooldown state.
 * - Fire: Fires a shell if the tank is ready and sets the state to cooldown.
 * - CurrentFireState (Property): Gets or sets the current firing state.
 * - OnGUI: Displays the current firing state for debugging purposes.
 */

using UnityEngine;

namespace CE6127.Tanks.Basic
{
    public class TankFiringSystem : MonoBehaviour
    {
        public float cooldown = 0.5f;           // The time between each shot.
        public Rigidbody shellPrefabRigidbody;  // The Prefab's Rigidbody of the shell.
        public float launchForce = 15f;         // The force given to the shell when firing.
        public Transform spawnPoint;            // The position and direction of the shell when firing.

        // The state of the tank firing system.
        public enum FireState
        {
            ReadyToFire,    // The tank is ready to fire.
            OnCooldown      // The tank is on cooldown.
        }
        protected FireState currentState = FireState.ReadyToFire; // The current state of the tank firing system.

        protected float cooldownCounter; // The counter for cooldown.

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

        private void OnGUI() => GUILayout.Label($"\n\n<color='red'><size=35>{CurrentFireState}</size></color>");
    }
}
