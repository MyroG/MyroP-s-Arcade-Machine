﻿
using System;
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
		public TextMeshProUGUI	 TextScoreboard;
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

		/// <summary>
		/// Converts a dt[str] = str data dictionary to dt[int] = str, because TrySerialize can only serialize string keys, so you need to convert the string back to int
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		private DataDictionary GetNumberIndexedDT(DataDictionary dt)
		{
			DataDictionary ret = new DataDictionary();
			DataList keys = dt.GetKeys();

			for (int i = 0; i < keys.Count; i++)
			{
				ret[Convert.ToInt32((keys[i].String))] = dt[keys[i]]; 
			}
			return ret;
		}

		private DataList InvertDataList(DataList dl)
		{
			DataList ret = new DataList();
			for (int i = dl.Count - 1; i >= 0; i--)
			{
				ret.Add(dl[i]);
			}
			return ret;
		}

		private string FormatName(string name)
		{
			if (UpperCaseNames)
				name = name.ToUpper();

			if (name.Length > MaxLengthName)
				name = name.Substring(0, MaxLengthName);

			return name;
		}
		//Called from the Scoreboard script
		public void RequestScoreboardUpdate()
		{
			if (!_scoreboardReference)
				return; //Should never happen, but we never know

			DataDictionary ranking = GetNumberIndexedDT(_scoreboardReference.GetRanking());
			//the format is ranking[score (int)] = name (str)

			DataList scores = ranking.GetKeys();
			scores.Sort();
			scores = InvertDataList(scores);

			int numberScores = scores.Count;

			if (TextScoreboard)
			{
				TextScoreboard.text = "";
				numberScores = numberScores > 10 ? 10 : numberScores; //we only want to show the first 10 scores
				for (int scoreIndex = 0; scoreIndex < numberScores; scoreIndex++)
				{
					string name = ranking[scores[scoreIndex]].String;
					string score = scores[scoreIndex].Int.ToString();

					name = FormatName(name);
					TextScoreboard.text += $"{name} {score}\n";
				}
			}
			else
			{
				int numberFields = Names.Length;
				
				for (int fieldIndex = 0; fieldIndex < numberFields; fieldIndex++)
				{
					bool hasScoreAtPosition = fieldIndex < numberScores;
					string name = hasScoreAtPosition ? ranking[scores[fieldIndex]].String : "";
					string score = hasScoreAtPosition ? scores[fieldIndex].Int.ToString() : "";

					name = FormatName(name);

					Names[fieldIndex].text = name;
					Scores[fieldIndex].text = score;
				}
			}
		}
	}
}
