# MyroP's Arcade Machine

A fully working arcade machine you can add in your VRChat world.
It currently features one game called HeliCave : Survive as long as possible for a certain amount of time, the hight score is then calculated based on the number of death and the amount of time you survived.
The game is synced for other players, but not for late joiners.
The machine can be scaled up and down for smaller and bigger avatars.

It can be tested here : https://vrchat.com/home/world/wrld_22e9b1a3-1d2e-4800-b46d-ce3501b07001

A short gameplay video can be seen here

[![IMAGE ALT TEXT](http://img.youtube.com/vi/W5GbZqcVN9A/0.jpg)](http://www.youtube.com/watch?v=W5GbZqcVN9A "MyroP's arcade machine")

# Installation

Before importing this asset, make sure you have imported
- The latest version of VRCSDK3
- UdonSharp 1.0b minimum.
- TextMeshPro

Installation steps :
- Get the latest release from https://github.com/MyroG/MyroP-s-Arcade-Machine/releases, download the .unitypackage file, and install it into your project.
- In the folder MyroP/Arcade there's a prefab named "HeliCave", you just need to drag&drop it into your scene.
- Done

Unfortunately this asset doesn't include any music yet, it's really hard to find good 8-bit music that can be shared on GitHub, but feel free to add your own music, you just need to add an AudioClip on the "Music" GameObject.

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

# Some more technical informations

### The camera

The game itself is actually filmed by a camera inside the arcade machine, which outputs it on a render texture, the render texture is then applied on the screen.
I implemented it this way so it's easier to apply custom shaders on the screen, like a CRT screen shader! Those kind of shaders are not included in the package but can be downloaded elsewhere.

As mentioned above, it is not really easy to duplicate an arcade machine, that's because duplicating the arcade machine also requires to duplicate and use a different render texture. I tried to make it more straightforward by automatically creating a Render texture and applying it on the prefab via script, but creating a new Render texture with `RenderTexture texture = new RenderTexture(...);` is not supported by Udon yet.

The game is placed on the "Environment" layer and the camera only picks up that layer, if you want you can change that and the game should still work correctly.

### The trigger

For performance reasons the game (located under the "game" GameObject) is disabled by default and gets enabled once a player enters a trigger, if you want you can resize the trigger or even remove it (in that case you just need to enable the "game" GameObject because it is disabled by default)

### Customization

If you want to customize the game with different textures, you can perfectly do it, but be careful concerning the walls, walls are stretched so if you want to add a custom wall texture you need to account the stretch, I would recommend using a triplanar shader in those cases.

