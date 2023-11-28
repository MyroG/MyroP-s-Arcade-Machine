
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

		private float	_targetScale;
		private float	_currentScale;
		private float	_gravityMultiplicator;
		private bool	_hadCollision;

		public void Construct(Transform parent, Vector3 localPosition, MelonGameLoop melonGameLoopInstance, int rank, float gameScale, bool isFused)
		{
			transform.parent = parent;
			transform.localPosition = localPosition;
			FruitCollider.enabled = false;
			RigidbodyInstance.isKinematic = true;
			_rank = rank;
			_hadCollision = isFused;

			//In the original game, a cherry is 22 wide, and a melon 184px, the box is 315px wide
			float fruitScale = 0.22f + rank * 0.147272f;
			_currentScale = fruitScale;
			_targetScale = fruitScale;
			MeshRendererInstance.material.SetTextureOffset("_MainTex", new Vector2((rank % 4) / 4.0f, (rank / 4) / 4.0f));;
			_gravityMultiplicator = gameScale;
			_melonGameLoopInstance = melonGameLoopInstance;


			SetScaleAndRotation();
		}

		public int GetRank()
		{
			return _rank;
		}

		private void SetScaleAndRotation()
		{
			transform.localScale = new Vector3(_currentScale, _currentScale, _targetScale / 1.5f);
			transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z);
		}

		public void DropFruit()
		{
			FruitCollider.enabled = true;
			RigidbodyInstance.isKinematic = false;
			RigidbodyInstance.collisionDetectionMode = CollisionDetectionMode.Continuous;
			FruitCollider.contactOffset = 0.0001f;
			_currentScale = _targetScale * 0.5f;
			SetScaleAndRotation();
		}

		private void FixedUpdate()
		{
			//Local velocity contraint
			Vector3 localVelocity = transform.parent.InverseTransformDirection(RigidbodyInstance.velocity);
			localVelocity.z = 0;
			RigidbodyInstance.velocity = transform.parent.TransformDirection(localVelocity);

			//Gravity on the local space
			RigidbodyInstance.AddForce(transform.parent.up.normalized * -9.8f * _gravityMultiplicator);
		}

		private void Update()
		{
			_currentScale += Time.deltaTime;
			if (_currentScale > _targetScale)
			{
				_currentScale = _targetScale;
			}

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
			if (gameObject.activeSelf)
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
