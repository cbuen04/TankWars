using System;
using System.Collections.Generic;
using System.Text;

namespace Model
{
    /// <summary>
    /// This class holds all information about an explosion: where it needs to be drawn, and how many frames left until it needs to be destructed
    /// </summary>
    public class Explosion
    {
        public double xCoordinate { get; set; } = 0;
        public double yCoordinate { get; set; } = 0;
        private int framesTillDone;
        public int explosionID { get; set; } = 0; // The ID cooresponding to the tank that died.

        /// <summary>
        /// The constructor which sets the locations of the explosions as soon as a tank is dead
        /// </summary>
        /// <param name="xCoordinate">x location of where the tank died and explosion is drawn</param>
        /// <param name="yCoordinate">y location of where the tank died and explosion is drawn</param>
        public Explosion(double xCoordinate, double yCoordinate, int explosionID)
        {
            this.xCoordinate = xCoordinate;
            this.yCoordinate = yCoordinate;
            framesTillDone = Constants.MaxDFE;
        }

        public void DecrementFramesTillDone()
        {
            framesTillDone--;
        }
        public int GetFramesTillDone()
        {
            return framesTillDone;
        }
    }
}
