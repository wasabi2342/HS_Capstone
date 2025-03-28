using StarphaseTools.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace RNGNeeds.Editor
{
    #pragma warning disable 0618
    [CustomPropertyDrawer(typeof(PLCollection<>))]
    public class PLCollectionDrawer : LabDrawerBase
    {
        private IPLCollectionEditorActions PlCollectionInterface;
        private SerializedProperty p_Lists;

        private bool m_AllowRemove;
        private bool m_AllowReorder;
        
        private Event m_CurrentEvent;
        private Rect m_InnerRect;
        private readonly GUIContent m_TempContent = new GUIContent();
        
        private float m_TotalHeight;
        private const float m_InnerRectPadding = 8f;
        
        protected override bool Initialize(SerializedProperty property)
        {
            p_Lists = property.FindPropertyRelative("pl_collection");
            PlCollectionInterface = property.GetTargetObjectOfProperty() as IPLCollectionEditorActions;
            return true;
        }

        protected override void BaseDraw(Rect position, SerializedProperty property, GUIContent label)
        {
            m_CurrentEvent = Event.current;
            if (m_CurrentEvent.type == EventType.Layout) return;
            var arraySize = p_Lists.arraySize;
            
            m_InnerRect = new Rect(position.x + m_InnerRectPadding, position.y + m_InnerRectPadding + 5f, position.width - m_InnerRectPadding * 2, position.height - m_InnerRectPadding * 2);
            var headerRectWidths = m_InnerRect.width / 3f;
            
            var propertyNameRect = new Rect(m_InnerRect.x, m_InnerRect.y - 2f, headerRectWidths, 28f);
            var addButtonRect = new Rect(propertyNameRect.xMax + 4f, m_InnerRect.y, headerRectWidths - 32f, 24f);
            var clearButtonRect = new Rect(addButtonRect.xMax + 4f, m_InnerRect.y, headerRectWidths - 32f, 24);
            var lockButtonRect = new Rect(clearButtonRect.xMax + 4f, m_InnerRect.y, 24f, 24f);
            var lockIconRect = new Rect(lockButtonRect.x + 4f, m_InnerRect.y + 4f, 16f, 16f);
            var reorderButtonRect = new Rect(lockButtonRect.xMax + 4f, m_InnerRect.y, 24f, 24f);
            var reorderIconRect = new Rect(reorderButtonRect.x + 4f, m_InnerRect.y + 4f, 16f, 16f);

            if (m_CurrentEvent.type == EventType.Repaint)
            {
                var headerRect = new Rect(position.x, position.y + 5f, position.width, 40f);
                
                if (arraySize > 0)
                {
                    var contentRect = new Rect(headerRect.x, headerRect.yMax, headerRect.width, position.height - headerRect.height - 20f);
                    PLDrawerTheme.CollectionFrameImage.Draw(contentRect, false, false, false, false);    
                }
                PLDrawerTheme.CollectionHeaderImage.Draw(headerRect, false, false, false, false);
            }
            
            EditorGUI.LabelField(propertyNameRect, $"{property.displayName} ({arraySize})", PLDrawerTheme.CollectionPropertyNameStyle);
            
            // Add List to Collection
            if (GUI.Button(addButtonRect, "Add List to Collection", PLDrawerTheme.OptionButtonOn))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Add List to Collection");
                PlCollectionInterface.AddList();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
            }

            if (GUI.Button(lockButtonRect, m_AllowRemove ? PLDrawerContents.CollectionUnlocked : PLDrawerContents.CollectionLocked, m_AllowRemove ? PLDrawerTheme.OptionButtonOff : PLDrawerTheme.OptionButtonOn)) m_AllowRemove = !m_AllowRemove;
            if (GUI.Button(reorderButtonRect, PLDrawerContents.CollectionAllowReorder, m_AllowReorder ? PLDrawerTheme.OptionButtonOn : PLDrawerTheme.OptionButtonOff)) m_AllowReorder = !m_AllowReorder;
            
            if (m_CurrentEvent.type == EventType.Repaint)
            {
                if(!m_AllowRemove) PLDrawerTheme.LockIconEnabled.Draw(lockIconRect, false, false, false, false);
                else PLDrawerTheme.UnlockedIcon.Draw(lockIconRect, false, false, false, false);
                if(m_AllowReorder) PLDrawerTheme.ReorderIconEnabled.Draw(reorderIconRect, false, false, false, false);
                else PLDrawerTheme.ReorderIconDisabled.Draw(reorderIconRect, false, false, false, false);
            }

            EditorGUI.BeginDisabledGroup(m_AllowRemove == false);
            if (GUI.Button(clearButtonRect, "Clear Collection", m_AllowRemove ? PLDrawerTheme.CriticalActionButton : PLDrawerTheme.OptionButtonOn))
            {
                Undo.RecordObject(property.serializedObject.targetObject, "Clear Collection");
                PlCollectionInterface.ClearCollection();
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                m_AllowRemove = false;
                return;
            }
            EditorGUI.EndDisabledGroup();

            var height = arraySize > 0 ? m_InnerRect.y + 22f : m_InnerRect.y + 32f;
            m_TotalHeight = height;
            
            for (var i = 0; i < arraySize; i++)
            {
                var list = p_Lists.GetArrayElementAtIndex(i);
                var listInterface = list.GetTargetObjectOfProperty() as IProbabilityListEditorActions;
                if (string.IsNullOrEmpty(listInterface.ListName)) listInterface.ListName = $"#{i}";
                
                var listHeight = EditorGUI.GetPropertyHeight(list);
                
                var rect = new Rect(m_InnerRect.x, height + 20f, m_InnerRect.width, listHeight);
                EditorGUI.PropertyField(rect, list, true);
                
                m_TempContent.text = listInterface.ListName;
                var titleFieldWidth = PLDrawerTheme.InputField.CalcSize(m_TempContent).x + 30f;
                var titleRect = new Rect(rect.xMax - titleFieldWidth - 60f, rect.y + 8f, titleFieldWidth, 20f);
                
                EditorGUI.BeginChangeCheck();
                listInterface.ListName = EditorGUI.TextField(titleRect, listInterface.ListName, PLDrawerTheme.InputField);
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(property.serializedObject.targetObject);
                }
                
                // Reordering
                if(m_AllowReorder)
                {
                    var rightReorderButtonRect = new Rect(titleRect.x + -20f, titleRect.y + 1f, 16f, 18f);
                    var leftReorderButtonRect = new Rect(rightReorderButtonRect);
                    
                    if(i > 0)   // can move up
                    {
                        leftReorderButtonRect.Set(rightReorderButtonRect.x - 20f, rightReorderButtonRect.y, 16f, 18f);
                        if (GUI.Button(rightReorderButtonRect, PLDrawerContents.MoveUpButton, PLDrawerTheme.MoveUpButton))
                        {
                            Undo.RecordObject(property.serializedObject.targetObject, $"Move List '{listInterface.ListName}' up");
                            PlCollectionInterface.MoveListUp(i);
                            EditorUtility.SetDirty(property.serializedObject.targetObject);
                        }
                    }
                    
                    if(i < arraySize - 1)   // can move down
                        if (GUI.Button(leftReorderButtonRect, PLDrawerContents.MoveDownButton, PLDrawerTheme.MoveDownButton))
                        {
                            Undo.RecordObject(property.serializedObject.targetObject, $"Move List '{listInterface.ListName}' down");
                            PlCollectionInterface.MoveListDown(i);
                            EditorUtility.SetDirty(property.serializedObject.targetObject);
                        }
                }
                
                var isListEmptyCheck = PlCollectionInterface.IsListEmpty(i);
                var removeButtonOffset = isListEmptyCheck.IsEmpty ? 2f : 10f;
                var removeListButtonRect = new Rect(m_InnerRect.x + 4f, rect.yMax - removeButtonOffset, m_InnerRect.width - 12f, m_AllowRemove ? 20f : 0f);
                
                if(m_AllowRemove)
                    if (GUI.Button(removeListButtonRect, "Remove List from Collection", PLDrawerTheme.CriticalActionButton))
                    {
                        Undo.RecordObject(property.serializedObject.targetObject, "Remove List");
                        PlCollectionInterface.RemoveList(i);
                        list.serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(property.serializedObject.targetObject);
                        return;
                    }
                
                if(i < arraySize - 1) EditorGUI.DrawRect(new Rect(removeListButtonRect.x + 80f, removeListButtonRect.yMax + 15f, removeListButtonRect.width - 160f, 1f), PLDrawerTheme.CollectionSeparatorColor);
                height = removeListButtonRect.yMax + 8f;
            }
            
            m_TotalHeight = height - position.y;
        }

        protected override float OnGetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return m_TotalHeight + 24f;
        }
    }
    #pragma warning restore 0618
}