
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace MyroP.Arcade
{
	public class GameSettings : UdonSharpBehaviour
	{
		public RenderTexture RenderTextureToUse;
		public int GameDuration = 120;

		public string ScreenShaderEmissionPropertyName = "_EmissionMap";

		[HideInInspector]
		public int NumberWalls = 15; //currently hidden, because a different number can cause some issues

		void Start()
		{

		}
	}
}
