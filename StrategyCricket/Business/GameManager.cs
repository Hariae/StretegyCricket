using StrategyCricket.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace StrategyCricket.Business
{
    public class GameManager
    {

        Random random = new Random();
        private int RandomNumber(int range)
        {
            return random.Next(range);
        }

        public BatsmenScoreModel[] FormatGame(int[] gameSet, int target)
        {
            int[] batsmenScore = new int[11];
            BatsmenScoreModel[] batsmenScoreModel = new BatsmenScoreModel[11];

            for (int i = 0; i < 11; i++)
                batsmenScoreModel[i] = new BatsmenScoreModel();

            int wickets = 0, sum = 0;
            int strikerIndex = 0, nonStrikerIndex = 1;
            const int WICKETSDOWN = 1;
            for (int i = 0, j = 1; (i < gameSet.Length && wickets < 10 && sum <= target); i++, j++)
            {
                if (gameSet[i] == -1)
                {
                    batsmenScoreModel[strikerIndex].balls = batsmenScoreModel[strikerIndex].balls + 1;
                    wickets++;
                    if (wickets <= 9)
                        strikerIndex = wickets + WICKETSDOWN;
                }
                else
                {
                    batsmenScore[strikerIndex] = batsmenScore[strikerIndex] + gameSet[i];
                    batsmenScoreModel[strikerIndex].score = batsmenScoreModel[strikerIndex].score + gameSet[i];
                    batsmenScoreModel[strikerIndex].balls = batsmenScoreModel[strikerIndex].balls + 1;
                    sum = sum + gameSet[i];
                }


                //Strike Rotation

                if (gameSet[i] == 1 || gameSet[i] == 3)
                {
                    int temp = strikerIndex;
                    strikerIndex = nonStrikerIndex;
                    nonStrikerIndex = temp;
                }

                if (j % 6 == 0)
                {
                    int temp = strikerIndex;
                    strikerIndex = nonStrikerIndex;
                    nonStrikerIndex = temp;
                }
            }


            return batsmenScoreModel;
        }

        public BowlerScoreModel[] FormatBowlersData(int[] gameSet, GameModel game)
        {
            BowlerScoreModel[] bowlerScoreModel = new BowlerScoreModel[11];
            int bowlerIndex = 0;
            PlayerModel currentBowler = new PlayerModel();

            for (int i = 0; i < 11; i++)
                bowlerScoreModel[i] = new BowlerScoreModel();

            int wickets = 0, overNumber = 0, numberOfBowlers = 1;
            currentBowler = getCurrentBowlerID(game, overNumber);
            bowlerScoreModel[bowlerIndex].ID = currentBowler.ID;
            bowlerScoreModel[bowlerIndex].Name = currentBowler.Name;
            bowlerScoreModel[bowlerIndex].Overs = bowlerScoreModel[bowlerIndex].Overs + 1;

            for (int i = 0, j = 1; (i < gameSet.Length && wickets < 10); i++, j++)
            {

                if (gameSet[i] == -1)
                {
                    bowlerScoreModel[bowlerIndex].wickets = bowlerScoreModel[bowlerIndex].wickets + 1;
                    wickets++;
                }
                else
                    bowlerScoreModel[bowlerIndex].runs = bowlerScoreModel[bowlerIndex].runs + gameSet[i];

                if (j % 6 == 0)
                {
                    overNumber = j / 6;
                    currentBowler = getCurrentBowlerID(game, overNumber);
                    int currentBowlerIndex = Array.FindIndex(bowlerScoreModel, x => x.ID == currentBowler.ID);

                    if (currentBowlerIndex != -1)
                    {
                        bowlerIndex = currentBowlerIndex;                        
                    }
                    else
                    {
                        bowlerIndex = numberOfBowlers;
                        bowlerScoreModel[bowlerIndex].ID = currentBowler.ID;
                        bowlerScoreModel[bowlerIndex].Name = currentBowler.Name;                        
                        numberOfBowlers++;
                    }
                    bowlerScoreModel[bowlerIndex].Overs = bowlerScoreModel[bowlerIndex].Overs + 1;
                }

            }
            return bowlerScoreModel;
        }

        public PlayerModel getCurrentBowlerID(GameModel game, int overNumber)
        {
            PlayerModel bowler = new PlayerModel();
            bowler.BowlingOvers = new List<int>();
            foreach (var currentBowler in game.BowlingTeam.Players)
            {
                if (currentBowler.BowlingOvers != null && currentBowler.BowlingOvers.Contains(overNumber))
                {
                    bowler.ID = currentBowler.ID;
                    bowler.Name = currentBowler.Name;                    
                }
            }
            return bowler;
        }


        public int getCurrentBowlerOver(GameModel game, int ID)
        {
            
            foreach (var currentBowler in game.BowlingTeam.Players)
            {
                if (currentBowler.ID == ID)
                {
                    return currentBowler.BowlingOvers.Count;
                }
            }
            return 0;
        }

        public int[] SimulateGame(GameModel game, int target)
        {

            TeamModel battingTeam = game.BattingTeam;
            TeamModel bowlingTeam = game.BowlingTeam;
            int[] gameSet = new int[120];
            int totalBalls = 120;
            int totalScore = 0;
            //Iterate ball

            int wickets = 0, currentBatsmanAttribute = 0, currentBowlerAttribute = 0, overNumber = 0, currentBallScore = 0, strikerIndex = 0, nonStrikerIndex = 1;

            string currentBowler = "";

            for (int i = 0; (i < totalBalls && wickets < 10 && totalScore <= target); i++)
            {

                if (i != 0 && i % 6 == 0)
                    overNumber++;

                currentBatsmanAttribute = battingTeam.Players[strikerIndex].BattingAttribute;

                //Get Bowlers              
                foreach (var bowler in bowlingTeam.Players)
                {
                    if (bowler.BowlingOvers != null && bowler.BowlingOvers.Contains(overNumber))
                    {
                        currentBowler = bowler.Name;
                        currentBowlerAttribute = bowler.BowlingAttribute;
                    }
                }

                currentBallScore = SimulateBall(currentBatsmanAttribute, currentBowlerAttribute);

                if (currentBallScore == -1)
                {
                    wickets++;
                    if (wickets <= 9)
                        strikerIndex = wickets + 1;
                }

                if (currentBallScore == 1 || currentBallScore == 3)
                {
                    int temp = strikerIndex;
                    strikerIndex = nonStrikerIndex;
                    nonStrikerIndex = temp;
                }

                if (i != 0 && i % 6 == 0)
                {
                    int temp = strikerIndex;
                    strikerIndex = nonStrikerIndex;
                    nonStrikerIndex = temp;
                }

                gameSet[i] = currentBallScore;

                if (currentBallScore != -1)
                    totalScore = totalScore + currentBallScore;
            }

            //Call probability method

            //Fill the array

            //return Array


            return gameSet;
        }

       
            public int[] Shuffle(int[] array)
            {
                int n = array.Length;
                while (n > 1)
                {
                    int k = new Random().Next(n--);
                    int temp = array[n];
                    array[n] = array[k];
                    array[k] = temp;
                }
                return array;
            }
        

        public int SimulateBall(int battingAttribute, int bowlingAttribute)
        {
            int[] probabilityPicker = new int[battingAttribute + bowlingAttribute];

            //Fill probablility array with batting and bowling powers.

            for (int i = 0; i < battingAttribute; i++)
                probabilityPicker[i] = 1;
            for (int j = battingAttribute; j < battingAttribute + bowlingAttribute; j++)
                probabilityPicker[j] = 0;

            //randomize
            probabilityPicker = Shuffle(probabilityPicker);
            
            //Batsmen or Bowler picker 
            int batsmanOrBowlerPicker = probabilityPicker[RandomNumber(probabilityPicker.Length)];
            int score = 0;
            //Batting or Bowler probe
            if (batsmanOrBowlerPicker == 1)
                score = ProbeBattingAttribute(battingAttribute);
            else
                score = ProbeBowlingAttribute(bowlingAttribute);

            return score;
            //return probabilityPicker[RandomNumber(probabilityPicker.Length)];
        }

        public int ProbeBattingAttribute(int battingAttribute)
        {
            int[] probabilityPicker = new int[battingAttribute];

            //Fix threshold for 4 types of runs

            const int PROPORTION_SIX = 10;
            const int PROPORTION_FOUR = 20;
            const int PROPORTION_TRIPLE = 5;
            const int PROPORTION_DOUBLE = 40;
            const int PROPORTION_SINGLE = 25;
            int threshold = 0;

            //Fill each segment
            int sixThreshold = (battingAttribute * PROPORTION_SIX) / 100;
            for (int i = threshold; i < sixThreshold; i++)
                probabilityPicker[i] = 6;
            threshold = threshold + sixThreshold;

            int fourThreshold = (battingAttribute * PROPORTION_FOUR) / 100;
            for (int i = threshold; i < threshold + fourThreshold; i++)
                probabilityPicker[i] = 4;
            threshold = threshold + fourThreshold;

            int tripleThreshold = (battingAttribute * PROPORTION_TRIPLE) / 100;
            for (int i = threshold; i < threshold + tripleThreshold; i++)
                probabilityPicker[i] = 3;
            threshold = threshold + tripleThreshold;

            int doubleThreshold = (battingAttribute * PROPORTION_DOUBLE) / 100;
            for (int i = threshold; i < threshold + doubleThreshold; i++)
                probabilityPicker[i] = 2;
            threshold = threshold + doubleThreshold;

            int singleThreshold = (battingAttribute / PROPORTION_SINGLE) / 100;
            for (int i = threshold; i < threshold + singleThreshold; i++)
                probabilityPicker[i] = 1;
            threshold = threshold + singleThreshold;

            //fill remaining array
            for (int i = threshold; i < battingAttribute; i++)
                probabilityPicker[i] = 1;

            //randomize
            probabilityPicker = Shuffle(probabilityPicker);


            //pick the winner run
            int randomizedIndex = RandomNumber(probabilityPicker.Length);

            return probabilityPicker[randomizedIndex];
        }

        public int ProbeBowlingAttribute(int bowlingAttribute)
        {
            int[] probabilityPicker = new int[bowlingAttribute];

            //Threshold segments for bowler
            const int PROPORTION_WICKET = 15;
            const int PROPORTION_DOT = 20;
            const int PROPORTION_SINGLE = 50;
            const int PROPORTION_DOUBLE = 15;
            int threshold = 0;

            //Fill each segment
            int wicketThreshold = (bowlingAttribute * PROPORTION_WICKET) / 100;
            for (int i = threshold; i < wicketThreshold; i++)
                probabilityPicker[i] = -1;
            threshold = threshold + wicketThreshold;

            int dotThreshold = (bowlingAttribute * PROPORTION_DOT) / 100;
            for (int i = threshold; i < threshold + dotThreshold; i++)
                probabilityPicker[i] = 0;
            threshold = threshold + dotThreshold;

            int singleThreshold = (bowlingAttribute * PROPORTION_SINGLE) / 100;
            for (int i = threshold; i < threshold + singleThreshold; i++)
                probabilityPicker[i] = 1;
            threshold = threshold + singleThreshold;

            int doubleThreshold = (bowlingAttribute * PROPORTION_DOUBLE) / 100;
            for (int i = threshold; i < threshold + doubleThreshold; i++)
                probabilityPicker[i] = 2;
            threshold = threshold + doubleThreshold;

            //fill remaining array
            for (int i = threshold; i < bowlingAttribute; i++)
                probabilityPicker[i] = 2;


            //randomize
            probabilityPicker = Shuffle(probabilityPicker);

            int randomizedIndex = RandomNumber(probabilityPicker.Length);
            return probabilityPicker[randomizedIndex];
        }
    }
}