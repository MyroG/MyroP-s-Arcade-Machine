
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Trigger : UdonSharpBehaviour
	{
		public UdonSharpBehaviour MainGameInstance;
		public Joystick JoystickInstance;
		public GameObject Toggleable;
		void Start()
		{
			Toggleable.gameObject.SetActive(false);
		}

		public override void OnPlayerTriggerEnter(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
				Toggleable.SetActive(true);
				MainGameInstance.SendCustomEvent("OnPlayerEnteredArea");
			}
		}

		public override void OnPlayerTriggerExit(VRCPlayerApi player)
		{
			if (player.isLocal)
			{
				Toggleable.SetActive(false);
				MainGameInstance.SendCustomEvent("OnPlayerExitedArea");
			}
		}
	}
}
