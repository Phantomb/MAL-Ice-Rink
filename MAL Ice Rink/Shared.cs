using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MAL_Ice_Rink
{
    class Shared
    {
        // This helper class is used to share a single Random object between threads & objects across the namespace.
        private static Random sharedRandom;

        public static Random GetRandom
        {
            get
            {
                if (sharedRandom == null) sharedRandom = new Random();
                return sharedRandom;
            }
        }

        public static double Mod(double x, double m)
        {
            return (x % m + m) % m;
        }
        public static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }
    }

    static class Parameters
    {
        // Interface Parameters
        // Regarding functionality
        public static int NumSkaters { get; private set; }
        public static int NumMoveDirections { get; private set; }
        public static double CollisionRadius { get; private set; }
        public static double MovementSpeed { get; private set; }
        // Learning Parameters
        public static double SuccesReward { get; private set; }
        public static double FailReward { get; private set; }
        public static bool LearningRateDecreasing { get; private set; }
        public static double LearningRate { get; set; } 
        public static bool EpsilonDecreasing { get; private set; }
        public static double Epsilon { get; set; }
        // Aesthetics
        public static int DotSize { get; private set; }
        public static Palette SelectedPalette { get; private set; }
        public static bool ShowRadius { get; set; } = true;

        // Internal Parameters
        public static double StepSize { get; } = 0.01;
        public static int CollisionFadeSpeed { get; private set; }
        public static List<double> DirectionList { get; private set; } = new List<double>();
        public static List<Color> PaletteColors { get; private set; }
        public static Policy CurrentPolicy { get; } = Policy.EGreedy;
        public static int MaxRecentChartPoints = 200;
        public static int MaxChartPoints = 2000;
        public static bool AverageOnly = true; // True solves an error with the graphs. If true, only plots moving average.
        public static bool ModifiedRewards; // Modify rewards by weighing them with how many skaters use the same direction.

        // Output-related
        public static bool OutputToCSV = true;
        public static string SessionStartTime { get; private set; }

        public static void InitParameters(
            decimal _numSkaters, 
            string _moveDirections, 
            decimal _collisionRadius, 
            decimal _moveSpeed, 
            decimal _successReward, 
            decimal _failReward, 
            bool _modifyReward,
            bool _learningRateDecreasing,
            decimal _learningRate, 
            bool _epsilonDecreasing,
            decimal _epsilonForGreedy, 
            decimal _dotSize, 
            string _selectedPalette, 
            decimal _fadeSpeed)
        {
            // Interface Parameters
            // Regarding functionality
            NumSkaters = (int)_numSkaters;
            NumMoveDirections = Int32.Parse(_moveDirections);
            CollisionRadius = (double)_collisionRadius / 100;
            MovementSpeed = (double)_moveSpeed;
            // Learning Parameters
            SuccesReward = (double)_successReward;
            FailReward = (double)_failReward;
            ModifiedRewards = _modifyReward;
            LearningRateDecreasing = _learningRateDecreasing;
            LearningRate = (double) _learningRate;
            EpsilonDecreasing = _epsilonDecreasing;
            Epsilon = (double)_epsilonForGreedy;
            // Aesthetics
            DotSize = (int)_dotSize;
            SelectedPalette = (Palette)Enum.Parse(typeof(Palette), _selectedPalette);
            CollisionFadeSpeed = (int)_fadeSpeed;

            // Internal / code
            GeneratePalette();

            DirectionList.Clear();
            for (int i = 0; i < NumMoveDirections; i++)
            {
                // Convert to Radian
                DirectionList.Add((i * 360.0f / NumMoveDirections) * (Math.PI / 180));
            }

            SessionStartTime = DateTime.Now.ToString("yyyyMMddhhmmss");
        }

        private static void GeneratePalette()
        { // Populates the Palette list based upon the option selected in the palette combo box.

            PaletteColors = new List<Color>();

            switch (SelectedPalette)
            {
                case Palette.Complex:
                    // Generates a set of random colors to initially populate the skater list with.
                    // Skaters retain this color. (Regain this color after a collision.)
                    // Red is reserved for collisions.
                    PaletteColors.Add(Color.FromArgb(255, 244, 157));
                    PaletteColors.Add(Color.FromArgb(255, 236, 0));
                    PaletteColors.Add(Color.FromArgb(181, 200, 50));
                    PaletteColors.Add(Color.FromArgb(40, 148, 72));
                    PaletteColors.Add(Color.FromArgb(56, 102, 57));
                    PaletteColors.Add(Color.FromArgb(171, 193, 158));
                    PaletteColors.Add(Color.FromArgb(86, 159, 152));
                    PaletteColors.Add(Color.FromArgb(0, 156, 223));
                    PaletteColors.Add(Color.FromArgb(68, 112, 179));
                    PaletteColors.Add(Color.FromArgb(50, 43, 128));
                    PaletteColors.Add(Color.FromArgb(230, 176, 56));
                    PaletteColors.Add(Color.FromArgb(212, 122, 39));
                    PaletteColors.Add(Color.FromArgb(195, 0, 122));
                    PaletteColors.Add(Color.FromArgb(132, 119, 178));
                    PaletteColors.Add(Color.FromArgb(128, 33, 126));
                    PaletteColors.Add(Color.FromArgb(215, 169, 203));
                    PaletteColors.Add(Color.FromArgb(255, 255, 255));
                    PaletteColors.Add(Color.FromArgb(34, 34, 34));
                    PaletteColors.Add(Color.FromArgb(94, 79, 68));
                    break;

                case Palette.Simple:
                    // Prepopulate colors for efficiency.
                    PaletteColors.Add(Color.White);
                    PaletteColors.Add(Color.Red);
                    break;

                case Palette.Gradual:
                    // Generates a set of colors that indicate collision recency.
                    PaletteColors.Add(Color.FromArgb(255, 204, 0));         // A warm yellow.
                    PaletteColors.Add(Color.FromArgb(255, 173, 0));
                    PaletteColors.Add(Color.FromArgb(255, 126, 0));
                    PaletteColors.Add(Color.FromArgb(255, 94, 0));
                    PaletteColors.Add(Color.FromArgb(255, 49, 0));          // Almost pure red
                    break;

                case Palette.White:
                    // Is boring. (We do prepopulate colors for efficiency.)
                    PaletteColors.Add(Color.White);
                    break;

                case Palette.Black:
                    // Is boring. (We do prepopulate colors for efficiency.)
                    PaletteColors.Add(Color.Black);
                    break;
            }
 
        }

        public static Color GetColor(int collisionRecency)
        { // Return a color 

            switch (SelectedPalette)
            {
                case Palette.Complex:
                    // If called right after initialisation of Skaters, return a random color from the palette.
                    if (collisionRecency == -1)
                        return PaletteColors[Shared.GetRandom.Next(PaletteColors.Count())];

                    // If we do have a recent collision, we return red.
                    else if (collisionRecency > 0) return Color.Red;

                    // Return white if no collision.
                    else return Color.White;

                case Palette.Gradual:
                    // White as base color, colors indicate collision recency.
                    if (collisionRecency > 0)
                    {
                        // Cannot rely on default rounding of ints, so we have to cast twice.
                        //  ->  We want 10 / (10 / 5) = 4 (because that is the highest index in PaletteColors if Count == 5)
                        //  ->  Substracting 1 is not a solution, because then 1 / (10 / 5) - 1 = -1.
                        //  ->  Substracting 0.5 causes perfect behaviour, but only if initial division result is already a double.
                        return PaletteColors[(int)(collisionRecency / (CollisionFadeSpeed / (double)PaletteColors.Count) - 0.5)];
                    }
                    else return Color.White;

                case Palette.Simple:
                    // White as base color, red indicates recent collision.
                    if (collisionRecency > 0)
                    {
                        return PaletteColors[1];
                    }
                    else return PaletteColors[0];

                case Palette.White:
                    // Returns white, always.
                    return PaletteColors[0];

                case Palette.Black:
                    // Returns white, always.
                    return PaletteColors[0];
            }

            return Color.White;
        }
    }

    public enum DirectionForComboBox
    {
        d120 = 3,
        d90 = 4,
        d72 = 5,
        d60 = 6,
        d45 = 8,
        d40 = 9,
        d36 = 10,
        d30 = 12,
        d24 = 15,
        d20 = 18,
        d18 = 20,
        d15 = 24,
        d12 = 30,
        d10 = 36,
        d9 = 40,
        d8 = 45,
        d6 = 60,
        d5 = 72,
        d4 = 90,
        d3 = 120,
        d2 = 180,
        d1 = 360
    }

    public enum Palette
    {
        //
        //  Determines the color palette. Define a new palette in three steps:
        //
        //  1) Add its name to this enum
        //  2) Implement a Case in Parameters.GeneratePalette
        //      -> Set a CollisionMemory value (this determines how long a skater is colored differently after a collision)
        //      -> Add at colors to List<Color> PaletteColors, until PaletteColors.Count == CollisionMemory
        //  3) Implement a Case in Parameters.GetColor
        //      -> Determines what color value is used at any given time.
        //      -> To set a initial color value, return it when collisionRecency == -1
        //      -> To force a skater to use its initial color value, return Color.White.
        //          -> If you didn't explicitly set an initial value, it'll just use Color.White.
        //      -> Use PaletteColors[collisionRecency-1] to access colors from the list.
        //
        
        Simple,
        Gradual,
        Complex,
        White,
        Black
    }

    public enum Policy
    {
        Optimal,
        EGreedy,
        Random
    }
}
