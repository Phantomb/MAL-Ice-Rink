using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            CollisionTest testje = new CollisionTest();
            //AngleTest angles = new AngleTest();
            //DivisionTest division = new DivisionTest();
        }
    }

    class DivisionTest
    {
        public DivisionTest()
        {
            double result;
            double length = 5;

            for (int i = 10; i > 0; i--)
            {
                result = (int) (i / (10 / length)-0.5);

                Console.WriteLine("CollisionRecency {0}, length {1}, result {2}.", i, length, result);

                if (result < 0)
                {
                    Console.WriteLine("Illegal Result!");
                    break;
                }

            }
            Console.ReadKey();
        }
    }

    class AngleTest
    {
        public AngleTest()
        {
            for(int i = 1; i <= 300; i++)
            {
                if (360 % i == 0)
                {
                    Console.WriteLine("d{0} = {1},", 360 / i, i);
                }
            }
            Console.ReadKey();
        }
    }

    class CollisionTest
    {
        Stopwatch watch1;
        Stopwatch watch2;
        List<Box> BoxList;

        uint count1;
        uint count2;

        long Ecount1;
        long Ecount2;

        double CollisionRadius = 0.01;

        public CollisionTest()
        {
            List<long> times1 = new List<long>();
            List<long> times2 = new List<long>();
            List<long> Etimes1 = new List<long>();
            List<long> Etimes2 = new List<long>();

            for (int i = 0; i < 1000; i++)
            {
                BoxList = new List<Box>();
                watch1 = new Stopwatch();
                watch2 = new Stopwatch();
                count1 = 0;
                count2 = 0;

                BoxList.Add(new Box(0.99, 0.99));
                BoxList.Add(new Box(0.01, 0.01));
                BoxList.Add(new Box(0.99, 0.01));
                BoxList.Add(new Box(0.01, 0.99));

                for (int j = 0; j < 1000; j++)
                    BoxList.Add(new Box());

                watch1.Start();
                // Execute 1.

                foreach (Box b in BoxList)
                {
                    if (SpeedyCollisionCheck(b, b.X, b.Y)) count1++;
                }

                watch1.Stop();
                times1.Add(watch1.ElapsedMilliseconds);
                Etimes1.Add(Ecount1);

                watch2.Start();
                // Execute 2.

                foreach (Box b in BoxList)
                {
                    if (NewSpeedyCollisionCheck(b, b.X, b.Y)) count2++;
                }

                watch2.Stop();
                times2.Add(watch2.ElapsedMilliseconds);
                Etimes2.Add(Ecount2);

                if (count1 != count2) throw new ArgumentOutOfRangeException("Disparity between counts, this cannot be!");
            }
            Console.WriteLine("Averages are {0} for time-set 1 and {1} for time-set 2.", times1.Average(), times2.Average());
            Console.WriteLine("Averages are {0} for count-set 1 and {1} for count-set 2.", Etimes1.Average(), Etimes2.Average());
            Console.ReadKey();
        }

        public bool NewSpeedyCollisionCheck(Box thisBox, double newXPos, double newYPos)
        {
            double absX;
            double absY;

            foreach (Box s in BoxList)
            {
                if (s != thisBox)
                {
                    absX = Math.Abs(s.X - newXPos);

                    if (absX < CollisionRadius || absX > 1 - CollisionRadius)
                    {
                        absY = Math.Abs(s.Y - newYPos);

                        if (absY < CollisionRadius || absY > 1 - CollisionRadius)
                        {
                            Ecount2++;
                            double minX = absX < 1 - absX ? absX : 1 - absX;
                            double minY = absY < 1 - absY ? absY : 1 - absY;
                            if (Math.Sqrt(minX * minX + minY * minY) < CollisionRadius) return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool SpeedyCollisionCheck(Box thisBox, double newXPos, double newYPos)
        {
            double delta;

            foreach (Box s in BoxList)
            {
                if (s != thisBox)
                {
                    delta = Math.Abs(s.X - newXPos);

                    if (delta < CollisionRadius || delta > 1 - CollisionRadius)
                    {
                        delta = Math.Abs(s.Y - newYPos);

                        if (delta < CollisionRadius || delta > 1 - CollisionRadius)
                        {
                            Ecount1++;
                            double v = Math.Abs(s.X - newXPos);
                            double v1 = Math.Abs(s.Y - newYPos);
                            double v2 = v < 1 - v ? v : 1 - v;
                            double v3 = v1 < 1 - v1 ? v1 : 1 - v1;
                            if (Math.Sqrt(v2 * v2 + v3 * v3) < CollisionRadius) return true;
                        }
                    }
                }
            }

            return false;
        }

        public bool CollisionCheck(Box thisBox, double newXPos, double newYPos)
        {
            foreach (Box s in BoxList)
            {
                if (s != thisBox)
                {
                    Ecount2++;
                    double v = Math.Abs(s.X - newXPos);
                    double v1 = Math.Abs(s.Y - newYPos);
                    double v2 = v < 1 - v ? v : 1 - v;
                    double v3 = v1 < 1 - v1 ? v1 : 1 - v1;
                    if (Math.Sqrt(v2 * v2 + v3 * v3) < CollisionRadius) return true;
                }
            }

            return false;
        }
    }

    class Box
    {
        public double X { get; private set; }
        public double Y { get; private set; }

        public Box()
        {
            X = Shared.GetRandom.NextDouble();
            Y = Shared.GetRandom.NextDouble();
        }
        public Box(double x, double y)
        {
            X = x;
            Y = y;
        }
    }

    static class Shared
    {
        private static Random sharedRandom;

        public static Random GetRandom
        {
            get
            {
                if (sharedRandom == null) sharedRandom = new Random();
                return sharedRandom;
            }
        }
    }
}
