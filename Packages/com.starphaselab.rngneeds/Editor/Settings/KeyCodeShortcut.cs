using UnityEditor;
using UnityEngine;

namespace RNGNeeds.Editor
{
    internal static class KeyCodeShortcut
    {
        public static void ShortcutDropdown(ref KeyCode[] keyCodes, GUIContent content)
        {
            EditorGUILayout.BeginHorizontal();
                GUILayout.Label(content, GUILayout.Width(EditorGUIUtility.labelWidth));
                EditorGUILayout.BeginHorizontal();
                for (var i = 0; i < keyCodes.Length; i++)
                {
                    keyCodes[i] = (KeyCode)EditorGUILayout.EnumPopup(keyCodes[i]);
                }
                EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }
        
        public static void ShortcutDropdown(ref EventModifiers[] modifiers, GUIContent content)
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label(content, GUILayout.Width(EditorGUIUtility.labelWidth));
            EditorGUILayout.BeginHorizontal();
            for (var i = 0; i < modifiers.Length; i++)
            {
                modifiers[i] = (EventModifiers)EditorGUILayout.EnumPopup(modifiers[i]);
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndHorizontal();
        }
    }
}