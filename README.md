# Yggdrasil Ascension
![teaser image](game_teaser.jpg)

The gods are in crisis, and humanity has to come to the rescue. Climb the world tree to show the gods what you are capable of…

:video_game: Game Programming Lab 2023 - House Rapture - Team3

Members: Veit Billinger, Tilman de Lanversin, David Colonna, Ivana Klasovita, Patrick Wicki and Dumeni Manatschal

# Background story
The gods have been warned of a great battle ahead, of Ragnarok bringing down the worlds as they know them. But as it is with all prophecies, no one knows, when it will happen. And so, the speculating gods where waiting. For days. Days turned into years, and years into centuries. And the more time passed, the more it was forgotten. Until today....

Today the gods are old and rusty and need the human's help to beat Ragnarock. This is where our game story starts. The gods want to test human players and see if they can beat their challenge in reaching the Boss room.

# Goal of the game
The game starts with a tutorial that will introduce the various components of the game. Initially, after the tutorial concludes, the players will have to move one of the ghosts to register in the current game. Afterwards, the players can simply move into one of the displayed characters to select it as their game character.

When all players (max 4, min 1) have selected their character, they can step onto the START button. When all registered players stepped on that button, the game will start by lowering the first set of doors to the next rooms of the level.

The players will then have to move from room to room to clear a path to the top of the tree that the players can see when they enter the pause menu. At the top of the tree, somewhere, in different possible places, the BigBoss is located that has to be beaten by the players to win the game.

In the Boss room there is a button named Escape, that the players can step on to escape from the Boss room. This feature can be used only once during a play-through, in case the players feel they can collect more items in the rest of the rooms to come back stronger.

# Level concept
At each game start (or restart) the game generates a new level based on a set of different rooms and layouts. This should yield a fair amount of variation for the levels, although, due to time constraints the amount of variation is not excessive. The goal in the level concept is, to offer some randomness but still containing elements that players can recognize. This should be a mix betweeen rogue-like and party-game.

# Encounters
The moving-to-the-top is divided into various encounters. An encounter starts as soon as all players that are not currently ghosts are inside the same room (Note that there is a small offset of 1 tile between the exit of a corridor and the room). Before an Encounter starts, the players can see the enmies in a frozen state but can't shoot them. The encounter starts by closing off all exits to that room, so that the players are trapped until they kill all the enemies inside that room.

After a room is cleared, it stays that way until the end.

# Freeroam
Between two encounters, players are free to move around in all cleared rooms. The camera will zoom out to capture all of them. This is supposed to provide incentive to keep the players together.

Enemies in un-cleared rooms are visible as half-transparend static shades. This way, the players can decide on tactics how to best clear a room before the encounter starts.

# Ghosts
If a player dies during the game, they will be changed into a ghost that can't do anyhting but collect a possible revive item that will turn the player back into the chosen character. Dying will make the player loose the current weapon which may be the most powerful of the game.

# Pick Ups
Various items that can be picked up are located in the rooms. The two simplest ones are Hearts and Revives. These can be picked up by alive characters and ghost characters respectively.

Some items, like better guns are inside larger, more powerful enemies and can be collected when they are killed.

There are 4 special weapons available, that will be distributed to stronger enemies in random level rooms.

Weapons can be exchanged by walking into another weapon's floating icon.

# Level Ups
Each character that enters the game can pick up one or two level ups. If up to 2 players are registered, each player can find 2 level ups, leveling them up to level 3. if 3 or more players are registered there is only one level up available for each character.

# Controls
The controls are explained during the tutorial. Also, the image that is shown below is displayed in the start room. If the players press the pause button during the game, the camera will zoom out and show the entire tree including the so-far uncovered rooms.
![teaser image](controller.jpg)

### Keyboard steering
One player can be controlled using the keyboard. The controls are **WASD**. Shooting is with the **left mouse button** in direction of the mouse pointer. Special abilities are triggered with **LeftAlt**. Press **SPACE** to dash. **Note** that this is supposed to facilitate playing when controllers are not available, but that it is much easier because one can simply point the mouse at an enemy and shoot while with a controller aiming is harder.

### Controller steering
On the start menu, all connected controllers are visible. The controller steering is: **left stick** steering, **right stick** aiming, **right trigger button** shooting, **left trigger button** dashing, **left shoulder button** special ability.

### Additional functions (only available, if Settings=>Debug is ON)
* **F1/F2**: open/close all doors of the level (debugging)
* **F3/F4**: lock/unlock all doors of the level (debugging)
* **F5/F6**: open/close all adjacent doors
* **F7/F8**: lock/unlock all adjacent doors
* **F10**: toggle camera mode (track player, focus on room, free roam (**TFGH** to steer camera and **mouse wheel** to zoom))
* **F11**: toggle fullscreen
