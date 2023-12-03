
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Components;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.Continuous)] //Continuous because that script is attached to a VRCObjectSync component
	[RequireComponent(typeof(JoystickLoop))]
	public class Joystick : UdonSharpBehaviour
	{
		public UdonSharpBehaviour MainGameInstance;
		public GameObject HoloJoystick;
		public Transform LeftScreen;
		public Transform RightScreen;
		private VRCObjectSync _objectSync;
		private VRCPickup _pickup;
		private JoystickLoop _joystickLoop;

		void Start()
		{
			_objectSync = GetComponent<VRCObjectSync>();
			_pickup = GetComponent<VRCPickup>();
			CheckForInitialisation();
		}

		private void CheckForInitialisation()
		{
			if (!_joystickLoop)
			{
				_joystickLoop = GetComponent<JoystickLoop>();
				_joystickLoop.SetLeftAndRightTransform(LeftScreen, RightScreen);
				_joystickLoop.enabled = false;
			}
		}

		public override void OnPickup()
		{
			MainGameInstance.SendCustomEvent("PlayerPickedUpJoystick");
			SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnPickupEvent));
		}

		public void OnPickupEvent()
		{
			_joystickLoop.enabled = true;
			HoloJoystick.SetActive(true);
		}

		public override void OnDrop()
		{
			_joystickLoop.enabled = false;
			MainGameInstance.SendCustomEvent("PlayerDroppedJoystick");
			SendCustomNetworkEvent(NetworkEventTarget.All, nameof(OnDropEvent));
			_objectSync.Respawn();
		}

		public void OnDropEvent()
		{
			HoloJoystick.SetActive(false);
		}

		public override void OnPickupUseDown()
		{
			MainGameInstance.SendCustomEvent("OnPress");
		}

		public override void OnPickupUseUp()
		{
			MainGameInstance.SendCustomEvent("OnRelease");
		}

		public void ForceDrop()
		{
			if (Networking.IsOwner(gameObject))
			{
				_pickup.Drop();
			}
		}

		public float GetPositionXRatio()
		{
			CheckForInitialisation();
			return _joystickLoop.GetRatio();
		}
	}
}