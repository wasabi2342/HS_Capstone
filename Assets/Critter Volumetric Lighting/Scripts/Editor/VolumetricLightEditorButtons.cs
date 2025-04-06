using UnityEngine;
using UnityEditor;

namespace CritterVolumetricLighting
{
	[CustomEditor(typeof(VolumetricLightManager))]
	public class VolumetricLightEditorButtons : Editor
	{
		public override void OnInspectorGUI()
		{
			base.OnInspectorGUI();

			if (EditorApplication.isPlaying)
			{
				VolumetricLightManager vLManager = (VolumetricLightManager)target;

				if (GUILayout.Button("Apply settings"))
				{
					Debug.Log("Changes applied.");
					vLManager.ApplySettings();
				}
			} else 
			{
				if (GUILayout.Button("Apply settings"))
				{
					Debug.Log("Volumetric lighting can be seen in play mode.");
				}
			}
			
		}
	}
}