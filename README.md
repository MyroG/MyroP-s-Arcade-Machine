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
- Get the latest release from ... and install it into your project.
- In the folder MyroP/Arcade there's a prefab named "HeliCave", you just need to drag&drop it into your scene.
- Done

Unfortunately this asset doesn't include any music yet, it's really hard to find good 8-bit music that can be shared on GitHub, but feel free to add your own music, you just need to add an AudioClip on the "Music" GameObject.

### Adding multiple arcade machines

It is possible to add multiple arcade machines in the same world, but it's unfortunately not as straighforward as just copy/pasting the arcade machine.
The arcade machine uses a Render texture to render the game, and render textures are unique for each camera, since each arcade machine has a separate camera the render texture needs to be duplicated :
- In the folder MyroP/Arcade/RenderTextures, duplicate a Render texture.
- In the root prefab, drag&drop the render texture in the "Render texture" property field
- You also need to set that render texture on the camera, which can be found under the "game" GameObject.

A short video showing how to duplicate the arcade machine can be seen here :

[![IMAGE ALT TEXT](http://img.youtube.com/vi/BAtLaxdguMQ/0.jpg)](http://www.youtube.com/watch?v=BAtLaxdguMQ "How to duplicate the arcade machine")

# License

MIT unless explicitly marked otherwise (see `LICENSE` files), this prefab uses a few other external assets :
- The font is licenced under OFL (Open font license), a copy of the license can be found in the font folder
- The hologram shader by andydbc is licenced under the MIT license

# Credits and attribution

No need to credit me, but if you want to add me to the credits you can mention my VRChat name (MyroP, which is different from the name on GitHub) with a link to this GitHub page.

# Socials & Support

Feature requests and bugs reports can be submitted on this page or on one of my socials :
- My Twitter account : https://twitter.com/MyroDev
- My Discord group : https://discord.com/invite/kBQWu2jzcb

I also have a Patreon if you're interested in donating https://www.patreon.com/myrop

# Some more technical informations

The game itself is actually filmed by a camera inside the arcade machine, which outputs it on a render texture, the render texture is then applied on the screen.
I implemented it this way so it's easier to apply custom shaders on the screen, like a CRT screen shader! Those kind of shaders are not included in the package but can be downloaded elsewhere.

As mentioned above, it is not really easy to duplicate an arcade machine, that's because duplicating the arcade machine also requires to duplicate and use a different render texture. I tried to make it more straightforward by automatically creating a Render texture and applying it on the prefab via script, but there are two issues :
- Creating a new Render texture with `RenderTexture texture = new RenderTexture(...);` is not supported by Udon yet.
- It's also not possible to assign a new Render Texture to a camera via script, so this line of code `Camera.targetTexture = RenderTextureToUse;`doesn't work... Not sure if that's a Udon bug or an expected behavior.

Lastly, if you want to customize the game with different textures, you can perfectly do it, but be careful concerning the walls, walls are stretched so if you want to add a custom wall texture you need to account the stretch, I would recommend using a triplanar shader in those cases.