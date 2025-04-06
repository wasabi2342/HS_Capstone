using UnityEditor;
using UnityEngine;

namespace RNGNeeds.Editor
{
    public class IntLabelField
    {
        private int m_OriginalValue;
        private int m_StoredValue;
        private const string ControlIdPrefix = "IntLabelField";
        private int PreviousSelectedIndex { get; set; } = -1;
        public int SelectedIndex { get; set; } = -1;
        
        public (int Value, bool Finished) DrawAndHandleInput(Rect rect, int index, int currentValue, bool locked, GUIStyle style)
        {
            var newValue = currentValue;
            var finishedEditing = false;

            if (SelectedIndex == index)
            {
                var controllID = $"{ControlIdPrefix}_{index.ToString()}";
                EditorGUI.FocusTextInControl(controllID);
                GUI.SetNextControlName(controllID);

                if (SelectedIndex != PreviousSelectedIndex)
                {
                    m_StoredValue = currentValue;
                    PreviousSelectedIndex = SelectedIndex;
                }

                m_StoredValue = EditorGUI.IntField(rect, m_StoredValue);

                if (Event.current.type == EventType.KeyUp)
                {
                    if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
                    {
                        SelectedIndex = -1;
                        GUIUtility.keyboardControl = 0;
                        newValue = m_StoredValue;
                        finishedEditing = true;
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.Escape)
                    {
                        SelectedIndex = -1;
                        GUIUtility.keyboardControl = 0;
                        newValue = m_OriginalValue;
                        Event.current.Use();
                    }
                    else if (Event.current.keyCode == KeyCode.Tab)
                    {
                        SelectedIndex = index + 1;
                        newValue = m_StoredValue;
                        finishedEditing = true;
                        Event.current.Use();
                    }
                }
            }
            else
            {
                EditorGUI.LabelField(rect, currentValue.ToString(), style);
                
                if (Event.current.type == EventType.MouseDown && rect.Contains(Event.current.mousePosition) && !locked)
                {
                    SelectedIndex = index;
                    m_OriginalValue = currentValue;
                    Event.current.Use();
                }
            }
            
            return (newValue, finishedEditing);
        }
    }
}