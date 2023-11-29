
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
		public GameObject ScoreboardWrapper;
		public GameObject GameOverMessage;
		public MelonGameSettings MelonGameSettingsInstance;
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
			ScoreboardWrapper.SetActive(MelonGameSettingsInstance.ScoreboardInstance);
			if (MelonGameSettingsInstance.ScoreboardInstance)
			{
				MelonGameSettingsInstance.ScoreboardInstance.RegisterBehaviour(this);
			}
			StartGame();
		}

		private Vector3 GetCursorPosition()
		{
			float ratioX = JoystickInstance.GetPositionXRatio();
			float radiusFruit = 0;
			if (_currentFruit)
				radiusFruit = _currentFruit.transform.localScale.x / 2.0f + 0.01f; //We add a little threshold at the end
			Vector3 left = new Vector3(LeftWall.localPosition.x + radiusFruit, LeftWall.localPosition.y, LeftWall.localPosition.z);
			Vector3 right = new Vector3(RightWall.localPosition.x - radiusFruit, RightWall.localPosition.y, RightWall.localPosition.z);
			return Vector3.Lerp(left, right, ratioX);
		}

		public void NewFruit()
		{
			int rank = Random.Range(0, 5);
			_currentFruit = InstantiateNewFruitAt(GetCursorPosition(), rank, false);
		}

		public void OnPress()
		{
			if (_gameState == GameState.PLAY)
			{
				if (_currentFruit != null)
				{
					_currentFruit.DropFruit();
					_currentFruit = null;
				}
			}
			else
			{
				StartGame();
			}
		}

		private void StartGame()
		{
			if (_instantiatedFruits != null)
			{
				for (int i = 0; i < _instantiatedFruits.Count; i++)
				{
					Fruit fruit = (Fruit)_instantiatedFruits[i].Reference;
					if (fruit)
						Destroy(fruit.gameObject);
				}
			}
			_instantiatedFruits = new DataList();

			GameOverMessage.SetActive(false);
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
			if (_currentFruit != null)
			{
				Destroy(_currentFruit.gameObject);
				_currentFruit = null;
			}
			GameOverMessage.SetActive(true);
			_gameState = GameState.FINISH;

			for (int i = 0; i < _instantiatedFruits.Count; i++)
			{
				Fruit fruit = (Fruit)_instantiatedFruits[i].Reference;
				if (fruit)
					fruit.RigidbodyInstance.isKinematic = true;
			}
		}

		internal Fruit InstantiateNewFruitAt(Vector3 localPosition, int rank, bool isFused)
		{
			Fruit newFruit = Instantiate(FruitPrefab).GetComponent<Fruit>();
			newFruit.Construct(transform, localPosition, this, rank, transform.lossyScale.x, DeathZone.localPosition.y, isFused);
			_instantiatedFruits.Add(newFruit);

			AddToScore((rank + 1) << 1);
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
