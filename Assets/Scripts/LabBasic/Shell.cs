/*
 * Shell.cs
 * 
 * Controls the behavior of a shell in a Unity game. It manages the shell's state, handles collisions, and triggers explosions.
 * 
 * Features:
 * - State management for the shell (Moving, Explode).
 * - Collision detection to trigger state changes.
 * - Explosion mechanism affecting objects within a specified radius.
 * - Application of explosion force to affected objects.
 * 
 * Components:
 * - LayerMask explosionMask: Defines the layers affected by the explosion.
 * - float explosionRadius: The radius within which objects are affected by the explosion.
 * - float explosionForce: The force applied to objects within the explosion radius.
 * 
 * Methods:
 * - OnTriggerEnter: Detects collisions and changes the shell's state to Explode.
 * - FixedUpdate: Manages state transitions.
 * - Explosion: Applies explosion force to objects within the explosion radius and destroys the shell.
 */

using UnityEngine;

namespace CE6127.Tanks.Basic
{
    public class Shell : MonoBehaviour
    {
        public LayerMask explosionMask;         // The layers that will be affected by the explosion.
        public float explosionRadius = 5f;      // The radius of the explosion.
        public float explosionForce = 1000f;    // The force of the explosion.

        // The state of the shell.
        public enum ShellState
        {
            Moving,
            Explode
        }
        protected ShellState currentState; // The current state of the shell.

        public ShellState CurrentShellState
        {
            get { return currentState; }
            set
            {
                if (currentState != value)
                {
                    switch (value)
                    {
                        case ShellState.Moving:
                            break;
                        case ShellState.Explode:
                            {
                                Explosion();
                                break;
                            }
                        default:
                            break;
                    }

                    currentState = value;
                }
            }
        }

        // Called when collide with other collider.
        protected void OnTriggerEnter(Collider other)
        {
            if (CurrentShellState == ShellState.Moving)
                CurrentShellState = ShellState.Explode;
        }

        private void FixedUpdate()
        {
            switch (currentState)
            {
                case ShellState.Moving:
                    break;
                case ShellState.Explode:
                    break;
                default:
                    break;
            }
        }

        public void Explosion()
        {
            // Get all the tanks caught in the explosion.
            Collider[] tankColliders = Physics.OverlapSphere(transform.position, explosionRadius, explosionMask);

            // Loop through the collider to apply force and damage.
            foreach (var collider in tankColliders)
            {
                // Apply physics to the tank.
                var rBody = collider.GetComponent<Rigidbody>();
                if (rBody == null)
                    continue;
                rBody.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            // Destroy the shell.
            Destroy(gameObject);
        }
    }
}
