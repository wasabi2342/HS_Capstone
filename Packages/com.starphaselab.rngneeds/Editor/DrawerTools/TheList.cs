using StarphaseTools.Core;
using UnityEditor;
using UnityEngine;

namespace RNGNeeds.Editor
{
    internal static class TheList
    {
        private static readonly GUIContent m_TempContent = new GUIContent();
        private const float borderWidthFactor = 1f;
        private const float borderWidthOffset = 2f;
        private const float previewRectHeight = 10f;

        private static Rect m_UnitsRect;
        private static Rect m_MaxUnitsRect;
        private static Rect m_DepletableItemToggleRect;
        private static Rect m_DepletableItemSeparatorRect;

        private static Rect m_WeightRect;
        
        internal static void ShiftDataOnReorder(PropertyData propertyData, int oldIndex, int newIndex)
        {
            var movedInfoCache = propertyData.ItemInfoCache[oldIndex];
            // var movedPropertyCache = propertyData.ItemPropertyCache[oldIndex];
            var movedTestResult = -1;

            var shiftTestResults = propertyData.TestResults.indexPicks.Count > 0;
            if (shiftTestResults) movedTestResult = propertyData.TestResults.indexPicks[oldIndex];

            var step = oldIndex < newIndex ? 1 : -1;
            var start = oldIndex + step;
            var end = newIndex + step;

            for (var i = start; i != end; i += step)
            {
                var targetIndex = i - step;

                propertyData.ItemInfoCache[i].index = targetIndex.ToString();
                propertyData.ItemInfoCache[targetIndex] = propertyData.ItemInfoCache[i];

                // propertyData.ItemPropertyCache[targetIndex] = propertyData.ItemPropertyCache[i];
                if (shiftTestResults) propertyData.TestResults.indexPicks[targetIndex] = propertyData.TestResults.indexPicks[i];
            }

            movedInfoCache.index = newIndex.ToString();
            propertyData.ItemInfoCache[newIndex] = movedInfoCache;
            // propertyData.ItemPropertyCache[newIndex] = movedPropertyCache;
            
            if (shiftTestResults) propertyData.TestResults.indexPicks[newIndex] = movedTestResult;
        }

        private static void DrawUtilitySlider(PropertyData propertyData, Rect position, int index, bool drawInfluenceSlider, bool normalizeBars, bool showSpreadOnBars)
        {
            var sliderRect = position;
        
            var itemPropertyCache = propertyData.ItemPropertyCache[index];
            var range = itemPropertyCache.p_InfluenceSpread.vector2Value;
            var probability = propertyData.GetBaseProbability(index);
            var previewRectXPosition = sliderRect.x + 5f;
            var previewRectWidth = sliderRect.width - 6f;
            
            if (drawInfluenceSlider)
            {
                EditorGUI.MinMaxSlider(new Rect(sliderRect.x, sliderRect.y, sliderRect.width + 4f, sliderRect.height), ref range.x, ref range.y, 0f, 1f);
                // Probability Indicator over Slider
                EditorGUI.DrawRect(new Rect(previewRectXPosition + (sliderRect.width - 8f) * probability, position.y + 4f, 2f, 10f), itemPropertyCache.p_InvertInfluence.boolValue ? PLDrawerTheme.PreviewIndicatorInvertedColor : PLDrawerTheme.PreviewIndicatorColor);
            }
            else
            {
                var width = normalizeBars ? previewRectWidth * (probability / propertyData.MaxProbability) : previewRectWidth * probability;
                var height = showSpreadOnBars ? 6f : previewRectHeight;
                var posY = showSpreadOnBars ? 6f : 4f;
        
                var previewRect = new Rect(previewRectXPosition, position.y + posY, width, height);
        
                if (!EditorGUIUtility.isProSkin && propertyData.ShouldColorizeBars)
                {
                    var borderWidth = borderWidthFactor * EditorGUIUtility.pixelsPerPoint;
                    var previewBackgroundRect = new Rect(previewRectXPosition - borderWidth, position.y + borderWidthOffset + borderWidth,
                        showSpreadOnBars ? previewRectWidth * propertyData.ItemInfoCache[index].InfluencedProbabilityLimits.x + borderWidth * 2f : width + borderWidth * 2f, 12f);
        
                    if (Event.current.type == EventType.Repaint)
                        PLDrawerTheme.RectBorder.Draw(previewBackgroundRect, false, false, false, false);
        
                    EditorGUI.DrawRect(previewRect, PLDrawerTheme.DimBackgroundColor);
                }
        
                if (showSpreadOnBars)
                {
                    var spreadBarsColor = EditorGUIUtility.isProSkin && propertyData.ShouldColorizeBars ? propertyData.ProbabilityItemColors[index] : Color.gray;
                    var spreadMaxRect = new Rect(previewRectXPosition, position.y + 4f, previewRectWidth * propertyData.ItemInfoCache[index].InfluencedProbabilityLimits.y, 2f);
                    var spreadMinRect = new Rect(previewRectXPosition, position.y + 12f, previewRectWidth * propertyData.ItemInfoCache[index].InfluencedProbabilityLimits.x, 2f);
                    EditorGUI.DrawRect(spreadMaxRect, spreadBarsColor);
                    EditorGUI.DrawRect(spreadMinRect, spreadBarsColor);
                }
        
                if (propertyData.DrawerSettings.DimColors) EditorGUI.DrawRect(previewRect, PLDrawerTheme.DimBackgroundColor);
                EditorGUI.DrawRect(previewRect, propertyData.ShouldColorizeBars ? propertyData.ProbabilityItemColors[index] : Color.gray);
            }
        
            range.x = Mathf.Clamp(range.x, 0f, probability);
            range.y = Mathf.Clamp(range.y, probability, 1f);
            itemPropertyCache.p_InfluenceSpread.vector2Value = range;
        }
        
        internal static void DrawElement(PropertyData propertyData, Rect rect, int index, FloatLabelField floatLabelField, IntLabelField intLabelField, float itemRectAlignmentFixX)
        {
            var toggleRect = new Rect(rect.position.x, rect.position.y + 2f, 18f, EditorGUIUtility.singleLineHeight);
            var indexRect = new Rect(toggleRect.xMax, rect.position.y + 1f, 28f, EditorGUIUtility.singleLineHeight);
            var utilityRect = new Rect(indexRect.xMax, rect.position.y + 1f, rect.width * .6f * propertyData.DrawerSettings.ElementInfoSpace, EditorGUIUtility.singleLineHeight);
            var influenceProviderToggleRect = new Rect(utilityRect.xMax + 4f, rect.position.y + 2f, propertyData.DrawInfluenceProviderToggle ? 16f : 0f, 16f);
            var probRect = new Rect(influenceProviderToggleRect.xMax + 4f, rect.position.y + 1f, 26f + propertyData.DrawerSettings.ItemPercentageDigits * 9f, EditorGUIUtility.singleLineHeight);
            
            var lastX = probRect.xMax;
            
            if (propertyData.DrawerSettings.ShowWeights)
            {
                m_WeightRect.Set(probRect.xMax + 4f, rect.position.y + 1f, 38f, EditorGUIUtility.singleLineHeight);
                lastX = m_WeightRect.xMax;
            }
            
            var colorRect = new Rect(lastX + 4f, rect.position.y + 3f, 16f, 16f);
            var removeButtonRect = new Rect(rect.xMax - 25f, rect.position.y + 1f, 25f, EditorGUIUtility.singleLineHeight);
            
            lastX = colorRect.xMax;
            
            if (propertyData.p_DepletableList.boolValue)
            {
                m_DepletableItemToggleRect.Set(colorRect.xMax + 4f, rect.position.y + 2f, 18f, EditorGUIUtility.singleLineHeight);
                m_UnitsRect.Set(m_DepletableItemToggleRect.xMax, rect.position.y + 2f, 22f, EditorGUIUtility.singleLineHeight);
                m_DepletableItemSeparatorRect.Set(m_UnitsRect.xMax + 1f, rect.position.y + 2f, 0f, EditorGUIUtility.singleLineHeight);
                m_MaxUnitsRect.Set(m_DepletableItemSeparatorRect.xMax + 4f, rect.position.y + 2f, 22f, EditorGUIUtility.singleLineHeight);
                lastX = m_MaxUnitsRect.xMax;
            }
            
            var propertyWidth = Mathf.Abs(removeButtonRect.xMin - lastX - 10f - itemRectAlignmentFixX);
            var itemRect = new Rect(lastX + 4f + itemRectAlignmentFixX, rect.position.y + 2f, propertyWidth, propertyData.ItemPropertyCache[index].p_Value.isExpanded ? rect.height - 3f : EditorGUIUtility.singleLineHeight);
            
            EditorGUI.BeginChangeCheck();
            var itemEnabled = EditorGUI.Toggle(toggleRect, propertyData.ItemPropertyCache[index].p_Enabled.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                propertyData.ItemPropertyCache[index].p_Enabled.boolValue = itemEnabled;
                propertyData.SetUnitsInfoRequired = true;
            }
            
            EditorGUI.LabelField(indexRect, propertyData.ItemInfoCache[index].index, style: itemEnabled ? PLDrawerTheme.ElementEnabledTextStyle : PLDrawerTheme.ElementDisabledTextStyle);
            
            // PREVIEW SLIDER / TEST RESULTS / INFLUENCE PREVIEW
            var valueIsInfluenceProvider = propertyData.ItemInfoCache[index].probabilityItemObject.ValueIsInfluenceProvider;
            var isInfluencedItem = propertyData.ItemInfoCache[index].probabilityItemObject.IsInfluencedItem;
            
            if (propertyData.TestResults.indexPicks.Count > 0)
            {
                if (propertyData.TestResults.indexPicks.TryGetValue(index, out var _value) && _value > 0)
                {
                    var pickCountResultRect = new Rect(utilityRect.x, utilityRect.y, utilityRect.width * .4f, EditorGUIUtility.singleLineHeight);
                    var percentageResultRect = new Rect(pickCountResultRect.xMax, pickCountResultRect.y, utilityRect.width * .6f, EditorGUIUtility.singleLineHeight);
                    EditorGUI.LabelField(pickCountResultRect, $"{_value:N0} x", PLDrawerTheme.ElementEnabledPercentageStyle);

                    var color = PLDrawerTheme.NormalTextColor;
                    if (propertyData.DrawerSettings.ColorizeTestResults)
                    {
                        var percentage = ((float)_value / propertyData.TestResults.pickCount);
                        var difference = (propertyData.GetBaseProbability(index) - percentage) * propertyData.DrawerSettings.TestColorizeSensitivity * 7f;
                        color = RNGNStaticData.Settings.TestColorGradient.Evaluate(-difference + .5f);
                    }

                    PLDrawerTheme.TestResultPercentageStyle.normal.textColor = color;
                    EditorGUI.LabelField(percentageResultRect, $"{((float)_value / propertyData.TestResults.pickCount).ToString($"P{propertyData.DrawerSettings.TestPercentageDigits.ToString()}")}", PLDrawerTheme.TestResultPercentageStyle);
                }
            }
            else
            {
                if (propertyData.IsInfluencedList || isInfluencedItem)
                {
                    var spreadMinValueRect = new Rect(utilityRect.x, utilityRect.y, 40f, EditorGUIUtility.singleLineHeight);
                    utilityRect.width -= 98f;
                    utilityRect.x = spreadMinValueRect.xMax + 4f;
                    var spreadMaxValueRect = new Rect(utilityRect.xMax + 4f, utilityRect.y, 50f, EditorGUIUtility.singleLineHeight);

                    // Colorize Spread Difference
                    PLDrawerTheme.SpreadValueStyle.normal.textColor = propertyData.DrawerSettings.ColorizeSpreadDifference ? RNGNStaticData.Settings.SpreadColorGradient.Evaluate(-((propertyData.GetBaseProbability(index) - propertyData.ItemInfoCache[index].InfluencedProbabilityLimits.x) * propertyData.DrawerSettings.SpreadColorizeSensitivity * 3f) + .5f) : PLDrawerTheme.NormalTextColor;
                    EditorGUI.LabelField(spreadMinValueRect, propertyData.ItemInfoCache[index].spreadMinPercentage, PLDrawerTheme.SpreadValueStyle);
                    PLDrawerTheme.SpreadValueStyle.normal.textColor = propertyData.DrawerSettings.ColorizeSpreadDifference ? RNGNStaticData.Settings.SpreadColorGradient.Evaluate(-((propertyData.GetBaseProbability(index) - propertyData.ItemInfoCache[index].InfluencedProbabilityLimits.y) * propertyData.DrawerSettings.SpreadColorizeSensitivity * 3f) + .5f) : PLDrawerTheme.NormalTextColor;
                    EditorGUI.LabelField(spreadMaxValueRect, propertyData.ItemInfoCache[index].spreadMaxPercentage, PLDrawerTheme.SpreadValueStyle);
                }
                
                EditorGUI.BeginChangeCheck();
                DrawUtilitySlider(propertyData, utilityRect, index, isInfluencedItem, 
                    propertyData.IsInfluencedList == false && propertyData.DrawerSettings.NormalizePreviewBars, 
                    propertyData.IsInfluencedList && propertyData.DrawerSettings.ShowSpreadOnBars);
                if (EditorGUI.EndChangeCheck())
                {
                    propertyData.ItemPropertyCache[index].p_InfluenceSpread.serializedObject.ApplyModifiedProperties();
                    propertyData.SetSpreadCache();
                }
            }

            // SHOW INFLUENCE PROVIDER TOGGLE
            if (propertyData.DrawInfluenceProviderToggle)
            {
                if (GUI.Button(influenceProviderToggleRect, string.Empty, propertyData.ItemInfoCache[index].InfluenceProviderExpanded ? PLDrawerTheme.PickCountsLinkedStyle : PLDrawerTheme.PickCountsUnlinkedStyle))
                {
                    var itemInfoCache = propertyData.ItemInfoCache[index];
                    itemInfoCache.InfluenceProviderExpanded = !itemInfoCache.InfluenceProviderExpanded;
                }
            }
            
            // DIRECT PROBABILITY INPUT
            if (floatLabelField.SelectedIndex > -1) intLabelField.SelectedIndex = -1;
            var edit = floatLabelField.DrawAndHandleInput(probRect,
                index, 
                propertyData.GetBaseProbability(index),
                propertyData.ItemPropertyCache[index].p_Locked.boolValue,
                // todo: option to disable percentage caching
                // propertyData.GetBaseProbability(index).ToString($"P{propertyData.DrawerSettings.ItemPercentageDigits.ToString()}", CultureInfo.InvariantCulture),
                propertyData.ItemInfoCache[index].listElementPercentage,
                itemEnabled ? PLDrawerTheme.ElementEnabledPercentageStyle : PLDrawerTheme.ElementDisabledPercentageStyle);
            if (edit.Finished)
            {
                Undo.RecordObject(propertyData.p_ProbabilityListProperty.serializedObject.targetObject, $"Set Item {index} Probability to {edit.Value}");
                propertyData.ProbabilityListEditorInterface.SetItemBaseProbability(index, edit.Value);
                propertyData.SetupPropertiesRequired = true;
            }

            if (floatLabelField.SelectedIndex == index + 1) // Tab pressed
            {
                var nextIndex = index + 1;
                while (nextIndex < propertyData.ItemPropertyCache.Count && propertyData.ItemPropertyCache[nextIndex].p_Locked.boolValue) nextIndex++;
                floatLabelField.SelectedIndex = nextIndex < propertyData.ItemPropertyCache.Count ? nextIndex : -1;
            }
            
            // DIRECT WEIGHT INPUT
            if (propertyData.DrawerSettings.ShowWeights)
            {
                if (intLabelField.SelectedIndex > -1) floatLabelField.SelectedIndex = -1;
                var weightEdit = intLabelField.DrawAndHandleInput(m_WeightRect,
                    index,
                    propertyData.ItemPropertyCache[index].p_Weight.intValue,
                    propertyData.ItemPropertyCache[index].p_Locked.boolValue,
                    itemEnabled ? PLDrawerTheme.ElementEnabledPercentageStyle : PLDrawerTheme.ElementDisabledPercentageStyle);
                if (weightEdit.Finished)
                {
                    Undo.RecordObject(propertyData.p_ProbabilityListProperty.serializedObject.targetObject, $"Set Item {index} Weight to {weightEdit.Value}");
                    propertyData.ProbabilityListEditorInterface.SetItemWeight(index, weightEdit.Value);
                    propertyData.SetupPropertiesRequired = true;
                }

                if (intLabelField.SelectedIndex == index + 1) // Tab pressed
                {
                    var nextIndex = index + 1;
                    while (nextIndex < propertyData.ItemPropertyCache.Count && propertyData.ItemPropertyCache[nextIndex].p_Locked.boolValue) nextIndex++;
                    intLabelField.SelectedIndex = nextIndex < propertyData.ItemPropertyCache.Count ? nextIndex : -1;
                }
            }

            EditorGUI.DrawRect(colorRect, PLDrawerTheme.DimBackgroundColor);
            EditorGUI.DrawRect(colorRect, propertyData.ProbabilityItemColors[index]);

            if (Event.current.type == EventType.Repaint) PLDrawerTheme.RectBorder.Draw(colorRect, false, false, false, false);

            if (GUI.Button(colorRect, string.Empty, propertyData.ItemPropertyCache[index].p_Locked.boolValue ? PLDrawerTheme.LockIconEnabled : PLDrawerTheme.LockIconHint)) propertyData.ToggleItemLocked(index);
            
            // Depletable Units
            if (propertyData.p_DepletableList.boolValue)
            {
                EditorGUI.BeginChangeCheck();
                var itemDepletable = EditorGUI.Toggle(m_DepletableItemToggleRect, propertyData.ItemPropertyCache[index].p_DepletableItem.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    propertyData.ItemPropertyCache[index].p_DepletableItem.boolValue = itemDepletable;
                    propertyData.SetUnitsInfoRequired = true;
                }
                
                EditorGUI.BeginDisabledGroup(itemDepletable == false);
                    EditorGUI.LabelField(m_UnitsRect, propertyData.ItemPropertyCache[index].p_Units.intValue.ToString(), style: itemEnabled ? PLDrawerTheme.UnitsEnabledTextStyle : PLDrawerTheme.UnitsDisabledTextStyle);
                    EditorGUI.LabelField(m_DepletableItemSeparatorRect, "/", style: itemEnabled ? PLDrawerTheme.UnitsEnabledTextStyle : PLDrawerTheme.UnitsDisabledTextStyle);
                    EditorGUI.LabelField(m_MaxUnitsRect, propertyData.ItemPropertyCache[index].p_MaxUnits.intValue.ToString(), style: itemEnabled ? PLDrawerTheme.UnitsEnabledTextStyle : PLDrawerTheme.UnitsDisabledTextStyle);
                EditorGUI.EndDisabledGroup();
                
                if (Event.current.type != EventType.Repaint)
                {
                    var evt = Event.current;
                    if (PLDrawerUtils.ShouldAdjustUnits(m_UnitsRect, evt, out var unitsScrollWheel))
                    {
                        var units = propertyData.ItemPropertyCache[index].p_Units.intValue;
                        var maxUnits = propertyData.ItemPropertyCache[index].p_MaxUnits.intValue;
                        var adjustment = PLDrawerUtils.GetUnitsAdjustment(evt, units, maxUnits, unitsScrollWheel, out var unitsClamp);
                        if (adjustment != 0)
                        {
                            units += adjustment;
                            units = Mathf.Clamp(units, 0, unitsClamp ? int.MaxValue : maxUnits);
                            propertyData.ItemPropertyCache[index].p_Units.intValue = units;
                            propertyData.SetUnitsInfoRequired = true;
                        }
                        
                        evt.Use();
                    }

                    if (PLDrawerUtils.ShouldAdjustUnits(m_MaxUnitsRect, evt, out var maxUnitsScrollWheel))
                    {
                        var units = propertyData.ItemPropertyCache[index].p_Units.intValue;
                        var maxUnits = propertyData.ItemPropertyCache[index].p_MaxUnits.intValue;
                        var adjustment = PLDrawerUtils.GetMaxUnitsAdjustment(evt, maxUnits, maxUnitsScrollWheel, out var maxUnitsClamp);
                        if (adjustment != 0)
                        {
                            maxUnits += adjustment;
                            maxUnits = Mathf.Clamp(maxUnits, 0, int.MaxValue);
                            propertyData.ItemPropertyCache[index].p_MaxUnits.intValue = maxUnits;
                            if (maxUnitsClamp == false) propertyData.ItemPropertyCache[index].p_Units.intValue = Mathf.Clamp(units, 0, maxUnits);
                            propertyData.SetUnitsInfoRequired = true;
                        }

                        evt.Use();
                    }
                }
            }

            if (propertyData.ValueIsGenericType)
            {
                if (propertyData.ItemInfoCache[index].ProviderType.HasAny(ItemProviderType.InfoProvider) && propertyData.ItemInfoCache[index].valueObject != null)
                {
                    m_TempContent.text = propertyData.ItemInfoCache[index].info;
                }
                else m_TempContent.text = propertyData.ItemPropertyCache[index].p_Value.type;
                EditorGUIUtility.labelWidth = EditorGUIUtility.currentViewWidth * .3f * (1.001f - propertyData.DrawerSettings.ElementInfoSpace);
                propertyData.ItemPropertyCache[index].p_Value.isExpanded = propertyData.ItemInfoCache[index].isExpandedProperty;
            }
            else
            {
                m_TempContent.text = string.Empty;
                EditorGUIUtility.labelWidth = 0;
            }
            
            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(itemRect, propertyData.ItemPropertyCache[index].p_Value, m_TempContent, propertyData.ValueIsGenericType);
            if (EditorGUI.EndChangeCheck())
            {
                propertyData.ValuesChangedFor = index;
                propertyData.p_ProbabilityItems.GetArrayElementAtIndex(index).serializedObject.ApplyModifiedProperties();
                propertyData.SetupPropertiesRequired = true;
                propertyData.ItemPropertyCache[index].p_Value.serializedObject.ApplyModifiedProperties();
                propertyData.ItemInfoCache[index].probabilityItemObject.UpdateProperties();
            }

            if (propertyData.ValueIsGenericType) propertyData.ItemInfoCache[index].isExpandedProperty = propertyData.ItemPropertyCache[index].p_Value.isExpanded;

            // Item Remove Button
            var onlyUnlockedItemWithProbability = propertyData.IndexOfUnremovableItem == index;
            if (GUI.Button(removeButtonRect, "X", onlyUnlockedItemWithProbability || propertyData.UnlockedItems == 0 ? PLDrawerTheme.OptionButtonDisabled : PLDrawerTheme.OptionButtonOn))
            {
                if (propertyData.UnlockedItems != 0) propertyData.RemoveItem(index);
                GUIUtility.ExitGUI();
            }
            
            // INFLUENCE PROVIDER FIELD
            if (propertyData.ItemInfoCache[index].InfluenceProviderExpanded && propertyData.DrawInfluenceProviderToggle)
            {
                var influenceProviderRect = new Rect(indexRect.xMax + 5f, utilityRect.yMax + 4f, rect.width * .6f * propertyData.DrawerSettings.ElementInfoSpace, EditorGUIUtility.singleLineHeight);
                if (valueIsInfluenceProvider)
                {
                    EditorGUI.LabelField(influenceProviderRect, "Value is Influence Provider.", PLDrawerTheme.NormalTextStyle);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    propertyData.ItemPropertyCache[index].p_InfluenceProvider.objectReferenceValue = EditorGUI.ObjectField(influenceProviderRect,
                        propertyData.ItemPropertyCache[index].p_InfluenceProvider.objectReferenceValue, typeof(IProbabilityInfluenceProvider), true);
                    if (EditorGUI.EndChangeCheck())
                    {
                        propertyData.p_ProbabilityItems.GetArrayElementAtIndex(index).serializedObject.ApplyModifiedProperties();
                        propertyData.ItemInfoCache[index].probabilityItemObject.UpdateProperties();
                        propertyData.SetupPropertiesRequired = true;
                    }
                }
                
                // Invert Influence Control
                var invertInfluenceRect = new Rect(influenceProviderRect.xMax + 5f, utilityRect.yMax + 4f, rect.width * .4f * propertyData.DrawerSettings.ElementInfoSpace, EditorGUIUtility.singleLineHeight);
                EditorGUI.BeginChangeCheck();
                var invertInfluence = EditorGUI.ToggleLeft(invertInfluenceRect, PLDrawerContents.InfluenceInvertToggle, propertyData.ItemPropertyCache[index].p_InvertInfluence.boolValue);
                if (EditorGUI.EndChangeCheck())
                {
                    propertyData.ItemPropertyCache[index].p_InvertInfluence.boolValue = invertInfluence;
                    propertyData.p_ProbabilityItems.GetArrayElementAtIndex(index).serializedObject.ApplyModifiedProperties();
                    propertyData.SetupPropertiesRequired = true;
                }
                
                if(isInfluencedItem)
                {
                    var influenceInfo = "-> " + propertyData.ItemInfoCache[index].probabilityItemObject.InfluenceProvider?.InfluenceInfo;
                    m_TempContent.text = influenceInfo;
                    propertyData.ItemInfoCache[index].influenceInfoHeight = PLDrawerTheme.LabelInfoStyle.CalcHeight(m_TempContent, influenceProviderRect.width);
                    var influenceInfoRect = new Rect(influenceProviderRect.x + 4f, influenceProviderRect.yMax, influenceProviderRect.width, propertyData.ItemInfoCache[index].influenceInfoHeight);
                    EditorGUI.LabelField(influenceInfoRect, influenceInfo, PLDrawerTheme.LabelInfoStyle);
                }
                else
                {
                    propertyData.ItemInfoCache[index].influenceInfoHeight = 0f;
                    m_TempContent.text = string.Empty;
                }
            }
        }
    }
}