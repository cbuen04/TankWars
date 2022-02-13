using Model;
using System;
using System.Collections.Generic;
using System.Text;
using NetworkUtil;
using System.Threading;
using System.Diagnostics;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Data.SqlClient;


namespace TankWars
{
    /// <summary>
    /// Authors: Charly Bueno and Emma Kerr
    /// 
    /// This class utlizies the networking dll to listen for clients and connect them to the greater server logic to
    /// display all the necessary information about the game and the world. This works with the world class/project to 
    /// detect collisions, handle adding and removing clients and tanks, etc. 
    /// 
    /// </summary>
    class ServerController
    {
        private Settings settings { get; }
        private World theWorld;
        private Dictionary<int, SocketState> clients = new Dictionary<int, SocketState>();
        private string startupInfo;

        /// <summary>
        /// The constructor for the server controller. This takes in the read settings and 
        /// constructs a world accordingly and populates it. It also constructs the startup info with
        /// world info to send to the client.
        /// </summary>
        /// <param name="settings">Settings read from an xml file</param>
        public ServerController(Settings settings)
        {
            // Getting information collected by the setting reading
            this.settings = settings;
            theWorld = new World(settings.UniverseSize);
            theWorld.MSPerFrame = settings.MSPerFrame;
            theWorld.RespawnRate = settings.RespawnRate;
            theWorld.FramesPerShot = settings.FramesPerShot;

            foreach (Wall wall in settings.walls)
            {
                theWorld.Walls[wall.wall] = wall;
            }

            // Creating the list of startup info
            StringBuilder sb = new StringBuilder();
            sb.Append(theWorld.size);
            sb.Append("\n");
            foreach (Wall wall in theWorld.Walls.Values)
            {
                sb.Append(wall.ToString());
            }

            // Placing the max powerups as dictated by the constants class
            for (int i = 0; i < Constants.maxPowerups; i++)
            {
                Powerup powerup = new Powerup(theWorld.setLoctaion());
                theWorld.Powerups[powerup.power] = powerup;
                sb.Append(powerup.ToString());
            }

            startupInfo = sb.ToString();
        }

        /// <summary>
        /// Starts the server and creates a new thread to update and listen for clients joining
        /// </summary>
        internal void Start()
        {
            Networking.StartServer(NewClient, 11000);
            Thread t = new Thread(Update);
            t.Start();
        }

        /// <summary>
        /// This method is called every frame to update the world. It cycles through all the objects in the world
        /// and removes them if dead
        /// </summary>
        /// <param name="obj"></param>
        private void Update(object obj)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();
            while (true)
            {

                while (watch.ElapsedMilliseconds < settings.MSPerFrame);
 
                HashSet<int> tankIDToRemove = new HashSet<int>();
                HashSet<int> projectilesToRemove = new HashSet<int>();
                HashSet<int> powerupsToRemove = new HashSet<int>();
                HashSet<int> beamToRemove = new HashSet<int>();


                watch.Restart();
                StringBuilder sb = new StringBuilder();
                lock (theWorld)
                {
                    theWorld.Update();
                    foreach (Tank tank in theWorld.Tanks.Values)
                    {
                        sb.Append(tank.ToString());

                        if (tank.died && !tank.disconnected)
                        {
                            tank.died = false; // If a tank dies, we only want to send that it has died once
                        }

                        if (tank.framesSinceDeath > 0) // The world will reset this to the respawn rate, and this will display the new tank location once the allotted time has passed
                        {

                            tank.framesSinceDeath--;
                            if (tank.framesSinceDeath == 0)
                            {
                                tank.location = theWorld.setLoctaion();
                                tank.healthPoints = Constants.MaxHP;
                            }
                        }
                        if (tank.disconnected && tank.died)
                        {
                            tankIDToRemove.Add(tank.ID);
                        }

                    }
                    // Removes all the dead tanks
                    foreach(int id in tankIDToRemove)
                    {
                        theWorld.Tanks.Remove(id);
                    }


                    foreach (Projectile proj in theWorld.Projectiles.Values)
                    {
                        sb.Append(proj.ToString());
                        if (proj.died)
                        {
                            projectilesToRemove.Add(proj.proj);
                        }
                    }
                    // Removes all the dead projectiles
                    foreach(int id in projectilesToRemove)
                    {
                        theWorld.Projectiles.Remove(id);
                    }

                    foreach (Beam beam in theWorld.Beams.Values)
                    {
                        sb.Append(beam.ToString());
                        if (beam.died)
                        {
                            beamToRemove.Add(beam.beam);
                        }
                    }
                    foreach(int id in beamToRemove)
                    {
                        theWorld.Beams.Remove(id);
                    }

                    foreach (Powerup powerup in theWorld.Powerups.Values)
                    {
                        sb.Append(powerup.ToString());
                        if (powerup.died)
                        {
                            powerupsToRemove.Add(powerup.power);
                        }
                    }
                    foreach(int id in powerupsToRemove)
                    {
                        theWorld.Powerups.Remove(id);
                    }
                }
                string frame = sb.ToString();
                lock (clients)
                {
                    // Sends all the information about the world to the clients
                    foreach(SocketState client in clients.Values)
                    {
                        Networking.Send(client.TheSocket, frame);
                    }
                }
            }
        }

        /// <summary>
        /// This method adds a new client and sets the callback to recevie the rest of the information
        /// from the client
        /// </summary>
        /// <param name="client">The client socket</param>
        private void NewClient(SocketState client)
        {
            client.OnNetworkAction = ReceivePlayerName;
            Console.WriteLine("Client " + client.ID + " has joined");
            Networking.GetData(client);
        }

        /// <summary>
        /// The networking callback to listen to the client name that it will send
        /// </summary>
        /// <param name="client">The client socket</param>
        private void ReceivePlayerName(SocketState client)
        {
            string name = client.GetData();
            if (!name.EndsWith("\n"))
            {
                client.GetData();
                return;
            }
            client.RemoveData(0, name.Length);
            name = name.Trim();
            
            // Send the client its id and the world startup info (walls, projectile, size)
            Networking.Send(client.TheSocket, client.ID + "\n");
            Networking.Send(client.TheSocket, startupInfo);

            // Adds the new tank at a new random location
            lock (theWorld)
            {
                theWorld.Tanks[(int)client.ID] = new Tank((int)client.ID, name, theWorld.setLoctaion());
            }
            
            // Adds this new client to the list of clients
            lock (clients)
            {
                clients.Add((int)client.ID, client);
            }
            client.OnNetworkAction = ReceiveControlCommand;
            Networking.GetData(client);
        }

        /// <summary>
        /// The callback to receive movement commands from the client and move the tank/shoot projectiles/shoot beams
        /// 'Verifies the movement is legal and doesn't cause collisions
        /// 
        /// This also checks for disconnected clients and removes them. 
        /// 
        /// This method also runs our extra feature of hooking it up to a SQL database. The top scores will be
        /// displayed to the console when all clients have disconnected. 
        /// </summary>
        /// <param name="client">The client socket state</param>
        private void ReceiveControlCommand(SocketState client)
        {
            if (client.ErrorOccured)
            {
                lock (theWorld)
                {
                    // display message that a client has disconnected
                    Console.WriteLine("Client " + client.ID + " disconnected");
                    theWorld.Commands.Remove((int)client.ID);
                    Tank disconnectedTank = theWorld.Tanks[(int)client.ID];
                    disconnectedTank.disconnected = true;
                    // Adds the client's score to the top score database
                    WriteToDB(disconnectedTank.name, disconnectedTank.score);
                    clients.Remove((int)client.ID);
                    client.TheSocket.Close();
                    // Displays top scores to console when there are no more clients connected
                    if(clients.Count == 0)
                    {
                        DisplayTopScores();
                    }
                }
            }
            else
            {
                string totalData = client.GetData();
                string[] parts = Regex.Split(totalData, @"(?<=[\n])");
                foreach (string p in parts)
                {
                    if (p.Length == 0)
                    {
                        continue;
                    }


                    if (p[p.Length - 1] != '\n')
                    {
                        break;
                    }


                    try
                    {
                        ControlCommand ctrlCmd = JsonConvert.DeserializeObject<ControlCommand>(p);

                        lock (theWorld)
                        {
                            theWorld.Commands[(int)client.ID] = ctrlCmd;
                        }
                        client.RemoveData(0, p.Length);
                    }
                    catch
                    {

                    }
                }
                //error when client disconnects
                Networking.GetData(client);
            }
        }

        /// <summary>
        /// This method stores a player's score to a database 
        /// </summary>
        /// <param name="playerName">The player name to be stored</param>
        /// <param name="score">The score to be stored</param>
        public void WriteToDB(string playerName, int score)
        {
            SqlConnection connection;
            // The sql database credentials
            string connetionString = "Data Source=10.211.55.3,49170;" +
                                     "Initial Catalog=master;" +
                                     "User ID=syntactic_sugar_daddies;" +
                                     "Password=LittleBobbyTables;";

            using (connection = new SqlConnection(connetionString))
            {
                // The query statement to insert the scores
                string query = "INSERT INTO dbo.high_scores (player_name, score) VALUES (@player_name,@score);";
                // Structure taken from : https://stackoverflow.com/questions/19956533/sql-insert-query-using-c-sharp
                using (SqlCommand newCommand = new SqlCommand(query, connection))
                {
                    try
                    {
                        if (!playerName.Contains("DROP TABLE")) // Sanitizing Database 💅 https://xkcd.com/327/
                        {
                            newCommand.Parameters.AddWithValue("@player_name", playerName);
                            newCommand.Parameters.AddWithValue("@score", score);

                            connection.Open();
                            int result = newCommand.ExecuteNonQuery();
                        }
                    }
                    catch(Exception e)
                    {
                        Console.WriteLine("Sql exception: " + e.ToString());
                    }
                }
            }
        }

        /// <summary>
        /// This method is used to display the top 5 scores of all time. It connects to our sql database
        /// </summary>
        public void DisplayTopScores()
        {
            SqlConnection connection;
            // Database credentials
            string connetionString = "Data Source=10.211.55.3,49170;" +
                                     "Initial Catalog=master;" +
                                     "User ID=syntactic_sugar_daddies;" +
                                     "Password=LittleBobbyTables;"; // We couldn't figure out how to encrypt... please don't hack us!!!
            Console.WriteLine("TOP SCORES");
            using (connection = new SqlConnection(connetionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = connection.CreateCommand();
                    // The query to be run. Sorts by top score
                    command.CommandText = "SELECT * FROM dbo.high_scores ORDER BY score DESC;";

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        int count = 0;

                        // This while loop grabs the top five scores
                        while (reader.Read() && count < 5)
                        {
                            Console.WriteLine("Player Name: " + reader["player_name"] + "    Score: " + reader["score"]);
                            count++;
                        }
                    }
                }
                catch(Exception e)
                {
                    Console.WriteLine("Sql exception: " + e.ToString());
                }
            }
        }
    }
}
