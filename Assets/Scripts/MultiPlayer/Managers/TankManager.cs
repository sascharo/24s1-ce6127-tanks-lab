using System;
using UnityEngine;
using UnityEngine.InputSystem;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.MultiPlayer
{
    /// <summary>
    /// Class <c>TankManager</c> is used to manage various settings on a tank.
    /// <para>
    /// This class is to manage various settings on a tank. It works with the GameManager class to control how the tanks
    /// behave and whether or not players have control of their tank in the different phases of the game.
    /// </para>
    /// </summary>
    [Serializable]
    public class TankManager
    {
        public Color m_PlayerColor;                                                         // This is the color this tank will be tinted.
        public Transform m_SpawnPoint;                                                      // The position and direction the tank will have when it spawns.
        [HideInInspector] public int m_PlayerNumber;                                        // This specifies which player this the manager for.
        [Tooltip("Player number's input action map.")] public string m_inputActionMapName;  // The name of the player number's input action map.
        [HideInInspector] public string m_ColoredPlayerText;                                // A string that represents the player with their number colored to match their tank.
        [HideInInspector] public GameObject m_Instance;                                     // A reference to the instance of the tank when it is created.
        [HideInInspector] public int m_Wins;                                                // The number of wins this player has so far.

        private TankMovement m_Movement;                // Reference to tank's movement script, used to disable and enable control.
        private TankShooting m_Shooting;                // Reference to tank's shooting script, used to disable and enable control.
        private GameObject m_CanvasGameObject;          // Used to disable the world space UI during the Starting and Ending phases of each round.
        private string m_inputActionMapBase = "Player"; // The base name of the input action map.

        /// <summary>
        /// Method <c>Setup</c> used to set up the tank based on the the player's number and player's color.
        /// </summary>
        public void Setup()
        {
            // Get references to the components.
            m_Movement = m_Instance.GetComponent<TankMovement>();
            m_Shooting = m_Instance.GetComponent<TankShooting>();
            m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;

            // Set up the player number's input action mapping.
            PlayerInput playerInput = m_Instance.GetComponent<PlayerInput>();
            m_inputActionMapName = m_inputActionMapBase + m_PlayerNumber;
            playerInput.defaultActionMap = m_inputActionMapName;
            playerInput.SwitchCurrentActionMap(m_inputActionMapName);
            m_Movement.m_InputActionMap = playerInput.actions.FindActionMap(m_inputActionMapName);
        
            // Create a string using the correct color that says 'PLAYER 1' etc based on the tank's color and the player's number.
            m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

            // Get all of the renderers of the tank.
            MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

            // Go through all the renderers...
            for (var i = 0; i < renderers.Length; ++i)
                renderers[i].material.color = m_PlayerColor; // ... set their material color to the color specific to this tank.
        }

        /// <summary>
        /// Method <c>DisableControl</c> used during the phases of the game where the player shouldn't be able to control their tank.
        /// </summary>
        public void DisableControl()
        {
            m_Movement.enabled = false;
            m_Shooting.enabled = false;

            m_CanvasGameObject.SetActive (false);
        }

        /// <summary>
        /// Method <c>EnableControl</c> used during the phases of the game where the player should be able to control their tank.
        /// </summary>
        public void EnableControl()
        {
            m_Movement.enabled = true;
            m_Shooting.enabled = true;

            m_CanvasGameObject.SetActive (true);
        }

        /// <summary>
        /// Method <c>Reset</c> used at the start of each round to put the tank into it's default state.
        /// </summary>
        public void Reset()
        {
            m_Instance.transform.position = m_SpawnPoint.position;
            m_Instance.transform.rotation = m_SpawnPoint.rotation;

            m_Instance.SetActive (false);
            m_Instance.SetActive (true);
        }
    }
}
