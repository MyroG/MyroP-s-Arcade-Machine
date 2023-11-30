
using TMPro;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class ScoreboardUI : UdonSharpBehaviour
	{
		public GameSettings GameSettingsInstance;
		public TextMeshProUGUI[] Names;
		public TextMeshProUGUI[] Scores;
		public GameObject ScoreboardWrapper;

		private SharedScoreboard _scoreboardReference;

		public int MaxLengthName = 10;
		public bool UpperCaseNames = false;

		void Start()
		{
			_scoreboardReference = GameSettingsInstance.SharedScoreboardPrefab;
			ScoreboardWrapper.SetActive(_scoreboardReference);

			if (_scoreboardReference)
			{
				_scoreboardReference.RegisterBehaviour(this);
				RequestScoreboardUpdate();
			}
		}

		//Called from the Scoreboard script
		public void RequestScoreboardUpdate()
		{
			if (!_scoreboardReference)
				return; //Should never happen, but we never know

			DataDictionary ranking = _scoreboardReference.GetRanking();

			//the format is ranking[score] = name

			DataList scores = ranking.GetKeys();
			scores.Sort();

			int numberFields = Names.Length;
			int numberScores = scores.Count;

			for (int fieldIndex = 0; fieldIndex < numberFields; fieldIndex++)
			{
				bool hasScoreAtPosition = fieldIndex < numberScores;
				string name = hasScoreAtPosition ? ranking[scores[fieldIndex]].String : "";
				string score = hasScoreAtPosition ? scores[fieldIndex].Double.ToString() : "";

				if (UpperCaseNames)
					name = name.ToUpper();

				if (name.Length > MaxLengthName)
					name = name.Substring(0, MaxLengthName);

				Names[fieldIndex].text = name;
				Scores[fieldIndex].text = score;
			}
		}
	}
}
