using UnityEditor;
using UnityEngine;

namespace RNGNeeds.Samples.CardDeck.Editor
{
    [CustomEditor(typeof(DeckBuilder))]
    public class DeckBuilderEditor : UnityEditor.Editor
    {
        private SerializedProperty m_DeckBuilderProperty;
        
        private DeckBuilder m_DeckBuilder;
        private readonly Color separatorColor = new Color(.5f, .6f, .7f);
        private GUIStyle m_UnitsStyle;
        private GUIStyle m_CardStyle;
        private Rect lightSkinBackgroundRect;
        private Texture2D lightSkinBackgroundTexture;
        
        private void OnEnable()
        {
            m_DeckBuilder = (DeckBuilder)serializedObject.targetObject;
            m_CardStyle = new GUIStyle();
            m_CardStyle.padding = new RectOffset(10, 0, 2, 0);
            if (EditorGUIUtility.isProSkin == false)
            {
                lightSkinBackgroundTexture = new Texture2D(1, 1);
                lightSkinBackgroundTexture.SetPixel(0, 0, new Color(.32f, .32f, .32f, 1f));
                lightSkinBackgroundTexture.Apply();
                m_CardStyle.normal.background = lightSkinBackgroundTexture;
            }
            m_UnitsStyle = new GUIStyle(m_CardStyle) { normal = { textColor = Color.white }, alignment = TextAnchor.MiddleRight, padding = new RectOffset(0, 4, 0, 0)};
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            m_DeckBuilderProperty = serializedObject.GetIterator();
            if (m_DeckBuilderProperty.NextVisible(true))
            {
                do
                {
                    if(m_DeckBuilderProperty.name == "m_Script") continue;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(m_DeckBuilderProperty.name), true);
                }
                while (m_DeckBuilderProperty.NextVisible(false));
            }
            serializedObject.ApplyModifiedProperties();


            var deckToFillSet = m_DeckBuilder.deckToFill != null;
            EditorGUI.BeginDisabledGroup(!deckToFillSet);
            if (GUILayout.Button("Fill Deck", GUILayout.Height(30f)))
            {
                m_DeckBuilder.FillDeck();
                EditorUtility.SetDirty(m_DeckBuilder.deckToFill);
                AssetDatabase.SaveAssets();
            }
            EditorGUI.EndDisabledGroup();
            
            if (deckToFillSet == false) return;
            
            DrawSeparator();
            DrawDeckContents();
            DrawSeparator();
        }
        
        private void DrawSeparator()
        {
            var controlRect = EditorGUILayout.GetControlRect(true, 4f);
            GUI.BeginGroup(controlRect);
            EditorGUI.DrawRect(new Rect(0f, 2f, controlRect.xMax - 12f, 1f), separatorColor);
            GUI.EndGroup();
        }

        private void DrawDeckContents()
        {
            for (var i = 0; i < m_DeckBuilder.deckToFill.cards.ItemCount; i++)
            {
                var probabilityItem = m_DeckBuilder.deckToFill.cards.GetProbabilityItem(i);
                var card = probabilityItem.Value;
                var color = card.ItemColor;
                m_CardStyle.normal.textColor = color;
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"{probabilityItem.MaxUnits} x", m_UnitsStyle, GUILayout.Width(40f));
                EditorGUILayout.LabelField(card.name, m_CardStyle);
                EditorGUILayout.EndHorizontal();
            }
        }
    }
}