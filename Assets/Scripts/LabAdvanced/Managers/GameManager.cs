using System.Collections;
using UnityEngine;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.Advanced
{
    public class GameManager : MonoBehaviour
    {
        protected TankManager m_TankManager;    // Reference to the tank manager script, used to disable control when a game ends.

        private int[] m_WinHistory;             // History of wins for each player. Used to determine the winner of the game.

        const float k_MaxDepenetrationVelocity = float.PositiveInfinity; // This is to fix a change to the physics engine

        // State machine for the game. This is used to control the flow of the game.
        public enum SMState
        {
            GameLoads = 0,  // This is the default state of the game.
            GamePrep,       // This state is used to initialize the game.
            GameLoop,       // This state is used to run the game.
            GameEnds        // This state is used to end the game.
        };
        private SMState m_State = SMState.GameLoads; // This is the current state of the game.

        private void Awake()
        {
            m_TankManager = GetComponent<TankManager>();
        }

        private void Start()
        {
            // This line fixes a change to the physics engine.
            Physics.defaultMaxDepenetrationVelocity = k_MaxDepenetrationVelocity;

            m_WinHistory = new int[m_TankManager.NumberOfPlayers];
            m_TankManager.OneTankLeft += OnLastTank;

            State = SMState.GamePrep;
        }

        public void OnLastTank(Tank winner)
        {
            // Check if the game is in the game loop state.
            if (State == SMState.GameLoop)
            {
                // Winning player.
                m_WinHistory[winner.PlayerNum]++;

                // End the round.
                State = SMState.GameEnds;
            }
        }

        private void InitGamePrep()
        {
            // Initialize all tanks.
            m_TankManager.Restart();

            // Change state to game loop.
            State = SMState.GameLoop;
        }

        private IEnumerator InitGameEnd()
        {
            // Delay before starting a new round.
            yield return new WaitForSeconds(3f);

            // Reinitialize tanks.
            State = SMState.GamePrep;
        }

        public SMState State
        {
            get { return m_State; }
            set
            {
                if (m_State != value)
                {
                    m_State = value;

                    switch (value)
                    {
                        case SMState.GamePrep:
                            {
                                InitGamePrep();
                                break;
                            }
                        case SMState.GameLoop:
                            {
                                break;
                            }
                        case SMState.GameEnds:
                            {
                                StartCoroutine(InitGameEnd());
                                break;
                            }
                        default:
                            break;
                    }
                }
            }
        }
    }
}
