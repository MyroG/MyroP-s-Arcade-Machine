﻿
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class Scoreboard : UdonSharpBehaviour
	{
		[UdonSynced]
		private string _syncedScoreboard;

		private DataDictionary _scoreboard;
		private DataList _connectedScoreboards;
		void Start()
		{
			_scoreboard = new DataDictionary();
			_connectedScoreboards = new DataList();
		}

		public void RegisterBehaviour(UdonSharpBehaviour usharpBehaviour)
		{
			if (_connectedScoreboards.IndexOf(usharpBehaviour) == -1)
			{
				_connectedScoreboards.Add(usharpBehaviour);
			}
		}

		public void Insert(VRCPlayerApi player, double score)
		{
			_scoreboard[player.displayName] = score;

			
			SerializeJSON();
		}

		private void SerializeJSON()
		{
			if (!Networking.IsOwner(gameObject))
				return;

			if (VRCJson.TrySerializeToJson(_scoreboard, JsonExportType.Minify, out DataToken result))
			{
				_syncedScoreboard = result.String;
				RequestSerialization();

			}
		}

		private void DeserializeJSON()
		{
			if (VRCJson.TryDeserializeFromJson(_syncedScoreboard, out DataToken result))
			{
				_scoreboard = result.DataDictionary;
			}
		}

		private void HandleOnDeserialization()
		{
			DeserializeJSON();

			for (int i = 0; i < _connectedScoreboards.Count; i++)
			{
				UdonSharpBehaviour behaviour = (UdonSharpBehaviour)_connectedScoreboards[i].Reference;
				if (behaviour)
				{
					behaviour.SendCustomEvent("RequestScoreboardUpdate");
				}
			}
		}

		public override void OnDeserialization()
		{
			HandleOnDeserialization();
		}
	}
}