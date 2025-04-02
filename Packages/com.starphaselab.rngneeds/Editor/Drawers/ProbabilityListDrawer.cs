using System;
using System.Collections.Generic;
using StarphaseTools.Core;
using StarphaseTools.Core.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RNGNeeds.Editor
{
    #pragma warning disable 0618
    [CustomPropertyDrawer(typeof(ProbabilityList<>))]
    internal class ProbabilityListDrawer : LabDrawerBase
    {
        protected override bool m_KeepAliveOnLockedInspectorSelectionChange => true;
        private static RNGNeedsSettings m_Settings;
        private Event m_CurrentEvent;
        private float m_GrabPoint;
        private float itemRectAlignmentFixX;

        private PropertyData cpd;   // Current Property Data
        private readonly Dictionary<string, PropertyData> propertyPathDictionary = new Dictionary<string, PropertyData>();

        // RECTS (in order of appearance)
        private Rect m_CogRect;
        private Rect m_ShowListEntriesButtonRect;
        private Rect m_SeparatorARect;
        
        // Swatch Rects
        private Rect m_DisplayIndexButtonRect;
        private Rect m_DisplayInfoButtonRect;
        private Rect m_DisplayPercentageButtonRect;
        private Rect m_SeparatorBRect;
        private Rect m_DimColorsButtonRect;
        private Rect m_ColorizeBarsButtonRect;
        private Rect m_NormalizeBarsButtonRect;
        private Rect m_ShowWeightsButtonRect;

        private Rect m_DepletableStripeButtonRect;
        
        // Section Buttons Rects
        private Rect m_SeparatorCRect;
        private Rect m_ThemeSectionButtonRect;
        private Rect m_PickSectionButtonRect;
        private Rect m_AddButtonRect;
        
        private Rect m_StripeRect;
        private Rect m_StripeBorderRect;
        
        // Theme Section Rects
        private Rect m_PaletteButtonRect;
        private Rect m_PaletteDropdownRect;
        private Rect m_ReverseColorOrderButtonRect;
        private Rect m_MonochromeColorPickerRect;
        
        // Stripe Section Rects
        private Rect m_StripeHeightSliderRect;
        
        // Pick Section Rects
        private Rect m_TestRollButtonRect;
        private Rect m_ClearResultsButtonRect;
        private Rect m_DepletableListButtonRect;
        private Rect m_DepletableListHelpButtonRect;
        private Rect m_DepletableListActionButtonRect;
        private Rect m_DepletableListBoolValueButtonRect;
        private Rect m_DepletableListUnitsValueButtonRect;
        private Rect m_DepletableListExecuteActionButtonRect;

        private readonly GUIContent m_TempContent = new GUIContent();

        protected override void OnEnable()
        {
            Undo.undoRedoPerformed += UndoRedo;
        }

        protected override void OnDisable()
        {
            Undo.undoRedoPerformed -= UndoRedo;
        }
        
        private void UndoRedo()
        {
            // RLogger.Log($"{baseID} UndoRedo", LogMessageType.Debug);
            foreach (var propertyData in propertyPathDictionary) propertyData.Value.SetupPropertiesRequired = true;
        }
        
        protected override bool Initialize(SerializedProperty property)
        {
            // RLogger.Log($"{baseID} Initialize {property.propertyPath}", LogMessageType.Debug);
            m_Settings = RNGNStaticData.Settings;

            if (m_Settings == null)
            {
                m_Settings = RNGNeedsSettings.instance;
                RNGNStaticData.SetSettings();
                return false;
            }
            
            cpd = SwitchProperty(property);

            return true;
        }
        
        private PropertyData SwitchProperty(SerializedProperty property)
        {
            var id = property.FindPropertyRelative("m_PLID").stringValue;
            if (propertyPathDictionary.TryGetValue(id, out var propertyData))
            {
                if (propertyData.PropertyPath == property.propertyPath) return propertyData;

                if (propertyData.IsArrayAndNotCollection && propertyData.p_ID.arrayElementType == "char")
                {
                    RLogger.Log("Nesting ProbabilityList in Array or List is not fully supported in Inspector. Please use 'PLCollection' class instead.", LogMessageType.Hint);
                    GetOrCreateDrawerID(propertyData, string.Empty);
                }
                
                // IF PLCollection
                propertyPathDictionary.Clear();
                return AddPropertyData(property);
            }
            
            return AddPropertyData(property);
        }

        private static void GetOrCreateDrawerID(PropertyData propertyData, string drawerID = "", bool ignoreSavedSettings = false)
        {
            if (string.IsNullOrEmpty(drawerID))
            {
                propertyData.p_ID.stringValue = Guid.NewGuid().ToString();
                propertyData.p_ID.serializedObject.ApplyModifiedProperties();
                RLogger.Log($"Created New DrawerID {propertyData.p_ID.stringValue}", LogMessageType.Debug);
            } else RLogger.Log($"Initialized DrawerID {propertyData.p_ID.stringValue}", LogMessageType.Debug);
            
            propertyData.DrawerSettings = RNGNStaticData.Settings.GetOrCreateDrawerSettings(propertyData.p_ID.stringValue, ignoreSavedSettings);
        }
        
        private PropertyData AddPropertyData(SerializedProperty property)
        {
            var propertyPath = property.propertyPath;
            var lp = property.FindPropertyRelative("m_ProbabilityItems");
            
            var newPropertyData = new PropertyData();
            
            newPropertyData.p_ID = property.FindPropertyRelative("m_PLID");
            newPropertyData.IsArray = CacheTools.IsArray(property.propertyPath);
            newPropertyData.IsArrayAndNotCollection = CacheTools.IsArrayAndNotCollection(property.propertyPath);
            GetOrCreateDrawerID(newPropertyData, newPropertyData.p_ID.stringValue);
            
            propertyPathDictionary[newPropertyData.p_ID.stringValue] = newPropertyData;
            
            newPropertyData.PropertyPath = propertyPath;
            newPropertyData.p_ProbabilityListProperty = property;
            
            newPropertyData.NameOfProperty = CacheTools.GetNameOfProperty(property, newPropertyData.IsArray);
            newPropertyData.ItemInfoCache = new Dictionary<int, ItemInfoCache>();
            newPropertyData.ProbabilityListEditorInterface = (IProbabilityListEditorActions)property.GetTargetObjectOfProperty();

            newPropertyData.p_ProbabilityItems = lp;
            newPropertyData.ItemPropertyCache = new List<ItemPropertyCache>();
            newPropertyData.FloatLabelField = new FloatLabelField();
            newPropertyData.IntLabelField = new IntLabelField();
            newPropertyData.TestResults = new TestResults();

            newPropertyData.p_PickCountMin = property.FindPropertyRelative("m_PickCountMin");
            newPropertyData.p_PickCountMax = property.FindPropertyRelative("m_PickCountMax");
            newPropertyData.p_PickCountCurve = property.FindPropertyRelative("m_PickCountCurve");
            newPropertyData.p_PreventRepeat = property.FindPropertyRelative("m_PreventRepeat");
            newPropertyData.p_ShuffleIterations = property.FindPropertyRelative("m_ShuffleIterations");
            newPropertyData.p_LinkPickCounts = property.FindPropertyRelative("m_LinkPickCounts");
            newPropertyData.p_MaintainPickCountIfDisabled = property.FindPropertyRelative("m_MaintainPickCountIfDisabled");
            newPropertyData.p_Seed = property.FindPropertyRelative("m_Seed");
            newPropertyData.p_KeepSeed = property.FindPropertyRelative("m_KeepSeed");
            
            newPropertyData.p_DepletableList = property.FindPropertyRelative("m_DepletableList");
            newPropertyData.DepletableListAction = RNGNStaticData.DepletableListActions[(int)newPropertyData.DrawerSettings.DepletableListAction];
            
            newPropertyData.p_WeightsPriority = property.FindPropertyRelative("m_WeightsPriority");
            newPropertyData.p_BaseWeight = property.FindPropertyRelative("m_BaseWeight");

            newPropertyData.ModifierRects = new List<Rect>();
            newPropertyData.ProbabilityRects = new List<Rect>();
            newPropertyData.ProbabilityItemColors = new List<Color>();

            newPropertyData.ModifierState = ModifierState.Unselected;
            newPropertyData.SelectedModifier = -1;
            newPropertyData.StripeColors = RNGNStaticData.Settings.GetColorsFromPalette(newPropertyData.DrawerSettings.PalettePath);
            
            newPropertyData.CanDisplayItemInfo = true;
            
            if (newPropertyData.ValueCannotBeObtained) return newPropertyData;
            
            var itemType = newPropertyData.ProbabilityListEditorInterface.ItemType;
            if (itemType == typeof(Color)
                || itemType == typeof(AnimationCurve)
                || itemType == typeof(Gradient)
                #if !UNITY_2021_1_OR_NEWER
                || itemType == typeof(Hash128)
                #endif
               )
            {
                newPropertyData.CanDisplayItemInfo = false;
                newPropertyData.DrawerSettings.ShowInfo = false;
            }
            
            newPropertyData.SetHotStyles();
            newPropertyData.SetupProperties(this);
            
            if (newPropertyData.ValueIsGenericType)
            {
                #if UNITY_2022_1_OR_NEWER
                itemRectAlignmentFixX = -3f;
                #else
                itemRectAlignmentFixX = 10f;
                #endif
            }

            return newPropertyData;
        }

        #region ListCallbacks
        
        internal void OnReorderCallbackWithDetails(ReorderableList list, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex) return;
            TheList.ShiftDataOnReorder(cpd, oldIndex, newIndex);
            cpd.FloatLabelField.SelectedIndex = -1;
            cpd.IntLabelField.SelectedIndex = -1;
            cpd.SetupPropertiesRequired = true;
        }
        
        internal void DrawElementCallback(Rect rect, int index, bool isActive, bool isFocused)
        {
            if (m_CurrentEvent.type == EventType.Layout || m_CurrentEvent.type == EventType.Used) return;
            if (index > cpd.p_ProbabilityItems.arraySize - 1) return;
            TheList.DrawElement(cpd, rect, index, cpd.FloatLabelField, cpd.IntLabelField, itemRectAlignmentFixX);
        }
        
        internal void DrawElementBackgroundCallback(Rect rect, int index, bool active, bool focused)
        {
            if (cpd.ShouldHighlightListElements && rect.Contains(m_CurrentEvent.mousePosition)) cpd.HoveredListElement = index;
            if (index >= 0 && (index == cpd.HoveredProbabilityRect || index == cpd.HoveredListElement))
            {
                var elementBackgroundColor = cpd.ProbabilityItemColors[index];
                elementBackgroundColor.a = cpd.IsItemEnabled(index) ? .35f : .15f;
                EditorGUI.DrawRect(rect, elementBackgroundColor);
            }
            else if (index % 2 == 0) EditorGUI.DrawRect(rect, PLDrawerTheme.ElementAltColor);
        }
        
        #endregion
        
        protected override float OnGetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var data = SwitchProperty(property);
            return (data.p_ProbabilityItems.arraySize > 0 ? data.ReorderableList.GetHeight() + data.DrawerSettings.StripeHeightPixels : 8f) + data.OptionsRect.height + data.SubOptionsRect.height;
        }
        
        #region Draw

        protected override void BaseDraw(Rect position, SerializedProperty property, GUIContent label)
        {
            cpd = SwitchProperty(property);
            m_CurrentEvent = Event.current;

            if (cpd.SetupPropertiesRequired)
            {
                cpd.SetupProperties(this);
                cpd.SetupPropertiesRequired = false;
            }

            if (cpd.ValuesChangedFor >= 0)
            {
                cpd.IsInfluencedList = cpd.ProbabilityListEditorInterface.IsListInfluenced;
                cpd.SetProviderTypeFor(cpd.ValuesChangedFor);
                cpd.SetObjectFor(cpd.ValuesChangedFor);
                cpd.SetInfoCacheFor(cpd.ValuesChangedFor);
                cpd.SetupColors();
                cpd.ValuesChangedFor = -1;
            }
            
            if(cpd.SetUnitsInfoRequired)
            {
                cpd.SetUnitsInfo();
                cpd.SetUnitsInfoRequired = false;
            }
            
            if (m_CurrentEvent.type == EventType.Layout) return;

            var focusedWindow = EditorWindow.focusedWindow != null;

            m_TempContent.text = "";
            m_TempContent.tooltip = "";
            
            cpd.ShouldHighlightListElements = m_CurrentEvent.type != EventType.MouseDrag && focusedWindow && EditorWindow.focusedWindow.ToString().Contains("InspectorWindow");

            var themeSectionButtonContent = PLDrawerUtils.GetThemeSectionButtonContent(RNGNStaticData.Settings.DrawerOptionButtons, cpd.DrawerSettings);
            var pickSectionButtonContent = PLDrawerUtils.GetPickSectionButtonContent(RNGNStaticData.Settings.DrawerOptionButtons, cpd);

            var showAdvancedOptions = m_Settings.DrawerOptionsLevel == DrawerOptionsLevel.Advanced;
            var optionsYPosition = position.y + 8f;
            cpd.OptionsRect.Set(position.x, position.y + 2f, position.width, 32f);
            
            m_CogRect.Set(cpd.OptionsRect.x + 6f, cpd.OptionsRect.y + 7f, 18f, 18f);
            m_ShowListEntriesButtonRect.Set(m_CogRect.xMax + 2f, optionsYPosition, 20f, 20f);
            m_SeparatorARect.Set(m_ShowListEntriesButtonRect.xMax + 1f, optionsYPosition, 14f, 20f);
            m_DisplayIndexButtonRect.Set(m_SeparatorARect.xMax, optionsYPosition, 20f, 20f);
            m_DisplayInfoButtonRect.Set(m_DisplayIndexButtonRect.xMax + 6f, optionsYPosition, 20f, 20f);
            m_DisplayPercentageButtonRect.Set(m_DisplayInfoButtonRect.xMax + 6f, optionsYPosition, 20f, 20f);
            m_SeparatorBRect.Set(m_DisplayPercentageButtonRect.xMax, optionsYPosition, 14f, 20f);
            m_DimColorsButtonRect.Set(m_SeparatorBRect.xMax, optionsYPosition, 20f, 20f);
            m_ColorizeBarsButtonRect.Set(m_DimColorsButtonRect.xMax + 6f, optionsYPosition, 20f, 20f);
            m_NormalizeBarsButtonRect.Set(m_ColorizeBarsButtonRect.xMax + 6f, optionsYPosition, 20f, 20f);
            
            m_ShowWeightsButtonRect.Set(m_NormalizeBarsButtonRect.xMax + 6f, optionsYPosition, 20f, 20f);
            
            if(cpd.p_DepletableList.boolValue)
            {
                m_DepletableStripeButtonRect.Set(m_ShowWeightsButtonRect.xMax + 6f, optionsYPosition, 20f, 20f);
                m_SeparatorCRect.Set(m_DepletableStripeButtonRect.xMax, optionsYPosition, 14f, 20f);
            } else m_SeparatorCRect.Set(m_ShowWeightsButtonRect.xMax, optionsYPosition, 14f, 20f);
            
            m_AddButtonRect.Set(cpd.OptionsRect.xMax - 49f, cpd.OptionsRect.y + 4f, 44f, 24f);
            cpd.SubOptionsRect.Set(cpd.OptionsRect.x, cpd.OptionsRect.yMax, position.width, 0f);
            cpd.SubOptionsRect.height = PLDrawerUtils.GetSubOptionsRectHeight(cpd.DrawerSettings, showAdvancedOptions);

            var subOptionsYPosition = cpd.SubOptionsRect.y + 6f;
            var stripeRectPosition = cpd.SubOptionsRect.yMax;
            
            m_StripeBorderRect.Set(position.x, stripeRectPosition + 2f, position.width, cpd.DrawerSettings.StripeHeightPixels);
            m_StripeRect.Set(m_StripeBorderRect.x + 2f, m_StripeBorderRect.y + 4f, position.width - 4f, cpd.DrawerSettings.StripeHeightPixels - 8f);

            // Sections
            m_ThemeSectionButtonRect.Set(m_SeparatorCRect.xMax, m_DisplayIndexButtonRect.y, PLDrawerTheme.SectionButtonOn.CalcSize(themeSectionButtonContent).x + 12f, 20f);
            m_PickSectionButtonRect.Set(m_ThemeSectionButtonRect.xMax + 6f, m_DisplayIndexButtonRect.y, PLDrawerTheme.SectionButtonOn.CalcSize(pickSectionButtonContent).x + 18f, 20f);
            
            cpd.DrawModifierRects = m_StripeRect.Contains(m_CurrentEvent.mousePosition);

            var indentLevel = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.BeginProperty(position, label, property);
            {
                if (m_CurrentEvent.type == EventType.Repaint)
                {
                    PLDrawerTheme.OptionRectImage.Draw(cpd.OptionsRect, false, false, false, false);
                    PLDrawerTheme.Separator.Draw(m_SeparatorARect, false, false, false, false);
                    PLDrawerTheme.Separator.Draw(m_SeparatorBRect, false, false, false, false);
                    PLDrawerTheme.Separator.Draw(m_SeparatorCRect, false, false, false, false);
                    
                    if (cpd.DrawerSettings.DrawerOptionSection != DrawerOptionSection.None)
                    {
                        cpd.MenuArrowRect.Set(cpd.SubOptionsRect.x + 4f, subOptionsYPosition, 20f, 20f);
                        PLDrawerTheme.OptionRectImage.Draw(cpd.SubOptionsRect, false, false, false, false);
                        PLDrawerTheme.MenuArrow.Draw(cpd.MenuArrowRect, false, false, false, false);
                    }
                }
                
                if (GUI.Button(m_CogRect, string.Empty, cpd.DrawerSettings.DrawerOptionSection == DrawerOptionSection.Cog ? PLDrawerTheme.CogIconOn : PLDrawerTheme.CogIconOff))
                {
                    cpd.DrawerSettings.DrawerOptionSection = cpd.DrawerSettings.DrawerOptionSection != DrawerOptionSection.Cog ? DrawerOptionSection.Cog : DrawerOptionSection.None;
                }
                
                EditorGUI.LabelField(cpd.OptionsRect, cpd.NameOfProperty, PLDrawerTheme.PropertyNameStyle);

                m_TempContent.text = cpd.DrawerSettings.HideListEntries ? cpd.p_ProbabilityItems.arraySize.ToString() : PLDrawerContents.ShowListEntriesButton.text;
                m_TempContent.tooltip = PLDrawerContents.ShowListEntriesButton.tooltip;
                // m_ShowListEntriesButtonRect.width = PLDrawerTheme.OptionButtonOn.CalcSize(m_TempContent).x + 10f;
                if (GUI.Button(m_ShowListEntriesButtonRect, m_TempContent, cpd.DrawerSettings.HideListEntries ? PLDrawerTheme.OptionButtonOn : PLDrawerTheme.HideEntriesButtonOff))
                {
                    cpd.DrawerSettings.HideListEntries = !cpd.DrawerSettings.HideListEntries;
                    cpd.SetupPropertiesRequired = true;
                }
                
                if (GUI.Button(m_DisplayIndexButtonRect, PLDrawerContents.ShowItemIndexButton, cpd.DrawerSettings.ShowIndex ? PLDrawerTheme.ShowIndexButtonOn : PLDrawerTheme.ShowIndexButtonOff))
                {
                    cpd.DrawerSettings.ShowIndex = !cpd.DrawerSettings.ShowIndex;
                    cpd.SetHotStyles();
                }

                if (cpd.CanDisplayItemInfo)
                {
                    if (GUI.Button(m_DisplayInfoButtonRect, PLDrawerContents.ShowItemInfoButton, cpd.DrawerSettings.ShowInfo ? PLDrawerTheme.ShowInfoButtonOn : PLDrawerTheme.ShowInfoButtonOff))
                    {
                        cpd.DrawerSettings.ShowInfo = !cpd.DrawerSettings.ShowInfo;
                        cpd.SetHotStyles();
                    }
                } else GUI.Button(m_DisplayInfoButtonRect, PLDrawerContents.CannotShowItemInfoButton, PLDrawerTheme.OptionButtonDisabled);

                if (GUI.Button(m_DisplayPercentageButtonRect, PLDrawerContents.ShowItemPercentageButton, cpd.DrawerSettings.ShowPercentage ? PLDrawerTheme.ShowPercentageButtonOn : PLDrawerTheme.ShowPercentageButtonOff))
                {
                    cpd.DrawerSettings.ShowPercentage = !cpd.DrawerSettings.ShowPercentage;
                    cpd.SetHotStyles();
                }

                if (GUI.Button(m_DimColorsButtonRect, PLDrawerContents.DimColorsButton, cpd.DrawerSettings.DimColors ? PLDrawerTheme.DimColorsButtonOn : PLDrawerTheme.DimColorsButtonOff))
                {
                    cpd.DrawerSettings.DimColors = !cpd.DrawerSettings.DimColors;
                    cpd.SetupColors();
                }
                
                if (GUI.Button(m_ColorizeBarsButtonRect, PLDrawerContents.ColorizeBarsButton, cpd.DrawerSettings.ColorizePreviewBars ? PLDrawerTheme.ColorizeBarsButtonOn : PLDrawerTheme.ColorizeBarsButtonOff))
                {
                    cpd.DrawerSettings.ColorizePreviewBars = !cpd.DrawerSettings.ColorizePreviewBars;
                }

                if (GUI.Button(m_NormalizeBarsButtonRect, PLDrawerContents.NormalizeBarsButton, cpd.DrawerSettings.NormalizePreviewBars ? PLDrawerTheme.NormalizeBarsButtonOn : PLDrawerTheme.NormalizeBarsButtonOff))
                {
                    cpd.DrawerSettings.NormalizePreviewBars = !cpd.DrawerSettings.NormalizePreviewBars;
                }
                
                if (GUI.Button(m_ShowWeightsButtonRect, PLDrawerContents.ShowWeightsButton, cpd.DrawerSettings.ShowWeights ? 
                        cpd.p_WeightsPriority.boolValue ? PLDrawerTheme.WeightsPriorityButtonOn : PLDrawerTheme.ShowWeightsButtonOn : 
                        cpd.p_WeightsPriority.boolValue ? PLDrawerTheme.WeightsPriorityButtonOff : PLDrawerTheme.ShowWeightsButtonOff))
                {
                    cpd.DrawerSettings.ShowWeights = !cpd.DrawerSettings.ShowWeights;
                }
                
                if(cpd.p_DepletableList.boolValue)
                {
                    if (GUI.Button(m_DepletableStripeButtonRect, PLDrawerContents.DepletableStripeButton, cpd.DrawerSettings.VisualizeDepletableItems ? PLDrawerTheme.DepletableStripeButtonOn : PLDrawerTheme.DepletableStripeButtonOff))
                    {
                        cpd.DrawerSettings.VisualizeDepletableItems = !cpd.DrawerSettings.VisualizeDepletableItems;
                    }
                }

                // Section Buttons
                if (GUI.Button(m_ThemeSectionButtonRect, themeSectionButtonContent, cpd.DrawerSettings.DrawerOptionSection == DrawerOptionSection.Theme ? PLDrawerTheme.SectionButtonOn : PLDrawerTheme.SectionButtonOff))
                {
                    cpd.DrawerSettings.DrawerOptionSection = cpd.DrawerSettings.DrawerOptionSection != DrawerOptionSection.Theme ? DrawerOptionSection.Theme : DrawerOptionSection.None;
                }
                
                if (GUI.Button(m_PickSectionButtonRect, pickSectionButtonContent, cpd.DrawerSettings.DrawerOptionSection == DrawerOptionSection.Picks ? PLDrawerTheme.SectionButtonOn : PLDrawerTheme.SectionButtonOff))
                {
                    cpd.DrawerSettings.DrawerOptionSection = cpd.DrawerSettings.DrawerOptionSection != DrawerOptionSection.Picks ? DrawerOptionSection.Picks : DrawerOptionSection.None;
                }
                
                if (RNGNeedsSettings.DevMode)
                {
                    if(GUI.Button(new Rect(m_PickSectionButtonRect.xMax + 6f, m_DisplayIndexButtonRect.y, 30f, 20f), "M", PLDrawerTheme.CriticalActionButtonHover))
                    {
                        JsonUtils.SaveObjectToFile(cpd.DrawerSettings, $"{Application.dataPath}/plds_{cpd.DrawerSettings.DrawerID}.json");
                        AssetDatabase.Refresh();
                    }
                }
                
                switch (cpd.DrawerSettings.DrawerOptionSection)
                {
                    case DrawerOptionSection.None:
                        break;
                    case DrawerOptionSection.Cog:
                        DrawAdvancedOptions(showAdvancedOptions);
                        break;
                    case DrawerOptionSection.Stripe: // Deprecated in v0.9.7
                    case DrawerOptionSection.Theme: // Draw Theme options
                        m_TempContent.text = cpd.DrawerSettings.Monochrome ? "Monochrome" : cpd.DrawerSettings.PalettePath;
                        m_TempContent.tooltip = "";
                        var paletteDropdownWidth = PLDrawerTheme.OptionButtonOn.CalcSize(m_TempContent);
                        m_PaletteButtonRect.Set(cpd.MenuArrowRect.xMax, subOptionsYPosition + 1f, paletteDropdownWidth.x + 20f, 18f);
                        m_PaletteDropdownRect.Set(m_PaletteButtonRect.x, m_PaletteButtonRect.yMax + 3f, 0f, 0f);
                        m_MonochromeColorPickerRect.Set(m_PaletteButtonRect.xMax + 12f, m_PaletteButtonRect.y, 60f, 18f);

                        if (m_PaletteButtonRect.Contains(m_CurrentEvent.mousePosition) && m_CurrentEvent.type == EventType.ScrollWheel && m_CurrentEvent.modifiers.HasAny(EventModifiers.Shift))
                        {
                            PLDrawerUtils.SetPalette(cpd, m_Settings.ChangePalettePath(cpd.DrawerSettings.PalettePath, PLDrawerUtils.GetScrollValue(m_CurrentEvent)));
                            m_CurrentEvent.Use();
                        }
                        
                        if (EditorGUI.DropdownButton(m_PaletteButtonRect, m_TempContent, FocusType.Passive, PLDrawerTheme.OptionButtonOn))
                        {
                            DropdownMenus.SetupPaletteDropdownMenu(cpd);
                            cpd.PaletteDropdownMenu.DropDown(m_PaletteDropdownRect);
                        }

                        var lastX = 0f;

                        if (cpd.DrawerSettings.Monochrome)
                        {
                            EditorGUI.BeginChangeCheck();
                            cpd.DrawerSettings.MonochromeColor = EditorGUI.ColorField(m_MonochromeColorPickerRect, GUIContent.none, cpd.DrawerSettings.MonochromeColor, true, false, false);
                            if (EditorGUI.EndChangeCheck()) cpd.SetupColors();
                            lastX = m_MonochromeColorPickerRect.xMax;
                        }
                        else
                        {
                            // Color Palette Preview
                            var colorRect = new Rect(m_PaletteButtonRect.xMax + 8f, subOptionsYPosition + 2f, 12f, 16f);
                            for (var i = 0; i < cpd.StripeColors.Count; i++)
                            {
                                var colorIndex = cpd.DrawerSettings.ReverseColorOrder ? cpd.StripeColors.Count - 1 - i : i;
                                var stripeColor = cpd.StripeColors[colorIndex];
                                EditorGUI.DrawRect(colorRect, stripeColor);
                                colorRect.x = colorRect.xMax + 2f;
                            }

                            // Reverse Color Order
                            m_ReverseColorOrderButtonRect.Set(colorRect.xMax - 8f, subOptionsYPosition + 1f, 20f, 18f);
                            if (GUI.Button(m_ReverseColorOrderButtonRect, PLDrawerContents.ReverseColorOrderButton, cpd.DrawerSettings.ReverseColorOrder ? PLDrawerTheme.ReversePaletteButtonOn : PLDrawerTheme.ReversePaletteButtonOff))
                            {
                                cpd.DrawerSettings.ReverseColorOrder = !cpd.DrawerSettings.ReverseColorOrder;
                                cpd.SetupColors();
                            }
                            lastX = m_ReverseColorOrderButtonRect.xMax;
                        }
                        
                        var themeSubOptionsHalfWidth = cpd.SubOptionsRect.xMax - lastX - 24f;
                        m_StripeHeightSliderRect.Set(lastX + 18f, subOptionsYPosition + 1f, themeSubOptionsHalfWidth, 18f);
                        
                        EditorGUI.BeginChangeCheck();
                        cpd.DrawerSettings.StripeHeightPixels = OptionSliders.ValueSlider(m_StripeHeightSliderRect, 
                            PLDrawerContents.StripeHeightSlider, 
                            cpd.DrawerSettings.StripeHeightPixels, 
                            PLDrawerTheme.CompactStripeHeight, 
                            PLDrawerTheme.TallStripeHeight, 
                            false, 
                            true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            cpd.DrawerSettings.StripeHeight = PLDrawerTheme.GetStripeHeight(cpd.DrawerSettings.StripeHeightPixels);
                            cpd.SetHotStyles();
                        }
                        break;
                    case DrawerOptionSection.Picks:
                        var pickMinRectWidth = cpd.p_PickCountMin.intValue.ToString().Length * 10f;
                        var pickMaxRectWidth = cpd.p_PickCountMax.intValue.ToString().Length * 10f;
                        var pickCountMinRect = new Rect(cpd.MenuArrowRect.xMax, subOptionsYPosition + 1f, pickMinRectWidth < 34f ? 34f : pickMinRectWidth, EditorGUIUtility.singleLineHeight);
                        var linkPickCountsRect = new Rect(pickCountMinRect.xMax + 2f, subOptionsYPosition + 3f, 15f, 15f);
                        var pickCountMaxRect = new Rect(linkPickCountsRect.xMax + 2f, subOptionsYPosition + 1f, pickMaxRectWidth < 34f ? 34f : pickMaxRectWidth, EditorGUIUtility.singleLineHeight);
                        var pickCountCurveRect = new Rect(pickCountMaxRect.xMax + 4f, subOptionsYPosition + 1f, 40f, EditorGUIUtility.singleLineHeight);
                        var rollSectionSeparatorRect = new Rect(pickCountCurveRect.xMax, subOptionsYPosition, 20f, 20f);

                        m_TestRollButtonRect.Set(rollSectionSeparatorRect.xMax, subOptionsYPosition, 50f, 20f);
                        m_ClearResultsButtonRect.Set(m_TestRollButtonRect.xMax + 4f, subOptionsYPosition + 2f, 16f, 16f);

                        EditorGUI.BeginChangeCheck();
                        cpd.p_PickCountMin.intValue = EditorGUI.IntField(pickCountMinRect, GUIContent.none, cpd.p_PickCountMin.intValue, PLDrawerTheme.InputField);
                        if (EditorGUI.EndChangeCheck() && cpd.p_LinkPickCounts.boolValue) cpd.p_PickCountMax.intValue = cpd.p_PickCountMin.intValue;

                        EditorGUI.BeginDisabledGroup(cpd.p_LinkPickCounts.boolValue);
                        cpd.p_PickCountMax.intValue = EditorGUI.IntField(pickCountMaxRect, GUIContent.none, cpd.p_PickCountMax.intValue, PLDrawerTheme.InputField);

                        EditorGUI.PropertyField(pickCountCurveRect, cpd.p_PickCountCurve, GUIContent.none);
                        EditorGUI.EndDisabledGroup();
                        
                        if (GUI.Button(linkPickCountsRect, cpd.p_LinkPickCounts.boolValue ? PLDrawerContents.UnlinkPickCounts : PLDrawerContents.LinkPickCounts, cpd.p_LinkPickCounts.boolValue ? PLDrawerTheme.PickCountsLinkedStyle : PLDrawerTheme.PickCountsUnlinkedStyle))
                        {
                            cpd.p_LinkPickCounts.boolValue = !cpd.p_LinkPickCounts.boolValue;
                            if (cpd.p_LinkPickCounts.boolValue) cpd.p_PickCountMax.intValue = cpd.p_PickCountMin.intValue;
                        }

                        if (m_CurrentEvent.type == EventType.Repaint)
                        {
                            PLDrawerTheme.Separator.Draw(rollSectionSeparatorRect, false, false, false, false);
                        }
                        
                        if (GUI.Button(m_TestRollButtonRect, PLDrawerContents.TestButton, PLDrawerTheme.OptionLinkButtonOn))
                        {
                            if(m_CurrentEvent.shift) Application.OpenURL(RNGNStaticData.LinkDocsTestingOutcomes);
                            else
                            {
                                if(cpd.p_DepletableList.boolValue && (m_CurrentEvent.control || m_CurrentEvent.command)) cpd.ProbabilityListEditorInterface.RefillItems();
                                cpd.TestResults = cpd.ProbabilityListEditorInterface.RunTest();
                                cpd.SetUnitsInfoRequired = true;
                            }
                        }

                        if (GUI.Button(m_ClearResultsButtonRect, PLDrawerContents.ClearTestResultsButton, cpd.TestResults.indexPicks.Count > 0 ? PLDrawerTheme.OptionButtonOn : PLDrawerTheme.OptionButtonDisabled))
                        {
                            cpd.TestResults.Clear();
                        }
                        
                        var subOptionsSecondRowYPosition = subOptionsYPosition + 25f;
                        
                        if(GUI.Button(new Rect(pickCountMinRect.x, subOptionsSecondRowYPosition, pickCountCurveRect.xMax - pickCountMinRect.x, 20f), PLDrawerContents.MaintainPickCountButton, cpd.p_MaintainPickCountIfDisabled.boolValue ? PLDrawerTheme.OptionLinkButtonOn : PLDrawerTheme.OptionLinkButtonOff))
                        {
                            if(m_CurrentEvent.shift) Application.OpenURL(RNGNStaticData.LinkDocsDisabledItems);
                            else
                            {
                                cpd.p_MaintainPickCountIfDisabled.boolValue =
                                    !cpd.p_MaintainPickCountIfDisabled.boolValue;
                                cpd.p_MaintainPickCountIfDisabled.serializedObject.ApplyModifiedProperties();
                            }
                        }
                        
                        if(showAdvancedOptions)
                        {
                            m_TempContent.text = cpd.SelectionMethodName;
                            m_TempContent.tooltip = cpd.SelectionMethodTooltip;
                            var selectionRectWidth = PLDrawerTheme.OptionButtonOn.CalcSize(m_TempContent).x + 20f;
                            var rightAlignedXPosition = cpd.SubOptionsRect.xMax - selectionRectWidth - 6f;
                            var selectionMethodDropdownRect = new Rect(rightAlignedXPosition, subOptionsYPosition, selectionRectWidth, 20f);

                            if (GUI.Button(selectionMethodDropdownRect, m_TempContent, PLDrawerTheme.OptionButtonOn))
                            {
                                DropdownMenus.SetupSelectionMethodDropdownMenu(cpd);
                                cpd.SelectionMethodDropdownMenu.DropDown(selectionMethodDropdownRect);
                            }
                            
                            #region Depletable Options
                            
                            m_DepletableListButtonRect.Set(selectionMethodDropdownRect.x - 84f, subOptionsYPosition, 80f, 20f);
                            
                            if(GUI.Button(m_DepletableListButtonRect, PLDrawerContents.DepletableListButton, cpd.p_DepletableList.boolValue ? PLDrawerTheme.SectionLinkButtonOn : PLDrawerTheme.SectionLinkButtonOff))
                            {
                                if (m_CurrentEvent.shift) Application.OpenURL(RNGNStaticData.LinkDocsDepletableLists);
                                else
                                {
                                    cpd.p_DepletableList.boolValue = !cpd.p_DepletableList.boolValue;
                                    cpd.p_DepletableList.serializedObject.ApplyModifiedProperties();
                                }
                            }
                            
                            if (cpd.p_DepletableList.boolValue)
                            {
                                var actionRectWidth = PLDrawerTheme.OptionButtonOn.CalcSize(cpd.DepletableListActionContent).x + 20f;
                                m_DepletableListActionButtonRect.Set(rollSectionSeparatorRect.xMax, subOptionsSecondRowYPosition, actionRectWidth, 20f);
                                
                                if (GUI.Button(m_DepletableListActionButtonRect, cpd.DepletableListActionContent, PLDrawerTheme.OptionButtonOff))
                                {
                                    DropdownMenus.DepletableListActionDropdownMenu(cpd);
                                    cpd.DepletableListActionDropdownMenu.DropDown(m_DepletableListActionButtonRect);
                                    GUI.FocusControl(null);
                                }

                                var executeButtonX = m_DepletableListActionButtonRect.xMax;
                                
                                EditorGUI.BeginChangeCheck();
                                switch (cpd.DepletableListAction.Action)
                                {
                                    case DepletableListAction.Refill:
                                        break;
                                    case DepletableListAction.SetDepletable:
                                        m_DepletableListBoolValueButtonRect.Set(m_DepletableListActionButtonRect.xMax + 4f, subOptionsSecondRowYPosition, 28f, 20f);
                                        executeButtonX = m_DepletableListBoolValueButtonRect.xMax;
                                        if(GUI.Button(m_DepletableListBoolValueButtonRect, cpd.DrawerSettings.SetDepletableValue ? "On" : "Off", cpd.DrawerSettings.SetDepletableValue ? PLDrawerTheme.OptionButtonOn : PLDrawerTheme.OptionButtonOff))
                                        {
                                            cpd.DrawerSettings.SetDepletableValue = !cpd.DrawerSettings.SetDepletableValue;
                                        }
                                        break;
                                    case DepletableListAction.SetUnits:
                                        m_DepletableListUnitsValueButtonRect.Set(m_DepletableListActionButtonRect.xMax + 4f, subOptionsSecondRowYPosition, 30f, 20f);
                                        executeButtonX = m_DepletableListUnitsValueButtonRect.xMax;
                                        cpd.DrawerSettings.SetUnitsValue = EditorGUI.IntField(m_DepletableListUnitsValueButtonRect, cpd.DrawerSettings.SetUnitsValue, PLDrawerTheme.InputField);
                                        break;
                                    case DepletableListAction.SetMaxUnits:
                                        m_DepletableListUnitsValueButtonRect.Set(m_DepletableListActionButtonRect.xMax + 4f, subOptionsSecondRowYPosition, 30f, 20f);
                                        executeButtonX = m_DepletableListUnitsValueButtonRect.xMax;
                                        cpd.DrawerSettings.SetMaxUnitsValue = EditorGUI.IntField(m_DepletableListUnitsValueButtonRect, cpd.DrawerSettings.SetMaxUnitsValue, PLDrawerTheme.InputField);
                                        break;
                                    case DepletableListAction.Reset:
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                if (EditorGUI.EndChangeCheck())
                                {
                                    cpd.DrawerSettings.SetUnitsValue = Mathf.Clamp(cpd.DrawerSettings.SetUnitsValue, 0, int.MaxValue);
                                    cpd.DrawerSettings.SetMaxUnitsValue = Mathf.Clamp(cpd.DrawerSettings.SetMaxUnitsValue, 0, int.MaxValue);
                                    cpd.SetDepletableListActionContent();
                                }
                                
                                m_DepletableListExecuteActionButtonRect.Set(executeButtonX + 4f, subOptionsSecondRowYPosition, 20f, 20f);
                                
                                if (GUI.Button(m_DepletableListExecuteActionButtonRect, PLDrawerContents.DepletableActionExecuteButton, PLDrawerTheme.ExecuteActionButton))
                                {
                                    Undo.RecordObject(cpd.p_ProbabilityListProperty.serializedObject.targetObject, cpd.DepletableListActionContent.text);
                                    PLDrawerUtils.ExecuteDepletableListAction(cpd);
                                    cpd.SetupPropertiesRequired = true;
                                    GUI.FocusControl(null);
                                }
                            }
                            
                            #endregion

                            #region Weights Options
                            
                            if (m_CurrentEvent.type == EventType.Repaint)
                            {
                                PLDrawerTheme.Separator.Draw(new Rect(cpd.SubOptionsRect.xMax - 208f, subOptionsSecondRowYPosition, 20f, 20f), false, false, false, false);
                            }
                            
                            if (GUI.Button(new Rect(cpd.SubOptionsRect.xMax - 226f, subOptionsSecondRowYPosition, 20f, 20f), cpd.p_WeightsPriority.boolValue ? PLDrawerContents.WeightsPriorityButtonOn : PLDrawerContents.WeightsPriorityButtonOff, cpd.p_WeightsPriority.boolValue ? PLDrawerTheme.WeightsPriorityButtonOn : PLDrawerTheme.WeightsPriorityButtonOff))
                            {
                                cpd.p_WeightsPriority.boolValue = !cpd.p_WeightsPriority.boolValue;
                                cpd.p_WeightsPriority.serializedObject.ApplyModifiedProperties();
                            }
                            
                            if (GUI.Button(new Rect(cpd.SubOptionsRect.xMax - 250f, subOptionsSecondRowYPosition, 20f, 20f), PLDrawerContents.ResetWeightsButton, PLDrawerTheme.ResetWeightsButton))
                            {
                                Undo.RecordObject(cpd.p_ProbabilityListProperty.serializedObject.targetObject, "Reset Weights");
                                cpd.ProbabilityListEditorInterface.ResetWeights();
                                cpd.SetupPropertiesRequired = true;
                            }
                            
                            EditorGUI.BeginChangeCheck();
                            cpd.p_BaseWeight.intValue = EditorGUI.IntField(new Rect(cpd.SubOptionsRect.xMax - 304f, subOptionsSecondRowYPosition, 50f, 20f), GUIContent.none, cpd.p_BaseWeight.intValue, PLDrawerTheme.InputField);
                            if (EditorGUI.EndChangeCheck())
                            {
                                Undo.RecordObject(cpd.p_ProbabilityListProperty.serializedObject.targetObject, $"Set Base Weight to {cpd.p_BaseWeight.intValue}");
                                cpd.p_BaseWeight.intValue = Mathf.Clamp(cpd.p_BaseWeight.intValue, 1, 10000);
                            }
                            
                            #endregion
                            
                            #region Seeding Options
                            
                            if (GUI.Button(new Rect(cpd.SubOptionsRect.xMax - 86f, subOptionsSecondRowYPosition, 80f, 20f), PLDrawerContents.KeepSeedButton, cpd.p_KeepSeed.boolValue ? PLDrawerTheme.OptionLinkButtonOn : PLDrawerTheme.OptionLinkButtonOff))
                            {
                                if(m_CurrentEvent.shift) Application.OpenURL(RNGNStaticData.LinkDocsKeepSeed);
                                else
                                {
                                    cpd.p_KeepSeed.boolValue = !cpd.p_KeepSeed.boolValue;
                                    cpd.p_KeepSeed.serializedObject.ApplyModifiedProperties();
                                }
                            }
                            
                            EditorGUI.BeginDisabledGroup(!cpd.p_KeepSeed.boolValue);
                            
                            if (cpd.p_KeepSeed.boolValue)
                            {
                                #if UNITY_2022_1_OR_NEWER
                                var fieldSeedValue = EditorGUI.LongField(new Rect(cpd.SubOptionsRect.xMax - 192f, subOptionsSecondRowYPosition, 100f, 20f), PLDrawerContents.SeedReadoutField, cpd.p_Seed.uintValue, PLDrawerTheme.InputField);
                                cpd.p_Seed.uintValue = Convert.ToUInt32(Math.Clamp(fieldSeedValue, 0, uint.MaxValue));
                                #else
                                cpd.p_Seed.longValue = EditorGUI.LongField(new Rect(cpd.SubOptionsRect.xMax - 192f, subOptionsSecondRowYPosition, 100f, 20f), PLDrawerContents.SeedReadoutField, cpd.p_Seed.longValue, PLDrawerTheme.InputField);
                                #endif
                            }
                            else
                            {
                                EditorGUI.LabelField(new Rect(cpd.SubOptionsRect.xMax - 192f, subOptionsSecondRowYPosition, 100f, 20f), ((uint)cpd.p_Seed.intValue).ToString(), PLDrawerTheme.InputField);
                            }

                            EditorGUI.EndDisabledGroup();
                            
                            #endregion
                        }
                        
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                
                if (GUI.Button(m_AddButtonRect, PLDrawerContents.AddItemButton, cpd.p_ProbabilityItems.arraySize == 0 ? PLDrawerTheme.SectionButtonOn : PLDrawerTheme.OptionButtonOn))
                {
                    cpd.AddItem();
                    GUIUtility.ExitGUI();
                }

                if (cpd.ItemPropertyCache.Count != cpd.p_ProbabilityItems.arraySize) cpd.SetupProperties(this);

                if (cpd.p_ProbabilityItems.arraySize > 0 && m_CurrentEvent.type == EventType.Repaint)
                {
                    EditorGUI.DrawRect(m_StripeRect, PLDrawerTheme.DimBackgroundColor);
                    TheStripe.Draw(cpd, m_StripeRect, m_CurrentEvent);
                    PLDrawerTheme.StripeBorder.Draw(m_StripeBorderRect, false, false, false, false);
                }
                
                m_GrabPoint = TheStripe.StateLogic(cpd, m_StripeRect, m_CurrentEvent, m_GrabPoint);
                
                if (cpd.ShowListEntries && cpd.p_ProbabilityItems.arraySize > 0)
                {
                    var listRect = new Rect(position.x, m_StripeBorderRect.yMax + 2f, position.width, cpd.ReorderableList.GetHeight());
                    if (m_CurrentEvent.type == EventType.Repaint && listRect.Contains(m_CurrentEvent.mousePosition) == false) cpd.HoveredListElement = -1;
                    cpd.ReorderableList.DoList(listRect);
                }
                // else return;
            }
            EditorGUI.EndProperty();
            EditorGUI.indentLevel = indentLevel;

            // Prevent state lock when mouse leaves inspector window while modifying
            if (cpd.ModifierState == ModifierState.Modifying && (m_CurrentEvent.mousePosition.x < m_StripeRect.xMin || m_CurrentEvent.mousePosition.x > m_StripeRect.xMax))
                cpd.ModifierState = ModifierState.Unselected;

            switch (m_Settings.InspectorRefreshMode)
            {
                case InspectorRefreshMode.Responsive:
                    if (position.Contains(m_CurrentEvent.mousePosition) && focusedWindow) EditorWindow.focusedWindow.Repaint();
                    break;
                case InspectorRefreshMode.Optimized:
                    if (m_CurrentEvent.type == EventType.MouseDown) m_GrabPoint = TheStripe.StateLogic(cpd, m_StripeRect, m_CurrentEvent, m_GrabPoint);
                    if (position.Contains(m_CurrentEvent.mousePosition) && focusedWindow && cpd.ModifierState == ModifierState.Modifying) EditorWindow.focusedWindow.Repaint();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void DrawAdvancedOptions(bool advancedOptions)
        {
            var positionX = cpd.MenuArrowRect.xMax;
            var positionWidth = cpd.SubOptionsRect.xMax - cpd.MenuArrowRect.xMax - 10f;

            // First Row
            if (GUI.Button(GetRectForCoords(1, 1, 2), PLDrawerContents.ResetProbabilitiesButton, PLDrawerTheme.SectionButtonOff))
            {
                Undo.RecordObject(cpd.p_ProbabilityListProperty.serializedObject.targetObject, "Reset Probabilities");
                cpd.ProbabilityListEditorInterface.ResetAllProbabilities();
                cpd.SetupPropertiesRequired = true;
            }
            
            // if (GUI.Button(GetRectForCoords(1, 2, 2), PLDrawerContents.ResetWeightsButton, PLDrawerTheme.SectionButtonOff))
            // {
            //     Undo.RecordObject(cpd.p_ProbabilityListProperty.serializedObject.targetObject, "Reset Weights");
            //     cpd.ProbabilityListEditorInterface.ResetWeights();
            //     cpd.SetupPropertiesRequired = true;
            // }
            
            if (GUI.Button(GetRectForCoords(1, 2, 2), PLDrawerContents.GetNewDrawerIDButton, PLDrawerTheme.SectionButtonOff))
            {
                var currentSettings = cpd.DrawerSettings;
                GetOrCreateDrawerID(cpd, string.Empty, true);
                cpd.DrawerSettings.ApplySettings(currentSettings);
                cpd.DrawerSettings.DrawerOptionSection = DrawerOptionSection.Cog;
                EditorUtility.SetDirty(cpd.p_ProbabilityListProperty.serializedObject.targetObject);
                RLogger.Log("Created new ID for this Drawer. Current settings were transferred.", LogMessageType.Info);
            }
            
            if (GUI.Button(GetRectForCoords(2, 1, 2), PLDrawerContents.ResetSettingsToDefaultButton, PLDrawerTheme.SectionButtonOff))
            {
                cpd.DrawerSettings.ApplySettings(m_Settings.m_DefaultDrawerSettings);
                cpd.SetHotStyles();
                cpd.SetupPropertiesRequired = true;
            }
            
            if (GUI.Button(GetRectForCoords(2, 2, 2), PLDrawerContents.SetAsDefaultSettingsButton, PLDrawerTheme.CriticalActionButtonHover))    
            {
                m_Settings.m_DefaultDrawerSettings.ApplySettings(cpd.DrawerSettings);
            }

            if (advancedOptions == false) return;
            
            EditorGUI.DrawRect(GetRectForCoords(3, 1, 1), Color.gray);
            
            // Left Column
            EditorGUI.BeginChangeCheck();
            {
                cpd.DrawerSettings.StripePercentageDigits = OptionSliders.ValueSlider(GetRectForCoords(4, 1, 2), PLDrawerContents.StripePercentageDigits, cpd.DrawerSettings.StripePercentageDigits, 0, 2);
                cpd.DrawerSettings.ItemPercentageDigits = OptionSliders.ValueSlider(GetRectForCoords(5, 1, 2), PLDrawerContents.ItemPercentageDigits, cpd.DrawerSettings.ItemPercentageDigits, 0, 5);
            }
            if (EditorGUI.EndChangeCheck()) cpd.SetupInfoCache();
            
            cpd.DrawerSettings.TestPercentageDigits = OptionSliders.ValueSlider(GetRectForCoords(6, 1, 2), PLDrawerContents.TestPercentageDigits, cpd.DrawerSettings.TestPercentageDigits, 0, 5);
            cpd.DrawerSettings.TestColorizeSensitivity = OptionSliders.ValueSlider(GetRectForCoords(7, 1, 2), PLDrawerContents.TestColorSensitivity, cpd.DrawerSettings.TestColorizeSensitivity, 0f, 10f);
            
            // Right Column
            if (GUI.Button(GetRectForCoords(4, 2, 2), PLDrawerContents.InfluenceProviderButton, cpd.DrawerSettings.ShowInfluenceToggle || cpd.IsInfluencedList ? PLDrawerTheme.SectionLinkButtonOn : PLDrawerTheme.OptionLinkButtonOff))
            {
                if(m_CurrentEvent.shift) Application.OpenURL(RNGNStaticData.LinkDocsProbabilityInfluence);
                else
                if (cpd.DrawerSettings.ShowInfluenceToggle == false || !cpd.IsInfluencedList) cpd.DrawerSettings.ShowInfluenceToggle = !cpd.DrawerSettings.ShowInfluenceToggle;
            }

            var influenceDisabled = cpd.DrawerSettings.ShowInfluenceToggle == false && cpd.IsInfluencedList == false;
            EditorGUI.BeginDisabledGroup(influenceDisabled);
            {
                EditorGUI.BeginChangeCheck();
                {
                    cpd.DrawerSettings.SpreadPercentageDigits = OptionSliders.ValueSlider(GetRectForCoords(5, 2, 2), PLDrawerContents.SpreadPercentageDigits, cpd.DrawerSettings.SpreadPercentageDigits, 0, 2, influenceDisabled);
                    cpd.DrawerSettings.SpreadColorizeSensitivity = OptionSliders.ValueSlider(GetRectForCoords(6, 2, 2), PLDrawerContents.SpreadColorizeSensitivity, cpd.DrawerSettings.SpreadColorizeSensitivity, 0f, 10f, influenceDisabled);
                }
                if (EditorGUI.EndChangeCheck()) cpd.SetSpreadCache();

                if (GUI.Button(GetRectForCoords(7, 2, 2), PLDrawerContents.ShowSpreadOnBars, cpd.DrawerSettings.ShowSpreadOnBars ? PLDrawerTheme.OptionButtonOn : PLDrawerTheme.SectionButtonOff))
                {
                    cpd.DrawerSettings.ShowSpreadOnBars = !cpd.DrawerSettings.ShowSpreadOnBars;
                }
            }
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.DrawRect(GetRectForCoords(8, 1, 1), Color.gray);
            
            // Item Utility Space
            cpd.DrawerSettings.ElementInfoSpace = OptionSliders.ValueSlider(GetRectForCoords(9, 1, 1), PLDrawerContents.ItemUtilitySpace, cpd.DrawerSettings.ElementInfoSpace, .2f, 1f);
            return;

            Rect GetRectForCoords(int row, int column, int maxColumns)
            {
                var rowHeight = PLDrawerTheme.AdvancedOptionsRowHeights[row - 1];
                
                var columnWidth = (positionWidth - (maxColumns - 1) * PLDrawerTheme.AdvancedOptionsHPadding) / maxColumns;
                var startY = cpd.MenuArrowRect.y;
                for (var i = 0; i < row - 1; i++) startY += PLDrawerTheme.AdvancedOptionsRowHeights[i] + PLDrawerTheme.AdvancedOptionsVPadding;
                var startX = positionX + (column - 1) * (columnWidth + PLDrawerTheme.AdvancedOptionsHPadding);
                return new Rect(startX, startY, columnWidth, rowHeight);
            }
        }
        
        #endregion
    }
    #pragma warning restore 0618
}