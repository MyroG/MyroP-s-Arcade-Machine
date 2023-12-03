
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
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
		public Transform Root;
		public Joystick JoystickInstance;

		public Transform DropTransform;
		public Transform DeathZone;
		public GameObject FruitPrefab;
		public GameObject GameOverMessage;
		public MelonGameSettings MelonGameSettingsInstance;
		public TextMeshProUGUI Score;
		public Image NextRankImage;
		public GameObject WaitMessage;

		private Fruit _currentFruit;
		private DataList _instantiatedFruits;

		private const float SYNCING_RATE = 0.2f;
		private int _combo;

		[UdonSynced]
		private GameState _gameState;

		[UdonSynced]
		private int _score;

		[UdonSynced]
		private short _nextRank;

		[UdonSynced]
		private Vector3[] _positionFruits;

		[UdonSynced]
		private Quaternion[] _rotationFruits;

		[UdonSynced]
		private short[] _rankFruits;

		private void OnDisable()
		{
			if (Networking.IsOwner(gameObject))
			{
				GameOver();
			}
		}

		void Start()
		{
			_gameState = GameState.FINISH;			
			GameOver();
		}

		private short UpdateNextFruitViewAndReturnCurrentRank()
		{
			short newRank = _nextRank;
			_nextRank = (short) Random.Range(0, 5);

			UpdateNextRankImage();

			return newRank;
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

		public GameState GetGameState()
		{
			return _gameState;
		}

		public void NewFruit()
		{
			short rank = UpdateNextFruitViewAndReturnCurrentRank();
			_currentFruit = InstantiateNewFruitAt(rank, false);
			_currentFruit.transform.localPosition = GetCursorPosition();
		}


		public void SetTextureOffset(Material mat, int rank)
		{
			mat.SetTextureOffset("_MainTex", new Vector2((rank % 4) / 4.0f, (rank / 4) / 4.0f));
		}

		private void StartGame()
		{
			WaitMessage.SetActive(false);

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

			_score = 0;
			_gameState = GameState.PLAY;
			NewFruit();
			SyncingLoop();
			UpdateUIState();
		}

		public void AddToScore(int points)
		{
			_score += points;
			UpdateScore();
		}

		#region Events and syncing

		/// <summary>
		/// This looping method should only be called by the owner, it syncs the data
		/// </summary>
		public void SyncingLoop()
		{
			if (_gameState == GameState.FINISH || _instantiatedFruits == null || !Networking.IsOwner(gameObject))
				return;

			int length = _instantiatedFruits.Count;
			_positionFruits = new Vector3[length];
			_rotationFruits = new Quaternion[length];
			_rankFruits = new short[length];

			for (int i = 0; i < length; i++)
			{
				Fruit fruit = (Fruit)_instantiatedFruits[i].Reference;
				if (!fruit)
					return;

				_positionFruits[i] = fruit.transform.localPosition;
				_rotationFruits[i] = fruit.transform.localRotation;
				_rankFruits[i] = fruit.GetRank();
			}
			RequestSerialization();
			SendCustomEventDelayedSeconds(nameof(SyncingLoop), SYNCING_RATE);
		}

		/// <summary>
		/// This event is called from the Joystick script
		/// </summary>
		public void PlayerPickedUpJoystick()
		{
			WaitMessage.SetActive(!Networking.IsOwner(gameObject) && _gameState == GameState.PLAY);
			if (_gameState == GameState.FINISH)
			{
				Networking.SetOwner(Networking.LocalPlayer, gameObject);
			}
		}

		/// <summary>
		/// This event is called from the Joystick script
		/// </summary>
		public void PlayerDroppedJoystick()
		{
			WaitMessage.SetActive(false);
		}

		bool _isPlayerInArea;
		public void OnPlayerEnteredArea()
		{
			_isPlayerInArea = true;
		}

		public void OnPlayerExitedArea()
		{
			_isPlayerInArea = false;
		}

		public override void OnOwnershipTransferred(VRCPlayerApi player)
		{
			if (!player.isLocal)
			{
				_currentFruit = null;
			}
		}

		public void OnPress()
		{
			if (!Networking.IsOwner(gameObject))
				return;

			if (_gameState == GameState.PLAY)
			{
				if (_currentFruit != null)
				{
					_currentFruit.DropFruit();
					_currentFruit = null;
					_combo = 0;
					SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayDropAudio));
				}
			}
			else
			{
				StartGame();
			}
		}

		public void OnResetPressed()
		{
			if (!Networking.IsOwner(gameObject))
				Networking.SetOwner(Networking.LocalPlayer, gameObject);

			StartGame();
		}

		

		// Here, OnDeserialization should only be called for the remote player
		public override void OnDeserialization()
		{
			if (!_isPlayerInArea)
				return;

			UpdateUI();

			if (_instantiatedFruits == null)
			{
				_instantiatedFruits = new DataList();
			}

			int numberOfSyncedFruits = _positionFruits.Length;

			//first for-loop to check if all references are valid, this is to prevent possible errors
			for (int i = _instantiatedFruits.Count - 1; i >= 0; i--)
			{
				//if (_instantiatedFruits[i].Error == DataError.None)
				//{
					Fruit fruit = (Fruit)_instantiatedFruits[i].Reference;
					if (!Utilities.IsValid(fruit))
					{
						_instantiatedFruits.RemoveAt(i);
					}
				//}
			}

			if (_instantiatedFruits.Count < numberOfSyncedFruits)
			{
				//Here we need to instantiate more fruits
				for (int i = 0; i < numberOfSyncedFruits - _instantiatedFruits.Count; i++)
				{
					InstantiateNewFruitAt(0, false); //the settings will be set later
				}
			}
			else if (_instantiatedFruits.Count > numberOfSyncedFruits)
			{
				//Here we need to destroy a few fruits
				for (int i = 0; i < _instantiatedFruits.Count - numberOfSyncedFruits; i++)
				{
					Fruit fruit = (Fruit)_instantiatedFruits[0].Reference;
					Destroy(fruit.gameObject);
					_instantiatedFruits.RemoveAt(0);
				}
			}

			for (int i = 0; i < numberOfSyncedFruits; i++)
			{
				if (_instantiatedFruits[i].Error == DataError.None)
				{
					//Now we update each fruit
					Fruit fruit = (Fruit)_instantiatedFruits[i].Reference;
					fruit.transform.localPosition = _positionFruits[i];
					fruit.transform.localRotation = _rotationFruits[i];
					fruit.SetRank(_rankFruits[i]);
					fruit.enabled = false; //the scripts does not need to be enabled for the remote player
				}
			}
		}

		#endregion


		#region UI
		private void UpdateNextRankImage()
		{
			SetTextureOffset(NextRankImage.material, _nextRank);
			float scale = (_nextRank + 1) * 0.2f + 0.2f;
			NextRankImage.transform.localScale = new Vector3(scale, scale, scale);
		}

		private void UpdateScore()
		{
			Score.text = _score.ToString();
		}

		private void UpdateUIState()
		{
			GameOverMessage.SetActive(_gameState == GameState.FINISH);
		}

		private void UpdateUI()
		{
			UpdateScore();
			UpdateUIState();
			UpdateNextRankImage();
		}

		#endregion

		public void GameOver()
		{
			if (_currentFruit != null)
			{
				Destroy(_currentFruit.gameObject);
				_currentFruit = null;
			}
			_gameState = GameState.FINISH;

			WaitMessage.SetActive(false);
			UpdateUI();

			if (_instantiatedFruits != null)
			{
				for (int i = 0; i < _instantiatedFruits.Count; i++)
				{
					Fruit fruit = (Fruit)_instantiatedFruits[i].Reference;
					if (fruit)
					{
						fruit.RigidbodyInstance.collisionDetectionMode = CollisionDetectionMode.Discrete; //to avoid warnings...
						fruit.RigidbodyInstance.isKinematic = true;
						fruit.enabled = false;
					}
				}
			}

			if (_score > 32)
			{
				MelonGameSettingsInstance.SharedScoreboardPrefab.Insert(Networking.LocalPlayer, _score);
			}

			RequestSerialization();
		}

		public Fruit InstantiateNewFruitAt(short rank, bool isFused)
		{
			Fruit newFruit = Instantiate(FruitPrefab).GetComponent<Fruit>();
			newFruit.Construct(Root, this, rank, transform.lossyScale.x, DeathZone.localPosition.y, isFused);
			_instantiatedFruits.Add(newFruit);
			AddToScore(1 << rank);
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

		#region Audio

		public AudioSource AudioPlayer;
		public AudioClip Drop;
		public AudioClip Collision;
		public AudioClip Combo;

		public void IncrementComboAndPlayAudio()
		{
			_combo++;
			AudioPlayer.pitch = 1 + _combo / 6.0f;
			SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayComboAudio));
		}

		public void PlayComboAudio()
		{
			if (!_isPlayerInArea)
				return;

			AudioPlayer.clip = Combo;
			AudioPlayer.Play();
		}

		public void PlayCollisionAudio()
		{
			if (!_isPlayerInArea)
				return;

			AudioPlayer.pitch = 1;
			AudioPlayer.clip = Collision;
			AudioPlayer.Play();
		}

		public void PlayDropAudio()
		{
			if (!_isPlayerInArea)
				return;

			AudioPlayer.pitch = 1;
			AudioPlayer.clip = Drop;
			AudioPlayer.Play();
		}
		#endregion
	}
}
