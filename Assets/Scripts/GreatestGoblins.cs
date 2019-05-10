using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreatestGoblins : MonoBehaviour
{
    public Score[] HighScores;
    private static GreatestGoblins Instance;

    void Awake()
    {
        if (!Instance) Instance = this;
    }


    public struct Score
    {
        public string Name;
        public int Kills;
        public int Treasures;
        public int GoblinsInTribe;
        public int AreasExplored;
    }

    public static int TotalScore(Score score)
    {
        return score.Kills + score.Treasures + score.GoblinsInTribe + score.AreasExplored;
    }


}
