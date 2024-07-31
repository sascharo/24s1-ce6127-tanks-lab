using System;
using System.Collections.Generic;

using Debug = UnityEngine.Debug;

namespace CE6127.Tanks.AI
{
    /// <summary>
    /// Class <c>MatchData</c>
    /// </summary>
    [Serializable]
    internal class MatchData
    {
        /// <summary>
        /// Class <c>Results</c> holding the export data.
        /// </summary>
        public class Result
        {
            /// <summary>
            /// Class <c>Team</c> holding the team data.
            /// </summary>
            public class Team
            {
                public int Number { get; set; }         // Implicit field team number.
                public int Size { get; set; }           // Implicit field for the number of tanks in team.
                public float AccPoints { get; set; }    // Implicit field holding the accumulated points.
            }

            public Team TeamAI = new();             // AI team data.
            public Team TeamPlayer = new();         // Player team data.
            public int NumOfRounds { get; set; }    // Implicit field for the number of rounds played.
            public float AccTimeInSec { get; set; } // Implicit field for the accumulated time in seconds.

            public Result(int aiTeamNum, int aiTeamSize, int playerTeamNum, int playerTeamSize, int numOfRounds)
            {
                TeamAI.Number = aiTeamNum;
                TeamAI.Size = aiTeamSize;
                TeamPlayer.Number = playerTeamNum;
                TeamPlayer.Size = playerTeamSize;
                NumOfRounds = numOfRounds;
            }
        }

        public List<Result> Results = new(); // List of results.

        private int LastIdx; // Index of last added result.

        /// <summary>
        /// Method <c>AddResult</c>
        /// </summary>
        public int AddResult(int aiTeamNum, int aiTeamSize, int playerTeamNum, int playerTeamSize, int numOfRounds)
        {
            Result result = new(aiTeamNum, aiTeamSize, playerTeamNum, playerTeamSize, numOfRounds);
            Results.Add(result);

            LastIdx = Results.IndexOf(result);

            return LastIdx;
        }

#nullable enable
        /// <summary>
        /// Method <c>AccumulateTime</c>
        /// </summary>
        public float AccumulateTime(float time, int? idx = null)
        {
            if (idx == null)
                idx = LastIdx;
            var i = (int)idx;
            
            Results[i].AccTimeInSec += time;

            return Results[i].AccTimeInSec;
        }

        /// <summary>
        /// Method <c>AmendAccPoints</c>
        /// </summary>
        public void AmendAccPoints(float aiTeamAccPts, float playerTeamAccPts, int? idx = null)
        {
            if (idx == null)
                idx = LastIdx;
            var i = (int)idx;
            Results[i].TeamAI.AccPoints = aiTeamAccPts;
            Results[i].TeamPlayer.AccPoints = playerTeamAccPts;
        }
#nullable disable
    }
}
