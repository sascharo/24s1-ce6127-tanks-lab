/*
 * GameManager.cs
 * 
 * This script manages the overall game state in a Unity game. It handles the game state transitions, win history tracking, and interactions with the TankManager.
 * 
 * Features:
 * - State machine to control the flow of the game (GameLoads, GamePrep, GameLoop, GameEnds).
 * - Initialization and reinitialization of tanks at different game states.
 * - Tracking of win history for each player.
 * - Handling of game-ending conditions when only one tank is left.
 * 
 * Components:
 * - TankManager tankManager: Reference to the tank manager script.
 * - int[] winHistory: Array to store the win history for each player.
 * - const float k_MaxDepenetrationVelocity: Constant to fix a change in the physics engine.
 * 
 * Methods:
 * - Awake: Initializes the tank manager reference.
 * - Start: Sets up the initial game state and fixes the physics engine issue.
 * - OnLastTank: Handles the event when only one tank is left.
 * - InitGamePrep: Prepares the game for a new round.
 * - InitGameEnd: Ends the current round and prepares for the next one after a delay.
 * - CurrentGameState (Property): Manages the current state of the game and triggers state-specific actions.
 */

using System.Collections;
using UnityEngine;

namespace CE6127.Tanks.Advanced
{
    public class GameManager : MonoBehaviour
    {
        protected TankManager tankManager; // Reference to the tank manager script, used to disable control when a game ends.

        private int[] winHistory;          // History of wins for each player. Used to determine the winner of the game.

        // This is to fix a change to the physics engine.
        const float k_MaxDepenetrationVelocity = float.PositiveInfinity;

        // CurrentGameState machine for the game. This is used to control the flow of the game.
        public enum GameState
        {
            GameLoads = 0,  // This is the default state of the game.
            GamePrep,       // This state is used to initialize the game.
            GameLoop,       // This state is used to run the game.
            GameEnds        // This state is used to end the game.
        };
        private GameState currentState = GameState.GameLoads; // This is the current state of the game.

        private IEnumerator InitGameEnd()
        {
            // Delay before starting a new round.
            yield return new WaitForSeconds(3f);

            // Reinitialize tanks.
            CurrentGameState = GameState.GamePrep;
        }

        public GameState CurrentGameState
        {
            get { return currentState; }
            set
            {
                if (currentState != value)
                {
                    currentState = value;

                    switch (value)
                    {
                        case GameState.GamePrep:
                            {
                                InitGamePrep();
                                break;
                            }
                        case GameState.GameLoop:
                            {
                                break;
                            }
                        case GameState.GameEnds:
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

        private void Awake()
        {
            tankManager = GetComponent<TankManager>();
        }

        private void Start()
        {
            // This line fixes a change to the physics engine.
            Physics.defaultMaxDepenetrationVelocity = k_MaxDepenetrationVelocity;

            winHistory = new int[tankManager.NumberOfPlayers];
            tankManager.OneTankLeft += OnLastTank;

            CurrentGameState = GameState.GamePrep;
        }

        public void OnLastTank(Tank winner)
        {
            // Check if the game is in the game loop state.
            if (CurrentGameState == GameState.GameLoop)
            {
                // Winning player.
                winHistory[winner.playerNum]++;

                // End the round.
                CurrentGameState = GameState.GameEnds;
            }
        }

        private void InitGamePrep()
        {
            // Initialize all tanks.
            tankManager.Restart();

            // Change state to game loop.
            CurrentGameState = GameState.GameLoop;
        }
    }
}
