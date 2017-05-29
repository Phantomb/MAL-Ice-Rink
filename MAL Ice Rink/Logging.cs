using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAL_Ice_Rink
{
    static class Logging
    {
        private static string filename;
        private static bool LogReady;
        private static int NumColumns;

        static Logging()
        {
            LogReady = false;
        }

        public static void NewLogFile()
        {
            if (Parameters.OutputToCSV)
            {
                string path;
                StringBuilder linebuilder = new StringBuilder();

                // Output to MyDocuments
                path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (!path.EndsWith("\\")) path = path + "\\";

                // Create a filename.
                filename = path + Parameters.SessionStartTime + "skater.csv";

                LogReady = true;
                LogDataLine();
            }
        }

        public static void LogDataLine(ulong cycleNum, int collisionCount, int[] countperDirection, double[] rewardperDirection)
        {
            bool catchnull = (countperDirection == null);

            // Used to log data points.
            List<string> output = new List<string>();

            output.Add(cycleNum.ToString());
            output.Add(collisionCount.ToString());

            // Number of skaters pers direction
            for(int i = 0; i < NumColumns; i++)
            {
                if (!catchnull)
                {
                    output.Add(countperDirection[i].ToString());
                }
                else output.Add("n/a");
            }
            // Average reward per direction
            for (int i = 0; i < NumColumns; i++)
            {
                if (!catchnull)
                {
                    output.Add(countperDirection[i] == 0 ? "0" : (rewardperDirection[i] / countperDirection[i]).ToString());
                }
                else output.Add("n/a");
            }

            AddLineToLog(output);
        }

        private static void LogDataLine()
        {
            // Used to log the title.
            NumColumns = Parameters.NumMoveDirections;

            // Create list.
            List<string> output = new List<string>();

            // Log parameters
            output.Add("Parameters:");
            output.Add("NumSkaters: " + Parameters.NumSkaters);
            output.Add("NumMoveDirections: " + Parameters.NumMoveDirections);
            output.Add("CollisionRadius: " + Parameters.CollisionRadius);
            output.Add("SuccesReward: " + Parameters.SuccesReward);
            output.Add("FailReward: " + Parameters.FailReward);
            output.Add("ModifiyRewards: " + Parameters.ModifiedRewards);
            output.Add("decreasingLearningRate: " + Parameters.LearningRateDecreasing);
            output.Add("LearningRate: " + Parameters.LearningRate);
            output.Add("decreasingEpsilon: " + Parameters.EpsilonDecreasing);
            output.Add("Epsilon: " + Parameters.Epsilon);
            AddLineToLog(output);
            output.Clear();

            // Log title
            output.Add("Cycle");
            output.Add("Collision");
            for (int i = 0; i < NumColumns; i++)
            {
                output.Add("n"+((Parameters.DirectionList[i] * 180) / Math.PI).ToString());
            }
            for (int i = 0; i < NumColumns; i++)
            {
                output.Add("r" + ((Parameters.DirectionList[i] * 180) / Math.PI).ToString());
            }

            AddLineToLog(output);

            // Log file is now prepped and ready.
            
        }

        private static void AddLineToLog(List<string> stringlist)
        {
            if (Parameters.OutputToCSV && LogReady)
            {
                // Create output and write to file.
                using (StreamWriter outputfile = new StreamWriter(@filename, true))
                {
                    string line = stringlist[0];

                    for (int i = 1; i < stringlist.Count; i++)
                    {
                        line += ";" + stringlist[i];
                    }

                    outputfile.WriteLine(line);
                }
            }
        }

        public static void EndLogging()
        {
            LogReady = false;
        }
    }
}
