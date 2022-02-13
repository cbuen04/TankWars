using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using TankWars;

namespace Model
{
    /// <summary>
    /// the control commander which sets a fire, moving, or turret direction
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public class ControlCommand
    {
        [JsonProperty(PropertyName = "moving")]
        public string Moving { get; set; }

        [JsonProperty(PropertyName = "fire")]
        public string Fire { get; set; }

        [JsonProperty(PropertyName = "tdir")]
        public Vector2D TurretDirection { get; set; }
    }
}
