
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class GameSettings : UdonSharpBehaviour
	{
		public RenderTexture RenderTextureToUse;
		public string ScreenShaderEmissionPropertyName = "_EmissionMap";
		public SharedScoreboard SharedScoreboardPrefab;
		public AudioClip Music;
	}
}
