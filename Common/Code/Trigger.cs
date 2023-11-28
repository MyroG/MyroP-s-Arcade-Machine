
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	public class Trigger : UdonSharpBehaviour
	{
		public MainGame MainGameInstance;
		public Joystick JoystickInstance;
		void Start()
		{

		}

		public override void OnPlayerTriggerEnter(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
				MainGameInstance.gameObject.SetActive(true);
			}
		}

		public override void OnPlayerTriggerExit(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
				MainGameInstance.gameObject.SetActive(false);
				JoystickInstance.ForceDrop();
			}
		}
	}
}
