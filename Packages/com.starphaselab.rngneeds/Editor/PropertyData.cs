using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace RNGNeeds.Editor
{
    #pragma warning disable 0618
    internal class PropertyData
    {
        public IProbabilityListEditorActions ProbabilityListEditorInterface;
        public PLDrawerSettings DrawerSettings;
        public ReorderableList ReorderableList;
        public TestResults TestResults;
        public GenericMenu PaletteDropdownMenu;
        public GenericMenu SelectionMethodDropdownMenu;
        public GenericMenu DepletableListActionDropdownMenu;
        public Dictionary<int, ItemInfoCache> ItemInfoCache;
        public List<ItemPropertyCache> ItemPropertyCache;
        public Rect OptionsRect;
        public Rect SubOptionsRect;
        public Rect MenuArrowRect;
        
        public FloatLabelField FloatLabelField;
        public IntLabelField IntLabelField;
        public string PropertyPath;
        public string NameOfProperty;
        public float MaxProbability;
        public int ValuesChangedFor = -1;
        public int UnlockedItems;
        public int IndexOfUnremovableItem;
        public bool ValueIsGenericType;
        public bool ValueIsObjectReference;
        public bool ValueCannotBeObtained;
        public bool SetupPropertiesRequired;
        public bool ShouldHighlightListElements;
        public bool CanDisplayItemInfo;
        public bool IsInfluencedList;
        public bool IsArray;
        public bool IsArrayAndNotCollection;
        public bool ShowListEntries;
        public bool DrawInfluenceProviderToggle => DrawerSettings.ShowInfluenceToggle || IsInfluencedList;
        public bool ShouldColorizeBars => DrawerSettings.ColorizePreviewBars;
        
        public string SelectionMethodName;
        public string SelectionMethodTooltip;
        
        // Depletable List
        public (DepletableListAction Action, string Title, string Tooltip) DepletableListAction = RNGNStaticData.DepletableListActions[0];
        public GUIContent DepletableListActionContent;
        public int TotalUnits;
        public int TotalMaxUnits;
        public bool SetUnitsInfoRequired;

        // Serialized Properties
        public SerializedProperty p_ProbabilityListProperty;
        public SerializedProperty p_ProbabilityItems;
        public SerializedProperty p_ID;
        public SerializedProperty p_PickCountMin;
        public SerializedProperty p_PickCountMax;
        public SerializedProperty p_PickCountCurve;
        public SerializedProperty p_PreventRepeat;
        public SerializedProperty p_ShuffleIterations;
        public SerializedProperty p_LinkPickCounts;
        public SerializedProperty p_MaintainPickCountIfDisabled;
        public SerializedProperty p_Seed;
        public SerializedProperty p_KeepSeed;
        public SerializedProperty p_DepletableList;
        public SerializedProperty p_WeightsPriority;
        public SerializedProperty p_BaseWeight;

        // Modifiers
        public List<Rect> ModifierRects;
        public List<Rect> ProbabilityRects;
        public ModifierState ModifierState;
        public ModifierType ModifierType;
        public bool DrawModifierRects;
        public int SelectedModifier;
        public int HoveredProbabilityRect;
        public int HoveredListElement;

        // Colors & Styles
        public List<Color> StripeColors;
        public List<Color> ProbabilityItemColors;
        public readonly GUIStyle stripeIndexStyle = new GUIStyle();
        public readonly GUIStyle stripeNameStyle = new GUIStyle();
        public readonly GUIStyle stripePercentageStyle = new GUIStyle();
        
        internal void SetHotStyles()
        {
            stripeIndexStyle.fontSize = 12;
            stripeIndexStyle.alignment = DrawerSettings.StripeHeight == StripeHeight.Compact ? TextAnchor.MiddleLeft : DrawerSettings.StripeHeight == StripeHeight.Short ? DrawerSettings.ShowPercentageOrWeights ? TextAnchor.UpperLeft : TextAnchor.MiddleLeft : TextAnchor.UpperCenter;
            stripeIndexStyle.contentOffset = DrawerSettings.StripeHeight == StripeHeight.Compact ? new Vector2(6f, 0f) : DrawerSettings.StripeHeight == StripeHeight.Short ? new Vector2(6f, 0f) : Vector2.zero;

            stripeNameStyle.fontSize = PLDrawerTheme.versionSpecificFontSize;
            stripeNameStyle.alignment = DrawerSettings.StripeHeight == StripeHeight.Short ? DrawerSettings.ShowPercentageOrWeights ? TextAnchor.UpperCenter : TextAnchor.MiddleCenter : TextAnchor.MiddleCenter;
            stripeNameStyle.fontStyle = FontStyle.Bold;
            stripeNameStyle.contentOffset = Vector2.zero;
            
            var percentageOnly = DrawerSettings.ShowIndex == false && DrawerSettings.ShowInfo == false && DrawerSettings.ShowPercentageOrWeights;
            stripePercentageStyle.fontSize = 12;
            stripePercentageStyle.alignment = percentageOnly ? TextAnchor.MiddleCenter : DrawerSettings.StripeHeight == StripeHeight.Compact ? TextAnchor.MiddleRight : TextAnchor.LowerCenter;
            stripePercentageStyle.contentOffset = percentageOnly ? new Vector2(0f, 1f) : DrawerSettings.StripeHeight == StripeHeight.Compact ? new Vector2(-4f, 0f) : new Vector2(0f, 1f);
        }
        
        public float ElementHeightCallback(int index)
        {
            if(ShowListEntries == false) return 0f;
            
            var add = 0f;
            if(ItemInfoCache.TryGetValue(index, out var infoCache)) if (infoCache.InfluenceProviderExpanded) add = 24f + infoCache.influenceInfoHeight;
            
            return ValueCannotBeObtained ? 0 : p_ProbabilityItems.arraySize > 0
                ? EditorGUI.GetPropertyHeight(
                      ItemPropertyCache.Count < p_ProbabilityItems.arraySize ? 
                          p_ProbabilityItems.GetArrayElementAtIndex(index).FindPropertyRelative("m_Value") : ItemPropertyCache[index].p_Value, true) + add + 1f : 0;
        }
    }
    #pragma warning restore 0618
}