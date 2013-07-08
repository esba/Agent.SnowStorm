using System;
using Microsoft.SPOT;
using Microsoft.SPOT.Hardware;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;
using System.Threading;
using System.Collections;

namespace Agent.Snowstorm
{
    public class Program
    {
        static Bitmap _display;
        static Timer _updateClockTimer;
        
        static int MAX_SNOW_PER_ROW = 15;
        static int SNOW_MAX_SIZE = 6; // <-- X-1
        static int DIGIT_FLAKE_SPACING = 8;

        // set the following to true for 24-hour time (00-23 instead of 1-12)
        const bool DISPLAY_24_HOUR_TIME = true;

        // static int[] DIGIT_OFFSET = { 3, 1, 8, 1, 3, 8, 8, 8 };
        static int[] DIGIT_OFFSET = {
                                        DIGIT_FLAKE_SPACING * 4,
                                        DIGIT_FLAKE_SPACING * 1,
                                        DIGIT_FLAKE_SPACING * 10,
                                        DIGIT_FLAKE_SPACING * 1,
                                        DIGIT_FLAKE_SPACING * 4,
                                        DIGIT_FLAKE_SPACING * 8,
                                        DIGIT_FLAKE_SPACING * 10,
                                        DIGIT_FLAKE_SPACING * 8
                                    };

        static SnowFlake[] _snowFlakes = new SnowFlake[333];
        static Random _rand = new Random();

        public static void Main()
        {
            // initialize our display buffer
            _display = new Bitmap(Bitmap.MaxWidth, Bitmap.MaxHeight);

            // display the time immediately
            UpdateTime(null);

            // obtain the current time
            DateTime currentTime = DateTime.Now;
            // set up timer to refresh time every minute
            TimeSpan dueTime = new TimeSpan(0, 0, 0, 59 - currentTime.Second, 1000 - currentTime.Millisecond); // start timer at beginning of next minute
            TimeSpan period = new TimeSpan(0, 0, 1, 0, 0); // update time every minute
            _updateClockTimer = new Timer(UpdateTime, null, dueTime, period); // start our update timer

            // go to sleep; time updates will happen automatically every minute
            Thread.Sleep(Timeout.Infinite);
        }

        static void UpdateTime(object state)
        {
            // Get current time
            DateTime currentTime = DateTime.Now;
            int nowMinute = currentTime.Minute;
            int nowHour = currentTime.Hour;

            // set our hour and minute based on 12/24 hour settinsg
            if (!DISPLAY_24_HOUR_TIME)
            {
                nowHour = currentTime.Hour % 12;
                if (nowHour == 0)
                    nowHour = 12;
            }

            // Do hours
            int hoursFirst = (int)System.Math.Floor(nowHour / 10);
            int hoursSecond = (int)System.Math.Floor(nowHour % 10);

            // Do minutes
            int minutesFirst = (int)System.Math.Floor(nowMinute / 10);
            int minutesSecond = (int)System.Math.Floor(nowMinute % 10);

            int[] digitsNow = { hoursFirst, hoursSecond, minutesFirst, minutesSecond };

            StartSnowing(digitsNow);
        }

        static void StartSnowing(int[] time)
        {
            int c = 0;

            for (int i = MAX_SNOW_PER_ROW; i > 0; i--)
            {
                // Debug.Print("i: " + i);
                int snowDensity = MAX_SNOW_PER_ROW / i;

                for (int y = 0; y < snowDensity; y++)
                {
                    // Debug.Print("y " + y);
                    MoveSnow();
                    _display.Flush();

                    int spacePerFlake = _display.Width / snowDensity;

                    int setPosY = _rand.Next(6);

                    for (int x = 0; x < snowDensity; x++)
                    {
                        int randPosX = -snowDensity;
                      // int randPosY = -snowDensity;

                        while (randPosX == -snowDensity)
                            randPosX = (_rand.Next(snowDensity * 2) - snowDensity);

                        /*
                        while (randPosY == -snowDensity)
                            randPosY = (_rand.Next(snowDensity * 2) - snowDensity);
                        */
                        int setPosX = (spacePerFlake / 2) + (spacePerFlake * x);

                        _snowFlakes[c] = new SnowFlake(setPosX + randPosX, setPosY, _rand.Next(SNOW_MAX_SIZE));

                        c++;
                    }
                }
            }

            int s1 = 1;
            int s2 = 1;
            int counter = 0;
            while (_snowFlakes[_snowFlakes.Length - 1].Melted == false)
            {
                MoveSnow();

                ChangeDigit(0, time[0], s1);
                ChangeDigit(1, time[1], s1);

                if (counter > 15)
                {
                    ChangeDigit(2, time[2], s2);
                    ChangeDigit(3, time[3], s2);

                    if (s2 < SNOW_MAX_SIZE - 1)
                        s2++;
                }

                if (s1 < SNOW_MAX_SIZE - 1)
                    s1++;

                _display.Flush();

                counter++;
            }



            for (int i = 0; i < 20; i++)
            {
                MoveSnow();

                ChangeDigit(0, time[0], s1);
                ChangeDigit(1, time[1], s1);
                ChangeDigit(2, time[2], s1);
                ChangeDigit(3, time[3], s1);

                _display.Flush();
            }

            _display.DrawText("notification area", Resources.GetFont(Resources.FontResources.small), Color.White, 25, 115);
            _display.Flush();
        }

        static void ChangeDigit(int digitPlacement, int number, int size)
        {
            ArrayList digits = new ArrayList();

            int offsetX = DIGIT_OFFSET[digitPlacement * 2];
            int offsetY = DIGIT_OFFSET[(digitPlacement * 2) + 1];

            switch (number)
            {
                case 0:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 1:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 2:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 3:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 4:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 5:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 6:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 7:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 8:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;

                case 9:
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 0 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 1 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 2 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 3 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(0 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(1 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    digits.Add(new SnowFlake(2 * DIGIT_FLAKE_SPACING + offsetX, 4 * DIGIT_FLAKE_SPACING + offsetY, size));
                    break;
            }

            foreach (SnowFlake flake in digits)
            {
                _display.DrawEllipse(Color.White, 0, flake.X, flake.Y, flake.Size - 2, flake.Size - 2, Color.White, 0, 0, Color.White, 0, 0, 255);
                DrawSnow(flake.X, flake.Y, flake.Size, true);
                DrawSnow(flake.X, flake.Y, flake.Size + 1);
            }
        }

        static void MoveSnow(int windSpeed = 1, int windDirection = 0)
        {
            // if (windDirection != 0)            

            for (int i = 0; i < _snowFlakes.Length; i++)
            {
                if (!_snowFlakes[i].Melted)
                {
                    int yMove = 1;

                    // Remove
                    DrawSnow(_snowFlakes[i].X, _snowFlakes[i].Y, _snowFlakes[i].Size, true);

                    // Snow fall
                    yMove = yMove + _rand.Next(4);

                    _snowFlakes[i].Y = _snowFlakes[i].Y + yMove;

                    // Snow move
                    _snowFlakes[i].X = _snowFlakes[i].X + (_rand.Next(3) - 1);

                    if (_snowFlakes[i].Y > _display.Height)
                        _snowFlakes[i].Melted = true;
                    else
                        DrawSnow(_snowFlakes[i].X, _snowFlakes[i].Y, _snowFlakes[i].Size);
                }
            }
        }

        static void MeltSnow(int snowNumber)
        {
            while (_snowFlakes[snowNumber].Size > 0 && !_snowFlakes[snowNumber].Melted)
            {
                // Remove
                DrawSnow(_snowFlakes[snowNumber].X, _snowFlakes[snowNumber].Y, _snowFlakes[snowNumber].Size, true);

                // Make one smaller
                _snowFlakes[snowNumber].Size--;

                // Redraw
                DrawSnow(_snowFlakes[snowNumber].X, _snowFlakes[snowNumber].Y, _snowFlakes[snowNumber].Size);

                _display.Flush();
            }

            _snowFlakes[snowNumber].Melted = true;
        }

        static void DrawSnow(int xPos, int yPos, int size, bool remove = false)
        {
            Color color;

            if (remove)
                color = Color.Black;
            else
                color = Color.White;

            switch (size)
            {
                case 0:
                    break;
                case 1:
                    _display.DrawLine(color, 1, xPos, yPos, xPos, yPos);
                    break;
                case 2:
                    _display.DrawLine(color, 1, xPos, yPos - 1, xPos, yPos + 1);
                    _display.DrawLine(color, 1, xPos - 1, yPos, xPos + 1, yPos);
                    break;
                default:
                    _display.DrawLine(color, 1, xPos, yPos - size + 1, xPos, yPos + size - 1);
                    _display.DrawLine(color, 1, xPos - size + 1, yPos, xPos + size - 1, yPos);

                    _display.DrawLine(color, 1, xPos - size + size / 2, yPos - size + size / 2, xPos + size - size / 2, yPos + size - size / 2);
                    _display.DrawLine(color, 1, xPos + size - size / 2, yPos - size + size / 2, xPos - size + size / 2, yPos + size - size / 2);
                    break;
            }
        }

        public struct SnowFlake
        {
            public SnowFlake(int x, int y, int size, bool melted = false)
            {
                X = x;
                Y = y;
                Size = size;
                Melted = melted;
            }

            public int X;
            public int Y;
            public int Size;
            public bool Melted;
        }
    }
}
