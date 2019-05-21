using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GreatestGoblins : MonoBehaviour
{
    public List<Score> HighScores = new List<Score>();
    public Score ActiveScore;
    [SerializeField]
    private HighScoreEntry HighScoreEntry = null;
    private static GreatestGoblins Instance;

    void Awake()
    {
        if (!Instance) Instance = this;
    }

    public enum ScoreCount { Kill,Treasure,GoblinInTribe,AreaExplored,LeaderLevel, Equipment}

    [Serializable]
    public struct Score
    {
        public string Name;
        public int Kills;
        public int Treasures;
        public int GoblinsInTribe;
        public int AreasExplored;
        public int LeaderLevel;
        //TODO: check for finding the same equipment
        public int EquipmentFound;
        //FOOD?
    }

    public static int TotalScore(Score score)
    {
        return score.Kills + score.Treasures + score.GoblinsInTribe + score.AreasExplored;
    }

    public static void NewLeader(Goblin leader)
    {
        Instance.AddLeader(leader);
    }

    private void AddLeader(Goblin leader)
    {
        if (!string.IsNullOrEmpty(ActiveScore.Name))
        {
            HighScores.Add(ActiveScore);
        }
        ActiveScore = new Score
        {
            Name = leader.name,
            GoblinsInTribe = leader.Team.Members.Count,
            LeaderLevel = leader.CurrentLevel
        };

        leader.Team.OnTeamKill.AddListener(()=> AddCount(ScoreCount.Kill));
        leader.Team.OnEquipmentFound.AddListener((e,g) => AddCount(ScoreCount.Equipment));
        leader.Team.OnTreasureFound.AddListener(i => AddCount(ScoreCount.Treasure,i));
        leader.Team.OnMemberAdded.AddListener(() => AddCount(ScoreCount.GoblinInTribe));
        leader.OnLevelUp.AddListener(() => AddCount(ScoreCount.LeaderLevel));
    }

    private void AddCount(ScoreCount scoretype, int arg0 = 1)
    {
        if(arg0 < 1) return;

        switch (scoretype)
        {
            case ScoreCount.Kill:
                ActiveScore.Kills += arg0;
                break;
            case ScoreCount.Treasure:
                ActiveScore.Treasures += arg0;
                break;
            case ScoreCount.GoblinInTribe:
                ActiveScore.GoblinsInTribe += arg0;
                break;
            case ScoreCount.AreaExplored:
                ActiveScore.AreasExplored += arg0;
                break;
            case ScoreCount.LeaderLevel:
                ActiveScore.LeaderLevel += arg0;
                break;
            case ScoreCount.Equipment:
                ActiveScore.EquipmentFound += arg0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(scoretype), scoretype, null);
        }
    }

    private void CreateScoreScreen()
    {
        if(!ScoresContainName(ActiveScore.Name))
            HighScores.Add(ActiveScore);

        HighScores.Sort((x, y) => -(TotalScore(x).CompareTo(TotalScore(y))));
        foreach (var score in HighScores)
        {
            var e = Instantiate(HighScoreEntry, HighScoreEntry.transform.parent);
            e.Name.text = score.Name.Contains("Chief") ? score.Name.Remove(0, 6) : score.Name;
            e.Value.text = TotalScore(score).ToString();
            e.Score = score;
        }
        HighScoreEntry.gameObject.SetActive(false);
    }

    public static void ShowHighscores()
    {
        Instance.CreateScoreScreen();
    }

    public static List<Score> GetScores() => Instance.HighScores;

    public static bool ScoresContainName(string name) => Instance.HighScores.Any(s => s.Name == name);

    internal static void SetScores(List<Score> list)
    {
        if(list == null|| !list.Any())
            return;

        Instance.HighScores = list;
    }
}
