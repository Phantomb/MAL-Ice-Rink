using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAL_Ice_Rink
{
    class Skater
    {
        private double xCoord;
        private double yCoord;
        private Color InitialColor;     // Set by Skater.Color.Set

        public double X { get { return xCoord; } }
        public double Y { get { return yCoord; } }
        public int RecentCollision { get; private set; } = -1;

        public Color Color
        {
            get
            {
                Color c = Parameters.GetColor(RecentCollision);
                return c == Color.White ? InitialColor : c;
            }
            private set
            {
                InitialColor = value;
            }
        }

        //Learning
        private double[] qValues;

        public Skater(double startX, double startY)
        {
            xCoord = startX;
            yCoord = startY;
            // Set initial color - dependent on selected Palette.
            Color = Parameters.GetColor(RecentCollision);
            RecentCollision = 0;

            qValues = new double[Parameters.DirectionList.Count];
            InitialiseQValues();
        }
        public Skater(double startX, double startY, int collisionHistory, double [] qValueArray, Color initialColor)
        {
            xCoord = startX;
            yCoord = startY;

            RecentCollision = collisionHistory;
            Color = initialColor;
            qValues = qValueArray;
        }

        public Skater Move()
        {
            double reward;

            // Get an action following some policy/strategy
            int action = DetermineAction(Parameters.CurrentPolicy);
            Torus.actionHistogram[action]++;

            // Calculate possible new position
            double direction = Parameters.DirectionList[action];
            double xCoordNewPosition = Shared.Mod((xCoord + Parameters.StepSize * Math.Cos(direction)), 1);
            double yCoordNewPosition = Shared.Mod((yCoord + Parameters.StepSize * Math.Sin(direction)), 1);

            // Check Collision
            if (Torus.CollisionCheck(this, xCoordNewPosition, yCoordNewPosition))
            {
                RecentCollision = Parameters.CollisionFadeSpeed;
                // Due to collision risk, movement is cancelled!
                // The newly gennerated coordinates will not be applied.
                Torus.collisionCount++;
                reward = CalculateReward(action, false);
            }
            else
            {
                // No collision, so we update the coordinates of this skater!
                xCoord = xCoordNewPosition;
                yCoord = yCoordNewPosition;
                if (RecentCollision > 0) RecentCollision--;
                reward = CalculateReward(action, true);
            }

            // update RL tables
            UpdateQValue(action, reward);
            // And log reward to histogram.
            Torus.rewardHistogram[action] += reward;

            // No Collision -> Update position
            return new Skater(xCoord, yCoord, RecentCollision, qValues, InitialColor);
        }

        private double CalculateReward(int action, bool success)
        {
            if (!Parameters.ModifiedRewards || Torus.CycleCount == 0)
                // Default implementation: Just return the specified rewards.
                return success ? Parameters.SuccesReward : Parameters.FailReward;
            else
            {
                // Alternative implementation: Weigh the reward by the number of skaters that use the same direction.
                // Heuristic, or cheat?
                // S = (1 + (1/n) * c) * r1
                // F = (1 + (1 / n) * (n - c)) * r2
                return  (1.0 + ((1.0 / Parameters.NumSkaters) * (success ? Torus.previousActionHistogram[action] : Parameters.NumSkaters - Torus.previousActionHistogram[action]))) * (success ? Parameters.SuccesReward : Parameters.FailReward);
            }
        }

        private int DetermineAction(Policy policy)
        {
            // Determine the action following some strategy, e.g. exploiting, exploring, e-greedy, random, etc.
            switch (policy)
            {
                // Optimal
                case Policy.Optimal:
                    double maxValue = double.NegativeInfinity;

                    List<int> movesWithMaxValue = new List<int>();

                    for (int i = 0; i < Parameters.DirectionList.Count; i++)
                    {
                        if(qValues[i] > maxValue)
                        {
                            movesWithMaxValue.Clear();
                            maxValue = qValues[i];
                            movesWithMaxValue.Add(i);
                        }
                        else if(qValues[i] == maxValue)
                        {
                            movesWithMaxValue.Add(i);
                        }
                    }
                    return movesWithMaxValue[Shared.GetRandom.Next(movesWithMaxValue.Count)];

                // e-greedy
                case Policy.EGreedy:
                    return Shared.GetRandom.NextDouble() >= Parameters.Epsilon ? DetermineAction(Policy.Optimal) : DetermineAction(Policy.Random);

                // Random
                case Policy.Random:
                default:
                    return Shared.GetRandom.Next(Parameters.DirectionList.Count);
            }
        }

        private void InitialiseQValues()
        {
            for (int i = 0; i < qValues.Length; i++)
            {
                qValues[i] = 0f;
            }
        }

        private void UpdateQValue(int action, double reward)
        {
            qValues[action] = qValues[action] + Parameters.LearningRate * (reward - qValues[action]);
        }
    }
}
