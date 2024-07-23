
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

		private short _rank = 0;

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
			if (transform.localPosition.y - transform.localScale.x / 2.0f  < _yAxisLimit 
				|| RigidbodyInstance.velocity.magnitude > 0.5f * transform.lossyScale.x)
			{
				//everything is alright
				_startTimeAboveLimit = 0;
			}
			else if (_startTimeAboveLimit == 0 
				&& transform.localPosition.y - transform.localScale.x / 2.0f >= _yAxisLimit 
				&& RigidbodyInstance.velocity.magnitude <= 0.5f * transform.lossyScale.x)
			{
				//Dangerously close to a game over
				_startTimeAboveLimit = Time.time;
			}
			else if (Time.time - _startTimeAboveLimit > 1.0f && _melonGameLoopInstance.GetGameState() != GameState.FINISH)
			{
				_melonGameLoopInstance.GameOver();
			}
		}
#endregion

		public void Construct(Transform parent, MelonGameLoop melonGameLoopInstance, short rank, float gameScale, float yAxisLimit, bool isFused)
		{
			transform.parent = parent;
			FruitCollider.enabled = false;
			RigidbodyInstance.isKinematic = true;
			
			_hadCollision = isFused;
			_yAxisLimit = yAxisLimit;

			_currentColliderRadius = TARGET_COLLIDER_RADIUS;
			
			_gravityMultiplicator = gameScale;
			_melonGameLoopInstance = melonGameLoopInstance;
			
			RigidbodyInstance.drag = 0;
			RigidbodyInstance.angularDrag = 1f;

			SetRank(rank);
			SetScaleAndRotation();
		}

		public short GetRank()
		{
			return _rank;
		}

		public void SetRank(short rank)
		{
			_rank = rank;
			//In the original game, I found that the container is 315px wide, a cherry 22px wide, and a melon 184px
			//I am calculating the scale of each fruit based on their rank
			float fruitScale = 0.22f + rank * 0.147272f;
			transform.localScale = new Vector3(fruitScale, fruitScale, fruitScale);
			RigidbodyInstance.mass = fruitScale * fruitScale * 3.1415926f;
			_melonGameLoopInstance.SetTextureOffset(MeshRendererInstance.material, rank);
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
			
			_currentColliderRadius = TARGET_COLLIDER_RADIUS * 0.5f;
			FruitCollider.contactOffset = 0.0001f; //Using the script "ColliderParameterSetter" didn't worked, so I am setting it here. 
			SetScaleAndRotation();
		}

#region Unity Events

		private void FixedUpdate()
		{
			if (!enabled)
				return;

			//Local velocity contraint
			Vector3 localVelocity = transform.parent.InverseTransformDirection(RigidbodyInstance.velocity);
			localVelocity.z = 0;
			RigidbodyInstance.velocity = transform.parent.TransformDirection(localVelocity);

			//Gravity on the local space
			RigidbodyInstance.AddForce(transform.parent.up * -5f * _gravityMultiplicator * RigidbodyInstance.mass);
		}

		private void Update()
		{
			CheckGameOver();
			_currentColliderRadius += (TARGET_COLLIDER_RADIUS - _currentColliderRadius) * Time.deltaTime * 3f;

			//Local rotation contraint
			SetScaleAndRotation();

			//Local position contraint
			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
		}

		private void OnCollisionEnter(Collision collision)
		{
			if (gameObject.activeSelf && enabled)
			{
				if (_rank != 10)
				{
					Fruit anotherFruit = collision.gameObject.GetComponent<Fruit>();
					if (anotherFruit && _rank == anotherFruit.GetRank() && collision.gameObject.activeSelf)
					{
						_melonGameLoopInstance.DestroyFruit(this);
						_melonGameLoopInstance.DestroyFruit(anotherFruit);
						_melonGameLoopInstance.IncrementComboAndPlayAudio();
						gameObject.SetActive(false);
						anotherFruit.gameObject.SetActive(false);

						Fruit newFruit = _melonGameLoopInstance.InstantiateNewFruitAt((short)(_rank + 1), true);
						newFruit.transform.localPosition = (anotherFruit.transform.localPosition + transform.localPosition) / 2.0f;
						newFruit.DropFruit();
					}
					else if (!_hadCollision)
					{
						_melonGameLoopInstance.SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(_melonGameLoopInstance.PlayCollisionAudio));
					}
				}
				
				
			}
			if (!_hadCollision)
			{
				_melonGameLoopInstance.ReadyForNextFruit();
				_hadCollision = true;
			}
		}

#endregion

	}
}
