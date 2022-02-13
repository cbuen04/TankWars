using Model;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameController;
using System.Drawing.Drawing2D;

namespace TankWars
{
    /// <summary>
    /// This class inherits from panel to act as a drawing panel to display 
    /// the state of the world to the user. This object is initialized in view and accesses all 
    /// the objects in the world object to constantly display all players, walls, projectiles, powerups, 
    /// and beams
    /// </summary>
    public class DrawingPanel : Panel
    {
        private World theWorld;
        private GameCtrl ctrl;
        private double viewSize;
        private int ConnectionID = -1;

        private double lastXLocation = 0;
        private double lastYLocation = 0;

        // Initializes all images
        private Image Background = Image.FromFile(@"..\..\..\Resources\Images\Background.png");
        private Image wallImage = Image.FromFile(@"..\..\..\Resources\Images\WallSprite.png");
        private Image logo = Image.FromFile(@"..\..\..\Resources\Images\tankwars logo.png");

        private Image[] TankImages = new Image[]
        {
            Image.FromFile(@"..\..\..\Resources\Images\BlueTank.png"),
            Image.FromFile(@"..\..\..\Resources\Images\DarkTank.png"),
            Image.FromFile(@"..\..\..\Resources\Images\GreenTank.png"),
            Image.FromFile(@"..\..\..\Resources\Images\LightGreenTank.png"),
            Image.FromFile(@"..\..\..\Resources\Images\OrangeTank.png"),
            Image.FromFile(@"..\..\..\Resources\Images\PurpleTank.png"),
            Image.FromFile(@"..\..\..\Resources\Images\RedTank.png"),
            Image.FromFile(@"..\..\..\Resources\Images\YellowTank.png")
        };

        private Image[] TurretImages = new Image[]
        {
            Image.FromFile(@"..\..\..\Resources\Images\BlueTurret.png"),
            Image.FromFile(@"..\..\..\Resources\Images\DarkTurret.png"),
            Image.FromFile(@"..\..\..\Resources\Images\GreenTurret.png"),
            Image.FromFile(@"..\..\..\Resources\Images\LightGreenTurret.png"),
            Image.FromFile(@"..\..\..\Resources\Images\OrangeTurret.png"),
            Image.FromFile(@"..\..\..\Resources\Images\PurpleTurret.png"),
            Image.FromFile(@"..\..\..\Resources\Images\RedTurret.png"),
            Image.FromFile(@"..\..\..\Resources\Images\YellowTurret.png")
        };

        private Image[] ProjectileImages = new Image[]
        {
            Image.FromFile(@"..\..\..\Resources\Images\shot-blue.png"),
            Image.FromFile(@"..\..\..\Resources\Images\shot-grey.png"),
            Image.FromFile(@"..\..\..\Resources\Images\shot-green.png"),
            Image.FromFile(@"..\..\..\Resources\Images\shot-white.png"),
            Image.FromFile(@"..\..\..\Resources\Images\shot-brown.png"),
            Image.FromFile(@"..\..\..\Resources\Images\shot-violet.png"),
            Image.FromFile(@"..\..\..\Resources\Images\shot-red.png"),
            Image.FromFile(@"..\..\..\Resources\Images\shot-yellow.png"),
    };
        private Image deadTankImage = Image.FromFile(@"..\..\..\Resources\Images\DeadTank.png");


        /// <summary>
        /// The constructor of the object which takes in theWorld used by the game and the controller. 
        /// </summary>
        /// <param name="w">The world used by this instance of the game</param>
        /// <param name="gameCtrl">The controller to get the main player's location</param>
        public DrawingPanel(World w, GameCtrl gameCtrl)
        {
            DoubleBuffered = true;
            theWorld = w;
            ctrl = gameCtrl;
            viewSize = gameCtrl.viewSize;
            // Resizing all images to be the proper size standards
            for (int i = 0; i < 8; i++)
            {
                TankImages[i] = ImageRsz(TankImages[i], 60, 60);
                TurretImages[i] = ImageRsz(TurretImages[i], 50, 50);
                ProjectileImages[i] = ImageRsz(ProjectileImages[i], 30, 30);
            }
            wallImage = ImageRsz(wallImage, 50, 50);
            deadTankImage = ImageRsz(deadTankImage, 100, 100);
        }

        // A delegate for DrawObjectWithTransform
        // Methods matching this delegate can draw whatever they want using e  
        public delegate void ObjectDrawer(object o, PaintEventArgs e);

        /// <summary>
        /// This method performs a translation and rotation to drawn an object in the world.
        /// </summary>
        /// <param name="e">PaintEventArgs to access the graphics (for drawing)</param>
        /// <param name="o">The object to draw</param>
        /// <param name="worldX">The X coordinate of the object in world space</param>
        /// <param name="worldY">The Y coordinate of the object in world space</param>
        /// <param name="angle">The orientation of the objec, measured in degrees clockwise from "up"</param>
        /// <param name="drawer">The drawer delegate. After the transformation is applied, the delegate is invoked to draw whatever it wants</param>
        private void DrawObjectWithTransform(PaintEventArgs e, object o, double worldX, double worldY, double angle, ObjectDrawer drawer)
        {
            // "push" the current transform
            System.Drawing.Drawing2D.Matrix oldMatrix = e.Graphics.Transform.Clone();

            e.Graphics.TranslateTransform((int)worldX, (int)worldY);
            e.Graphics.RotateTransform((float)angle);
            drawer(o, e);

            // "pop" the transform
            e.Graphics.Transform = oldMatrix;
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void TankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            int width = 60;
            int height = 60;
            Image tankImage = null;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            {
                if (!t.died)
                {

                    if (t.ID % 8 == 0)
                    {
                        tankImage = TankImages[0];
                    }
                    else if (t.ID % 8 == 1)
                    {
                        tankImage = TankImages[1];
                    }
                    else if (t.ID % 8 == 2)
                    {
                        tankImage = TankImages[2];

                    }
                    else if (t.ID % 8 == 3)
                    {
                        tankImage = TankImages[3];
                    }
                    else if (t.ID % 8 == 4)
                    {
                        tankImage = TankImages[4];
                    }
                    else if (t.ID % 8 == 5)
                    {
                        tankImage = TankImages[5];
                    }
                    else if (t.ID % 8 == 6)
                    {
                        tankImage = TankImages[6];
                    }
                    else if (t.ID % 8 == 7)
                    {
                        tankImage = TankImages[7];
                    }
                    // Draw image to screen.
                    e.Graphics.DrawImage(tankImage, -(width / 2), -(height / 2));
                }
            }
        }

        /// <summary>
        /// Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method
        /// </summary>
        /// <param name="o">The object to draw</param>
        /// <param name="e">The PaintEventArgs to access the graphics</param>
        private void PowerupDrawer(object o, PaintEventArgs e)
        {
            Powerup p = o as Powerup;

            int width = 8;
            int height = 8;
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            {


                // Circles are drawn starting from the top-left corner.
                // So if we want the circle centered on the powerup's location, we have to offset it
                // by half its size to the left (-width/2) and up (-height/2)
                Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);

                e.Graphics.FillEllipse(redBrush, r);
            }
        }
        /// <summary>
        ///  Acts as a drawing delegate for DrawObjectWithTransform
        /// After performing the necessary transformation (translate/rotate)
        /// DrawObjectWithTransform will invoke this method. 
        /// This draws turrets
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void TurretDrawer(object o, PaintEventArgs e)
        {
            int width = 50;
            int height = 50;
            Tank t = o as Tank;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            {
                Image turret = null;
                if (t.ID % 8 == 0)
                {
                    turret = TurretImages[0];
                }
                else if (t.ID % 8 == 1)
                {
                    turret = TurretImages[1];
                }
                else if (t.ID % 8 == 2)
                {
                    turret = TurretImages[2];

                }
                else if (t.ID % 8 == 3)
                {
                    turret = TurretImages[3];
                }
                else if (t.ID % 8 == 4)
                {
                    turret = TurretImages[4];
                }
                else if (t.ID % 8 == 5)
                {
                    turret = TurretImages[5];
                }
                else if (t.ID % 8 == 6)
                {
                    turret = TurretImages[6];
                }
                else if (t.ID % 8 == 7)
                {
                    turret = TurretImages[7];
                }

                // Draw image to screen.
                e.Graphics.DrawImage(turret, -(width / 2), -(height / 2));
            }
        }

        /// <summary>
        /// Helper method to draw all the walls in the world. This will determine 
        /// a wall's orientation (horizontal or vertical) and cut the walls up into blocks to 
        /// draw the walls block by block
        /// </summary>
        /// <param name="w"></param>
        /// <param name="e"></param>
        private void DrawWallHelper(Wall w, PaintEventArgs e)
        {
            Vector2D p1 = w.p1;
            Vector2D p2 = w.p2;

            // Drawing Vertical Wall
            if (IsVertical(p1.GetX(), p2.GetX()))
            {
                // Determines if the 1st or second coordinate is bigger to get the accurate number of wall 
                // blocks
                if (p1.GetY() < p2.GetY())
                {
                    double iterationP1 = p1.GetY();
                    int wallIterations = (int)(Math.Abs(p2.GetY() - p1.GetY()) / 50); // tells how many sets of blocks there are in this wall
                    for (int i = 0; i <= wallIterations; i++)
                    {
                        DrawObjectWithTransform(e, w, p1.GetX(), iterationP1, 0, WallDrawer);
                        iterationP1 += 50;
                    }
                }
                else
                {
                    double iterationP1 = p1.GetY();
                    int wallIterations = (int)(Math.Abs(p2.GetY() - p1.GetY()) / 50); // tells how many sets of wall blocks there are to draw
                    for (int i = 0; i <= wallIterations; i++)
                    {
                        DrawObjectWithTransform(e, w, p1.GetX(), iterationP1, 0, WallDrawer);
                        iterationP1 -= 50;
                    }
                }
            }
            // Case where wall is horizontal. Logic is identical with a shift in orientation
            else
            {
                if (p1.GetX() < p2.GetX())
                {
                    double iterationP1 = p1.GetX();
                    int wallIterations = (int)(Math.Abs(p2.GetX() - p1.GetX()) / 50);
                    for (int i = 0; i <= wallIterations; i++)
                    {
                        DrawObjectWithTransform(e, w, iterationP1, p1.GetY(), 0, WallDrawer);
                        iterationP1 += 50;
                    }
                }
                else
                {
                    double iterationP1 = p1.GetX();
                    int wallIterations = (int)(Math.Abs(p2.GetX() - p1.GetX()) / 50);
                    for (int i = 0; i <= wallIterations; i++)
                    {
                        DrawObjectWithTransform(e, w, iterationP1, p1.GetY(), 0, WallDrawer);
                        iterationP1 -= 50;
                    }
                }
            }

        }
        /// <summary>
        /// This is the actual drawer of the wall which draws the designated wall image
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void WallDrawer(object o, PaintEventArgs e)
        {
            Wall w = o as Wall;

            Vector2D p1 = w.p1;
            Vector2D p2 = w.p2;

            int width = 50;
            int height = 50;

            e.Graphics.DrawImage(wallImage, -(width / 2), -(height / 2));
        }

        /// <summary>
        /// Determines if the wall is vertical
        /// </summary>
        /// <param name="x1">The first point's x coordinate</param>
        /// <param name="x2">The second point's x coordinate</param>
        /// <returns>A boolean saying if the wall is vertical. Will return false if horizonatl</returns>
        private bool IsVertical(double x1, double x2)
        {
            return (x1 == x2);
        }

        /// <summary>
        /// This is the drawer for the projectile which paints the actual shape to the world at the
        /// designated spot as dictated by the DrawObjectWithTransform called in the OnPaint.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void ProjectileDrawer(object o, PaintEventArgs e)
        {
            int width = 30;
            int height = 30;

            Projectile p = o as Projectile;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            {
                Image projectile = null;


                if (p.owner % 8 == 0)
                {
                    projectile = ProjectileImages[0];
                }
                else if (p.owner % 8 == 1)
                {
                    projectile = ProjectileImages[1];
                }
                else if (p.owner % 8 == 2)
                {
                    projectile = ProjectileImages[2];

                }
                else if (p.owner % 8 == 3)
                {
                    projectile = ProjectileImages[3];
                }
                else if (p.owner % 8 == 4)
                {
                    projectile = ProjectileImages[4];
                }
                else if (p.owner % 8 == 5)
                {
                    projectile = ProjectileImages[5];
                }
                else if (p.owner % 8 == 6)
                {
                    projectile = ProjectileImages[6];
                }
                else if (p.owner % 8 == 7)
                {
                    projectile = ProjectileImages[7];
                }

                // Draw image to screen.
                e.Graphics.DrawImage(projectile, -(width / 2), -(height / 2));
            }

        }

        /// <summary>
        /// This draws the actual beam and implements beam logic to decrease the beam's thickness on every frame
        /// until it stops painting. 
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void BeamDrawer(object o, PaintEventArgs e)
        {
            Beam b = o as Beam;
            int width = theWorld.size * 3;
            // This ratio represents the number of frames left until the beam dies to how many frames 
            // it had to die initially. 
            double beamRatioLeft = b.framesTillDie / Constants.MaxDF;
            // This ratio will decrease (and is always <=1) as frames till die decrements, and the width of the
            // beam is proportional to this ratio. Thus, it shrinks as frames move on
            int height = (int)(5 * beamRatioLeft);

            using (System.Drawing.SolidBrush pinkBrush = new System.Drawing.SolidBrush(System.Drawing.Color.HotPink))
            {
                Rectangle r = new Rectangle(0, 0, width, height);
                e.Graphics.FillRectangle(pinkBrush, r);
            }
        }

        /// <summary>
        /// this paints the picture of the dead tank once it has been declared dead
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void DeadTankDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            int width = 60;
            int height = 60;

            e.Graphics.DrawImage(deadTankImage, -(width / 2), -(height / 2));

        }

        /// <summary>
        /// This helper method resizes an image to its designated size
        /// </summary>
        /// <param name="image">The image to be resized</param>
        /// <param name="width">The width to be resized to</param>
        /// <param name="height">The height to be resized to</param>
        /// <returns>The resized image</returns>
        public Image ImageRsz(Image image, int width, int height)
        {
            return image = (Image)new Bitmap(image, width, height);
        }

        /// <summary>
        /// This drawer will draw the health bar of the tank. The length of this 
        /// bar is proportional to how many health points it has left.
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private void HealthDrawer(object o, PaintEventArgs e)
        {
            Tank t = o as Tank;

            double health = t.healthPoints;
            double maxHealth = Constants.MaxHP;
            // This ratio is the tank's current health proportional to the max points it could have
            double healthRatio = health / maxHealth;

            // The width of this bar is proprotional to how much health it has
            int width = (int)(50 * healthRatio);
            int height = 6;
            using (System.Drawing.SolidBrush redBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Red))
            using (System.Drawing.SolidBrush greenBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Green))
            using (System.Drawing.SolidBrush yellowBrush = new System.Drawing.SolidBrush(System.Drawing.Color.Yellow))
            {
                if (healthRatio > .67)
                {
                    Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
                    e.Graphics.FillRectangle(greenBrush, r);
                }
                if (healthRatio > .34 && healthRatio < .67)
                {
                    Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
                    e.Graphics.FillRectangle(yellowBrush, r);
                }
                if(healthRatio > 0 && healthRatio < .34)
                {
                    Rectangle r = new Rectangle(-(width / 2), -(height / 2), width, height);
                    e.Graphics.FillRectangle(redBrush, r);
                }
            }

        }

        /// <summary>
        /// This method is invoked when the DrawingPanel needs to be re-drawn, and is invoked every frame. It extracts the
        /// contents of the world and paints them
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e) // how do we make this not called when the world hasn't been drawn yet?
        {
            this.theWorld = ctrl.getWorld();
            Dictionary<int, Explosion> newExplosions = new Dictionary<int, Explosion>();

            if (theWorld != null)
            {
                ConnectionID = theWorld.id;
                int width = theWorld.size; // should be 2000 pixels
                int height = theWorld.size;

                // Checks if a connection has been established
                if (!(ConnectionID == -1))
                {
                    int mainPlayer = ctrl.getPlayerID();
                    lock (theWorld)
                    {
                        if (Background.Width != theWorld.size)
                        {
                            Background = ImageRsz(Background, theWorld.size, theWorld.size);
                        }


                        if (theWorld.Tanks.TryGetValue(mainPlayer, out Tank player))
                        {

                            double playerX = player.location.GetX(); // (the player's world-space X coordinate)
                            double playerY = player.location.GetY(); // (the player's world-space Y coordinate)

                            // added to get death location which will be where the tank is last seen
                            lastXLocation = playerX;
                            lastYLocation = playerY;

                            base.BackColor = Color.Pink;

                            // place the view to the player joined
                            e.Graphics.TranslateTransform((float)(-playerX + (viewSize / 2)), (float)(-playerY + (viewSize / 2)));
                        }
                        else
                        {
                            // paints scene at death location and animates death
                            // place the view to the player joined
                            e.Graphics.TranslateTransform((float)(-lastXLocation + (viewSize / 2)), (float)(-lastYLocation + (viewSize / 2)));
                        }

                        // draw image to screen
                        e.Graphics.DrawImage(Background, -(width / 2), -(height / 2));

                        // Draw the players
                        foreach (Tank tank in theWorld.Tanks.Values)
                        {
                            DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), tank.orientation.ToAngle(), TankDrawer);
                            DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY(), tank.aiming.ToAngle(), TurretDrawer);
                            DrawObjectWithTransform(e, tank, tank.location.GetX(), tank.location.GetY() - 40, 0, HealthDrawer);

                            string name = tank.name;
                            int score = tank.score;

                            using (Font arialFont = new Font("Comic Sans MS", 10))
                            {
                                e.Graphics.DrawString(name + " : " + score, arialFont, Brushes.Black, (float)tank.location.GetX() - 25f, (float)tank.location.GetY() + 35);

                            }
                        }

                        // Draw the powerups
                        foreach (Powerup pow in theWorld.Powerups.Values)
                        {
                            DrawObjectWithTransform(e, pow, pow.loc.GetX(), pow.loc.GetY(), 0, PowerupDrawer);
                        }

                        // Draw the projectiles
                        foreach (Projectile proj in theWorld.Projectiles.Values)
                        {
                            int projectileOwner = proj.owner;
                            theWorld.Tanks.TryGetValue(projectileOwner, out Tank ownerTank); // do we need to fix?
                            DrawObjectWithTransform(e, proj, proj.loc.GetX(), proj.loc.GetY(), proj.dir.ToAngle(), ProjectileDrawer);
                        }

                        // Draw the beams
                        foreach (Beam beam in theWorld.Beams.Values)
                        {
                            if (beam.framesTillDie > 0)
                            {
                                DrawObjectWithTransform(e, beam, beam.org.GetX(), beam.org.GetY(), beam.dir.ToAngle() + 270, BeamDrawer);
                                beam.framesTillDie--;
                            }
                        }

                        // Draw the walls
                        foreach (Wall wall in theWorld.Walls.Values)
                        {
                            DrawWallHelper(wall, e);
                        }
                        // Draw the dead tanks
                        foreach (Explosion explosion in theWorld.Explosions.Values)
                        {
                            if (explosion.GetFramesTillDone() > 0)
                            {
                                Console.WriteLine(explosion.GetFramesTillDone());
                                newExplosions[explosion.explosionID] = explosion;
                                DrawObjectWithTransform(e, explosion, explosion.xCoordinate - (60/2), explosion.yCoordinate - (60/2), 0, DeadTankDrawer);
                                explosion.DecrementFramesTillDone();
                            }
                 
                        }
                        theWorld.Explosions = newExplosions;
                    }
                   
                }
                base.OnPaint(e);
            }
            else // Draws the startup image of the custom background.
            {

                logo = ImageRsz(logo, 900, 800);

                e.Graphics.DrawImage(logo, 0, 0);
                base.OnPaint(e);
            }
        }
    }
}