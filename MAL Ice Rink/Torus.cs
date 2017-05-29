using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;

namespace MAL_Ice_Rink
{
    static class Torus
    {
        /*
         * This class describes the internal state of the torus. The simulation is ran here.
         * 
         * Skaters move on a coordinate space that ranges 0 -> 1 on both axes. (Torus is 1x1 in size.)
         * Per cycle, skaters travel 1% of the height (or width) of the torus.
         * 
         */

        private static List<Skater> SkaterList;
        private static List<Skater> DrawSkaterList;
        private static List<Skater> NewDrawSkaterList;
        private static bool SyncTest = true;

        public static List<Skater> Skaters { get { return NewDrawSkaterList; } }
        public static ulong CycleCount { get; private set; }
        public static int[] actionHistogram;
        public static double[] rewardHistogram;
        public static int[] previousActionHistogram;
        public static int collisionCount;
        public static List<int> collisions = new List<int>();

        public static void InitializeTorus()
        {
            SkaterList = new List<Skater>();
            actionHistogram = new int[Parameters.NumMoveDirections];
            rewardHistogram = new double[Parameters.NumMoveDirections];
            previousActionHistogram = new int[Parameters.NumMoveDirections];
            CycleCount = 0;

            double x, y;
            int maxTries = 100;
            for (int i = 0; i < Parameters.NumSkaters; i++)
            {
                for (int tries = 1; tries <= maxTries; tries++)
                {
                    x = Shared.GetRandom.NextDouble();
                    y = Shared.GetRandom.NextDouble();

                    if (!CollisionCheck(null, x, y) || tries == maxTries)
                    {
                        SkaterList.Add(new Skater(x, y));
                        break;
                    }
                }
            }
        }

        public static void DoCycle()
        {
            SyncTest = false;
            DrawSkaterList = new List<Skater>();

            if (Parameters.EpsilonDecreasing) // 1 / n
                Parameters.Epsilon = 1 / (double)(CycleCount + 1);
            if (Parameters.LearningRateDecreasing) // 1 / n
                Parameters.LearningRate = 1 / (double)(CycleCount + 1);

            actionHistogram.CopyTo(previousActionHistogram, 0);
            Array.Clear(actionHistogram, 0, actionHistogram.Length);
            Array.Clear(rewardHistogram, 0, rewardHistogram.Length);


            collisionCount = 0;
            foreach (Skater s in SkaterList)
            {
                DrawSkaterList.Add(s.Move());
            }
            collisions.Add(collisionCount);

            CycleCount++;
            SyncTest = true;
        }

        public static void Update()
        {
            // This method is executed on the GUI thread.
            
            
            if (CycleCount > 0)
            {
                //if (!SyncTest) throw new TimeoutException("Could not keep up! Update Cycle not completed!");
                NewDrawSkaterList = DrawSkaterList;
            }
            else NewDrawSkaterList = SkaterList;
        }

        public static void Clear()
        {
            SkaterList.Clear();
            DrawSkaterList.Clear();
            NewDrawSkaterList.Clear();
            collisions.Clear();
        }

        /// <summary>
        /// Returns true if there is another agent within the collision radius.
        /// </summary>
        /// <param name="thisSkater">Reference to the current skater to ignore self.</param>
        /// <param name="newXPos">X position to check around.</param>
        /// <param name="newYPos">Y position to check around.</param>
        /// <returns></returns>
        public static bool CollisionCheck(Skater thisSkater, double newXPos, double newYPos)
        {

            foreach (Skater s in SkaterList)
            {
                if (s != thisSkater)
                {
                    // First, calculate absolute delta between points per axis.
                    double absX = Math.Abs(s.X - newXPos);

                    // Does this approach the collision radius? (reduces compute time by a factor 7).
                    if (absX < Parameters.CollisionRadius || absX > 1 - Parameters.CollisionRadius)
                    {
                        // Same for Y.
                        double absY = Math.Abs(s.Y - newYPos);

                        if (absY < Parameters.CollisionRadius || absY > 1 - Parameters.CollisionRadius)
                        {
                            // Take minimum value (deals with wrapping).
                            double minX = absX < 1 - absX ? absX : 1 - absX;
                            double minY = absY < 1 - absY ? absY : 1 - absY;

                            // Calc euclidean, return true if within collision radius
                            if (Math.Sqrt(minX * minX + minY * minY) < Parameters.CollisionRadius) return true;
                        }
                    }
                }
            }

            return false;
        }
    }
}
