
using System;
using UdonSharp;
using UnityEngine;
using VRC.SDK3.Data;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
	public class SharedScoreboard : UdonSharpBehaviour
	{
		[UdonSynced]
		private string _syncedScoreboard;

		private DataDictionary _scoreboard;
		private DataList _connectedScoreboards;
		void Start()
		{

		}

		private void CheckReferences()
		{
			if (_connectedScoreboards == null)
				_connectedScoreboards = new DataList();

			if (_scoreboard == null)
				_scoreboard = new DataDictionary();
		}

		public void RegisterBehaviour(UdonSharpBehaviour usharpBehaviour)
		{
			CheckReferences();

			if (_connectedScoreboards.IndexOf(usharpBehaviour) == -1)
			{
				_connectedScoreboards.Add(usharpBehaviour);
			}
		}


		public void Insert(VRCPlayerApi player, int score)
		{
			CheckReferences();
			_scoreboard[score.ToString()] = player.displayName;

			SerializeJSON();
		}

		public DataDictionary GetRanking()
		{
			CheckReferences();
			return _scoreboard;
		}

		private void SerializeJSON()
		{
			// TODO : Known issue : if two players finish the game at the exact same time on two different arcade machines, then the next statement will fail for one of them
			// This issue is a bit difficult to fix since the owner of the scoreboard might have disabled the arcade machine.
			// A very rare edge case
			Networking.SetOwner(Networking.LocalPlayer, gameObject);

			if (VRCJson.TrySerializeToJson(_scoreboard, JsonExportType.Minify, out DataToken result))
			{
				_syncedScoreboard = result.String;
				RequestSerialization();
				HandleOnDeserialization();
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
			CheckReferences();
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
