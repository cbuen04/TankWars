using Newtonsoft.Json;
using System;
using TankWars;

namespace Model
{
    /// <summary>
    /// This is the wall class. Contains all properties and respective getters and setters
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Wall
    {
        [JsonProperty]
        public int wall { get; private set; } // The unique wall ID
        [JsonProperty]
        public Vector2D p1 { get; internal set; } // The coordinate of the first wall endpoint
        [JsonProperty]
        public Vector2D p2 { get; private set; } = null; // The coordinate of the second wall endpoint

        private static int nextId = 0; // may want to set to negative 1
        private const double thickness = 50;
        double top, bottom, left, right;

        /// <summary>
        /// The constructor which sets the initial fields. 
        /// </summary>
        public Wall()
        {
            wall = 0;
            p1 = new Vector2D();
            p2 = new Vector2D();

        }

        /// <summary>
        /// The wall constructor to create a wall at the specified points
        /// </summary>
        /// <param name="p1"></param>
        /// <param name="p2"></param>
        public Wall(Vector2D p1, Vector2D p2)
        {
            wall = nextId++;
            this.p1 = p1;
            this.p2 = p2;

            left = Math.Min(p1.GetX(), p2.GetX());
            right = Math.Max(p1.GetX(), p2.GetX());
            top = Math.Min(p1.GetY(), p2.GetY());
            bottom = Math.Max(p1.GetY(), p2.GetY());
        }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }

        /// <summary>
        /// Checks if a tank will collide with the wall
        /// </summary>
        /// <param name="tankLoc"></param>
        /// <returns></returns>
        public bool CollidesTank(Vector2D tankLoc)
        {
            double expansion = thickness / 2 + Tank.size / 2;

            return left - expansion < tankLoc.GetX() &&
                right + expansion > tankLoc.GetX() && top - expansion < tankLoc.GetY()
                && bottom + expansion > tankLoc.GetY();
        }

        /// <summary>
        /// Checks if a projectile will collide with the wall
        /// </summary>
        /// <param name="projLoc"></param>
        /// <returns></returns>
        public bool CollidesProj(Vector2D projLoc)
        {
            double expansion = thickness / 2; // because projectiles have no area, buffer is set to the thickness of the wall 

            return left - expansion < projLoc.GetX() &&
                right + expansion > projLoc.GetX() && top - expansion < projLoc.GetY()
                && bottom + expansion > projLoc.GetY();
        }

    }
}
