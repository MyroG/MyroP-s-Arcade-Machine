
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class Button : UdonSharpBehaviour
	{
		public UdonSharpBehaviour MainGameInstance;
		public string EventName;

		void Start()
		{

		}

		public override void Interact()
		{
			MainGameInstance.SendCustomEvent(EventName);
		}
	}
}