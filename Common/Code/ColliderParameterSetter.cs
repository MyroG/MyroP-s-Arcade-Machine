
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class ColliderParameterSetter : UdonSharpBehaviour
	{
		public Collider ColliderInstance;
		public float ContactOffset = 0.0001f;

		void Start()
		{
			ColliderInstance.contactOffset = ContactOffset;
		}
	}
}
