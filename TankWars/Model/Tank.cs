using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using TankWars;

namespace Model
{
    /// <summary>
    /// This is the Tank class which holds its properties and respective getters and setters
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class Tank
    {
        [JsonProperty(PropertyName = "tank")]
        public int ID { get; private set; } = 0; // The unique tank ID

        [JsonProperty(PropertyName = "loc")]
        public Vector2D location { get; set; } // The current location of the tank

        [JsonProperty(PropertyName = "bdir")]
        public Vector2D orientation { get; internal set; } // The orientation of the actual tank body

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D aiming { get; internal set; } // The orientation of the turret

        [JsonProperty(PropertyName = "name")]
        public string name { get; private set; } = null; // The name of the player

        [JsonProperty(PropertyName = "hp")]
        public int healthPoints { get; set; } = Constants.MaxHP; // The tank's current health

        [JsonProperty(PropertyName = "score")]
        public int score { get; set; } = Constants.score; // The score of this player

        [JsonProperty(PropertyName = "died")]
        public bool died { get; set; } = false; // If the tank has been killed

        [JsonProperty(PropertyName = "dc")]
        public bool disconnected { get; set; } = false; // If the tank has been disconnected from the server

        [JsonProperty(PropertyName = "join")]
        public bool joined { get; private set; } = true; // If the tank has joined yet

        public int framesDead { get; set; } = 500; // How many frames until a dead tank will stop displaying

        public const int size = 60;

        public int framesTillShoot { get; internal set; } = 0;
            
        public int beamsAvailable { get; internal set; } = 0;

        public int framesSinceDeath { get; set; } // server side variable

        public Vector2D Velocity { get; internal set; }

        /// <summary>
        /// Required constructor
        /// </summary>
        public Tank()
        {

        }

        /// <summary>
        /// the new tank ocnstructor to set the ID to the tank, the name and the initial location
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="startupLocation"></param>
        public Tank(int id, string name, Vector2D startupLocation)
        {
            ID = id;
            location = startupLocation;
            Velocity = new Vector2D(0, 0);
            orientation = new Vector2D(0, -1);
            aiming = orientation;
            this.name = name;
            healthPoints = Constants.MaxHP;
            score = 0;
            died = false;
            disconnected = false;
            joined = true;
            framesSinceDeath = 0;
        }

        /// <summary>
        /// The to string method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this) + "\n";
        }
        
        /// <summary>
        /// Determines if a tnak has collided with a point. To be used for projectiles and powerups
        /// </summary>
        /// <param name="projLoc"></param>
        /// <returns></returns>
        public bool CollidesPoint(Vector2D projLoc)
        { 
            double left = location.GetX() - 30;
            double right = location.GetX() + 30;
            double top = location.GetY() - 30;
            double bottom = location.GetY() + 30;

            return left < projLoc.GetX() &&
                right > projLoc.GetX() && top < projLoc.GetY()
                && bottom > projLoc.GetY();
        }
    }
}
