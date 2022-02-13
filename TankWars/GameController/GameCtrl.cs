using Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Text.RegularExpressions;
using TankWars;
using NetworkUtil;
using System.Collections.Generic;

namespace GameController
{
    /// <summary>
    /// This class acts as the main controller of the project. This class connects to the server and recieves 
    /// world information and deserializes all information to add to the structures of the World (model) project.
    /// This also handles requests from the view for player movement/action and sends this request to the server accordingly. 
    /// </summary>
    public class GameCtrl
    {
        private int id;
        private int worldSize = 2000;
        private SocketState theServer = null;
        private World TheWorld;

        public event Action UpdateArrived;
        public event Action ErrorOccured; 
        private string playerName;
        public string errorMessage;

        // json information strings
        public string fire;
        public List<string> direction;

        // The location of the user's mouse as dictated by the view
        public int xCoordinate = 0;
        public int yCoordinate = 0;

        public int viewSize = 900;
        public HashSet<int> explosionIDsToRemove;

        /// <summary>
        /// This is the constructor which initializes a blank world, a "direction" list which holds
        /// requests of movement by the view from the user in the proper order, and a list of dead tanks
        /// to be filled as it detects a tank has been killed. These fields will be
        /// populated later.
        /// </summary>
        public GameCtrl()
        {
            TheWorld = null;
            direction = new List<string>();
            direction.Add("none");
            errorMessage = null;
            explosionIDsToRemove = new HashSet<int>();
        }

        /// <summary>
        /// public getter which returns the world the controller has been populating. 
        /// </summary>
        /// <returns>The World the controller has been populating</returns>
        public World getWorld()
        {
            return TheWorld;
        }

        /// <summary>
        /// Establishes a connection with the server with the given player and host name as 
        /// dictated by the view. 
        /// </summary>
        /// <param name="player"></param>
        /// <param name="hostname"></param>
        public void Connect(string player, string hostname)
        {
            playerName = player;
            Networking.ConnectToServer(OnConnect, hostname, 11000);
        }

        /// <summary>
        /// Public getter which returns the "main" player's (the player of which the user is playing with)
        /// unique ID
        /// </summary>
        /// <returns>This instance's player ID</returns>
        public int getPlayerID()
        {
            return id;
        }

        /// <summary>
        /// The callback of the Connect method's ConnectToServer call. 
        /// This handles actually sending the server the startup info and 
        /// asks to recieve the startup data (walls, ID, worldsize) in return.
        /// </summary>
        /// <param name="state">The socket state of the intial connection</param>
        private void OnConnect(SocketState state)
        {
            // The case where there is no established server connection
            if (state.ErrorOccured)
            {
                errorMessage = "Unable to connect to server";
                ErrorOccured?.Invoke();
                return;
            }
            state.OnNetworkAction = RecieveStartupInfo;

            // creating the socket state of the server
            theServer = state;

            Networking.Send(state.TheSocket, playerName + "\n");
            Networking.GetData(state);
        }

        /// <summary>
        /// The new OnNetworkAction delegate designated to recieve all the initial startup information
        /// recieved by the server. This processes the world size and server designated player ID
        /// </summary>
        /// <param name="state">The socket state of the connection</param>
        private void RecieveStartupInfo(SocketState state)
        {
            // Case where the server is shut off while the client is still on
            if (state.ErrorOccured)
            {
                errorMessage = "Client disconnected from server";
                ErrorOccured?.Invoke();
                return;
            }

            string info = state.GetData();
            string[] parts = Regex.Split(info, @"(?<=[\n])");

            // Breaks the server message into two parts which are 
            // going to be the id and world size
            if((parts.Length < 2) || !(parts[1].EndsWith("\n")))
            {
                Networking.GetData(state);
                return;
            }

            int.TryParse(parts[0], out id);
            int.TryParse(parts[1], out worldSize);
            state.RemoveData(0, parts[0].Length + parts[1].Length);
            TheWorld = new World(worldSize);
            TheWorld.id = id;

            // Changing delegate to now receive all of the server
            // information indefinitely
            state.OnNetworkAction = ReceiveJson;
            Networking.GetData(state);
        }

        /// <summary>
        /// The new OnNetworkActionDelegate to be set once startup infor has been received. This 
        /// is responsible for getting all of the information sent by the server constantly and indefinitely
        /// until the client is disconnected. This receives all information about the world, such as the walls, 
        /// tanks, projectiles, beams, and powerups that make up the world and populates the world structures accordingly
        /// 
        /// This also constructs a message to send to the server every frame to inform the server of the current state of the 
        /// tank and whether the user requests to move. 
        /// </summary>
        /// <param name="state">The socket state of the connection</param>
        private void ReceiveJson(SocketState state)
        {
            // Case where the server is shut off while the client is still on
            if (state.ErrorOccured)
            {
                errorMessage = "Client disconnected from server";
                ErrorOccured?.Invoke();
                return;
            }
            string message = state.GetData();
            int length = message.Length;
            string[] parts = Regex.Split(message, @"(?<=[\n])");

            foreach (string part in parts)
            {
                if (part.Length == 0)
                {
                    continue;
                }
                if (part[part.Length - 1] != '\n')
                {
                    break;
                }
                lock (TheWorld)
                {
                    // Tests if this information is a wall
                    JObject obj = JObject.Parse(part);
                    JToken wallToken = obj["wall"];
                    if (wallToken != null)
                    {
                        Wall w = JsonConvert.DeserializeObject<Wall>(part);
                        TheWorld.Walls[w.wall] = w;
                        state.RemoveData(0, part.Length);
                        continue;
                    }

                    // Tests if this information is a tank
                    JToken tankToken = obj["tank"];
                    if (tankToken != null)
                    {
                        Tank t = JsonConvert.DeserializeObject<Tank>(part);
                        // if the tank has died, remove it from the world so it won't repaint
                        if (t.died || t.healthPoints == 0)
                        {
                            TheWorld.Tanks.Remove(t.ID);
                            // add an explosion object located at the death location of the tank
                            TheWorld.Explosions[t.ID] = new Explosion(t.location.GetX(), t.location.GetY(), t.ID);
                        }
                        else
                            TheWorld.Tanks[t.ID] = t;
                        // Tests if the dead tank has been displayed for its allotted time
                        // if so, removes it from the dead tanks list. 
                        state.RemoveData(0, part.Length);
                        continue;
                    }

                    // Tests if this information is a projectile
                    JToken projToken = obj["proj"];
                    if (projToken != null)
                    {
                        Projectile p = JsonConvert.DeserializeObject<Projectile>(part);
                        // If the projectile has died, remove it
                        if (p.died)
                            TheWorld.Projectiles.Remove(p.proj);
                        else
                            TheWorld.Projectiles[p.proj] = p;
                        state.RemoveData(0, part.Length);
                        continue;
                    }

                    // Tests if this information is a beam
                    JToken beamToken = obj["beam"];
                    if (beamToken != null)
                    {
                        Beam b = JsonConvert.DeserializeObject<Beam>(part);
                        TheWorld.Beams[b.beam] = b;
                        // checks to see if this beam has been painted for its designated
                        // number of frames. If so, remove it from the list of beams in the world
                        if(b.framesTillDie == 0)
                        {
                            TheWorld.Beams.Remove(b.beam);
                        }
                        state.RemoveData(0, part.Length);
                        continue;
                    }

                    // Tests if this information is a powerup
                    JToken powerupToken = obj["power"];
                    if (powerupToken != null)
                    {
                        Powerup p = JsonConvert.DeserializeObject<Powerup>(part);
                        // Checks if it has died, if so, removes from the world
                        if (p.died)
                            TheWorld.Powerups.Remove(p.power);
                        else
                            TheWorld.Powerups[p.power] = p;
                        state.RemoveData(0, part.Length);
                        continue;
                    }



                }
                state.RemoveData(0, part.Length);
            }
            
            // Invokes the OnFrame in the view to keep informing it of changes
            UpdateArrived?.Invoke();

            // continues the event loop by asking the server to keep sending information
            Networking.GetData(state);
            // Constructs a message and sends to the server with any movement/fire requests
            Networking.Send(state.TheSocket, ConstructSendString());
        }

        /// <summary>
        /// Handles the view's request to change the location of the mouse. This resets 
        /// instance variables which can be reached by drawing panel to draw the turret direction
        /// and also tell the server where a shot was made from. 
        /// </summary>
        /// <param name="x">The most recent x coordinate of the mouse</param>
        /// <param name="y">The most recent y corrdinate of the mouse</param>
        public void MouseMoved(int x, int y)
        {
            xCoordinate = x - (viewSize / 2);
            yCoordinate = y - (viewSize / 2);
        }

        /// <summary>
        /// This takes in the changes of instance variables from the view (which are 
        /// signals to the controller that certain actions have been requested for movement/firing) and constructs 
        /// a message to be used in the call to the server in ReceiveJSON method
        /// containing the current state of the tank and if it wants to move/fire. 
        /// </summary>
        /// <returns>A string which contains the current state of the tank</returns>
        public string ConstructSendString()
        {
            Vector2D directionVector = new Vector2D(xCoordinate, yCoordinate);
            directionVector.Normalize();
            string message = "{\"moving\":\"" + direction[direction.Count - 1] + "\",\"fire\":\"" + fire + "\",\"tdir\": { \"x\":" + directionVector.GetX() + ",\"y\":" + directionVector.GetY() + "} }" + '\n';
            return message;
        }
    }
}
