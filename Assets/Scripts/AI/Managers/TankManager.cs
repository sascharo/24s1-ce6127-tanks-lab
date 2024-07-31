using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;
using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
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
        [HideInInspector] public GameObject Instance;           // A reference to the instance of the tank when it is created.
        [HideInInspector] public AudioSource[] AudioSources;    // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
#nullable enable // Allows nullable reference type for InputActionMapName.
        [HideInInspector] public string? InputActionMapName;    // The name of the input action map.
#nullable disable // Disables nullable reference type for InputActionMapName.

        private GameManager m_GameManager;      // Reference to the game manager.
        private TankMovement m_Movement;        // Reference to tank's movement script, used to disable and enable control.
        private TankShooting m_Shooting;        // Reference to tank's shooting script, used to disable and enable control.
        private GameObject m_CanvasGameObject;  // Used to disable the world space UI during the Starting and Ending phases of each round.

        /// <summary>
        /// Constructor <c>TankManager</c>
        /// </summary>
        public TankManager() => InputActionMapName = null;

        /// <summary>
        /// Constructor <c>TankManager</c>
        /// </summary>
        public TankManager(InputActionMaps inputActionMapName) => InputActionMapName = inputActionMapName.ToString();

        /// <summary>
        /// Destructor <c>~TankManager</c> destroy all the tank instances.
        /// </summary>
        ~TankManager()
        {
            DestroyInstance();
        }

        public void DestroyInstance()
        {
            // Need to call Destroy from base class Object since TankManager doesn't inherit from MonoBehaviour.
            //UnityEngine.Object.Destroy(Instance);
            UnityEngine.Object.DestroyImmediate(Instance);
        }

        /// <summary>
        /// Method <c>Setup</c> used to set up the tank based on the the player's number and player's color.
        /// </summary>
        public void Setup(Color color)
        {
            m_GameManager = GameManager.Instance;

            Positioning();

            // Get references to the components.
            AudioSources = Instance.GetComponents<AudioSource>();
            m_Movement = Instance.GetComponent<TankMovement>();
            m_Shooting = Instance.GetComponent<TankShooting>();
            m_CanvasGameObject = Instance.GetComponentInChildren<Canvas>().gameObject;

            // Set up the player number's input action mapping.
            if (m_Movement)
            {
                PlayerInput playerInput = Instance.GetComponent<PlayerInput>();

                playerInput.defaultActionMap = InputActionMapName;
                playerInput.SwitchCurrentActionMap(InputActionMapName);
                m_Movement.InputActionMap = playerInput.actions.FindActionMap(InputActionMapName);
            }

            // Get all of the renderers of the tank.
            MeshRenderer[] renderers = Instance.GetComponentsInChildren<MeshRenderer>();
            // Go through all the renderers...
            for (var i = 0; i < renderers.Length; ++i)
                renderers[i].material.color = color; // ... set their material color to the color specific to this tank.
        }

        /// <summary>
        /// Method <c>DisableControl</c> used during the phases of the game where the player shouldn't be able to 
        /// control their tank.
        /// </summary>
        public void DisableControl()
        {
            if (m_Movement)
                m_Movement.enabled = false;
            if (m_Shooting)
                m_Shooting.enabled = false;

            m_CanvasGameObject.SetActive(false);
        }

        /// <summary>
        /// Method <c>EnableControl</c> used during the phases of the game where the player should be able to control 
        /// their tank.
        /// </summary>
        public void EnableControl()
        {
            if (m_Movement)
                m_Movement.enabled = true;
            if (m_Shooting)
                m_Shooting.enabled = true;

            m_CanvasGameObject.SetActive(true);
        }

        /// <summary>
        /// Method <c>Reset</c> used at the start of each round to put the tank into it's default state.
        /// </summary>
        public void Reset()
        {
            //m_Instance.transform.position = m_Position;
            //m_Instance.transform.rotation = m_Rotation;
            Positioning();

            Instance.SetActive(false);
            Instance.SetActive(true);
        }

        /// <summary>
        /// Method <c>Positioning</c> used to position the tank on the nav mesh.
        /// </summary>
        private void Positioning()
        {
            // Pick the first indice of a random triangle in the nav mesh.
            int vertexSelected = Random.Range(0, m_GameManager.SpawnIndices.Length);
            // Spawn on verticies.
            Vector3 pointSelected = m_GameManager.NavMeshTriang.vertices[m_GameManager.SpawnIndices[vertexSelected]];
            if(NavMesh.SamplePosition(pointSelected, out var hit, 1f, 1 << NavMesh.GetAreaFromName(m_GameManager.WalkableNavMeshArea)))
                pointSelected = hit.position;
            Instance.transform.position = pointSelected;

            Instance.transform.rotation = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up);
        }
    }
}
