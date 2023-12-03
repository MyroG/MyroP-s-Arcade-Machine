# MyroP's Arcade Machine

A fully working arcade machine you can add in your VRChat world.
It currently features two games :

- My implementation of the Suika game

![Showcase](https://github.com/MyroG/MyroP-s-Arcade-Machine/blob/master/Doc/fruit.gif)

- HeliCave : Survive as long as possible for a certain amount of time, the hight score is then calculated based on the number of deaths and the amount of time you survived.

![Showcase](https://github.com/MyroG/MyroP-s-Arcade-Machine/blob/master/Doc/heli.gif)

Both games are synced for remote players
The machine can be scaled up and down for smaller and bigger avatars.

The latest version of the arcade machine can be tested here : https://vrchat.com/home/world/wrld_22e9b1a3-1d2e-4800-b46d-ce3501b07001

# Installation

Before importing this asset, make sure you have imported
- The latest version of VRCSDK3
- UdonSharp
- TextMeshPro

Installation steps :
- Get the latest release from https://github.com/MyroG/MyroP-s-Arcade-Machine/releases, download the .unitypackage file, and install it into your project.
- In the folder MyroP/Arcade you can find 3 prefabs "ArcadeHeliCave", "ArcadeMelonGame" and "SharedScoreboard"
- If you want to add the "HeliCave" game into your world, just drag&drop it into your scene.
- If you want to add the Suika/Melon game into your world, just drag&drop it into your scene.
- If you want to have a scoreboard, drag&drop the "SharedScoreboard" prefab into the scene, then select the "ArcadeMelonGame" in the scene, and set the "Shared Scoreboard Prefab" field, currently the scoreboard doesn't work for the HeliCave game, but I am working on it. Multiple arcade machines can share the same scoreboard.
- Done

### Adding multiple arcade machines

It is possible to add multiple arcade machines in the same world, but it's unfortunately not as straighforward as just copy/pasting the arcade machine.
The arcade machine uses a Render texture to render the game, and render textures are unique for each camera, since each arcade machine has a separate camera the render texture needs to be duplicated :
- In the folder MyroP/Arcade/RenderTextures, duplicate a Render texture.
- In the root prefab, drag&drop the render texture in the "Render texture to use" property field

### Settings

There are three settings :
- Render texture to use : If you want to duplicate the arcade machine, make sure to set a new render texture in that field
- Duration : How long the game should last in seconds
- Screen shader emission property name : If you decide to use a custom shader on your screen, you can change the keyword here so that the render texture still gets applied on the material. Leave it blank if you only want the render texture to affect the main texture

# License

MIT unless explicitly marked otherwise (see `LICENSE` files), this prefab uses a few other external assets :
- The font is licenced under OFL (Open font license), a copy of the license can be found in the font folder
- The hologram shader by andydbc is licenced under the MIT license

# Credits and attribution

No need to credit me, but if you want to add me to the credits you can :
- Mention my VRChat name (MyroP, which is different from the name on GitHub)
- And if you want you can also add a link to this GitHub page.

# Socials & Support

Feature requests and bugs reports can be submitted on this page or on one of my socials :
- My Twitter account : https://twitter.com/MyroDev
- My Discord group : https://discord.com/invite/kBQWu2jzcb

I also have a Patreon if you're interested in donating https://www.patreon.com/myrop
