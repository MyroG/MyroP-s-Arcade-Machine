
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace MyroP.Arcade
{
	public class Joystick : UdonSharpBehaviour
	{
		public MainGame MainGameInstance;
		public GameObject HoloJoystick;

		private VRCObjectSync _objectSync;
		private VRCPickup _pickup;

		void Start()
		{
			_objectSync = GetComponent<VRCObjectSync>();
			_pickup = GetComponent<VRCPickup>();
		}

		public override void OnPickup()
		{
			MainGameInstance.LocalPlayerBecomesOwner();
			SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnPickupEvent));
		}

		public void OnPickupEvent()
		{
			HoloJoystick.SetActive(true);
		}

		public override void OnDrop()
		{
			MainGameInstance.PrepareResetGame();
			SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnDropEvent));
			_objectSync.Respawn();
		}

		public void OnDropEvent()
		{
			HoloJoystick.SetActive(false);
		}

		public override void OnPickupUseDown()
		{
			MainGameInstance.OnPress();
		}

		public override void OnPickupUseUp()
		{
			MainGameInstance.OnRelease();
		}

		public void ForceDrop()
		{
			if (Networking.IsOwner(gameObject))
			{
				_pickup.Drop();
			}
		}
	}
}