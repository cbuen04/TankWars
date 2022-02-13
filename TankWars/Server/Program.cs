using System;

namespace TankWars
{
    /// <summary>
    /// Authors: Charly Bueno and Emma Kerr
    /// 
    /// This class runs the server. It reads the settings file given and 
    /// Runs the server class
    /// </summary>
    class Program
    {
        /// <summary>
        /// The main method to kick off the server
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Settings settings = new Settings(@"..\..\..\..\Resources\settings.xml");
            ServerController serverController = new ServerController(settings);
            serverController.Start();
            Console.WriteLine("Server is running...");
            Console.Read();
        }
    }
}
