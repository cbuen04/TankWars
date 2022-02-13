using System;
using System.Collections.Generic;
using TankWars;

namespace Model
{
    /// <summary>
    /// The world class which holds all the dictionaries of world objects and their respective getters and setters
    /// to be accessed by the game controller and drawing panel to check status and repaint. 
    /// </summary>
    public class World
    {
        public Dictionary<int, Tank> Tanks = new Dictionary<int, Tank>();
        public Dictionary<int, Powerup> Powerups = new Dictionary<int, Powerup>();
        public Dictionary<int, Projectile> Projectiles = new Dictionary<int, Projectile>();
        public Dictionary<int, Beam> Beams = new Dictionary<int, Beam>();
        public Dictionary<int, Wall> Walls = new Dictionary<int, Wall>();
        public Dictionary<int, Explosion> Explosions = new Dictionary<int, Explosion>();
        public Dictionary<int, ControlCommand> Commands = new Dictionary<int, ControlCommand>();

        public int MSPerFrame { get; set; }
        public int RespawnRate { get; set; }
        public int FramesPerShot { get; set; }
        public int framesTillPowerup;


        public int size { get; set; }
        public int id { get; set; }

        /// <summary>
        /// Constructor which initializes a new world with the specified size
        /// </summary>
        /// <param name="size">The requested size of the world to be drawn at</param>
        public World(int size)
        {
            this.size = size;
        }

        /// <summary>
        /// This method is responsible for detecting updates from the client with requests to move. It 
        /// gets the requested commands and sets the velocities. 
        /// </summary>
        public void Update()
        {
            lock (this)
            {
                foreach (KeyValuePair<int, ControlCommand> cmd in Commands)
                {
                    // this extracts the tank from the command dictionary and processes its requests
                    Tank tank = Tanks[cmd.Key];
                    {
                        // Handles the command key to reset the tank position
                        switch (cmd.Value.Moving)
                        {
                            case "up":
                                tank.Velocity = new Vector2D(0, -1);
                                tank.orientation = new Vector2D(0, -1);
                                break;
                            case "down":
                                tank.Velocity = new Vector2D(0, 1);
                                tank.orientation = new Vector2D(0, 1);
                                break;
                            case "left":
                                tank.Velocity = new Vector2D(-1, 0);
                                tank.orientation = new Vector2D(-1, 0);
                                break;
                            case "right":
                                tank.Velocity = new Vector2D(1, 0);
                                tank.orientation = new Vector2D(1, 0);
                                break;
                            default:
                                tank.Velocity = new Vector2D(0, 0);
                                break;
                        }
                        switch (cmd.Value.Fire)
                        {
                            case "main":
                                // check to see if they can fire based on server credentials
                                if (tank.framesTillShoot == 0)
                                {
                                    tank.framesTillShoot = FramesPerShot;
                                    Projectile proj = new Projectile(tank.location, tank.ID, tank.aiming);
                                    proj.Velocity = proj.dir;
                                    Projectiles[proj.proj] = proj;
                                }
                                break;

                            case "alt":
                                // checks to see if a tank has picked up a powerup and is allowed to shoot a beam
                                if (tank.beamsAvailable > 0)
                                {
                                    Beam beam = new Beam(tank.location, tank.ID, tank.aiming);
                                    Beams.Add(beam.beam, beam);
                                    tank.beamsAvailable--;
                                }
                                break;
                            default:
                                break;
                        }
                        // sets the velocity of the tank
                        tank.Velocity *= Constants.TankSpeed;
                        tank.aiming = cmd.Value.TurretDirection;
                    }
                }
                Commands.Clear();

                // Checks for tank collisions and moves the tank to the new position if it can move
                foreach (Tank tank in Tanks.Values)
                {
                    Vector2D newLoc = tank.location + tank.Velocity;

                    bool collision = false;
                    if (tank.disconnected)
                    {
                        tank.healthPoints = 0;
                        tank.died = true;
                    }
                    if (tank.healthPoints == 0 && tank.framesSinceDeath == 0 && !tank.disconnected)
                    {
                        tank.died = true;
                        tank.framesSinceDeath = RespawnRate;
                    }

                    if (tank.framesTillShoot > 0)
                    {
                        tank.framesTillShoot--;
                    }

                    if (tank.Velocity.Length() == 0)
                    {
                        continue;
                    }
                    foreach (Wall wall in Walls.Values)
                    {
                        if (wall.CollidesTank(newLoc))
                        {
                            collision = true;
                            tank.Velocity = new Vector2D(0, 0);
                            break;
                        }
                    }

                    // If the tank is over the powerup, collect it
                    foreach(Powerup p in Powerups.Values)
                    {
                        if (tank.CollidesPoint(p.loc))
                        {
                            tank.beamsAvailable++;
                            p.died = true;
                            framesTillPowerup = Constants.powerupDelay;
                        }
                    }
                    if (newLoc.GetX() > (size / 2))
                    {
                        double y = tank.location.GetY();
                        newLoc = new Vector2D((-size / 2), y);
                    }
                    if (newLoc.GetX() < (-size / 2))
                    {
                        double y = tank.location.GetY();
                        newLoc = new Vector2D((size / 2), y);
                    }
                    if (newLoc.GetY() > (size / 2))
                    {
                        double x = tank.location.GetX();
                        newLoc = new Vector2D(x, (-size / 2));
                    }
                    if (newLoc.GetY() < (-size / 2))
                    {
                        double x = tank.location.GetX();
                        newLoc = new Vector2D(x, (size / 2));
                    }
                    if (!collision)
                    {
                        tank.location = newLoc;
                    }
                }
                // Checks beam collsion and kills the tank if that's the case
                foreach (Beam b in Beams.Values)
                {
                    foreach (Tank tank in Tanks.Values)
                    {
                        if (b.owner != tank.ID)
                        {
                            if (Intersects(b.org, b.dir, tank.location, 30))
                            {
                                tank.healthPoints = 0;
                                Tank ownerTank = Tanks[b.owner];
                                ownerTank.score++; 
                            }
                        }
                    }
                    b.died = true;
                    break;
                }
                foreach (Projectile proj in Projectiles.Values)
                {
                    // Projectiles that go outside the bounds of the world should be destroyed.
                    if (!proj.velocitySet)
                    {
                        proj.Velocity *= Constants.ProjSpeed;
                        proj.velocitySet = true;
                    }
                    Vector2D newLoc2 = proj.loc + proj.Velocity;

                    // set died to true or remove dead projectiles

                    bool collision = false;
                    bool hit = false;

                    if (proj.Velocity.Length() == 0)
                    {
                        continue;
                    }
                    foreach (Wall wall in Walls.Values)
                    {
                        if (wall.CollidesProj(newLoc2))
                        {
                            collision = true;
                            proj.Velocity = new Vector2D(0, 0);
                            proj.died = true;
                            break;
                        }
                    }
                    foreach (Tank t in Tanks.Values)
                    {
                        if (t.CollidesPoint(newLoc2) && (proj.owner != t.ID)) 
                        {
                            hit = true;
                            proj.Velocity = new Vector2D(0, 0);
                            proj.died = true;
                            t.healthPoints--;
                            if (t.healthPoints == 0)
                            {
                                Tanks[proj.owner].score++;
                            }
                            break;
                        }
                    }
                    if (proj.loc.GetX() > (size / 2) || proj.loc.GetX() < (-size / 2) || proj.loc.GetY() > (size / 2) || proj.loc.GetY() < (-size / 2))
                    {
                        proj.died = true;
                        break;
                    }
                    if (!collision)
                    {
                        proj.loc = newLoc2;
                    }
                }

                // powerup logic to add new powerups after a certain time has elapsed
                if (framesTillPowerup > 0 && Powerups.Count < Constants.maxPowerups)
                {
                    framesTillPowerup--;
                    if (framesTillPowerup == 0)
                    {
                        Powerup p = new Powerup(setLoctaion());
                        Powerups[p.power] = p;
                    }
                }
            }
        }

        /// <summary>
        /// Determines if a ray interescts a circle
        /// </summary>
        /// <param name="rayOrig">The origin of the ray</param>
        /// <param name="rayDir">The direction of the ray</param>
        /// <param name="center">The center of the circle</param>
        /// <param name="r">The radius of the circle</param>
        /// <returns></returns>
        public static bool Intersects(Vector2D rayOrig, Vector2D rayDir, Vector2D center, double r)
        {
            // ray-circle intersection test
            // P: hit point
            // ray: P = O + tV
            // circle: (P-C)dot(P-C)-r^2 = 0
            // substituting to solve for t gives a quadratic equation:
            // a = VdotV
            // b = 2(O-C)dotV
            // c = (O-C)dot(O-C)-r^2
            // if the discriminant is negative, miss (no solution for P)
            // otherwise, if both roots are positive, hit

            double a = rayDir.Dot(rayDir);
            double b = ((rayOrig - center) * 2.0).Dot(rayDir);
            double c = (rayOrig - center).Dot(rayOrig - center) - r * r;

            // discriminant
            double disc = b * b - 4.0 * a * c;

            if (disc < 0.0)
                return false;

            // find the signs of the roots
            // technically we should also divide by 2a
            // but all we care about is the sign, not the magnitude
            double root1 = -b + Math.Sqrt(disc);
            double root2 = -b - Math.Sqrt(disc);

            return (root1 > 0.0 && root2 > 0.0);
        }

        public Vector2D setLoctaion()
        {
            Random r = new Random();
            bool collides = true;
            int x = 0;
            int y = 0;
            while (collides)
            {
                x = r.Next((-size / 2), (size / 2));
                y = r.Next((-size / 2), (size / 2));
                Vector2D testLoc = new Vector2D(x, y);
                collides = false;
                foreach (Wall wall in Walls.Values)
                {
                    if (wall.CollidesTank(testLoc))
                    {
                        collides = true;
                        break;
                    }
                }

            }
            return new Vector2D(x, y);

        }
    }
}
