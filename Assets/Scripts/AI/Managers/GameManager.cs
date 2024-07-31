using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Enum <c>InputActionMapNames</c> is used to store the names of the InputActionMaps.
    /// </summary>
    public enum InputActionMaps
    {
        Player1,
        Player2
        // Add additional Input Action Maps here.
    }

    /// <summary>
    /// Class <c>GameManager</c> is a singleton class that manages the game.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        /// <summary>
        /// Field <c>Instance</c> this class is a singleton; this means that only one instance of this class can exist at a time.
        /// </summary>
        private static GameManager m_instance;
        /// <summary>
        /// Property <c>Instance</c> gives access to the singleton instance of this class.
        /// </summary>
        public static GameManager Instance {
            get
            {
                if (m_instance == null)
                {
                    m_instance = FindObjectOfType<GameManager>();
                    if (m_instance == null)
                        Debug.LogError("Couldn't find GameManager!");
                }
                return m_instance;
            }
        }

        [Header("Camera")]                                                          // The header for the Camera section.
        [Tooltip("CameraRig AI Variant")] public CameraControl CameraRigControl;    // Reference to the CameraControl script for control during different phases.
        [Header("Game")]                                                            // The header for the Game section.
        [Range(1, 10)] public int NumOfRounds = 3;                                  // The number of rounds a single player has to win to win the game.
        [Range(0.001f, 15f)] public float MinutesPerRound = 3f;                     // The number of minutes per round.
        [Range(1, 10)] public int PointsRoundWin = 3;                               // The number of points a team gets for winning a round.
        [Range(0f, 10f)] public float CriticalMinutes = 0.25f;                      // The number of minutes left when the timer starts blinking.
        [Range(0f, 10f)] public float RoundStartDelay = 2.5f;                       // The delay between the start of RoundStarting and RoundPlaying phases.
        [Header("Tanks")]                                                           // The header for the Tanks section.
        [Range(0.001f, 100f)] public float Speed = 12f;                             // The speed of the tank.
        [Range(0.001f, 360f)] public float AngularSpeed = 180f;                     // The angular speed of the tank.
        [Header("Platoons")]                                                        // The header for the Platoons section.
        public PlatoonManagerPlayer PlayerPlatoon;                                  // Reference to the Player's PlatoonManager.
        public PlatoonManagerSM AIPlatoon;                                          // Reference to the AI's PlatoonManager.
        public NavMeshTriangulation NavMeshTriang;                                  // Reference to the NavMeshTriangulation.
        [HideInInspector] public int[] SpawnIndices;                                // The indices of the spawn points.
        [Header("Spawning")]                                                        // The header for the Spawning section.
        public string WalkableNavMeshArea = "Walkable";                             // The name of the walkable NavMesh area.
        [Range(1f, 200f), Tooltip("Square")] public float SpawnMaxDim = 80f;        // The maximum dimension of the spawn area.
        [Range(0f, 10f)] public float SpawnMaxHeight = 1f;                          // The maximum height of the spawn area.
        //[Header("Audio")]                                                         // The header for the Audio section.
        [HideInInspector] public bool PlayShellSFX = true;                          // Whether to play the shell SFX.
        [Header("UI")]                                                              // The header for the UI section.
        [Tooltip("UI Canvas")] public RectTransform InterfaceCanvasRectTrans;       // Reference to the Canvas UI's rect transform.
        [Range(1, 255)] public byte MaxNumOfTeams = 32;                             // The maximum number of teams allowed.
        public TMP_ColorGradient CriticalTimerGradient;                             // The timer gradient when the timer is critical.
        public Color FuelColor = new(0.901f, 1f, 0.9709285f, 1f);                   // The color of the fuel unicode character.
        public Color SkullColor = new(1f, 0f, 0.4756269f, 1f);                      // The color of the skull unicode character.
        [Range(0.01f, 5f)] public float InfoBlinkInterval = 0.4f;                   // The interval at which the info text blinks.
#if USE_OPENXML
        [Header("Extras")]                                                          // The header for the Extras section.
        public bool ExportXLSX = true;                                              // Whether to export the results to an Excel file.
        [EditorBrowsable] public string ExportConfig = @"./exportconfig.json";      // The path to the export config file.
#endif
        [HideInInspector] public bool IsRoundPlaying = false;                       // Whether the round is currently playing.

        private Canvas m_StartOptionsCanvas;                // Reference to the StartOptions Canvas.
        private TMP_Dropdown m_PlayerTeamNumDropdown;       // Reference to the Player Team Number Dropdown.
        private TMP_Dropdown m_AITeamNumDropdown;           // Reference to the AI Team Number Dropdown.
        private TextMeshProUGUI m_TimerText;                // Reference to the overlay Text to display the countdown timer.
        private TextMeshProUGUI m_StatsText;                // Reference to the overlay Text to display round statistics, etc.
        private TextMeshProUGUI m_RoundNumText;             // Reference to the overlay Text to display what round it is.
        private TextMeshProUGUI m_MatchText;                // Reference to the overlay Text to display what match it is.
        private TextMeshProUGUI m_InfoText;                 // Reference to the overlay Text to display winning text, etc.
        private TextMeshProUGUI m_ShortcutsText;            // Reference to the overlay Text to display the game shortcuts.
        private GameObject m_ErrorPanelGO;                  // Reference to the Error Panel.
        private uint m_RoundNum;                            // Which round the game is currently on.
        private WaitForSeconds m_RoundStartWaitSec;         // Used to have a delay whilst the round starts.
        private PlatoonManager m_RoundWinner;               // Reference to the winner of the current round.  Used to make an announcement of who won.
        private PlatoonManager m_GameWinner;                // Reference to the winner of the game. Used to make an announcement of who won.
        private float m_RoundTimeLeft;                      // The current time left in the round.
        private bool m_RoundStarted;                        // Whether the round has started.
        private float m_CriticalMin;                        // The minutes at which the timer becomes critical.
        private float m_CriticalSec;                        // The seconds at which the timer becomes critical.
        private TMP_ColorGradient m_DefaultTimerGradient;   // The default timer gradient.
        private InputActionMap m_GlobalInputActionMap;      // Reference to the global input action map.
        private InputActionMap m_MenuInputActionMap;        // Reference to the menu input action map.
        private InputAction m_MusicInputAction;             // Reference to the music input action.
        private InputAction m_TankSFXInputAction;           // Reference to the tank SFX input action.
        private InputAction m_ShellSFXInputAction;          // Reference to the shell SFX input action.
        private InputAction m_FullWinInputAction;           // Reference to the fullscreen/windowed input action.
        private InputAction m_ExitQuitInputAction;          // Reference to the exit/quit input action.
        private InputAction m_ContinueInputAction;          // Reference to the continue key input action.
        private bool m_ContinueKeyPerformed = false;        // Whether the continue key has been performed.
        private AudioSource m_AudioSource;                  // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
        private MatchData m_MatchData;                      // Reference to the MatchData.
        private int m_MatchNum;                             // The match number.
        private int m_MatchIdx = -1;                        // The current match index.
#if USE_OPENXML
        private MatchExportXLSX m_MatchExportXLSX;          // Reference to MatchExportXLSX.
#endif

        private const float c_MaxDepenetrationVelocity = float.PositiveInfinity; // The maximum velocity at which Rigidbody can depenetrate colliders.

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            DontDestroyOnLoad(this.gameObject);

            Init();
        }

        /// <summary>
        /// Method <c>Cleanup</c> cleans up the game match.
        /// </summary>
        private void Cleanup()
        {
            CameraRigControl.ClearTargets();

            PlayerPlatoon.Clear();
            AIPlatoon.Clear();

            m_RoundNum = 0;
        }

        /// <summary>
        /// Method <c>SetCameraTargets</c> set the camera targets.
        /// </summary>
        private void SetCameraTargets()
        {
            var cameraTargets = PlayerPlatoon.CameraTargets();
            var cameraTargetsAI = AIPlatoon.CameraTargets();
            var targets = new Transform[cameraTargets.Length + cameraTargetsAI.Length];
            cameraTargets.CopyTo(targets, 0);
            cameraTargetsAI.CopyTo(targets, cameraTargets.Length);
            
            // These are the targets the camera should follow.
            CameraRigControl.Targets = targets;
        }

        /// <summary>
        /// Method <c>DropdownTeamNumbers</c> returns a list of team numbers for the dropdown.
        /// </summary>
        private List<TMP_Dropdown.OptionData> DropdownTeamNumbers(byte maxNum = 20)
        {
            List<TMP_Dropdown.OptionData> options = new();

            for (byte i = 0; i < maxNum; ++i)
            {
                TMP_Dropdown.OptionData data = new();
                data.text = (i + 1).ToString();
                options.Add(data);
            }

            return options;
        }

        /// <summary>
        /// Method <c>Awake</c> is called when the script instance is being loaded.
        /// </summary>
        private void Init()
        {
            m_TimerText = InterfaceCanvasRectTrans.Find("Timer").GetComponent<TextMeshProUGUI>();
            m_DefaultTimerGradient = m_TimerText.colorGradientPreset;
            m_StatsText = InterfaceCanvasRectTrans.Find("Stats").GetComponent<TextMeshProUGUI>();
            m_RoundNumText = InterfaceCanvasRectTrans.Find("RoundNum").GetComponent<TextMeshProUGUI>();
            m_MatchText = InterfaceCanvasRectTrans.Find("Match").GetComponent<TextMeshProUGUI>();
            m_InfoText = InterfaceCanvasRectTrans.Find("Info").GetComponent<TextMeshProUGUI>();
            m_ShortcutsText = InterfaceCanvasRectTrans.Find("Shortcuts").GetComponent<TextMeshProUGUI>();
            m_ShortcutsText.enabled = false;
            m_ErrorPanelGO = InterfaceCanvasRectTrans.Find("ErrorPanel").gameObject;
            //m_ErrorPanelGO.SetActive(false);

            var startOptionsCanvasRectTrans = (RectTransform)InterfaceCanvasRectTrans.Find("StartOptions");
            m_StartOptionsCanvas = startOptionsCanvasRectTrans.GetComponent<Canvas>();
            m_PlayerTeamNumDropdown = startOptionsCanvasRectTrans.Find("HumanTeamNum").GetComponent<TMP_Dropdown>();
            m_AITeamNumDropdown = startOptionsCanvasRectTrans.Find("AITeamNum").GetComponent<TMP_Dropdown>();
            var teamNumbers = DropdownTeamNumbers(MaxNumOfTeams);
            m_PlayerTeamNumDropdown.AddOptions(teamNumbers);
            m_AITeamNumDropdown.AddOptions(teamNumbers);
            startOptionsCanvasRectTrans.Find("NumOfRoundsInputField").GetComponent<TMP_InputField>().text = NumOfRounds.ToString();
            startOptionsCanvasRectTrans.Find("MinPerRoundInputField").GetComponent<TMP_InputField>().text = MinutesPerRound.ToString();
            startOptionsCanvasRectTrans.Find("NumAITanksInputField").GetComponent<TMP_InputField>().text = AIPlatoon.Size.ToString();

            m_AudioSource = GetComponent<AudioSource>();

            m_GlobalInputActionMap = GetComponent<PlayerInput>().actions.FindActionMap("Global");
            m_MusicInputAction = m_GlobalInputActionMap.FindAction("Music");
            m_TankSFXInputAction = m_GlobalInputActionMap.FindAction("TankSFX");
            m_ShellSFXInputAction = m_GlobalInputActionMap.FindAction("ShellSFX");
            m_FullWinInputAction = m_GlobalInputActionMap.FindAction("FullscreenWindowed");
            m_ExitQuitInputAction = m_GlobalInputActionMap.FindAction("ExitQuit");

            m_MenuInputActionMap = GetComponent<PlayerInput>().actions.FindActionMap("Menu");
            m_ContinueInputAction = m_MenuInputActionMap.FindAction("Continue");

            GetIndicesToSpawn();

            var criticalSeconds = CriticalMinutes * 60f;
            m_CriticalMin = Mathf.FloorToInt(criticalSeconds / 60f);
            m_CriticalSec = Mathf.FloorToInt(criticalSeconds % 60f);

            m_MatchData = new();
#if USE_OPENXML
            if (ExportXLSX)
            {
                m_MatchExportXLSX = new(ExportConfig, ref m_MatchData);
                if (!m_MatchExportXLSX.IsConfigValid)
                    m_ErrorPanelGO.SetActive(true);
            }
#endif
        }

        /// <summary>
        /// Coroutine <c>OnEnable</c> is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable()
        {
            m_GlobalInputActionMap.Enable();
        }

        /// <summary>
        /// Method <c>GetIndicesToSpawn</c> gets the indices of the NavMesh triangles to spawn tanks on.
        /// </summary>
        private void GetIndicesToSpawn()
        {
            NavMeshTriang = NavMesh.CalculateTriangulation();
            SpawnIndices = new int[NavMeshTriang.indices.Length];
            var i = 0;
            foreach (var idx in NavMeshTriang.indices)
            {
                var pos = NavMeshTriang.vertices[idx];
                if (pos.x > -SpawnMaxDim && pos.x < SpawnMaxDim && pos.y < SpawnMaxHeight && pos.z > -SpawnMaxDim && pos.z < SpawnMaxDim)
                    SpawnIndices[i++] = idx;
            }
            Array.Resize(ref SpawnIndices, i);
        }

        /// <summary>
        /// Method <c>Start</c> is called on the frame when a script is enabled just before any of the Update methods are called the first time.
        /// </summary>
        private void Start()
        {
            // This line fixes a change to the physics engine.
            Physics.defaultMaxDepenetrationVelocity = c_MaxDepenetrationVelocity;
        
            // Subscribe to the F5 key event.
            m_MusicInputAction.performed += MusicToggle;
            // Subscribe to the F6 key event.
            m_TankSFXInputAction.performed += TankSFXToggle;
            // Subscribe to the F7 key event.
            m_ShellSFXInputAction.performed += ShellSFXToggle;
            // Subscribe to the F12 performed event.
            m_FullWinInputAction.canceled += FullscreenWindowed;
            // Subscribe to the Esc key up event.
            m_ExitQuitInputAction.canceled += EscapeKeyUp;

            // Create the delays so they only have to be made once.
            m_RoundStartWaitSec = new WaitForSeconds(RoundStartDelay);

            // Once the tanks have been created and the camera is using them as targets, start the game.
            StartCoroutine(GameLoop());
        }

        /// <summary>
        /// Method <c>OnSetup</c> is called when the tanks have been created and the camera is using them as targets.
        /// </summary>
        private void OnSetup()
        {
            PlayerPlatoon.Initialize();
            AIPlatoon.Initialize();

            PlayerPlatoon.NumTanksLeftCalc += UpdateStats;
            AIPlatoon.NumTanksLeftCalc += UpdateStats;

            SetCameraTargets();
        }

        /// <summary>
        /// Method <c>UpdateStats</c> updates the stats text.
        /// </summary>
        public void UpdateStats()
        {
            var skull = $"<color=#{ColorUtility.ToHtmlStringRGB(SkullColor)}>\u2620</color>";
            //var skullPlayer = $"<color=#{ColorUtility.ToHtmlStringRGB(SkullColor)}>\uD83D\uDC80</color>";
            var fuel = $"<color=#{ColorUtility.ToHtmlStringRGB(FuelColor)}>\u26FD</color>";
            //var fuel = $"<color=#{ColorUtility.ToHtmlStringRGB(FuelColor)}>\uD83D\uDE80</color>";
            
            string stats = "";

            for (var i = 0; i < AIPlatoon.NumTanks - AIPlatoon.NumTanksLeft; ++i)
                stats += skull;
            for (var i = 0; i < AIPlatoon.NumTanksLeft; ++i)
                stats += fuel;
            stats += $" <color=#{AIPlatoon.RGB}>AI</color>\n";

            for (var i = 0; i < PlayerPlatoon.NumTanks - PlayerPlatoon.NumTanksLeft; ++i)
                stats += skull;
            for (var i = 0; i < PlayerPlatoon.NumTanksLeft; ++i)
                stats += fuel;
            stats += $" <color=#{PlayerPlatoon.RGB}>Human</color>";

            m_StatsText.text = stats;
        }

        /// <summary>
        /// Method <c>OnDestroy</c> is called when the behaviour becomes disabled.
        /// </summary>
        private void OnDisable()
        {
            // Unsubscribe from the F5 key event.
            m_MusicInputAction.performed -= MusicToggle;
            // Unsubscribe from the F6 key event.
            m_TankSFXInputAction.performed -= TankSFXToggle;
            // Unsubscribe from the F7 key event.
            m_ShellSFXInputAction.performed -= ShellSFXToggle;
            // Unsubscribe from the F12 performed event.
            m_FullWinInputAction.canceled -= FullscreenWindowed;
            // Unsubscribe from the Esc key up event.
            m_ExitQuitInputAction.canceled -= EscapeKeyUp;

            m_GlobalInputActionMap.Disable();
        }

        /// <summary>
        /// Method <c>MusicToggle</c> toggle AudioSource mute.
        /// </summary>
        private void MusicToggle(InputAction.CallbackContext cc)
        {
            m_AudioSource.mute ^= true;
        }

        /// <summary>
        /// Method <c>TankSFXToggle</c> toggle tank sound FX.
        /// </summary>
        private void TankSFXToggle(InputAction.CallbackContext cc)
        {
            foreach (var tank in PlayerPlatoon.Tanks)
                foreach (var audioSource in tank.AudioSources)
                    audioSource.mute ^= true;
            foreach (var tank in AIPlatoon.Tanks)
                foreach (var audioSource in tank.AudioSources)
                    audioSource.mute ^= true;
        }

        /// <summary>
        /// Method <c>ShellSFXToggle</c> toggle shell sound FX.
        /// </summary>
        private void ShellSFXToggle(InputAction.CallbackContext cc) => PlayShellSFX ^= true;

        /// <summary>
        /// Method <c>FullscreenWindowed</c> toggle fullscreen.
        /// </summary>
        private void FullscreenWindowed(InputAction.CallbackContext cc)
        {
            if (Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen)
                Screen.fullScreenMode = FullScreenMode.Windowed;
            else
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
        }

        /// <summary>
        /// Method <c>EscapeKeyUp</c> exit the game.
        /// </summary>
        private void EscapeKeyUp(InputAction.CallbackContext cc) => Application.Quit();

        /// <summary>
        /// Method <c>Update</c> call UpdateTimer.
        /// </summary>
        private void Update()
        {
            UpdateTimer();
        }

        /// <summary>
        /// Method <c>UpdateTimer</c> update the timer.
        /// </summary>
        private void UpdateTimer()
        {
            if (m_RoundStarted)
                m_RoundTimeLeft = MinutesPerRound * 60f;
            else if (IsRoundPlaying)
                m_RoundTimeLeft = Mathf.Max(0f, m_RoundTimeLeft - Time.deltaTime);

            float minutes = Mathf.FloorToInt(m_RoundTimeLeft / 60f);
            float seconds = Mathf.FloorToInt(m_RoundTimeLeft % 60f);
            m_TimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);

            if (minutes <= m_CriticalMin && seconds <= m_CriticalSec)
                m_TimerText.colorGradientPreset = CriticalTimerGradient;
        }

        /// <summary>
        /// Coroutine <c>BreakContinueKeyPerformed</c>
        /// </summary>
        private IEnumerator BreakContinueKeyPerformed()
        {
            yield return new WaitForSecondsRealtime(0.2f);
        }

        /// <summary>
        /// Coroutine <c>GameLoop</c> this is called from start and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator GameLoop()
        {
            // Start off by running the 'GameStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(GameStarting());

            // Running the 'RoundStarting' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundStarting());

            // Once the 'RoundStarting' coroutine is finished, run the 'RoundPlaying' coroutine but don't return until it's finished.
            yield return StartCoroutine(RoundPlaying());

            // Once execution has returned here, run the 'RoundEnding' coroutine, again don't return until it's finished.
            yield return StartCoroutine(RoundEnding());

            // This code is not run until 'RoundEnding' has finished. At which point, check if a game winner has been found.
            if (m_RoundNum == NumOfRounds)
            {
                m_MatchData.AmendAccPoints(AIPlatoon.AccPoints, PlayerPlatoon.AccPoints);
#if USE_OPENXML
                if (ExportXLSX)
                    m_MatchExportXLSX.Write(m_MatchIdx);
#endif

                Cleanup();

                yield return StartCoroutine(BreakContinueKeyPerformed());
            }

            // This coroutine doesn't yield. This means that the current version of the GameLoop will end.
            StartCoroutine(GameLoop());
        }

        /// <summary>
        /// Coroutine <c>GameStarting</c> this is called from GameLoop and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator GameStarting()
        {            
            m_TimerText.enabled = false;
            m_StatsText.enabled = false;
            m_RoundNumText.enabled = false;
            m_MatchText.enabled = true;
            m_InfoText.enabled = false;
            m_ShortcutsText.enabled = true;

            if (m_RoundNum == 0)
            {
                ++m_MatchNum;
                TeamChanged();

                m_StartOptionsCanvas.enabled = true;

                // Subscribe to the ContinueInputAction delegate.
                m_ContinueKeyPerformed = false;
                m_MenuInputActionMap.Enable();
                m_ContinueInputAction.performed += _ => m_ContinueKeyPerformed = true;

                while (!m_ContinueKeyPerformed) // Wait for the layer to continue until yielding control back to the game loop.
                    yield return null;
            }
            else
                yield return new WaitForSecondsRealtime(0f); // Move on.
        }

        /// <summary>
        /// Coroutine <c>RoundStarting</c> this is called from GameLoop and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator RoundStarting()
        {
            m_ShortcutsText.enabled = false;

            // Unsubscribe from the Continue key event.
            m_ContinueInputAction.performed -= _ => m_ContinueKeyPerformed = true;
            m_MenuInputActionMap.Disable();
            m_ContinueKeyPerformed = false;

            if (m_RoundNum == 0)
            {
                m_StartOptionsCanvas.enabled = false;

                OnSetup();

                m_MatchIdx = m_MatchData.AddResult(m_AITeamNumDropdown.value + 1, PlayerPlatoon.NumTanks,
                                                   m_PlayerTeamNumDropdown.value + 1, AIPlatoon.NumTanks,
                                                   NumOfRounds);
            }

            // As soon as the round starts reset the tanks and make sure they can't move.
            PlayerPlatoon.ResetAllTanks();
            PlayerPlatoon.DisableTankControls();
            AIPlatoon.ResetAllTanks();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            CameraRigControl.SetStartPositionAndSize();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNum++;
            m_InfoText.text = m_RoundNumText.text = $"Round {m_RoundNum}/{NumOfRounds}";
            m_RoundNumText.enabled = true;
            m_InfoText.enabled = true;

            m_TimerText.colorGradientPreset = m_DefaultTimerGradient;
            m_TimerText.enabled = true;

            m_RoundStarted = true;
            IsRoundPlaying = false;

            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_RoundStartWaitSec;
        }

        /// <summary>
        /// Coroutine <c>RoundPlaying</c> this is called from GameLoop and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator RoundPlaying()
        {
            m_StatsText.enabled = true;
            m_InfoText.enabled = false;

            // As soon as the round begins playing let the players control the tanks.
            PlayerPlatoon.EnableTankControls();

            m_RoundStarted = false;
            IsRoundPlaying = true;

            // While there are not zero tanks of one team left...
            while (!ZeroTimeLeft() &&
                   !PlayerPlatoon.ZeroTanksLeft() &&
                   !AIPlatoon.ZeroTanksLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        }

        /// <summary>
        /// Coroutine <c>RoundEnding</c> this is called from GameLoop and will run each phase of the game one after another.
        /// </summary>
        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving.
            PlayerPlatoon.DisableTankControls();

            // Subscribe to the ContinueInputAction delegate.
            m_MenuInputActionMap.Enable();
            m_ContinueInputAction.performed += _ => m_ContinueKeyPerformed = true;

            // See if there is a winner now the round is over.
            RoundResultCalc();
  
            // Get a message based on the scores and whether or not there is a game winner and display it.
            m_InfoText.enabled = true;
            m_InfoText.text = EndMessage();
            
            m_ShortcutsText.enabled = true;

            m_RoundStarted = false;
            IsRoundPlaying = false;

            m_MatchData.AccumulateTime(MinutesPerRound * 60f - m_RoundTimeLeft);

            // Wait for the layer to continue until yielding control back to the game loop.
            while (!m_ContinueKeyPerformed)
                yield return null;
        }

        /// <summary>
        /// Method <c>ZeroTimeLeft</c>
        /// </summary>
        private bool ZeroTimeLeft()
        {
            return m_RoundTimeLeft <= 0f;
        }

        /// <summary>
        /// Method <c>RoundResult</c> this function is to find out if there is a winner of the round.
        /// <para>
        /// This function is called with the assumption that 1 or fewer tanks are currently active.
        /// </para>
        /// </summary>
        private void RoundResultCalc()
        {
            m_RoundWinner = null;

            PlayerPlatoon.NumTanksLeftActive();
            AIPlatoon.NumTanksLeftActive();

            float playerPts = (float)PointsRoundWin / AIPlatoon.NumTanks * (AIPlatoon.NumTanks - AIPlatoon.NumTanksLeft);
            float aiPts = 0f;

            if (PlayerPlatoon.NumTanksLeft == 0)
            {
                aiPts = PointsRoundWin;
                m_RoundWinner = AIPlatoon;
            }
            else if (AIPlatoon.NumTanksLeft == 0)
            {
                playerPts = PointsRoundWin;
                m_RoundWinner = PlayerPlatoon;
            }
            // If none of the tanks are active, it is a draw so m_RoundWinner equals null.

            var roundIdx = m_RoundNum - 1;
            PlayerPlatoon.RoundPoints[roundIdx] = playerPts;
            AIPlatoon.RoundPoints[roundIdx] = aiPts;
            PlayerPlatoon.AccPoints += PlayerPlatoon.RoundPoints[roundIdx];
            AIPlatoon.AccPoints += AIPlatoon.RoundPoints[roundIdx];

            if (m_RoundWinner != null)
                m_RoundWinner.NumWins++;
        }

        /// <summary>
        /// Method <c>GameWinnerCalc</c> this function is to find out if there is a winner of the game.
        /// </summary>
        private PlatoonManager GameWinnerCalc()
        {
            if (PlayerPlatoon.AccPoints > AIPlatoon.AccPoints)
                return PlayerPlatoon;
            else if (AIPlatoon.AccPoints > PlayerPlatoon.AccPoints)
                return AIPlatoon;

            return null;
        }

        /// <summary>
        /// Method <c>EndMessage</c> returns a string message to display at the end of each round.
        /// </summary>
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            var message = $"Round {m_RoundNum} is a Draw!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = $"{m_RoundWinner.CategoryRGB} Wins Round {m_RoundNum}!";

            // If there is a game winner, change the message to reflect that.
            if (m_RoundNum == NumOfRounds)
            {
                // See if someone has won the game.
                m_GameWinner = GameWinnerCalc();
                if (m_GameWinner != null)
                    message = $" {m_GameWinner.CategoryRGB} Wins the Match\nafter {m_RoundNum} Rounds!";
                else
                    message = $"Game Over!\nIt's a Draw after {m_RoundNum} Rounds!";
            }

            // Go through all the tanks and add each of their scores to the message.
            var roundIdx = m_RoundNum - 1;
            message += $"\n\n{PlayerPlatoon.CategoryRGB}:  <color=#{PlayerPlatoon.RGB}>{PlayerPlatoon.RoundPoints[roundIdx]}  Pts</color>";
            message += $"\t{AIPlatoon.CategoryRGB}:  <color=#{AIPlatoon.RGB}>{AIPlatoon.RoundPoints[roundIdx]}  Pts</color>";
            message += $"\n\n<color=#{PlayerPlatoon.RGB}>Acc. Score</color>:  <color=#{PlayerPlatoon.RGB}>{PlayerPlatoon.AccPoints}  Pts</color>   <i>\u2044</i>   <color=#{PlayerPlatoon.RGB}>{PlayerPlatoon.NumWins}  Wins</color>";
            message += $"\n<color=#{AIPlatoon.RGB}>Acc. Score</color>:  <color=#{AIPlatoon.RGB}>{AIPlatoon.AccPoints}  Pts</color>   <i>\u2044</i>   <color=#{AIPlatoon.RGB}>{AIPlatoon.NumWins}  Wins</color>";

            return message;
        }

        /// <summary>
        /// Method <c>NumOfRoundsChanged</c> used to set the number of rounds.
        /// </summary>
        public void NumOfRoundsChanged(String numRoundsStr)
        {
            if (int.TryParse(numRoundsStr, out int numRounds))
                NumOfRounds = numRounds;
        }

        /// <summary>
        /// Method <c>MinPerRoundChanged</c> used to set the minutes per rounds.
        /// </summary>
        public void MinPerRoundChanged(String minPerRoundStr)
        {
            if (float.TryParse(minPerRoundStr, out float minPerRound))
                MinutesPerRound = minPerRound;
        }

        /// <summary>
        /// Method <c>NumAITanksChanged</c> used to set the number of AI tanks.
        /// </summary>
        public void NumAITanksChanged(String numAITanksStr)
        {
            if (int.TryParse(numAITanksStr, out int numAITanks))
                AIPlatoon.Size = numAITanks;
        }

        /// <summary>
        /// Method <c>TeamChanged</c> used to set the team number in the HUD.
        /// </summary>
        public void TeamChanged(Int32 _ = 0) => m_MatchText.text = 
            $"Match {m_MatchNum}\nAI Team {m_AITeamNumDropdown.value + 1} vs Human Team {m_PlayerTeamNumDropdown.value + 1}";
    }
}
