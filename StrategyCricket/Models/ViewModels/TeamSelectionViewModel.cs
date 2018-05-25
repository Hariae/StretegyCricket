using StategyCricket.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace StrategyCricket.Models.ViewModels
{
    public class TeamSelectionViewModel
    {
        public List<Team> Teams { get; set; }
        public string FirstTeam { get; set; }
        public string SecondTeam { get; set; }
        public List<Player> FirstTeamPlayers { get; set; }
        public List<Player> SecondTeamPlayers { get; set; }
        public List<string> FirstTeamSelectedPlayers { get; set; }
        public List<string> SecondTeamSelectedPlayers { get; set; }        
        public List<string> FirstTeamBowlingLineup { get; set; }
        public List<string> SecondTeamBowlingLineup { get; set; }
    }
}