
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class JoystickLoop : UdonSharpBehaviour
	{
		Transform _left, _right;

		float _ratio; // [0,1]
		private MeshRenderer _renderer;

		private void Start()
		{
			_renderer = GetComponent<MeshRenderer>();
		}

		public float GetRatio()
		{
			return _ratio;
		}
		public void SetLeftAndRightTransform(Transform left, Transform right)
		{
			_left = left;
			_right = right;
		}

		private void Update() 
		{
			if (!_left || !_right || !_renderer) return;
			//We project "transform.position" between _left and _right
			Vector3 a = _left.position;
			Vector3 b = _right.position;
			Vector3 ab = b - a;
			Vector3 projectedVector = Vector3.Project(_renderer.bounds.center - a, ab) + a;

			projectedVector = Vector3.ClampMagnitude(projectedVector - a, ab.magnitude) + a;
			projectedVector = Vector3.ClampMagnitude(projectedVector - b, ab.magnitude) + b;

			_ratio = (projectedVector - a).magnitude / ab.magnitude;
		}
	}
}
