TANK WARS
Assignment collaborators: Charly Bueno and Emma Kerr

PS8
Design decisions: 

	Tank Controls:
	In order to create a smooth controller experience, a list of strings was used to hold the control priority, this ensured that the
	last pressed key was communicated to the server. This works by always reading the last element of the list. The list is preloaded
	with a "none" string when a key is pressed, the list checks to make sure its the first instance of the control, if it is, the control 
	is added, otherwise, the instance is removed and added to the end of the list making it the latest control. When the user lifts the key, 
	it is removed from the list. when all keys are lifted, the only string in the list is "none"

	Beam Animation:
	for the beam animation a constant was created in the beam class called frames till die, this is an int used as a frame
	counter that determines how many frames the beam will stay on the screen before disapearing. in order to animate this,
	the beam shrinks as the frames till die decrease. this is done by updating the size of the beam to a ratio int that takes
	the frames the beam currently has left, and dividing it by the frames constant it was given initially, this creates a shrinking
	effect that lasts exactly the amount of time it has on screen.

	Health Bar:
	similar to the animation of the beam, the health bar was constructed by creating a health ratio, which would be the current health
	a player has over the constant max health a player gets, the health bar would shrink according to this ratio. additionally
	the color of the health bar would change in intervals of thirds, 100-67% hp paints green, 66-34% paints yellow and 33-0% paints red
	this was done to give players an idea of how much health they had left

	Images Preloaded:
	for performance, the images had to be preloaded and resized for the world all at once in the drawing panel. Similar photos were
	grouped together in an image array for quick accessibility to loading objects to the world. This idea of preloading and resizing
	sped our game up from aprox 11 fps to 60 fps

	Death Tank:
	it was decided that for this there would be no animation for time constraints, we decided it would be best to load a death image
	that lasts as long as the tank is dead, and then the image disapears, it honestly looks funnier this was anyway and we both liked
	the outcome

	String constructor for json:
	the communication between the client and server was done by creating a helper method in the game controller called
	construct send string. the way it works is by having the view create string messages depending on the control being executed
	(tank moving, mouse moving, firing, etc.) and would add these string to member variables from the game controller class. This
	would relay accurate realtime information to the controller, and the controller could send it to the server on every frame request
	by calling the helper methid that constructs the string using the member variables.


Features: Extra things we want the graders aware of

	Start screen:
	for our game in order to add an extra level of polish, we decided to create a personalized start screen to our game
	it makes the game feel more finished.


PS9 README

Design Decisions:

	Set Location: this is a method that we created inorder to randomize the location of objects that need to be placed in the world
	this method checks to make sure it doesn't place anything in areas outside the world or in areas that cannot be normally accessed such 
	as the walls of the world.

	Tank Spawning: when a player connects to the server a tank will be created with it's unique tank ID. The tank will then be placed
	at a random location of the world using a set location method.

	Control Commands:
	for this assignment we incorporated a control commands class, the point of this class was to create the string fields
	required for the JSON for the tanks movements such as turret directions, firing, and main tank movement controls. we 
	loaded this into a dictionary that would create personal control commands for each individual tank so the server can properly
	execute the proper commands. The beam permission is determined using an int field that is in the tank class. This int called
	beamsAvailable increments each time a tank passes over a powerup. If there are beams available, a beam can fire and the int decrements.

	Death Tanks: The death of the tanks is triggered when the health of the tank drops to zero, this flips the death boolean to true
	and the tank will disapear and be randomly placed to another part of the screen using the set location method.

	Wall Collision: wall collisions are handled in the wall class, the wall creates a boundary box that checks if a tank is allowed
	to move. if the tank doesn't hit the box, then the tank's location can be updated, but if the tank's location intersects the boundary,
	the tank's location cannot move. this is all done through a method in the wall class called collides tank which returns a boolean if the tanks
	next move will collide with the wall.

	Projectile Collisions: similar to the tanks, there is another class that deals with the projectile's collision relative to the wall. with the
	method collides proj, the wall checks if a projectile's position intersects with the wall's collision box. This will destroy the projectile and 
	the projectile disapears. The other collision is with a tank. If the projectile collides with a tank, (which is checked in the tank class using
	collides point method). the projectile is destroyed and the tank score is decremented. If the tank's health is depleated, the owner of the projectile
	earns a point.

	Power Up: the powerups respawn every 1650 ms as stated in the assignment. A max of 2 powerup will be on screen at a time. The powerup also takes
	advantage of the collides with point in the tank class. if the powerup collides with a tank, the powerup dies, and the beams available variable
	in the tank increments.

	Settings: The settings file was read into the game using an XML reader, the values load into member variables in the settings class, the setting then 
	get set into the world. This is how the world is properly created.



Features:
		
	SQL Database: we created a sql database using microsoft SQL. The database holds the scores and names of the players that play the game.
	When a player disconnects the server sends the players name and score and records it to the DB. When all the players disconnect, the server
	fills a query that requests the top 5 scores of the game. We also did some sanitization by filtering out bad potential queries in our game.
	Players cannot have a name that could potentially create data loss.

	***SQL DATABASE*** unfortunately we could not figure out remote access for the server. we tested it on our personal machine and it worked
	just fine. we just could not get it to work across the network where the DB was on another computer. 

	Server Console: the server console displays the status of the server, if its running and who connects/ disconnects. the server also displays 
	the top 5 scores from the players that play the game, this is accessed from the SQL database.

Bad Design:
	The data base does not have an encrypted password file. The password is hard coded into the program. We know this is very bad in a real world
	example, we just were not sure how to create an encrypted file and we were running out of time. If we were actually publising this game we would 
	want to make sure the game doesn't have the password hard coded into the program.

	The game does not record current player scores if the server crashes. we would need to implement something that captures scores in real time.

