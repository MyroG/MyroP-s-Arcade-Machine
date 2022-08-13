# MyroP's Arcade Machine

A fully working arcade machine you can add in your VRChat world, it currently features one game called HeliCave : Survive as long as possible to beat your hight score.
The game is synced for other players, but not for late joiners.
The machine can de scaled up and down for smaller and bigger avatars.

## Installation

This asset requires :
- The latest version of VRCSDK 3
- UdonSharp 1.0 minimum, 1.0b is fine too, it does not work with older versions of U#.
- TextMeshPro

Installation steps :
- Get the latest release from ... and install it into your project.
- In the folder MyroP/Arcade there's a prefab named "HeliCave", you just need to drag&drop it into your scene.
- Done

## License

MIT, but this prefab uses a few other external assets :
- The font is licenced under OFL (Open font license), a copy of the license can be found in the font folder
- The hologram shader by andydbc is licenced under the MIT license

## Some more technical informations

The game itself is actually filmed by a camera inside the arcade machine, which outputs it on a render texture, the render texture is then applied on the screen.
I implemented it this way so it's easier to apply custom shaders on the screen, like a CRT screen shader! Those kind of shaders are not included in the package but can be downloaded elsewhere.

## How to add multiple arcades?

It is possible to add multiple arcade machines in the same world, but it's unfortunately not as straighforward as just copy/pasting the arcade machine.
As mentioned above, the arcade machine uses a Render texture to render the game, and render textures are unique for each camera, since each arcade machine has a separate camera the render texture needs to be duplicated :
- In the folder MyroP/Arcade/RenderTextures, duplicate a Render texture.
- In the root prefab, drag&drop the render texture in the "Render texture" property field
- You also need to set that render texture on the camera, which is in the "game" sub-prefab

I tried to make it more straightforward by automatically creating a Render texture and applying it on the prefab, but there are two issues :
- Creating a new Render texture with `RenderTexture texture = new RenderTexture(...);` is not supported by Udon yet.
- It's also not possible to assign a new Render Texture to a camera via script, so this line of code `Camera.targetTexture = RenderTextureToUse;`doesn't work... Not sure if that's a Udon bug or an expected behavior.

## Credits

No need to credit me, but if you want to add me to the credits you can mention my VRC name (MyroP, which is different from the name on GitHub) with a link to this GitHub page.

## Socials & Support

Feature requests and bugs reports can be submitted on this page or on one of my socials :
- My Twitter account : https://twitter.com/MyroDev
- My Discord group : https://discord.com/invite/kBQWu2jzcb

I also have a Patreon if you're interested in donating https://www.patreon.com/myrop
