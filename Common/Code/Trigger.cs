
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Trigger : UdonSharpBehaviour
	{
		public GameObject MainGameInstance;
		public Joystick JoystickInstance;
		void Start()
		{

		}

		public override void OnPlayerTriggerEnter(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
				MainGameInstance.SetActive(true);
			}
		}

		public override void OnPlayerTriggerExit(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
				MainGameInstance.SetActive(false);
				JoystickInstance.ForceDrop();
			}
		}
	}
}
