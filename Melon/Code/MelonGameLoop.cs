
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class MelonGameLoop : UdonSharpBehaviour
	{
		public Transform LeftWall;
		public Transform RightWall;

		public Joystick JoystickInstance;

		public Transform DropTransform;
		public Transform DeathZone;
		public GameObject FruitPrefab;

		public TextMeshProUGUI Score;
		public TextMeshProUGUI Scoreboard;

		private Fruit _currentFruit;
		private DataList _instantiatedFruits;

		[UdonSynced]
		private GameState _gameState;

		[UdonSynced]
		private int _score;

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
			int rank = Random.Range(0, 5);
			_currentFruit = InstantiateNewFruitAt(GetCursorPosition(), rank, false);
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
			_score = 0;
			_gameState = GameState.PLAY;
			NewFruit();
		}

		public void AddToScore(int points)
		{
			_score += points;
			UpdateUI();
		}

		//Called from the Scoreboard script
		public void RequestScoreboardUpdate()
		{

		}

		private void UpdateUI()
		{
			Score.text = _score.ToString();
		}

		public void GameOver()
		{
			Destroy(_currentFruit.gameObject);
			_currentFruit = null;
		}

		internal Fruit InstantiateNewFruitAt(Vector3 localPosition, int rank, bool isFused)
		{
			Fruit newFruit = Instantiate(FruitPrefab).GetComponent<Fruit>();
			newFruit.Construct(transform, localPosition, this, rank, transform.lossyScale.x, DeathZone.localPosition.y, isFused);
			_instantiatedFruits.Add(newFruit);

			AddToScore(rank << 1);
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
