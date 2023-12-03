
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	[RequireComponent(typeof(Camera))]
	public class CameraScaleConstraint : UdonSharpBehaviour
	{
		public Transform ConstraintTo;
		void Start()
		{
			Camera cam = GetComponent<Camera>();
			float scale = ConstraintTo.lossyScale.x;
			cam.orthographicSize *= scale;
			cam.nearClipPlane *= scale;
			cam.farClipPlane *= scale;
		}
	}
}
