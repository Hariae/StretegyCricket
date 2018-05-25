using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StrategyCricket.Models
{
    public class GameModel
    {
        public TeamModel BattingTeam { get; set; }
        public TeamModel BowlingTeam { get; set; }
    }

    public class PlayerModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int BattingAttribute { get; set; }
        public int BowlingAttribute { get; set; }
        public bool IsWicketKeeper { get; set; }
        public List<int> BowlingOvers { get; set; }       
    }

    public class TeamModel
    {
        public List<PlayerModel> Players { get; set; }
    }

    public class BatsmenScoreModel
    {
        public int score { get; set; }
        public int balls { get; set; }
    }

    public class BowlerScoreModel
    {
        public int runs { get; set; }
        public int wickets { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public int Overs{ get; set; }
    }



    public class TeamSelectionModel
    {
        public List<string> Teams { get; set; }
    }
}