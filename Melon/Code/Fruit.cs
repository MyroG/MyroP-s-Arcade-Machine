
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	public class Fruit : UdonSharpBehaviour
	{
		public Rigidbody RigidbodyInstance;
		public MeshRenderer MeshRendererInstance;
		public CapsuleCollider FruitCollider;

		private int _rank = 0;

		private MelonGameLoop _melonGameLoopInstance;

		private const float	TARGET_COLLIDER_RADIUS = 0.5f;
		private float	_currentColliderRadius;
		private float	_gravityMultiplicator;
		private bool	_hadCollision;

		#region Game Over Detection
		private float _yAxisLimit; //If one fruit stays above that limit for more than a second, the game is lost
		private float _startTimeAboveLimit;

		private void CheckGameOver()
		{
			if (!_hadCollision)
			{
				return;
			}
			if (transform.localPosition.y - transform.localScale.x / 2.0f  < _yAxisLimit)
			{
				_startTimeAboveLimit = 0;
			}
			else if (_startTimeAboveLimit == 0)
			{
				_startTimeAboveLimit = Time.time;
			}
			else if (Time.time - _startTimeAboveLimit > 1.0f)
			{
				_melonGameLoopInstance.GameOver();
			}
		}
#endregion
		public void Construct(Transform parent, Vector3 localPosition, MelonGameLoop melonGameLoopInstance, int rank, float gameScale, float yAxisLimit, bool isFused)
		{
			transform.parent = parent;
			transform.localPosition = localPosition;
			FruitCollider.enabled = false;
			RigidbodyInstance.isKinematic = true;
			
			_rank = rank;
			_hadCollision = isFused;
			_yAxisLimit = yAxisLimit;

			//In the original game, a cherry is 22 wide, and a melon 184px, the box is 315px wide
			float fruitScale = 0.22f + rank * 0.147272f;
			transform.localScale = new Vector3(fruitScale, fruitScale, fruitScale);

			_currentColliderRadius = TARGET_COLLIDER_RADIUS;
			MeshRendererInstance.material.SetTextureOffset("_MainTex", new Vector2((rank % 4) / 4.0f, (rank / 4) / 4.0f));
			_gravityMultiplicator = gameScale;
			_melonGameLoopInstance = melonGameLoopInstance;
			RigidbodyInstance.mass = fruitScale * fruitScale * 3.1415926f;
			RigidbodyInstance.drag = 0;
			RigidbodyInstance.angularDrag = 1f;

			SetScaleAndRotation();
		}

		public int GetRank()
		{
			return _rank;
		}

		private void SetScaleAndRotation()
		{
			FruitCollider.radius = _currentColliderRadius;
			transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z);
		}

		public void DropFruit()
		{
			FruitCollider.enabled = true;
			RigidbodyInstance.isKinematic = false;
			RigidbodyInstance.collisionDetectionMode = CollisionDetectionMode.Continuous;
			FruitCollider.contactOffset = 0.0001f;
			_currentColliderRadius = TARGET_COLLIDER_RADIUS * 0.66f;
			SetScaleAndRotation();
		}

		private void FixedUpdate()
		{
			//Local velocity contraint
			Vector3 localVelocity = transform.parent.InverseTransformDirection(RigidbodyInstance.velocity);
			localVelocity.z = 0;
			RigidbodyInstance.velocity = transform.parent.TransformDirection(localVelocity);

			//Gravity on the local space
			RigidbodyInstance.AddForce(transform.parent.up.normalized * -5f * _gravityMultiplicator * RigidbodyInstance.mass);
		}

		private void Update()
		{
			CheckGameOver();
			_currentColliderRadius += (TARGET_COLLIDER_RADIUS - _currentColliderRadius) * Time.deltaTime * 2.5f;

			//Local rotation contraint
			SetScaleAndRotation();

			//Local position contraint
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (!_hadCollision)
			{
				_melonGameLoopInstance.ReadyForNextFruit();
				_hadCollision = true;
			}
			if (gameObject.activeSelf && _rank != 10)
			{
				Fruit anotherFruit = collision.gameObject.GetComponent<Fruit>();
				if (anotherFruit != null && _rank == anotherFruit.GetRank() && collision.gameObject.activeSelf)
				{
					_melonGameLoopInstance.DestroyFruit(this);
					_melonGameLoopInstance.DestroyFruit(anotherFruit);
					gameObject.SetActive(false);
					anotherFruit.gameObject.SetActive(false);

					Fruit newFruit = _melonGameLoopInstance.InstantiateNewFruitAt((anotherFruit.transform.localPosition + transform.localPosition) / 2.0f, _rank + 1, true);
					newFruit.DropFruit();
				}
			}
		}
	}
}
