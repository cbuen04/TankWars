using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// Beam class which contains all fields of a beam and its getters and setters
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Beam
    {
        [JsonProperty]
        public int beam { get; private set; } = 0; // beam ID
        [JsonProperty]
        public Vector2D org { get; private set; } = null; // where the beam was initially shot
        [JsonProperty]
        public Vector2D dir { get; private set; } = null; // Direction the beam was pointed
        [JsonProperty]
        public int owner { get; private set; } = 0; // Owner of the beam fired

        public bool died { get; set; } = false;

        public double framesTillDie { get; set; } = Constants.MaxDF; // Sets how many frames this beam will be displayed
        private static int nextId = 0; 

        /// <summary>
        /// Empty required constructor
        /// </summary>
        public Beam()
        {

        }

        /// <summary>
        /// The beam constructor which sets the origin, owner, and shot direction
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="owner"></param>
        /// <param name="direction"></param>
        public Beam(Vector2D origin, int owner, Vector2D direction)
        {
            beam = nextId++;
            org = origin;
            this.owner = owner;
            dir = direction;
        }
        /// <summary>
        /// The to string which serializes the object
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
    }
}
