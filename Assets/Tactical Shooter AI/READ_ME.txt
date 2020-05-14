Thank you for buying Tactical Shooter AI!  For full documentation, please see the included manual.

NOTE 1:  If you want the demo scenes to work properly out-of the box,
you'll need to set your eigth physics layer to "Level Parts".  Of course,
you can always put them on whatever layer you want and modify the layermask
variables to include/exclude it as appropriate.  For more on set-up, see the manual.

NOTE 2:  Due to an error in Unity's import process, the default animation controller 
may have some of it's animations randomly swapped out for other animations in the project.
You will need to either manually open the animation controller and replace the animations
or create a new one using the included tools.  As it stands, though, the default prefabs 
may not animate properly out of the box.

NOTE 3:  If you are having problems with agents seeing through walls, bullets going through walls, 
or agents not taking cover, then double check the LayerMask variables on the AI Controller, 
Bullet Scripts, and Cover Nodes!

NOTE 4:  If you are getting errors when you start the game and an agent is in the scene, make sure you
have added an AI Controller script to an object in your scene and that you have baked your navmesh.

Note 5:  If your agents are not engaging their targets, then try excluding the physics layer of the targets
from the layermasks on the AI Controller and Cover Nodes.  They might be interpreting the colliders of the enemy as 
a wall and thus not seeing them.  Also, make sure the targets have the right team variable on their TargetScripts.

