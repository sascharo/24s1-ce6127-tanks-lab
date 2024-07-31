using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.MultiPlayer
{
    /// <summary>
    /// Class <c>GameManager</c> this class is used to control the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public int m_NumRoundsToWin = 5;                                                // The number of rounds a single player has to win to win the game.
        public float m_StartDelay = 3f;                                                 // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;                                                   // The delay between the end of RoundPlaying and RoundEnding phases.
        [Tooltip("CameraRig 2Player Variant.")] public CameraControl m_CameraControl;   // Reference to the CameraControl script for control during different phases.
        [Tooltip("TextMeshPro.")] public TextMeshProUGUI m_MessageText;                 // Reference to the overlay Text to display winning text, etc.
        public GameObject m_TankPrefab;                                                 // Reference to the prefab the players will control.
        public TankManager[] m_Tanks;                                                   // A collection of managers for enabling and disabling different aspects of the tanks.

        private int m_RoundNumber;          // Which round the game is currently on.
        private WaitForSeconds m_StartWait; // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;   // Used to have a delay whilst the round or game ends.
        private TankManager m_RoundWinner;  // Reference to the winner of the current round.  Used to make an announcement of who won.
        private TankManager m_GameWinner;   // Reference to the winner of the game.  Used to make an announcement of who won.

        const float k_MaxDepenetrationVelocity = float.PositiveInfinity; // This is to fix a change to the physics engine.

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // This line fixes a change to the physics engine.
            Physics.defaultMaxDepenetrationVelocity = k_MaxDepenetrationVelocity;

            // Create the delays so they only have to be made once.
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);

            SpawnAllTanks();
            SetCameraTargets();

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }

        /// <summary>
        /// Method <c>SpawnAllTanks</c> this is called from Start and will run each phase of the game one after another.
        /// </summary>
        private void SpawnAllTanks()
        {
            // For all the tanks...
            for (var i = 0; i < m_Tanks.Length; ++i)
            {
                // ... create them, set their player number and references needed for control.
                m_Tanks[i].m_Instance = Instantiate(m_TankPrefab,
                    m_Tanks[i].m_SpawnPoint.position,
                    m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
                m_Tanks[i].m_PlayerNumber = i + 1;
                m_Tanks[i].Setup();
            }
        }

        /// <summary>
        /// Method <c>SetCameraTargets</c> this is called from Start and will run each phase of the game one after another.
        /// </summary>
        private void SetCameraTargets()
        {
            // Create a collection of transforms the same size as the number of tanks.
            Transform[] targets = new Transform[m_Tanks.Length];

            // For each of these transforms...
            for (var i = 0; i < targets.Length; ++i)
                targets[i] = m_Tanks[i].m_Instance.transform; // ... set it to the appropriate tank transform.

            // These are the targets the camera should follow.
            m_CameraControl.m_Targets = targets;
        }

        /// <summary>
        /// Coroutine <c>GameLoop</c> this is called from start and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator GameLoop()
        {
            // Start off by running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished.  At which point, check if a game winner has been found.
            if (m_GameWinner != null)
                SceneManager.LoadScene(0); // If there is a game winner, restart the level. // Application.LoadLevel(Application.loadedLevel); // DEPRECATED
            else
                StartCoroutine(GameLoop()); // If there isn't a winner yet, restart this coroutine so the loop continues. Note that this coroutine doesn't yield.  This means that the current version of the GameLoop will end.
        }

        /// <summary>
        /// Coroutine <c>RoundStarting</c> this is called from GameLoop and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move.
            ResetAllTanks();
            DisableTankControl();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            m_CameraControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;
            m_MessageText.text = "ROUND " + m_RoundNumber;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }

        /// <summary>
        /// Coroutine <c>RoundPlaying</c> this is called from GameLoop and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks.
            EnableTankControl();

            // Clear the text from the screen.
            m_MessageText.text = string.Empty;

            // While there is not one tank left...
            while (!OneTankLeft())
                yield return null; // ... return on the next frame.
        }

        /// <summary>
        /// Coroutine <c>RoundEnding</c> this is called from GameLoop and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving.
            DisableTankControl();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
                m_RoundWinner.m_Wins++;

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage();
            m_MessageText.text = message;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }

        /// <summary>
        /// Method <c>OneTankLeft</c> this is used to check if there is one or fewer tanks remaining and thus the round should end.
        /// </summary>
        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero.
            var numTanksLeft = 0;

            // Go through all the tanks...
            for (var i = 0; i < m_Tanks.Length; ++i)
            {
                // ... and if they are active, increment the counter.
                if (m_Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }

        /// <summary>
        /// Method <c>GetRoundWinner</c> this function is to find out if there is a winner of the round.
        /// <para>
        /// This function is called with the assumption that 1 or fewer tanks are currently active.
        /// </para>
        /// </summary>
        private TankManager GetRoundWinner()
        {
            // Go through all the tanks...
            for (var i = 0; i < m_Tanks.Length; ++i)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (m_Tanks[i].m_Instance.activeSelf)
                    return m_Tanks[i];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }

        /// <summary>
        /// Method <c>GetGameWinner</c> this function is to find out if there is a winner of the game.
        /// </summary>
        private TankManager GetGameWinner()
        {
            // Go through all the tanks...
            for (var i = 0; i < m_Tanks.Length; ++i)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                    return m_Tanks[i];
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }

        /// <summary>
        /// Method <c>EndMessage</c> returns a string message to display at the end of each round.
        /// </summary>
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (var i = 0; i < m_Tanks.Length; ++i)
                message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }

        /// <summary>
        /// Method <c>ResetAllTanks</c> this function is used to turn all the tanks back on and reset their positions and properties.
        /// </summary>
        private void ResetAllTanks()
        {
            for (var i = 0; i < m_Tanks.Length; ++i)
                m_Tanks[i].Reset();
        }

        /// <summary>
        /// Method <c>EnableTankControl</c> used to enable control for all tanks.
        /// </summary>
        private void EnableTankControl()
        {
            for (var i = 0; i < m_Tanks.Length; ++i)
                m_Tanks[i].EnableControl();
        }

        /// <summary>
        /// Method <c>DisableTankControl</c> used to disable tank control for all units.
        /// </summary>
        private void DisableTankControl()
        {
            for (var i = 0; i < m_Tanks.Length; ++i)
                m_Tanks[i].DisableControl();
        }
    }
}
