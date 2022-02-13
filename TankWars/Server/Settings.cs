using Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace TankWars
{
    /// <summary>
    /// This class reads the settings xml file and gets world credentials to be set when 
    /// the world is created in the server controller. Populates dicitonary with the walls
    /// </summary>
    class Settings
    {
        public int UniverseSize { get; private set; }  
        public int MSPerFrame { get; private set; }
        public int RespawnRate { get; private set; }
        public int FramesPerShot { get; private set; }
        public HashSet<Wall> walls { get; } = new HashSet<Wall>();

        /// <summary>
        /// The constructor which kicks off the method to read the xml
        /// </summary>
        /// <param name="filepath">The filepath string</param>
        public Settings(string filepath)
        {
            ReadSettingsXML(filepath);
        }
        
        /// <summary>
        /// This method reads the xml file and 
        /// </summary>
        /// <param name="filepath"></param>
        public void ReadSettingsXML(string filepath)
        {
            // Throws if there is no such file in existence
            if (!File.Exists(filepath))
            {
                Console.WriteLine("No settings file found");
            }
            try
            {
                // Create an XmlReader inside this block, and automatically Dispose() it at the end.
                using (XmlReader reader = XmlReader.Create(filepath))
                {
                    bool p1Next = false;
                    bool p2Next = false;
                    Vector2D p1 = null;
                    Vector2D p2 = null;
                    
                    // Sets the version and cell name/contents initially to nothing to be populated later
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "GameSettings":
                                    break;
                                case "UniverseSize":
                                    reader.Read();
                                    UniverseSize = int.Parse(reader.Value);
                                    break;
                                case "MSPerFrame":
                                    reader.Read();
                                    MSPerFrame = int.Parse(reader.Value);
                                    break;
                                case "FramesPerShot":
                                    reader.Read();
                                    FramesPerShot = int.Parse(reader.Value);
                                    break;
                                case "RespawnRate":
                                    reader.Read();
                                    RespawnRate = int.Parse(reader.Value);
                                    break;
                                case "Wall":
                                    break;
                                // The next p1 and p2 sections set the wall points
                                case "p1":
                                    bool x1Done = false;
                                    bool y1Done = false;
                                    double x1 = 0;
                                    double y1 = 0;
                                    while (!x1Done || !y1Done)
                                    {
                                        reader.Read();
                                        if (reader.Name == "x")
                                        {
                                            reader.Read();
                                            x1 = double.Parse(reader.Value);
                                            x1Done = true;
                                            reader.Read();
                                        }
                                        if (reader.Name == "y")
                                        {
                                            reader.Read();
                                            y1 = double.Parse(reader.Value); 
                                            y1Done = true;
                                            reader.Read();
                                        } 
                                    }
                                    p1 = new Vector2D(x1, y1);
                                    break;
                                case "p2":
                                    bool x2Done = false;
                                    bool y2Done = false;
                                    double x2 = 0;
                                    double y2 = 0;
                                    while (!x2Done || !y2Done)
                                    {
                                        reader.Read();
                                        if (reader.Name == "x")
                                        {
                                            reader.Read();
                                            x2 = double.Parse(reader.Value);
                                            x2Done = true;
                                            reader.Read();
                                        }
                                        if (reader.Name == "y")
                                        {
                                            reader.Read();
                                            y2 = double.Parse(reader.Value);
                                            y2Done = true;
                                            reader.Read();
                                        }
                                    }
                                    p2 = new Vector2D(x2, y2);
                                    break;
                            }
                        }
                        // Once the wall has set points, it will create a new wall and add to the dicitonary
                        if(!ReferenceEquals(p1, null) && !ReferenceEquals(p2,null))
                        {
                            Wall w = new Wall(p1, p2);
                            walls.Add(w);
                            p1 = null;
                            p2 = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.Read();
            }

        }
    }
}
