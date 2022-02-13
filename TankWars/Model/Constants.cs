using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    /// <summary>
    /// Constants class which holds the constants of the world
    /// </summary>
    public class Constants
    {
        // extra feature to implement xml reader
        public const int MaxHP = 3; // Max health points a tank has 
        public const int score = 0; // Score the tank initially has
        public const int MaxDF = 60; // How many frames a beam will be displayed (max death frames)
        public const int MaxDFE = 500; // How many frames a dead tank explosion will be displayed
        public const int TankSpeed = 3;
        public const int ProjSpeed = 25;
        public const int powerupDelay = 1650;
        public const int maxPowerups = 2;
    }
}
