using StategyCricket.DataAccess;
using StrategyCricket.Business;
using StrategyCricket.Models;
using StrategyCricket.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace StrategyCricket.Controllers
{
    public class GameController : Controller
    {
        // GET: Game
        public ActionResult Index()
        {
            return View();
        }
       

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult TeamSelectionInitialize()
        {
            TeamSelectionViewModel teamSelectionViewmodel = new TeamSelectionViewModel();
            //Getting Teams
            StrategyCricketEntities model = new StrategyCricketEntities();
            List<Team> teamsFromDB = new List<Team>();
            teamsFromDB = model.Teams.Where(x => x.Team1 != null).ToList();
            teamSelectionViewmodel.Teams = teamsFromDB;

            return View("TeamSelection", teamSelectionViewmodel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamSelectionViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult TeamSelectionGetPlayers(TeamSelectionViewModel teamSelectionViewModel)
        {
            StrategyCricketEntities model = new StrategyCricketEntities();
                        
            teamSelectionViewModel.FirstTeamPlayers = model.Players.Where(x => x.Team.Equals(teamSelectionViewModel.FirstTeam)).ToList();            
            teamSelectionViewModel.SecondTeamPlayers = model.Players.Where(x => x.Team.Equals(teamSelectionViewModel.SecondTeam)).ToList();

            //TOSS

            Random random = new Random();
            int toss = random.Next(1, 3);

            if (toss != 1)
            {
                var temp = teamSelectionViewModel.FirstTeamPlayers;
                teamSelectionViewModel.FirstTeamPlayers = teamSelectionViewModel.SecondTeamPlayers;
                teamSelectionViewModel.SecondTeamPlayers = temp;

                var tempBowling = teamSelectionViewModel.FirstTeamBowlingLineup;
                teamSelectionViewModel.FirstTeamBowlingLineup = teamSelectionViewModel.SecondTeamBowlingLineup;
                teamSelectionViewModel.SecondTeamBowlingLineup = tempBowling;

                var tempTeamName = teamSelectionViewModel.FirstTeam;
                teamSelectionViewModel.FirstTeam = teamSelectionViewModel.SecondTeam;
                teamSelectionViewModel.SecondTeam = tempTeamName;

                var order = new List<int> { 3, 2, 1, 0, 4, 6, 5, 7, 8, 9, 11 };
                teamSelectionViewModel.FirstTeamPlayers = order.Select(i => teamSelectionViewModel.FirstTeamPlayers[i]).ToList();


                order = new List<int> { 1, 2, 3, 0, 4, 5, 6, 7, 8, 9, 10 };
                teamSelectionViewModel.SecondTeamPlayers = order.Select(i => teamSelectionViewModel.SecondTeamPlayers[i]).ToList();
            }
            else
            {
                var order = new List<int> { 1, 2, 3, 0, 4, 5, 6, 7, 8, 9, 10 };
                teamSelectionViewModel.FirstTeamPlayers = order.Select(i => teamSelectionViewModel.FirstTeamPlayers[i]).ToList();


                order = new List<int> { 3, 2, 1, 0, 4, 6, 5, 7, 8, 9, 11 };
                teamSelectionViewModel.SecondTeamPlayers = order.Select(i => teamSelectionViewModel.SecondTeamPlayers[i]).ToList();
            }
            


            

            return View("TeamSelection", teamSelectionViewModel);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="teamSelectionViewModel"></param>
        /// <returns></returns>
        [HttpPost]
       public ActionResult SaveTeamPlayers(TeamSelectionViewModel teamSelectionViewModel)
        {

            GameViewModel gameViewModel = new GameViewModel();
            gameViewModel.Team_1_BatsmenIds = new List<int>();
            gameViewModel.Team_2_BatsmenIds = new List<int>();
            gameViewModel.Team_1_BowlerIds = new List<int>();
            gameViewModel.Team_2_BowlerIds = new List<int>();

            //Filling PLayers
            for (int i=0;i<11;i++)
            {
                gameViewModel.Team_1_BatsmenIds.Add(int.Parse(teamSelectionViewModel.FirstTeamSelectedPlayers[i]));
                gameViewModel.Team_2_BatsmenIds.Add(int.Parse(teamSelectionViewModel.SecondTeamSelectedPlayers[i]));
            }

            //Assigning Bowling Line up
            for(int j=0;j<20;j++)
            {
                gameViewModel.Team_1_BowlerIds.Add(int.Parse(teamSelectionViewModel.FirstTeamBowlingLineup[j]));
                gameViewModel.Team_2_BowlerIds.Add(int.Parse(teamSelectionViewModel.SecondTeamBowlingLineup[j]));
            }

            //RedirectToAction("GameSimulator", gameViewModel);
            TempData["gameViewModel"] = gameViewModel;
            return RedirectToAction("GameSimulator");


        }


        /// <summary>
        /// The InitializeGame invokes this method. The main method which carries out the Game. Invokes GameManager and acts as an interface to seperate business code from main controller.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //[HttpPost]
        public ActionResult GameSimulator(GameViewModel model)
        {
            int[] result = new int[120];


            model = (GameViewModel)TempData["gameViewModel"];

            //Invoke Initialize teams
            GameModel game = InitializeTeams(model);

            //GameManager invocation
            GameManager gameManager = new GameManager();
            int target = int.MaxValue;
            result = gameManager.SimulateGame(game, target);
           
            //int[] batsmenScore = gameManager.FormatGame(result);

            BatsmenScoreModel[] batsmenScoreModel = gameManager.FormatGame(result, int.MaxValue);
            BowlerScoreModel[] bowlerScoreModel = gameManager.FormatBowlersData(result, game);
 
            //Write Score to File
            writeScoresToFile(game, result, batsmenScoreModel, bowlerScoreModel, "First Innings", target);

            int firstInningsTotal = 0;
            foreach (int run in result)
            {
                if (run != -1)
                {
                    firstInningsTotal = firstInningsTotal + run;
                }
            }
            //End of 1st Innings

            GameModel secondInningsGame = new GameModel();
            secondInningsGame.BattingTeam = game.BowlingTeam;
            secondInningsGame.BowlingTeam = game.BattingTeam;

            int[] secondInningsScore = new int[120];
            secondInningsScore = gameManager.SimulateGame(secondInningsGame, firstInningsTotal);
            BatsmenScoreModel[] secondInningsBatsmenScoreModel = gameManager.FormatGame(secondInningsScore, target);
            BowlerScoreModel[] secondInningsBowlerScoreModel = gameManager.FormatBowlersData(secondInningsScore, secondInningsGame);


            writeScoresToFile(secondInningsGame, secondInningsScore, secondInningsBatsmenScoreModel, secondInningsBowlerScoreModel, "Second Innings", firstInningsTotal);



            ViewBag.firstInningsResult = result;
            ViewBag.firstInningsBatsmenScore = batsmenScoreModel;
            ViewBag.firstInningsBowlerScore = bowlerScoreModel;
            ViewBag.firstInningsBattingTeam = game.BattingTeam;
            ViewBag.firstInningsGameModel = game;

            ViewBag.secondInningsResult = secondInningsScore;
            ViewBag.secondInningsBatsmenScore = secondInningsBatsmenScoreModel;
            ViewBag.secondInningsBowlerScore = secondInningsBowlerScoreModel;
            ViewBag.secondInningsBattingTeam = secondInningsGame.BattingTeam;
            ViewBag.secondInningsGameModel = secondInningsGame;
            ViewBag.target = firstInningsTotal;
            return View();
        }



        /// <summary>
        /// This methods makes the DB interaction to get player details and initializes the complete teams for the game.
        /// </summary>
        /// <param name="Team_1_BatsmenIds"></param>
        /// <param name="Team_2_BatsmenIds"></param>
        /// <param name="Team_1_BowlerIds"></param>
        /// <param name="Team_2_BowlerIds"></param>
        /// <returns></returns>
        [HttpPost]
        public GameModel InitializeTeams(GameViewModel gameViewModel)
        {
            //Assigment                               
            List<int> Team_1_BatsmenIds = gameViewModel.Team_1_BatsmenIds;
            List<int> Team_2_BatsmenIds = gameViewModel.Team_2_BatsmenIds;
            List<int> Team_1_BowlerIds = gameViewModel.Team_1_BowlerIds;
            List<int> Team_2_BowlerIds = gameViewModel.Team_2_BowlerIds;

            Player player = new Player();
            PlayerModel playerModel = new PlayerModel();
            List<PlayerModel> team_1_players = new List<PlayerModel>();
            List<PlayerModel> team_2_players = new List<PlayerModel>();

            //EF initialization
            StrategyCricketEntities model = new StrategyCricketEntities();
            foreach (int item in Team_1_BatsmenIds)
            {
                player = model.Players.SingleOrDefault(p => p.ID == item);
                playerModel = new PlayerModel();
                playerModel.ID = player.ID;
                playerModel.Name = player.Name;
                playerModel.BattingAttribute = player.BattingAttribute;
                playerModel.BowlingAttribute = player.BowlingAttribute;
                playerModel.BowlingOvers = new List<int>();
                team_1_players.Add(playerModel);
            }

            int over = 0;
            foreach (int id in Team_1_BowlerIds)
            {
                player = model.Players.SingleOrDefault(p => p.ID == id);
                PlayerModel currentPlayer = team_1_players.Find(x => x.Name.Equals(player.Name));
                currentPlayer.BowlingOvers.Add(over);
                over++;
            }

            foreach (int item in Team_2_BatsmenIds)
            {
                player = model.Players.SingleOrDefault(p => p.ID == item);
                playerModel = new PlayerModel();
                playerModel.ID = player.ID;
                playerModel.Name = player.Name;
                playerModel.BattingAttribute = player.BattingAttribute;
                playerModel.BowlingAttribute = player.BowlingAttribute;
                playerModel.BowlingOvers = new List<int>();
                team_2_players.Add(playerModel);
            }

            over = 0;
            foreach (int id in Team_2_BowlerIds)
            {
                player = model.Players.SingleOrDefault(p => p.ID == id);
                PlayerModel currentPlayer = team_2_players.Find(x => x.Name.Equals(player.Name));
                currentPlayer.BowlingOvers.Add(over);
                over++;
            }

            TeamModel battingTeam = new TeamModel();
            TeamModel bowlingTeam = new TeamModel();
            battingTeam.Players = team_1_players;
            bowlingTeam.Players = team_2_players;

            GameModel game = new GameModel();
            game.BattingTeam = battingTeam;
            game.BowlingTeam = bowlingTeam;

            return game;
        }

        public string getCurrentBowler(int overNumber, GameModel game)
        {

            foreach (var bowler in game.BowlingTeam.Players)
            {
                if (bowler.BowlingOvers.Count > 0)
                {
                    if (bowler.BowlingOvers.Contains(overNumber))
                    {
                        return bowler.Name;
                    }
                }
            }

            return "NA";
        }

        public void writeScoresToFile(GameModel game, int[] result, BatsmenScoreModel[] batsmenScoreModel, BowlerScoreModel[] bowlerScoreModel, string fileName, int target)
        {
            int sum = 0, wickets = 0, boundaries = 0, sixes = 0, strikerIndex = 0, nonStrikerIndex = 1, overNumber = 0;
            string currentBowler = getCurrentBowler(overNumber, game);

            //Write scores to file


            using (StreamWriter writer = new StreamWriter("D:/" + fileName + ".txt"))
            {
                int Ballcounter = 1;
                foreach (int i in result)
                {
                    writer.Write("Over:" + Ballcounter / 6 + "." + Ballcounter % 6 + " ");


                    if (i != -1)
                    {
                        writer.Write(game.BattingTeam.Players[strikerIndex].Name + " faces " + currentBowler + ". ");
                        writer.WriteLine(i + " Runs scored");
                        sum = sum + i;
                    }


                    if (i == 4)
                        boundaries++;
                    if (i == 6)
                        sixes++;
                    if (i == -1)
                    {
                        wickets++;
                        writer.Write(game.BattingTeam.Players[strikerIndex].Name + " faces " + currentBowler + ". ");
                        writer.WriteLine("WICKET!!!. " + game.BattingTeam.Players[strikerIndex].Name + " scored " + batsmenScoreModel[strikerIndex].score + " runs");
                        if (wickets <= 9)
                            strikerIndex = wickets + 1;
                        else
                            break;
                    }

                    if (sum > target)
                        break;


                    if (i == 1 || i == 3)
                    {
                        int temp = strikerIndex;
                        strikerIndex = nonStrikerIndex;
                        nonStrikerIndex = temp;
                    }

                    if (Ballcounter % 6 == 0)
                    {                                                
                        int tempBallcounter = strikerIndex;
                        strikerIndex = nonStrikerIndex;
                        nonStrikerIndex = tempBallcounter;

                        overNumber = (Ballcounter / 6);
                        currentBowler = getCurrentBowler(overNumber, game);
                    }

                    Ballcounter++;
                }

                if (wickets <= 9)
                    writer.WriteLine("Batsman " + game.BattingTeam.Players[strikerIndex].Name + " scored " + batsmenScoreModel[strikerIndex].score + " runs");
                writer.WriteLine("Batsman " + game.BattingTeam.Players[nonStrikerIndex].Name + " scored " + batsmenScoreModel[nonStrikerIndex].score + " runs");
                writer.WriteLine("Total : " + sum + "/" + wickets);
                writer.WriteLine("Score Summary:");
                writer.WriteLine("Fours:" + boundaries);
                writer.WriteLine("Sixes:" + sixes);


                //Posting data to DB
                StrategyCricketEntities model = new StrategyCricketEntities();
                PlayersData batsmanData = new PlayersData();
                PlayersData bowlerData = new PlayersData();


                for (int k = 0; k < batsmenScoreModel.Length && k <= (wickets + 1); k++)
                {
                    writer.WriteLine(game.BattingTeam.Players[k].Name + " " + batsmenScoreModel[k].score + "(" + batsmenScoreModel[k].balls + ")");
                    int Id = game.BattingTeam.Players[k].ID;
                    batsmanData  = model.PlayersDatas.SingleOrDefault(x => x.ID == Id);
                    batsmanData.Runs = batsmanData.Runs + batsmenScoreModel[k].score;
                    batsmanData.BallsFaced = batsmanData.BallsFaced + batsmenScoreModel[k].balls;
                    if(batsmanData.HighestScore != null)
                    {
                        if (batsmenScoreModel[k].score > batsmanData.HighestScore)
                            batsmanData.HighestScore = batsmenScoreModel[k].score;
                    }                        
                    else
                        batsmanData.HighestScore = batsmenScoreModel[k].score;
                    batsmanData.MatchesBatted = batsmanData.MatchesBatted + 1;          
                    model.SaveChanges();

                }

                for (int i = 0; i < bowlerScoreModel.Length; i++)
                {
                    if (bowlerScoreModel[i].runs > 0)
                    {                                               
                        writer.WriteLine(bowlerScoreModel[i].Name + " : " + bowlerScoreModel[i].Overs + "-" + bowlerScoreModel[i].runs + "-" + bowlerScoreModel[i].wickets);
                        int Id = bowlerScoreModel[i].ID;
                        bowlerData = model.PlayersDatas.SingleOrDefault(x => x.ID == Id);
                        bowlerData.Wickets = bowlerData.Wickets + bowlerScoreModel[i].wickets;
                        bowlerData.Overs = bowlerData.Overs + bowlerScoreModel[i].Overs;

                        //BestBowling Calculation
                        if(bowlerData.BestBowling != null)
                        {
                            if(bowlerScoreModel[i].wickets >= int.Parse(bowlerData.BestBowling.Split('/')[1]))
                            {
                                if(bowlerScoreModel[i].wickets == int.Parse(bowlerData.BestBowling.Split('/')[1]))
                                {
                                    if(bowlerScoreModel[i].runs < int.Parse(bowlerData.BestBowling.Split('/')[0]))
                                        bowlerData.BestBowling = bowlerScoreModel[i].runs + "/" + bowlerScoreModel[i].wickets;                                    
                                }
                                else
                                    bowlerData.BestBowling = bowlerScoreModel[i].runs + "/" + bowlerScoreModel[i].wickets;                                
                            }
                        }
                        else
                        {
                            bowlerData.BestBowling = bowlerScoreModel[i].runs + "/" + bowlerScoreModel[i].wickets;
                        }

                        bowlerData.MatchesBowled = bowlerData.MatchesBowled + 1;
                        model.SaveChanges();
                    }
                }

                if (target < int.MaxValue)
                {
                    if (sum > target)
                        writer.WriteLine("MI won by " + (10 - wickets) + " wickets");
                    else
                        writer.WriteLine("CSK won by " + (target - sum) + " runs");
                }
            }
        }


        /// <summary>
        /// Main Entry Point of the Game. Intializes the view and gets inputs of the team players and intializes the game.
        /// </summary>
        /// <returns></returns>
        //public ActionResult InitializeGame()
        //{
        //    GameViewModel gameViewModel = new GameViewModel();
        //    gameViewModel.Team_1_BatsmenIds = new List<int>(new int[11]);
        //    gameViewModel.Team_2_BatsmenIds = new List<int>(new int[11]);
        //    gameViewModel.Team_1_BowlerIds = new List<int>(new int[20]);
        //    gameViewModel.Team_2_BowlerIds = new List<int>(new int[20]);
        //    return View(gameViewModel);
        //}


    }
}