using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameController;
using Model;
using TankWars;

namespace View
{
    /// <summary>
    /// This class inherits form to act as the view for the tank wars game. This class acts to 
    /// display any visual items of the game, as well as notify the controller if a user makes an action
    /// (such as pressing keys, moving/clicking the mouse, or entering connection information), and initializes
    /// a drawing panel to hold the actual graphics of the game. 
    /// </summary>
    public partial class View : Form
    {
        private GameCtrl gameCtrl;
        private DrawingPanel drawingPanel;
        private World theWorld; 
        private string serverName;
        private string playerName;

        private const int viewSize = 900;
        private const int menuSize = 40;
        private bool isConnected;

        /// <summary>
        /// This is the constructor of this class. This initializes the drawing panel and any
        /// mouse and button handlers.
        /// </summary>
        public View()
        {
            InitializeComponent();

            ServerAdderess.Text = "localhost";

            // drawing panel instantiation
            ClientSize = new Size(viewSize, viewSize + menuSize);
            gameCtrl = new GameCtrl();
            gameCtrl.UpdateArrived += OnFrame;
            gameCtrl.ErrorOccured += ShowError;
            theWorld = gameCtrl.getWorld();
            drawingPanel = new DrawingPanel(theWorld, gameCtrl);
            drawingPanel.Location = new Point(0, 0);
            drawingPanel.Size = new Size(2000, 2000);
            this.Controls.Add(drawingPanel);

            // Key Handlers
            isConnected = false;
            KeyPreview = true;
            this.KeyDown += HandleKeyDown;
            this.KeyUp += HandleKeyUp;
            drawingPanel.MouseMove += HandleMouseMoved;
            drawingPanel.MouseDown += HandleMouseDown;
            drawingPanel.MouseUp += HandleMouseUp;
        }

        /// <summary>
        /// This is the method that tells the drawing panel to repaint every frame. 
        /// Includes a method invoker to make sure multiple threads aren't being invalidated 
        /// </summary>
        private void OnFrame()
        {
            try
            {
                this.Invoke(new MethodInvoker(() => Invalidate(true)));
                MethodInvoker invalidator = new MethodInvoker(
                    () => this.Invalidate(true));
            }
            catch
            {
                // We know the application was closed by the user, throw no errors
            }
        }

        /// <summary>
        /// This method registers when the game controller has detected a server connection error via an event
        /// and displays a corresponding error message to the view 
        /// </summary>
        private void ShowError()
        {
            MessageBox.Show(gameCtrl.errorMessage);
        }

        /// <summary>
        /// This is the key handler which handles a user pushing down control keys W,S,D,A, and esc. This
        /// notifies the controller accordingly by changing member variables of the game controller class
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            // Checks connection establisment
            if (isConnected)
            {
                if (e.KeyCode == Keys.W)
                {
                    // This logic used in each of the individual key handlers adds the user's movement
                    // request to the list established in the controller once meeting certain requirments
                    // (further explained in readme)
                    if (gameCtrl.direction.Contains("up") && gameCtrl.direction.Last() != "up")
                    {
                        gameCtrl.direction.Remove("up");
                        gameCtrl.direction.Add("up");
                    }
                    else if (gameCtrl.direction.Last() != "up")
                        gameCtrl.direction.Add("up");

                }
                else if (e.KeyCode == Keys.S)
                {
                    if (gameCtrl.direction.Contains("down") && gameCtrl.direction.Last() != "down")
                    {
                        gameCtrl.direction.Remove("down");
                        gameCtrl.direction.Add("down");

                    }
                    else if (gameCtrl.direction.Last() != "down")
                    {
                        gameCtrl.direction.Add("down");
                    }
                }
                else if (e.KeyCode == Keys.D)
                {
                    if (gameCtrl.direction.Contains("right") && gameCtrl.direction.Last() != "right")
                    {
                        gameCtrl.direction.Remove("right");
                    }
                    else if(gameCtrl.direction.Last() != "right")
                        gameCtrl.direction.Add("right");
                }
                else if (e.KeyCode == Keys.A)
                {
                    if (gameCtrl.direction.Contains("left") && gameCtrl.direction.Last() != "left")
                    {
                        gameCtrl.direction.Remove("left");
                        gameCtrl.direction.Add("left");

                    }
                    else if(gameCtrl.direction.Last() != "left")
                        gameCtrl.direction.Add("left");
                }
            }
            if (e.KeyCode == Keys.Escape)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// This handles when a control key is released and notifies the game controller to stop registering
        /// keys and stop tank movement in that respective direction. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleKeyUp(object sender, KeyEventArgs e)
        {
            if (isConnected)
            {
                if (e.KeyCode == Keys.W)
                {
                    gameCtrl.direction.Remove("up");
                }
                else if (e.KeyCode == Keys.S)
                {
                    gameCtrl.direction.Remove("down");
                }
                else if (e.KeyCode == Keys.D)
                {
                    gameCtrl.direction.Remove("right");
                }
                else if (e.KeyCode == Keys.A)
                {
                    gameCtrl.direction.Remove("left");
                }
            }
        }

        /// <summary>
        /// This is the mouse handler which detects mouse movements and notifies the game controller of the 
        /// most recent location of the mouse.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseMoved(object sender, MouseEventArgs e)
        {
            if (isConnected)
            {
                gameCtrl.MouseMoved(e.X, e.Y);
            }
        }

        /// <summary>
        /// This method handles the mouse being pressed down to shoot a beam or projectile.
        /// This notifies the game controller of the tank's request to shoot.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseDown(object sender, MouseEventArgs e)
        {
            if (isConnected)
            {
                if (e.Button == MouseButtons.Left)
                {
                    gameCtrl.fire = "main";
                }
                else if (e.Button == MouseButtons.Right)
                {
                    gameCtrl.fire = "alt";
                }
            }
        }

        /// <summary>
        /// This cancels the request to the server (via notifying the game controller) that 
        /// the player wishes to stop shooting or firing a beam
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleMouseUp(object sender, MouseEventArgs e)
        {
            if (isConnected)
            {
                if (e.Button == MouseButtons.Left)
                {
                    gameCtrl.fire = "none";
                }
                else if (e.Button == MouseButtons.Right)
                {
                    gameCtrl.fire = "none";
                }
            }
        }

        /// <summary>
        /// This registers which name the user has input the server address to be and changes
        /// the instance variable corresponding to the address to be used at connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ServerAdderess_TextChanged(object sender, EventArgs e)
        {
            serverName = ServerAdderess.Text;
        }

        /// <summary>
        /// This registers which name the user has input the player name to be and changes
        /// the instance variable corresponding to the address to be used at connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PlayerName_TextChanged(object sender, EventArgs e)
        {
            playerName = PlayerName.Text;
        }

        /// <summary>
        /// This handles the user pressing the connect button and notifies the game controller
        /// (which later tells the server) that the user requests to connect. It will give the controller
        /// the server name and player name after checking that they are not null. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Connect_Click(object sender, EventArgs e)
        {
            // Checks if the information required is not null
            if (!ReferenceEquals(serverName, null) && !ReferenceEquals(playerName, null))
            {
                ServerAdderess.Visible = false;
                PlayerName.Visible = false;
                Connect.Visible = false;
                isConnected = true;
                gameCtrl.Connect(playerName, serverName);
                // need to handle when server isn't on
            }
            
        }

        /// <summary>
        /// This handles when the user wishes to see the help menu popup and populates
        /// it with the game instructions. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Help_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Controls: \n" +
                                                 "W:                Move up \n" +
                                                 "A:                 Move left \n" +
                                                 "S:                 Move down \n" + 
                                                 "Mouse:        Aim \n" +
                                                 "Left click:     Fire Projectile \n" +
                                                 "Right click:  Fire Beam \n" +
                                                 "esc:               Quit");
        }
    }
}
