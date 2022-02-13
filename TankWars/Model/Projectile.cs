using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// This is the projectile class which holds its properties and the getters and setters
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Projectile
    {
        [JsonProperty]
        public int proj { get; private set; } = 0; // The projectile ID
        [JsonProperty]
        public Vector2D loc { get; internal set; } // The current location (x,y coordinate) of the projectile
        [JsonProperty]
        public bool died { get; internal set; } = false; // The dead state of the projectile
        [JsonProperty]
        public int owner { get; private set; } = 0; // The tank owner of the projectile
        [JsonProperty]
        public Vector2D dir { get; private set; } = null;
        public Vector2D Velocity { get; internal set; }
        public bool velocitySet { get; internal set; } = false;

        private static int nextId = 0;
        /// <summary>
        /// Required constructor
        /// </summary>
        public Projectile()
        {
            loc = new Vector2D(0, 0);
            Velocity = new Vector2D(0, 0);
        }

        /// <summary>
        /// The new projectile constructor to set the location, owner, and shot direction
        /// </summary>
        /// <param name="location"></param>
        /// <param name="owner"></param>
        /// <param name="aimingDir"></param>
        public Projectile(Vector2D location, int owner, Vector2D aimingDir)
        {
            proj = nextId++;
            loc = location;
            dir = aimingDir;
            this.owner = owner;
        }


        /// <summary>
        /// The to string method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
