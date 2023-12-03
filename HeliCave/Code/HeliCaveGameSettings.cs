
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	public class HeliCaveGameSettings : GameSettings
	{
		public int GameDuration = 120;

		[HideInInspector]
		public int NumberWalls = 15; //currently hidden, because a different number can cause some issues

		void Start()
		{

		}
	}
}
