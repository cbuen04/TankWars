using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// The powerup class which holds all its properties and getters and setters
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Powerup
    {
        [JsonProperty]
        public int power { get; private set; } = 0; // The ID of the powerup
        [JsonProperty]
        public Vector2D loc { get; internal set; } // The location the powerup is displayed
        [JsonProperty]
        public bool died { get; internal set; } = false; // The death state of the powerup
        private static int nextId = 0;
        /// <summary>
        /// Required constructor
        /// </summary>
        public Powerup()
        {

        }

        /// <summary>
        /// Constructor which sets the location
        /// </summary>
        /// <param name="spawnLoc"></param>
        public Powerup(Vector2D spawnLoc)
        {
            power = nextId++;
            loc = spawnLoc;
        }

        /// <summary>
        /// The to string that serializes the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
