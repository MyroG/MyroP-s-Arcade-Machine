
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class GameOverCollider : UdonSharpBehaviour
	{
		public MelonGameLoop MelonGameLoopInstance;
		private void OnCollisionEnter(Collision collision)
		{
			Fruit anotherFruit = collision.gameObject.GetComponent<Fruit>();
			if (anotherFruit != null)
			{
				MelonGameLoopInstance.GameOver();
			}
		}
	}
}
