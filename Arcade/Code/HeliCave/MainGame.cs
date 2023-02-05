using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

namespace MyroP.Arcade
{
	public enum GameState
	{
		START_SCREEN,
		EXPLANATIONS,
		PLAY,
		PAUSE,
		FINISH
	}

	public class MainGame : UdonSharpBehaviour
	{
		

		public GameSettings GameSettingsInstance;
		public Camera CameraLookingAtGame;
		public MeshRenderer ScreenRenderer;

		//Basically the boundaries of the game, used to determine where stuff needs to be spawned
		public Transform TopBoundary;
		public Transform BottomBoundary;
		public Transform RightBoundary;
		public Transform LeftBoundary;

		public Transform PawnDefaultPosition;

		//Each canvas  used in-game
		public GameObject CanvasPressStart;
		public GameObject CanvasExplanation;
		public GameObject CanvasTimer;
		public GameObject CanvasFinalScore;
		public GameObject CanvasPauseScreen;

		//Prefabs that get spawned
		public GameObject PrefabWall;
		private Vector3 _boundingBoxPrefabWall;
		public GameObject PrefabControllable;

		public TextMeshProUGUI ProgressTxt;
		public TextMeshProUGUI ScoreTxt;
		public TextMeshProUGUI FinalScoreTxt;
		public TextMeshProUGUI TimerTxt;

		//SFX
		public AudioClip ClipExplosion;
		public AudioClip ClipJump;
		public AudioClip ClipSelection;
		public AudioSource SoundEffectSource;
		public AudioSource SoundEffectHelicopter;
		public AudioSource Music;

		public GameObject SpawnedWallsWrapper;
		private GameObject[] _spawnedWallsBottom;
		private GameObject[] _spawnedWallsTop;
		private GameObject _spawnedControllable;


		[UdonSynced]
		private GameState _gameState;
		private GameState _gameStateSaved;
		[UdonSynced]
		private int _seed;
		private int _seedSaved;
		private int _currentSeed;

		[UdonSynced]
		private double _startGameTime;
		[UdonSynced]
		private double _startRoundTime;
		[UdonSynced]
		private Vector3 _positionPlayer;
		[UdonSynced]
		private float _currentScore;
		private bool _gameStarted;

		private double _timePlayed;

		private float _distanceBetweenEachWall;
		private float _currentTraveledDistanceByWall;
		private int _indexLastWall;

		private float _currentDistanceBetweenTopAndBottomWall;
		private float _currentMiddleWallPosition;

		private float _currentTime;
		private int _currentFrameSinceNewGame;
		


		private bool _buttonPressed;
		
		private bool _gameCanBeStarted;
		private float _currentUpSpeed;

		private bool _inputBlocked;

		void Start()
		{
			SpawnWalls();
			SpawnPawn();
			ResetGame();

			//If the arcade got scaled down, we need to make sure that the camera still fits
			//not gonna check if the arcade machine got unevenly scaled, because...meh, probably pointless
			float scale = GameSettingsInstance.transform.lossyScale.x; //GameSettingsInstance is attached at the root
			CameraLookingAtGame.orthographicSize *= scale;
			CameraLookingAtGame.nearClipPlane *= scale;
			CameraLookingAtGame.farClipPlane *= scale;

			//Now we need to set the renderTexture on the camera, and on the screen material
			CameraLookingAtGame.targetTexture = GameSettingsInstance.RenderTextureToUse;
			ScreenRenderer.material.mainTexture = GameSettingsInstance.RenderTextureToUse;
			if (!System.String.IsNullOrEmpty(GameSettingsInstance.ScreenShaderEmissionPropertyName))
			{
				ScreenRenderer.material.SetTexture(GameSettingsInstance.ScreenShaderEmissionPropertyName, GameSettingsInstance.RenderTextureToUse);
			}

			gameObject.SetActive(false);
		}

		private void ShowCanvas(GameObject canvasToShow)
		{
			CanvasPressStart.SetActive(false);
			CanvasExplanation.SetActive(false);
			CanvasTimer.SetActive(false);
			CanvasFinalScore.SetActive(false);
			CanvasPauseScreen.SetActive(false);

			if (canvasToShow != null)
			{
				canvasToShow.SetActive(true);
			}
		}

		private void SpawnWalls()
		{
			_spawnedWallsBottom = new GameObject[GameSettingsInstance.NumberWalls];
			_spawnedWallsTop = new GameObject[GameSettingsInstance.NumberWalls];

			float direction = (RightBoundary.localPosition - LeftBoundary.localPosition).x;
			_distanceBetweenEachWall = direction / (GameSettingsInstance.NumberWalls - 1);

			for (int i = 0; i < GameSettingsInstance.NumberWalls; i++)
			{
				GameObject objBottom = SpawnWall();
				GameObject objTop = SpawnWall();

				_spawnedWallsBottom[GameSettingsInstance.NumberWalls - i - 1] = objBottom;
				_spawnedWallsTop[GameSettingsInstance.NumberWalls - i - 1] = objTop;

				//objBottom.transform.localRotation = new Quaternion(0, 0, 180, 0);
			}
		}

		public void PlayerPickedUpJoystick()
		{
			if (_gameState == GameState.PAUSE && IsLocalPlayerPlaying())
			{
				_gameState = GameState.PLAY;
				SetNewRoundTimer();
				RequestSerialization();
				HandleOnDeserialization();
			}
			else
			{
				Networking.SetOwner(Networking.LocalPlayer, gameObject);
				PrepareResetGame();
			}
		}

		private void SpawnPawn()
		{
			_spawnedControllable = Instantiate(PrefabControllable);
			_spawnedControllable.transform.parent = gameObject.transform;
			_spawnedControllable.transform.localScale = new Vector3(1, 1, 1);
		}

		private void PositionWall(GameObject wall, float positionX, bool shouldScaleToTop)
		{
			if (wall == null)
				return;

			float positionY = _currentDistanceBetweenTopAndBottomWall * (shouldScaleToTop ? 1 : -1) + _currentMiddleWallPosition;

			wall.transform.localPosition = new Vector3(0, positionY, 0);

			MoveWall(wall, positionX, shouldScaleToTop);
		}

		private void MoveWall(GameObject wall, float directionX, bool shouldScaleToTop)
		{
			if (wall == null)
				return;

			float positionY = wall.transform.localPosition.y;
			float requiredWidth = (RightBoundary.localPosition - LeftBoundary.localPosition).x / (GameSettingsInstance.NumberWalls - 1);
			float requiredHeight = shouldScaleToTop
				? TopBoundary.localPosition.y - positionY
				: positionY - BottomBoundary.localPosition.y;

			Vector3 newPosition = new Vector3(wall.transform.localPosition.x, positionY, 0);
			newPosition += new Vector3(directionX, 0, 0);

			wall.transform.localRotation = new Quaternion(0, 0, shouldScaleToTop ? 0 : 180, 0);
			wall.transform.localScale = new Vector3(requiredWidth / _boundingBoxPrefabWall.x, requiredHeight / _boundingBoxPrefabWall.y, 1);
			wall.transform.localPosition = newPosition;
			//TODO adapt position and scaling on x axis so there's no "bleeding"
		}

		private GameObject SpawnWall()
		{
			GameObject obj = Instantiate(PrefabWall);
			Wall wall = obj.GetComponent<Wall>();
			if (wall)
			{
				wall.MainGameInstance = this;
			}

			obj.transform.parent = SpawnedWallsWrapper.transform;

			if (_boundingBoxPrefabWall == Vector3.zero)
			{
				_boundingBoxPrefabWall = obj.GetComponent<MeshRenderer>().bounds.size;
			}

			return obj;
		}

		private void ResetWallTransforms()
		{
			SetDistanceTopAndBottomWall(1);

			for (int i = 0; i < GameSettingsInstance.NumberWalls; i++)
			{
				GameObject wallBottom = _spawnedWallsBottom[GameSettingsInstance.NumberWalls - i - 1];
				GameObject wallTop = _spawnedWallsTop[GameSettingsInstance.NumberWalls - i - 1];
				
				PositionWall(wallBottom	, RightBoundary.transform.localPosition.x - _distanceBetweenEachWall * i, false);
				PositionWall(wallTop	, RightBoundary.transform.localPosition.x - _distanceBetweenEachWall * i, true);
			}
		}

		private void SetDistanceTopAndBottomWall(float heightPercentage)
		{
			_currentDistanceBetweenTopAndBottomWall = 0.5f + heightPercentage * (TopBoundary.localPosition - BottomBoundary.localPosition).y / 5f;
		}

		public override void OnDeserialization()
		{
			HandleOnDeserialization();
		}

		private void HandleOnDeserialization()
		{
			if (_gameState == GameState.START_SCREEN)
			{
				ResetGame();
				if (_gameStateSaved != _gameState)
				{
					PlaySelectSound();
				}
			}
			else if (_gameState == GameState.EXPLANATIONS)
			{
				ShowCanvas(CanvasExplanation);
				PlaySelectSound();
			}
			else if (_gameState == GameState.PLAY)
			{
				if (_gameStateSaved != _gameState)
				{
					PlaySelectSound();
					PlayMusic(true);
					ShowCanvas(CanvasTimer);
				}

				if (_seed != _seedSaved)
				{
					_currentSeed = _seed;
					_seedSaved = _seed;
				}

				if (Networking.GetServerTimeInSeconds() < _startRoundTime && _gameStateSaved != GameState.PAUSE)
				{
					InitNextRoundValues();
				}
			}
			else if (_gameState == GameState.PAUSE && _gameStateSaved != _gameState)
			{
				ShowCanvas(CanvasPauseScreen);
				PlaySelectSound();
				PlayMusic(false);
				SoundEffectHelicopter.gameObject.SetActive(false);
				_gameStarted = false;
			}
			else if (_gameState == GameState.FINISH)
			{
				PlayMusic(false);
				ShowCanvas(CanvasFinalScore);
				FinalScoreTxt.text = ((long) _currentScore).ToString();
				InitNextRoundValues();
			}
			_gameStateSaved = _gameState;
		}

		public void ResetIfOwner()
		{
			if (IsLocalPlayerPlaying())
			{
				PrepareResetGame();
			}
		}
		private void ResetGame()
		{
			InitNextRoundValues();
			ShowCanvas(CanvasPressStart);
			_timePlayed = 0;
			_currentScore = 0;
			_startRoundTime = 0;
			Music.gameObject.SetActive(false);
			_inputBlocked = false;
		}

		/// <summary>
		/// Reset values between each round, doesn't fully reset the game
		/// </summary>
		private void InitNextRoundValues()
		{
			_currentMiddleWallPosition = RightBoundary.transform.localPosition.y;

			ResetWallTransforms();

			_buttonPressed = false;
			_currentTraveledDistanceByWall = _distanceBetweenEachWall / 2;
			_indexLastWall = GameSettingsInstance.NumberWalls - 1;
			_currentUpSpeed = 0;
			_spawnedControllable.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 0));
			_spawnedControllable.transform.localPosition = PawnDefaultPosition.localPosition;
			_gameStarted = false;
			SoundEffectHelicopter.gameObject.SetActive(false);
		}

		public void PrepareResetGame()
		{
			_gameState = 0;
			RequestSerialization();
			HandleOnDeserialization();
		}

		public void PlayerDroppedJoystick()
		{
			if (_gameState == GameState.PLAY)
			{
				_gameState = GameState.PAUSE;
			}
			else
			{
				PrepareResetGame();
			}
			RequestSerialization();
			HandleOnDeserialization();
		}

		private int GetNextSeed()
		{
			return Random.Range((int)0, (int)0xFFFFFF);
		}

		public void OnPress()
		{
			if (_inputBlocked)
			{
				return;
			}

			switch (_gameState)
			{
				case GameState.PLAY:
				{
					_buttonPressed = true;
					break;
				}
				default:
				{
					if (_gameState == GameState.START_SCREEN)
					{
						_gameState = GameState.EXPLANATIONS;
					}
					else if (_gameState == GameState.EXPLANATIONS)
					{
						_gameState = GameState.PLAY;
					}

					if (_gameState == GameState.PLAY)
					{
						InitNewRound();
						_startGameTime = _startRoundTime;
					}
					else if (_gameState == GameState.FINISH)
					{
						_gameState = GameState.START_SCREEN;
					}
					RequestSerialization();
					HandleOnDeserialization();
					break;
				}
			};
		}

		public void SyncPlayer()
		{
			if (_gameState == GameState.PLAY && _gameStarted)
			{
				_positionPlayer = _spawnedControllable.transform.localPosition;
				RequestSerialization();
				SendCustomEventDelayedSeconds(nameof(SyncPlayer), 0.2f);
			}
		}
		private void InitNewRound()
		{
			SetNewRoundTimer();
			while (_seed == _seedSaved)
			{
				_seed = GetNextSeed();
			}
			RequestSerialization();
			HandleOnDeserialization();
		}

		private void SetNewRoundTimer()
		{
			_startRoundTime = Networking.GetServerTimeInSeconds() + 4.0f;
		}

		public void OnRelease()
		{
			_buttonPressed = false;
		}

		private int GetIndexFirstWall()
		{
			return _indexLastWall + 1 >= GameSettingsInstance.NumberWalls ? 0 : _indexLastWall + 1;
		}

		private void UpdateMiddleBonePosition()
		{
			if (_gameStarted)
			{
				//Random.State previousState = Random.state; //Uncomment once it's exposed to Udon
				Random.InitState(_currentSeed);

				float newDirection = Random.Range(-0.7f, 0.7f);
				_currentMiddleWallPosition +=  newDirection;

				if (_currentMiddleWallPosition + _currentDistanceBetweenTopAndBottomWall > TopBoundary.localPosition.y || _currentMiddleWallPosition - _currentDistanceBetweenTopAndBottomWall < BottomBoundary.localPosition.y)
				{
					_currentMiddleWallPosition -= 2 * newDirection;
				}

				_currentSeed = GetNextSeed();
				//Random.state = previousState;  //Uncomment once it's exposed to Udon
			}
		}

		private bool IsLocalPlayerPlaying()
		{
			return Networking.IsOwner(gameObject);
		}

		private void UpdateIngameUI()
		{
			if (_gameState == GameState.PLAY)
			{
				if (!_gameStarted)
				{
					if (Networking.GetServerTimeInSeconds() > _startRoundTime)
					{
						TimerTxt.text = "";
					}
					else
					{
						long remainingTime = (long)(_startRoundTime - Networking.GetServerTimeInSeconds());
						TimerTxt.text = remainingTime == 0 ? "GO!" : remainingTime.ToString();
					}
				}

				if (GameSettingsInstance.GameDuration != 0)
				{
					ProgressTxt.text = $"Progress : {(long)(_timePlayed * 100.0f / GameSettingsInstance.GameDuration)}%";
				}
				ScoreTxt.text = ((long)_currentScore).ToString("D14");
			}
		}

		private void Update()
		{
			if (_gameState != GameState.PLAY)
			{
				return;
			}

			UpdateIngameUI();

			if (!_gameStarted && Networking.GetServerTimeInSeconds() > _startRoundTime)
			{
				_gameStarted = true;
				if (IsLocalPlayerPlaying())
				{
					SyncPlayer();
				}
				SoundEffectHelicopter.gameObject.SetActive(true);
			}

			if (!_gameStarted)
			{
				return;
			}

			//calculating score, progress and broadness of the cave
			_timePlayed += Time.deltaTime;
			SetDistanceTopAndBottomWall(1.0f - (float)(Networking.GetServerTimeInSeconds() - _startRoundTime) / GameSettingsInstance.GameDuration);

			if (IsLocalPlayerPlaying())
			{
				_currentScore += (float)(Networking.GetServerTimeInMilliseconds() / 1000.0f - _startRoundTime);

				//Check if the game is finished
				if (_timePlayed > GameSettingsInstance.GameDuration)
				{
					_gameState = GameState.FINISH;
					_inputBlocked = true;
					SendCustomEventDelayedSeconds(nameof(UnlockControls), 2.0f);
					RequestSerialization();
					HandleOnDeserialization();
				}
				//Applying forces to the helicopter
				_spawnedControllable.transform.localPosition += new Vector3(0, _currentUpSpeed, 0);
				if (_buttonPressed)
				{
					_currentUpSpeed += Time.deltaTime / 16.0f;
				}
				else
				{
					_currentUpSpeed -= Time.deltaTime / 16.0f;
				}


				SoundEffectHelicopter.pitch = Mathf.Clamp((_currentUpSpeed + 1) + _currentUpSpeed * 20, 0.2f, 1.8f);

			}
			else
			{
				_spawnedControllable.transform.localPosition = _positionPlayer;
			}
				
			//Moving the walls
			float direction = -Time.deltaTime;
			int startPosition = GetIndexFirstWall();

			for (int i = 0; i < GameSettingsInstance.NumberWalls; i++)
			{
				int indexToUse = i + startPosition >= GameSettingsInstance.NumberWalls ? (i + startPosition) - GameSettingsInstance.NumberWalls : i + startPosition;
				
				GameObject objBottom = _spawnedWallsBottom[indexToUse];
				GameObject objTop = _spawnedWallsTop[indexToUse];

				MoveWall(objBottom, direction, false);
				MoveWall(objTop, direction, true);
			}

			_currentTraveledDistanceByWall += -direction;
			MoveFirstWallToLast();
		}

		private void MoveFirstWallToLast()
		{
			while (_currentTraveledDistanceByWall >= _distanceBetweenEachWall)
			{
				UpdateMiddleBonePosition();
				int indexFirstWall = GetIndexFirstWall();

				//First wall will be placed behing last wall
				PositionWall(_spawnedWallsBottom[indexFirstWall], _spawnedWallsBottom[_indexLastWall].transform.localPosition.x + _distanceBetweenEachWall, false);
				PositionWall(_spawnedWallsTop[indexFirstWall], _spawnedWallsTop[_indexLastWall].transform.localPosition.x + _distanceBetweenEachWall, true);

				_currentTraveledDistanceByWall -= _distanceBetweenEachWall;

				//start and end position has changed, so we move them
				_indexLastWall++;
				if (_indexLastWall >= GameSettingsInstance.NumberWalls)
				{
					_indexLastWall = 0;
				}
			}
		}

		public void Crashed()
		{
			if (IsLocalPlayerPlaying())
			{
				InitNewRound();

				RequestSerialization();
				HandleOnDeserialization();
				SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, nameof(PlayExplosionSound));
			}
		}

		public void PlayJumpSound()
		{
			SoundEffectSource.clip = ClipJump;
			SoundEffectSource.Play();
		}

		public void PlaySelectSound()
		{
			SoundEffectSource.clip = ClipSelection;
			SoundEffectSource.Play();
		}

		public void PlayMusic(bool shouldPlayMusic)
		{
			Music.gameObject.SetActive(shouldPlayMusic);
		}

		public void PlayExplosionSound()
		{
			SoundEffectSource.clip = ClipExplosion;
			SoundEffectSource.Play();
		}

		public void UnlockControls()
		{
			_inputBlocked = false;
		}
	}
}