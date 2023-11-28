
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common;

namespace myro.arcade
{
	public class Button : UdonSharpBehaviour
	{
		public MainGame MainGameInstance;
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