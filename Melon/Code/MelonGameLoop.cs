
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	public class MelonGameLoop : UdonSharpBehaviour
	{
		public Transform LeftWall;
		public Transform RightWall;

		public Joystick JoystickInstance;

		public Transform DropTransform;
		public GameObject FruitPrefab;
		private Fruit _currentFruit;
		private DataList _instantiatedFruits;
		void Start()
		{
			_instantiatedFruits = new DataList();
			StartGame();
		}

		private Vector3 GetCursorPosition()
		{
			float ratioX = JoystickInstance.GetPositionXRatio();
			return Vector3.Lerp(LeftWall.localPosition, RightWall.localPosition, ratioX);
		}

		public void NewFruit()
		{
			_currentFruit = InstantiateNewFruitAt(GetCursorPosition(), Random.Range(0,5), false);
		}

		public void OnPress()
		{
			if (_currentFruit != null)
			{
				_currentFruit.DropFruit();
				_currentFruit = null;
			}
		}

		private void StartGame()
		{
			NewFruit();
		}

		

		internal Fruit InstantiateNewFruitAt(Vector3 localPosition, int rank, bool isFused)
		{
			Fruit newFruit = Instantiate(FruitPrefab).GetComponent<Fruit>();
			newFruit.Construct(transform, localPosition, this, rank, transform.lossyScale.x, isFused);
			newFruit.transform.localPosition = localPosition;
			_instantiatedFruits.Add(newFruit);
			return newFruit;
		}

		internal void DestroyFruit(Fruit anotherFruit)
		{
			_instantiatedFruits.Remove(anotherFruit);
			Destroy(anotherFruit.gameObject);
		}
		
		private void Update()
		{
			if (_currentFruit != null)
			{
				_currentFruit.transform.localPosition = GetCursorPosition();
			}
		}

		public void ReadyForNextFruit()
		{
			NewFruit();
		}
	}
}
