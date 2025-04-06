using System;
using StarphaseTools.Core;
using UnityEditor;
using UnityEngine;

namespace RNGNeeds.Editor
{
    internal static class TheStripe
    {
        private static readonly GUIContent m_TempContent = new GUIContent();

        private static bool WillInfoFitWithinStyle(GUIContent content, GUIStyle style, float width)
        {
            return style.CalcSize(content).x < width;
        }
        
        internal static void Draw(PropertyData propertyData, Rect stripeRect, Event currentEvent)
        {
            var cumulativePosition = 0f;
            var highestProbability = propertyData.MaxProbability();
            propertyData.MaxProbability = highestProbability;
            for (var i = 0; i < propertyData.p_ProbabilityItems.arraySize; i++)
            {
                var p_CurrentProbability = propertyData.GetBaseProbability(i);
                if (propertyData.DrawerSettings.DimColors)
                {
                    var rectColor = propertyData.ProbabilityItemColors[i];
                    rectColor.a = p_CurrentProbability.Remap(0f, highestProbability, 0.35f, 1f);
                    propertyData.ProbabilityItemColors[i] = rectColor;
                }

                var xMin = i == 0 ? stripeRect.xMin + 1f : stripeRect.xMin + stripeRect.width * cumulativePosition;

                cumulativePosition += p_CurrentProbability;
                var xMax = stripeRect.xMin + stripeRect.width * cumulativePosition;
                xMax = Mathf.Floor(xMax);
                xMin = Mathf.Floor(xMin);
                
                var newRect = new Rect(stripeRect) { xMin = xMin + 1, xMax = i < propertyData.p_ProbabilityItems.arraySize - 1 ? xMax - 1 : xMax};
                EditorGUI.DrawRect(newRect, propertyData.ProbabilityItemColors[i]);
                
                var depletableItem = propertyData.DrawerSettings.VisualizeDepletableItems && propertyData.ItemPropertyCache[i].p_DepletableItem.boolValue && propertyData.p_DepletableList.boolValue;
                
                if(depletableItem)
                {
                    var depletedPercentage = (float)propertyData.ItemPropertyCache[i].p_Units.intValue / propertyData.ItemPropertyCache[i].p_MaxUnits.intValue;
                    depletedPercentage = Mathf.Clamp01(depletedPercentage);
                    var depletedRect = new Rect(newRect) { height = newRect.height * (1 - depletedPercentage) };
                    EditorGUI.DrawRect(depletedRect, PLDrawerTheme.DepletableStripeBackgroundColor);
                }
                
                if (currentEvent.type == EventType.Repaint && propertyData.ItemPropertyCache[i].p_Locked.boolValue)
                {
                    GUI.DrawTextureWithTexCoords(newRect, PLDrawerTheme.LinkRectTexture, new Rect(newRect.x, newRect.y, newRect.width / 32, newRect.height / 32));
                }
                
                propertyData.ProbabilityRects[i] = newRect;

                if (propertyData.DrawerSettings.ShowIndex)
                {
                    m_TempContent.text = propertyData.ItemInfoCache[i].index;
                    if(WillInfoFitWithinStyle(m_TempContent, propertyData.stripeNameStyle, newRect.width)) GUI.Label(newRect, m_TempContent, propertyData.stripeIndexStyle);
                }
                if (propertyData.DrawerSettings.ShowInfo && propertyData.CanDisplayItemInfo)
                {
                    m_TempContent.text = propertyData.ItemInfoCache[i].info;
                    if(WillInfoFitWithinStyle(m_TempContent, propertyData.stripeNameStyle, newRect.width)) GUI.Label(newRect, m_TempContent, propertyData.stripeNameStyle);
                }

                if (propertyData.DrawerSettings.ShowPercentage || propertyData.DrawerSettings.ShowWeights)
                {
                    if (propertyData.DrawerSettings.ShowPercentage)
                    {
                        var stripePercentage = propertyData.ItemInfoCache[i].stripePercentage;
                        // todo: option to disable percentage caching
                        // var stripePercentage = propertyData.GetBaseProbability(i).ToString($"P{propertyData.DrawerSettings.StripePercentageDigits.ToString()}");
                        m_TempContent.text = propertyData.DrawerSettings.ShowWeights
                            ? $"{stripePercentage} ({propertyData.ItemInfoCache[i].weight})"
                            : $"{stripePercentage}";
                    }
                    else if (propertyData.DrawerSettings.ShowWeights)
                    {
                        m_TempContent.text = $"({propertyData.ItemInfoCache[i].weight})";
                    }

                    if (WillInfoFitWithinStyle(m_TempContent, propertyData.stripeNameStyle, newRect.width))
                    {
                        GUI.Label(newRect, m_TempContent, propertyData.stripePercentageStyle);
                    }
                }
            }

            // Highlight Rect
            if (propertyData.HoveredListElement >= 0)
            {
                var highlightRectWidth = propertyData.ProbabilityRects[propertyData.HoveredListElement].width * .75f;
                var highlightRect = new Rect(propertyData.ProbabilityRects[propertyData.HoveredListElement].center.x - highlightRectWidth / 2, stripeRect.y, highlightRectWidth, propertyData.DrawerSettings.StripeHeightPixels - 8f);
                EditorGUI.DrawRect(highlightRect, PLDrawerTheme.ProbabilityRectHighlightColor);
            }

            if (stripeRect.Contains(currentEvent.mousePosition) == false && propertyData.ModifierState != ModifierState.Modifying)
            {
                propertyData.HoveredProbabilityRect = -1;
                return;
            }

            // Draw Modifier Rects for hovered Probability Rect
            if (propertyData.DrawModifierRects && propertyData.HoveredProbabilityRect > -1)
            {
                // Left modifier rect
                if (propertyData.HoveredProbabilityRect > 0
                    && propertyData.ItemPropertyCache[propertyData.HoveredProbabilityRect - 1].p_Locked.boolValue == false
                    && propertyData.ItemPropertyCache[propertyData.HoveredProbabilityRect].p_Locked.boolValue == false)
                {
                    var modifierRectPosition = Mathf.Clamp(propertyData.ProbabilityRects[propertyData.HoveredProbabilityRect - 1].xMax - 4f,
                        propertyData.ProbabilityRects[propertyData.HoveredProbabilityRect - 1].xMin,
                        propertyData.ProbabilityRects[propertyData.HoveredProbabilityRect].xMax - 10f);
                    propertyData.ModifierRects[propertyData.HoveredProbabilityRect - 1] = new Rect(modifierRectPosition, stripeRect.y, 10f, propertyData.DrawerSettings.StripeHeightPixels - 8f);
                    EditorGUI.DrawRect(propertyData.ModifierRects[propertyData.HoveredProbabilityRect - 1], PLDrawerTheme.ModifierRectColor);
                    EditorGUIUtility.AddCursorRect(propertyData.ModifierRects[propertyData.HoveredProbabilityRect - 1], MouseCursor.SlideArrow);
                }

                // Right modifier rect
                if (propertyData.HoveredProbabilityRect < propertyData.p_ProbabilityItems.arraySize - 1
                    && propertyData.ItemPropertyCache[propertyData.HoveredProbabilityRect].p_Locked.boolValue == false
                    && propertyData.ItemPropertyCache[propertyData.HoveredProbabilityRect + 1].p_Locked.boolValue == false)
                {
                    var modifierRectPosition = Mathf.Clamp(propertyData.ProbabilityRects[propertyData.HoveredProbabilityRect].xMax - 4f,
                        propertyData.ProbabilityRects[propertyData.HoveredProbabilityRect].xMin,
                        propertyData.ProbabilityRects[propertyData.HoveredProbabilityRect + 1].xMax - 10f);
                    propertyData.ModifierRects[propertyData.HoveredProbabilityRect] = new Rect(modifierRectPosition, stripeRect.y, 10f, propertyData.DrawerSettings.StripeHeightPixels - 8f);
                    EditorGUI.DrawRect(propertyData.ModifierRects[propertyData.HoveredProbabilityRect], PLDrawerTheme.ModifierRectColor);
                    EditorGUIUtility.AddCursorRect(propertyData.ModifierRects[propertyData.HoveredProbabilityRect], MouseCursor.SlideArrow);
                }
            }
            else propertyData.HoveredProbabilityRect = -1;
        }

        internal static float StateLogic(PropertyData propertyData, Rect stripeRect, Event currentEvent, float grabPoint)
        {
            switch (propertyData.ModifierState)
            {
                case ModifierState.Selected:
                    if (currentEvent.button != 0) break;
                    if (RNGNStaticData.Settings.InspectorRefreshMode == InspectorRefreshMode.Optimized) SelectModifier();
                    if (propertyData.ModifierType == ModifierType.ProbabilityRect && propertyData.SelectedModifier != propertyData.HoveredProbabilityRect)
                    {
                        ResetState();
                        break;
                    }

                    switch (currentEvent.type)
                    {
                        case EventType.MouseUp:
                            ResetState();
                            break;
                        case EventType.MouseDrag:
                            propertyData.ModifierState = ModifierState.Modifying;
                            grabPoint = currentEvent.mousePosition.x;
                            break;
                    }
                    break;

                case ModifierState.Unselected:
                    if (propertyData.DrawModifierRects == false) break;
                    SelectModifier();
                    break;

                case ModifierState.Modifying:
                    var undoMessage = propertyData.ModifierType == ModifierType.ModifierRect
                        ? $"Adjust Items {propertyData.SelectedModifier} and {propertyData.SelectedModifier + 1}"
                        : $"Shift Item {propertyData.SelectedModifier}";
                    Undo.RecordObject(propertyData.p_ProbabilityListProperty.serializedObject.targetObject, undoMessage);
                    
                    if (currentEvent.type == EventType.MouseUp && currentEvent.button == 0)
                    {
                        ResetState();
                        propertyData.ProbabilityListEditorInterface.NormalizeProbabilities();
                        if (propertyData.p_WeightsPriority.boolValue)
                        {
                            propertyData.ProbabilityListEditorInterface.CalculatePercentageFromWeights();
                            propertyData.SetupPropertiesRequired = true;
                        }
                        break;
                    }

                    if (propertyData.ModifierType == ModifierType.ProbabilityRect
                        && (propertyData.ItemPropertyCache[propertyData.SelectedModifier - 1].p_Locked.boolValue
                            || propertyData.ItemPropertyCache[propertyData.SelectedModifier + 1].p_Locked.boolValue)) return grabPoint;

                    if (currentEvent.type == EventType.MouseDrag) grabPoint = propertyData.ModifyProbabilities(currentEvent, grabPoint, stripeRect);
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            void SelectModifier()
            {
                if (propertyData.HoveredProbabilityRect > 0 && propertyData.HoveredProbabilityRect < propertyData.p_ProbabilityItems.arraySize - 1)
                {
                    if (currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                    {
                        propertyData.SelectedModifier = propertyData.HoveredProbabilityRect;
                        propertyData.ModifierType = ModifierType.ProbabilityRect;
                        propertyData.ModifierState = ModifierState.Selected;
                    }
                }

                if (propertyData.ModifierRects.Count < 2) return;
                for (var i = propertyData.ModifierRects.Count - 1; i >= 0; i--)
                {
                    // If Control or Command key is held
                    if ((Application.platform == RuntimePlatform.OSXEditor && currentEvent.command) || (RNGNStaticData.WindowsOrLinuxEditor && currentEvent.control))
                    {
                        if (propertyData.ModifierRects[i].Contains(currentEvent.mousePosition) && currentEvent.type == EventType.MouseDown)
                        {
                            propertyData.EvenOutProbabilities(i);
                            break;
                        }

                        if (propertyData.ProbabilityRects[i].Contains(currentEvent.mousePosition) && currentEvent.type == EventType.ScrollWheel && currentEvent.delta.normalized.magnitude > 0)
                        {
                            Undo.RecordObject(propertyData.p_ProbabilityListProperty.serializedObject.targetObject, $"Adjust Item {i}");
                            var scroll = PLDrawerUtils.GetScrollValue(currentEvent);
                            if(scroll != 0) propertyData.ModifyProbabilityViaScrollWheel(i, currentEvent.shift ? scroll * 0.0001f : scroll * 0.001f, RNGNStaticData.Settings.InvertScrollDirection);
                            Event.current.Use();
                        }
                    }

                    if (propertyData.ModifierRects[i].Contains(currentEvent.mousePosition) && currentEvent.type == EventType.MouseDown && currentEvent.button == 0)
                    {
                        propertyData.SelectedModifier = i;
                        propertyData.ModifierType = ModifierType.ModifierRect;
                        propertyData.ModifierState = ModifierState.Selected;
                        break;
                    }

                    if (propertyData.ProbabilityRects[i].Contains(currentEvent.mousePosition)) propertyData.HoveredProbabilityRect = i;
                }
            }
            
            return grabPoint;

            void ResetState()
            {
                propertyData.SelectedModifier = -1;
                propertyData.ModifierState = ModifierState.Unselected;
                EditorUtility.SetDirty(propertyData.p_ProbabilityListProperty.serializedObject.targetObject);
            }
        }
    }
}