
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace myro.arcade
{
	[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
	public class GameSettingsApplier : UdonSharpBehaviour
	{
		public GameSettings GameSettingsInstance;
		public Camera CameraLookingAtGame;
		public MeshRenderer ScreenRenderer;
		public AudioSource Music;

		void Start()
		{
			CameraLookingAtGame.targetTexture = GameSettingsInstance.RenderTextureToUse;
			ScreenRenderer.material.mainTexture = GameSettingsInstance.RenderTextureToUse;
			if (!System.String.IsNullOrEmpty(GameSettingsInstance.ScreenShaderEmissionPropertyName))
			{
				ScreenRenderer.material.SetTexture(GameSettingsInstance.ScreenShaderEmissionPropertyName, GameSettingsInstance.RenderTextureToUse);
			}
			Music.clip = GameSettingsInstance.Music;
		}
	}
}
