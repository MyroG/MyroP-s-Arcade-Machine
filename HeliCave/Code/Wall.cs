
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	public class Wall : UdonSharpBehaviour
	{
		public MainGame MainGameInstance;

		void Start()
		{

		}

		private void OnTriggerEnter(Collider obj)
		{
			Controllable controllable = obj.gameObject.GetComponent<Controllable>();

			if (controllable != null)
			{
				MainGameInstance.Crashed();
			}
		}
	}
}
