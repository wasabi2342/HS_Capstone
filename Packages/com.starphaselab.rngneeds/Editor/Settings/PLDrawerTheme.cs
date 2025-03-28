using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RNGNeeds.Editor
{
    internal static class PLDrawerTheme
    {
        private static Texture2D OptionsRectTexture { get; set; }
        private static Texture2D CogIconTexture { get; set; }
        private static Texture2D CogHintIconTexture { get; set; }
        private static Texture2D StripeBorderTexture { get; set; }
        private static Texture2D OptionButtonOnTexture { get; set; }
        private static Texture2D OptionButtonOnHoverTexture { get; set; }
        private static Texture2D OptionButtonOffTexture { get; set; }
        private static Texture2D OptionButtonOffHoverTexture { get; set; }
        
        private static Texture2D OptionLinkButtonOnTexture { get; set; }
        private static Texture2D OptionLinkButtonHoverTexture { get; set; }
        private static Texture2D OptionLinkButtonOffTexture { get; set; }
        private static Texture2D OptionLinkButtonOffHoverTexture { get; set; }
        private static Texture2D SectionButtonOnTexture { get;  set; }
        private static Texture2D SectionButtonHoverTexture { get;  set; }
        private static Texture2D SectionButtonOffTexture { get; set; }
        
        private static Texture2D SectionLinkButtonOnTexture { get;  set; }
        private static Texture2D SectionLinkButtonHoverTexture { get;  set; }
        private static Texture2D SectionLinkButtonOffTexture { get; set; }
        private static Texture2D CriticalButtonHoverTexture { get; set; }
        private static Texture2D CriticalButtonTexture { get; set; }
        private static Texture2D SeparatorTexture { get; set; }
        private static Texture2D MenuArrowTexture { get; set; }
        private static Texture2D LinkIconTexture { get; set; }
        private static Texture2D LinkHintIconTexture { get; set; }
        private static Texture2D LockIconTexture { get; set; }
        private static Texture2D LockHintIconTexture { get; set; }
        private static Texture2D UnlockedIconTexture { get; set; }
        private static Texture2D InputFieldTexture { get; set; }
        
        private static Texture2D HideEntriesButtonOnTexture { get; set; }
        private static Texture2D HideEntriesButtonHoverTexture { get; set; }
        private static Texture2D HideEntriesButtonOffTexture { get; set; }
        
        private static Texture2D ShowIndexButtonOnTexture { get; set; }
        private static Texture2D ShowIndexButtonHoverTexture { get; set; }
        private static Texture2D ShowIndexButtonOffTexture { get; set; }
        
        private static Texture2D ShowInfoButtonOnTexture { get; set; }
        private static Texture2D ShowInfoButtonHoverTexture { get; set; }
        private static Texture2D ShowInfoButtonOffTexture { get; set; }
        
        private static Texture2D ShowPercentageButtonOnTexture { get; set; }
        private static Texture2D ShowPercentageButtonHoverTexture { get; set; }
        private static Texture2D ShowPercentageButtonOffTexture { get; set; }
        
        private static Texture2D ColorizeBarsButtonOnTexture { get; set; }
        private static Texture2D ColorizeBarsButtonHoverTexture { get; set; }
        private static Texture2D ColorizeBarsButtonOffTexture { get; set; }
        
        private static Texture2D NormalizeBarsButtonOnTexture { get; set; }
        private static Texture2D NormalizeBarsButtonHoverTexture { get; set; }
        private static Texture2D NormalizeBarsButtonOffTexture { get; set; }
        
        private static Texture2D DimColorsButtonOnTexture { get; set; }
        private static Texture2D DimColorsButtonHoverTexture { get; set; }
        private static Texture2D DimColorsButtonOffTexture { get; set; }
        
        private static Texture2D ShowWeightsButtonOnTexture { get; set; }
        private static Texture2D ShowWeightsButtonHoverTexture { get; set; }
        private static Texture2D ShowWeightsButtonOffTexture { get; set; }
        
        private static Texture2D WeightsPriorityButtonOnTexture { get; set; }
        private static Texture2D WeightsPriorityButtonHoverTexture { get; set; }
        private static Texture2D WeightsPriorityButtonOffTexture { get; set; }
        
        private static Texture2D ReversePaletteButtonOnTexture { get; set; }
        private static Texture2D ReversePaletteButtonHoverTexture { get; set; }
        private static Texture2D ReversePaletteButtonOffTexture { get; set; }
        
        private static Texture2D DepletableStripeButtonOnTexture { get; set; }
        private static Texture2D DepletableStripeButtonHoverTexture { get; set; }
        private static Texture2D DepletableStripeButtonOffTexture { get; set; }
        
        private static Texture2D ExecuteActionButtonTexture { get; set; }
        private static Texture2D ExecuteActionButtonHoverTexture { get; set; }
        private static Texture2D ExecuteActionButtonActiveTexture { get; set; }
        
        private static Texture2D ResetWeightsButtonTexture { get; set; }
        private static Texture2D ResetWeightsButtonHoverTexture { get; set; }
        private static Texture2D ResetWeightsButtonActiveTexture { get; set; }
        
        private static Texture2D RectBorderTexture { get; set; }
        private static Texture2D RectBorderAccentTexture { get; set; }
        private static Texture2D RectBorderDisabledTexture { get; set; }
        
        public static Texture2D LinkRectTexture { get; set; }
        
        // PLCollection Textures
        private static Texture2D CollectionFrameTexture { get; set; }
        private static Texture2D CollectionHeaderTexture { get; set; }
        private static Texture2D MoveUpButtonTexture { get; set; }
        private static Texture2D MoveUpButtonHoverTexture { get; set; }        
        private static Texture2D MoveDownButtonTexture { get; set; }
        private static Texture2D MoveDownButtonHoverTexture { get; set; }
        private static Texture2D ReorderIconEnabledTexture { get; set; }
        private static Texture2D ReorderIconDisabledTexture { get; set; }
        
        public static readonly GUIStyle StripeBorder;
        public static readonly GUIStyle CogIconOn;
        public static readonly GUIStyle CogIconOff;
        public static readonly GUIStyle ElementEnabledPercentageStyle;
        public static readonly GUIStyle ElementDisabledPercentageStyle;
        public static readonly GUIStyle ElementEnabledTextStyle;

        public static readonly GUIStyle UnitsEnabledTextStyle;
        public static readonly GUIStyle UnitsDisabledTextStyle;
        
        public static readonly GUIStyle ElementDisabledTextStyle;
        public static readonly GUIStyle TestResultPercentageStyle;
        public static readonly GUIStyle NormalTextStyle;
        public static readonly GUIStyle AdvancedOptionsLabelStyle;
        public static readonly GUIStyle ValueSliderInsetLabelStyle;
        public static readonly GUIStyle LabelInfoStyle;
        public static readonly GUIStyle PropertyNameStyle;
        public static readonly GUIStyle CollectionPropertyNameStyle;
        public static readonly GUIStyle OptionButtonOn;
        public static readonly GUIStyle OptionButtonOff;
        public static readonly GUIStyle OptionButtonDisabled;
        
        public static readonly GUIStyle OptionLinkButtonOn;
        public static readonly GUIStyle OptionLinkButtonOff;
        
        public static readonly GUIStyle SectionButtonOn;
        public static readonly GUIStyle SectionButtonOff;
        
        public static readonly GUIStyle SectionLinkButtonOn;
        public static readonly GUIStyle SectionLinkButtonOff;
        
        public static readonly GUIStyle CriticalActionButtonHover;
        public static readonly GUIStyle CriticalActionButton;
        public static readonly GUIStyle Separator;
        public static readonly GUIStyle MenuArrow;
        public static readonly GUIStyle LockIconEnabled;
        public static readonly GUIStyle LockIconDisabled;
        public static readonly GUIStyle LockIconHint;
        public static readonly GUIStyle UnlockedIcon;
        public static readonly GUIStyle PickCountsLinkedStyle;
        public static readonly GUIStyle PickCountsUnlinkedStyle;
        public static readonly GUIStyle InputField;
        public static readonly GUIStyle SpreadValueStyle;
        public static readonly GUIStyle OptionRectImage;
        public static readonly GUIStyle RectBorder;
        public static readonly GUIStyle RectBorderAccent;
        public static readonly GUIStyle RectBorderDisabled;
        
        // public static readonly GUIStyle HideEntriesButtonOn;
        public static readonly GUIStyle HideEntriesButtonOff;
        
        public static readonly GUIStyle ShowIndexButtonOn;
        public static readonly GUIStyle ShowIndexButtonOff;
        
        public static readonly GUIStyle ShowInfoButtonOn;
        public static readonly GUIStyle ShowInfoButtonOff;
        
        public static readonly GUIStyle ShowPercentageButtonOn;
        public static readonly GUIStyle ShowPercentageButtonOff;
        
        public static readonly GUIStyle ColorizeBarsButtonOn;
        public static readonly GUIStyle ColorizeBarsButtonOff;
        
        public static readonly GUIStyle NormalizeBarsButtonOn;
        public static readonly GUIStyle NormalizeBarsButtonOff;
        
        public static readonly GUIStyle DimColorsButtonOn;
        public static readonly GUIStyle DimColorsButtonOff;
        
        public static readonly GUIStyle ShowWeightsButtonOn;
        public static readonly GUIStyle ShowWeightsButtonOff;
        
        public static readonly GUIStyle WeightsPriorityButtonOn;
        public static readonly GUIStyle WeightsPriorityButtonOff;
        
        public static readonly GUIStyle ReversePaletteButtonOn;
        public static readonly GUIStyle ReversePaletteButtonOff;

        public static readonly GUIStyle DepletableStripeButtonOn;
        public static readonly GUIStyle DepletableStripeButtonOff;
        public static readonly GUIStyle ExecuteActionButton;
        public static readonly GUIStyle ResetWeightsButton;

        public static readonly GUIStyle CollectionFrameImage;
        public static readonly GUIStyle CollectionHeaderImage;

        public static readonly GUIStyle MoveUpButton;
        public static readonly GUIStyle MoveDownButton;
        public static readonly GUIStyle ReorderIconEnabled;
        public static readonly GUIStyle ReorderIconDisabled;
        
        public static readonly Color NormalTextColor;
        public static readonly Color ModifierRectColor;
        public static readonly Color ProbabilityRectHighlightColor;
        public static readonly Color CollectionSeparatorColor;
        public static readonly Color DepletableStripeBackgroundColor;
        
        public static readonly Color ElementAltColor;
        public static readonly Color DimBackgroundColor;
        public static readonly Color SliderBackgroundColor;
        public static readonly Color SliderDisabledBackgroundColor;
        public static readonly Color SliderHandleColor;
        public static readonly Color SliderHandleDisabledColor;

        public static readonly Color PreviewIndicatorColor;
        public static readonly Color PreviewIndicatorInvertedColor;
        
        public const int CompactStripeHeight = 26;
        public const int ShortStripeHeight = 34;
        public const int NormalStripeHeight = 50;
        public const int TallStripeHeight = 100;

        public static readonly float[] AdvancedOptionsRowHeights;
        public static readonly float AdvancedOptionsTotalHeight;
        public const float AdvancedOptionsHPadding = 10f;
        public const float AdvancedOptionsVPadding = 5f;
        
        public const int versionSpecificFontSize =
                #if UNITY_2023_1_OR_NEWER
                                11;
                #else
                                12;
                #endif

        // public const string hintButtonText =
        //         #if UNITY_2023_1_OR_NEWER
        //                         "?";
        //         #else
        //                         " ?";
        //         #endif
        
        public static StripeHeight GetStripeHeight(float height)
        {
            if(height <= 32f) return StripeHeight.Compact;
            if(height <= 43f) return StripeHeight.Short;
            if(height <= NormalStripeHeight) return StripeHeight.Normal;
            return StripeHeight.Tall;
        }
        
        public static int GetStripeHeightPixels(StripeHeight stripeHeight)
        {
            switch (stripeHeight)
            {
                case StripeHeight.Compact:
                    return CompactStripeHeight;
                case StripeHeight.Short:
                    return ShortStripeHeight;
                case StripeHeight.Normal:
                    return NormalStripeHeight;
                case StripeHeight.Tall:
                    return TallStripeHeight;
                default:
                    return NormalStripeHeight;
            }
        }
        
        private static Texture2D LoadTexture(string fileName)
        {
            return AssetDatabase.LoadAssetAtPath<Texture2D>($"{RNGNStaticData.PathToEditorAssets}{fileName}.png");
        }
        
        static PLDrawerTheme()
        {
            AdvancedOptionsRowHeights = new float[] { 
                20f, 
                20f, 
                1f,
                20f,
                20f,
                20f,
                20f,
                1f,
                20f,
            };
            
            AdvancedOptionsTotalHeight = AdvancedOptionsRowHeights.Sum();

            OptionsRectTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionsRect_00000" : "PL_OptionsRectLight_00000");
            CollectionFrameTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_CollectionFrame_00000" : "PL_CollectionFrameLight_00000");
            CollectionHeaderTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_CollectionHeader_00000" : "PL_CollectionHeaderLight_00000");
            MoveUpButtonTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PLC_MoveUpButton_00000" : "PLC_MoveUpButton_00000");
            MoveUpButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PLC_MoveUpButtonHover_00000" : "PLC_MoveUpButtonHover_00000");
            MoveDownButtonTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PLC_MoveDownButton_00000" : "PLC_MoveDownButton_00000");
            MoveDownButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PLC_MoveDownButtonHover_00000" : "PLC_MoveDownButtonHover_00000");
            ReorderIconEnabledTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PLC_ReorderIconEnabled_00000" : "PLC_ReorderIconEnabled_00000");
            ReorderIconDisabledTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PLC_ReorderIconDisabled_00000" : "PLC_ReorderIconDisabled_00000");
            
            CogIconTexture = LoadTexture("PL_CogIcon_00000");
            CogHintIconTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_CogHintIcon_00000" : "PL_CogHintIcon_00000");
            
            HideEntriesButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_HideEntriesButtonOn_00000" : "PL_HideEntriesButtonOn_00000");
            HideEntriesButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_HideEntriesButtonHover_00000" : "PL_HideEntriesButtonHover_00000");
            HideEntriesButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_HideEntriesButtonOff_00000" : "PL_HideEntriesButtonOff_00000");
            
            ShowIndexButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowIndexButtonOn_00000" : "PL_ShowIndexButtonOn_00000");
            ShowIndexButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowIndexButtonHover_00000" : "PL_ShowIndexButtonHover_00000");
            ShowIndexButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowIndexButtonOff_00000" : "PL_ShowIndexButtonOff_00000");
            
            ShowInfoButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowInfoButtonOn_00000" : "PL_ShowInfoButtonOn_00000");
            ShowInfoButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowInfoButtonHover_00000" : "PL_ShowInfoButtonHover_00000");
            ShowInfoButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowInfoButtonOff_00000" : "PL_ShowInfoButtonOff_00000");
            
            ShowPercentageButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowPercentageButtonOn_00000" : "PL_ShowPercentageButtonOn_00000");
            ShowPercentageButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowPercentageButtonHover_00000" : "PL_ShowPercentageButtonHover_00000");
            ShowPercentageButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowPercentageButtonOff_00000" : "PL_ShowPercentageButtonOff_00000");
            
            ColorizeBarsButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ColorizeBarsButtonOn_00000" : "PL_ColorizeBarsButtonOn_00000");
            ColorizeBarsButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ColorizeBarsButtonHover_00000" : "PL_ColorizeBarsButtonHover_00000");
            ColorizeBarsButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ColorizeBarsButtonOff_00000" : "PL_ColorizeBarsButtonOff_00000");
            
            NormalizeBarsButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_NormalizeBarsButtonOn_00000" : "PL_NormalizeBarsButtonOn_00000");
            NormalizeBarsButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_NormalizeBarsButtonHover_00000" : "PL_NormalizeBarsButtonHover_00000");
            NormalizeBarsButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_NormalizeBarsButtonOff_00000" : "PL_NormalizeBarsButtonOff_00000");
            
            DepletableStripeButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_DepletableStripeButtonOn_00000" : "PL_DepletableStripeButtonOn_00000");
            DepletableStripeButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_DepletableStripeButtonHover_00000" : "PL_DepletableStripeButtonHover_00000");
            DepletableStripeButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_DepletableStripeButtonOff_00000" : "PL_DepletableStripeButtonOff_00000");
            
            ExecuteActionButtonTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ExecuteActionButton_00000" : "PL_ExecuteActionButton_00000");
            ExecuteActionButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ExecuteActionButtonHover_00000" : "PL_ExecuteActionButtonHover_00000");
            ExecuteActionButtonActiveTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ExecuteActionButtonActive_00000" : "PL_ExecuteActionButtonActive_00000");
            
            ResetWeightsButtonTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ResetWeightsButton_00000" : "PL_ResetWeightsButton_00000");
            ResetWeightsButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ResetWeightsButtonHover_00000" : "PL_ResetWeightsButtonHover_00000");
            ResetWeightsButtonActiveTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ResetWeightsButtonActive_00000" : "PL_ResetWeightsButtonActive_00000");
            
            DimColorsButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_DimColorsButtonOn_00000" : "PL_DimColorsButtonOn_00000");
            DimColorsButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_DimColorsButtonHover_00000" : "PL_DimColorsButtonHover_00000");
            DimColorsButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_DimColorsButtonOff_00000" : "PL_DimColorsButtonOff_00000");
            
            ShowWeightsButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowWeightsButtonOn_00000" : "PL_ShowWeightsButtonOn_00000");
            ShowWeightsButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowWeightsButtonHover_00000" : "PL_ShowWeightsButtonHover_00000");
            ShowWeightsButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ShowWeightsButtonOff_00000" : "PL_ShowWeightsButtonOff_00000");
            
            WeightsPriorityButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_WeightsPriorityButtonOn_00000" : "PL_WeightsPriorityButtonOn_00000");
            WeightsPriorityButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_WeightsPriorityButtonHover_00000" : "PL_WeightsPriorityButtonHover_00000");
            WeightsPriorityButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_WeightsPriorityButtonOff_00000" : "PL_WeightsPriorityButtonOff_00000");
            
            ReversePaletteButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ReversePaletteButtonOn_00000" : "PL_ReversePaletteButtonOn_00000");
            ReversePaletteButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ReversePaletteButtonHover_00000" : "PL_ReversePaletteButtonHover_00000");
            ReversePaletteButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_ReversePaletteButtonOff_00000" : "PL_ReversePaletteButtonOff_00000");
            
            StripeBorderTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_StripeBorder_00000" : "PL_StripeBorderLight_00000");
            OptionButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionButtonOn_00000" : "PL_OptionButtonOn_00000");
            OptionButtonOnHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionButtonOnHover_00000" : "PL_OptionButtonOnHover_00000");
            OptionButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionButtonOff_00000" : "PL_OptionButtonOff_00000");
            OptionButtonOffHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionButtonOffHover_00000" : "PL_OptionButtonOffHover_00000");
            
            OptionLinkButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionLinkButtonOn_00000" : "PL_OptionLinkButtonOn_00000");
            OptionLinkButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionLinkButtonHover_00000" : "PL_OptionLinkButtonHover_00000");
            OptionLinkButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionLinkButtonOff_00000" : "PL_OptionLinkButtonOff_00000");
            OptionLinkButtonOffHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_OptionLinkButtonOffHover_00000" : "PL_OptionLinkButtonOffHover_00000");
            
            SectionButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_SectionButtonOn_00000" : "PL_SectionButtonOn_00000");
            SectionButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_SectionButtonHover_00000" : "PL_SectionButtonHover_00000");
            SectionButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_SectionButtonOff_00000" : "PL_SectionButtonOff_00000");
            SectionLinkButtonOnTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_SectionLinkButtonOn_00000" : "PL_SectionLinkButtonOn_00000");
            SectionLinkButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_SectionLinkButtonHover_00000" : "PL_SectionLinkButtonHover_00000");
            SectionLinkButtonOffTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_SectionLinkButtonOff_00000" : "PL_SectionLinkButtonOff_00000");
            
            CriticalButtonHoverTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_CriticalButtonHover_00000" : "PL_CriticalButtonHover_00000");
            CriticalButtonTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_CriticalButton_00000" : "PL_CriticalButton_00000");
            InputFieldTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_InputField_00000" : "PL_InputField_00000");
            SeparatorTexture = LoadTexture("PL_Separator_00000");
            MenuArrowTexture = LoadTexture(EditorGUIUtility.isProSkin ? "PL_MenuArrow_00000" : "PL_MenuArrow_00000");
            LinkIconTexture = LoadTexture("PL_LinkIcon_00000");
            LinkHintIconTexture = LoadTexture("PL_LinkHintIcon_00000");
            LockIconTexture = LoadTexture("PL_LockIcon_00000");
            LockHintIconTexture = LoadTexture("PL_LockHintIcon_00000");
            UnlockedIconTexture = LoadTexture("PL_UnlockedIcon_00000");
            LinkRectTexture = LoadTexture("PL_LockedStripeFG_00000");
            LinkRectTexture.wrapMode = TextureWrapMode.Repeat;
            RectBorderTexture = LoadTexture("PL_RectBorder_00000");
            RectBorderAccentTexture = LoadTexture("PL_RectBorderBlue_00000");
            RectBorderDisabledTexture = LoadTexture("PL_RectBorderGray_00000");
            
            OptionRectImage = new GUIStyle() { normal = { background = OptionsRectTexture }, border = new RectOffset(12, 12, 12, 12) };
            CollectionFrameImage = new GUIStyle() { normal = { background = CollectionFrameTexture }, border = new RectOffset(12, 12, 12, 12) };
            CollectionHeaderImage = new GUIStyle() { normal = { background = CollectionHeaderTexture }, border = new RectOffset(12, 12, 12, 12) };
            MoveUpButton = new GUIStyle() { normal = { background = MoveUpButtonTexture }, hover = { background = MoveUpButtonHoverTexture }};
            MoveDownButton = new GUIStyle() { normal = { background = MoveDownButtonTexture }, hover = { background = MoveDownButtonHoverTexture }};

            ReorderIconEnabled = new GUIStyle() { normal = { background = ReorderIconEnabledTexture } };
            ReorderIconDisabled = new GUIStyle() { normal = { background = ReorderIconDisabledTexture } };
            
            CogIconOn = new GUIStyle() { normal = { background = CogIconTexture } };
            CogIconOff = new GUIStyle() { normal = { background = CogHintIconTexture }, hover = { background = CogIconTexture}};
            
            ShowIndexButtonOn = new GUIStyle() { normal = { background = ShowIndexButtonOnTexture } };
            ShowIndexButtonOff = new GUIStyle() { normal = { background = ShowIndexButtonOffTexture }, hover = { background = ShowIndexButtonHoverTexture }};
            
            ShowInfoButtonOn = new GUIStyle() { normal = { background = ShowInfoButtonOnTexture } };
            ShowInfoButtonOff = new GUIStyle() { normal = { background = ShowInfoButtonOffTexture }, hover = { background = ShowInfoButtonHoverTexture }};
            
            ShowPercentageButtonOn = new GUIStyle() { normal = { background = ShowPercentageButtonOnTexture } };
            ShowPercentageButtonOff = new GUIStyle() { normal = { background = ShowPercentageButtonOffTexture }, hover = { background = ShowPercentageButtonHoverTexture }};

            ColorizeBarsButtonOn = new GUIStyle() { normal = { background = ColorizeBarsButtonOnTexture } };
            ColorizeBarsButtonOff = new GUIStyle() { normal = { background = ColorizeBarsButtonOffTexture }, hover = { background = ColorizeBarsButtonHoverTexture }};
            
            NormalizeBarsButtonOn = new GUIStyle() { normal = { background = NormalizeBarsButtonOnTexture } };
            NormalizeBarsButtonOff = new GUIStyle() { normal = { background = NormalizeBarsButtonOffTexture }, hover = { background = NormalizeBarsButtonHoverTexture }};
            
            DimColorsButtonOn = new GUIStyle() { normal = { background = DimColorsButtonOnTexture } };
            DimColorsButtonOff = new GUIStyle() { normal = { background = DimColorsButtonOffTexture }, hover = { background = DimColorsButtonHoverTexture }};
            
            ShowWeightsButtonOn = new GUIStyle() { normal = { background = ShowWeightsButtonOnTexture } };
            ShowWeightsButtonOff = new GUIStyle() { normal = { background = ShowWeightsButtonOffTexture }, hover = { background = ShowWeightsButtonHoverTexture }};
            
            WeightsPriorityButtonOn = new GUIStyle() { normal = { background = WeightsPriorityButtonOnTexture } };
            WeightsPriorityButtonOff = new GUIStyle() { normal = { background = WeightsPriorityButtonOffTexture }, hover = { background = WeightsPriorityButtonHoverTexture }};
            
            ReversePaletteButtonOn = new GUIStyle() { normal = { background = ReversePaletteButtonOnTexture } };
            ReversePaletteButtonOff = new GUIStyle() { normal = { background = ReversePaletteButtonOffTexture }, hover = { background = ReversePaletteButtonHoverTexture }};
            
            DepletableStripeButtonOn = new GUIStyle() { normal = { background = DepletableStripeButtonOnTexture } };
            DepletableStripeButtonOff = new GUIStyle() { normal = { background = DepletableStripeButtonOffTexture }, hover = { background = DepletableStripeButtonHoverTexture }};
            
            ExecuteActionButton = new GUIStyle() { normal = { background = ExecuteActionButtonTexture }, hover = { background = ExecuteActionButtonHoverTexture }, active = { background = ExecuteActionButtonActiveTexture } };
            
            ResetWeightsButton = new GUIStyle() { normal = { background = ResetWeightsButtonTexture }, hover = { background = ResetWeightsButtonHoverTexture }, active = { background = ResetWeightsButtonActiveTexture } };
            
            StripeBorder = new GUIStyle { normal = { background = StripeBorderTexture }, border = new RectOffset(8, 8, 8, 8) };

            // Font Styles
            ElementEnabledTextStyle = new GUIStyle { fontSize = 12, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(.85f, .85f, .85f, 1f) : new Color(.1f, .1f, .1f, 1f) }, alignment = TextAnchor.MiddleCenter };
            ElementDisabledTextStyle = new GUIStyle() { fontSize = 12, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(.5f, .5f, .5f, 1f) : new Color(.45f, .45f, .45f, 1f) }, alignment = TextAnchor.MiddleCenter };
            ElementEnabledPercentageStyle = new GUIStyle(ElementEnabledTextStyle) { alignment = TextAnchor.MiddleRight };
            ElementDisabledPercentageStyle = new GUIStyle(ElementDisabledTextStyle) { alignment = TextAnchor.MiddleRight };
            
            UnitsEnabledTextStyle = new GUIStyle(ElementEnabledTextStyle) { fontSize = 11 };
            UnitsDisabledTextStyle = new GUIStyle(ElementDisabledTextStyle) { fontSize = 11 };

            TestResultPercentageStyle = new GUIStyle() { fontSize = 12, normal = { textColor = Color.yellow }, alignment = TextAnchor.MiddleRight };
            NormalTextColor = EditorGUIUtility.isProSkin ? Color.white : Color.black;
            NormalTextStyle = new GUIStyle() { fontSize = 12, normal = { textColor = NormalTextColor } };
            AdvancedOptionsLabelStyle = new GUIStyle() { fontSize = 12, normal = { textColor = new Color(.8f, .8f, .8f, 1f) }, alignment = TextAnchor.MiddleLeft};
            ValueSliderInsetLabelStyle = new GUIStyle() { fontSize = 11, normal = { textColor = Color.white }, alignment = TextAnchor.MiddleCenter };
            
            LabelInfoStyle = new GUIStyle() { fontSize = 12, wordWrap = true, alignment = TextAnchor.UpperLeft, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f, 1f) : new Color(.2f, .2f, .2f, 1f) } };
            PropertyNameStyle = new GUIStyle() { alignment = TextAnchor.MiddleRight, padding = new RectOffset(0, 60, 0, 0), fontSize = versionSpecificFontSize, fontStyle = FontStyle.Bold, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f, 1f) : new Color(.9f, .9f, .9f, 1f) } };
            CollectionPropertyNameStyle = new GUIStyle() { alignment = TextAnchor.MiddleCenter, fontSize = versionSpecificFontSize, fontStyle = FontStyle.Bold, normal = { textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f, 1f) : new Color(.9f, .9f, .9f, 1f) } };
            
            OptionButtonOn = new GUIStyle()
            {
                fontSize = versionSpecificFontSize,
                // normal = { background = OptionButtonOnTexture, textColor = EditorGUIUtility.isProSkin ? new Color(.8f, .8f, .8f, 1f) : new Color(.35f, .35f, .35f, 1f) },
                normal = { background = OptionButtonOnTexture, textColor = Color.white },
                hover = { background = OptionButtonOnHoverTexture, textColor = Color.white },
                border = new RectOffset(8, 8, 8, 8), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold,
                active = { background = SectionButtonOnTexture, textColor = Color.white }
            };
            
            OptionButtonOff = new GUIStyle(OptionButtonOn) { normal = { background = OptionButtonOffTexture, textColor = new Color(.75f, .75f, .75f, 1f) }, hover = { background = OptionButtonOffHoverTexture, textColor = Color.white}, };
            OptionButtonDisabled = new GUIStyle(OptionButtonOff) { normal = { background = OptionButtonOffTexture, textColor = EditorGUIUtility.isProSkin ? new Color(.6f, .6f, .6f, 1f) : new Color(.5f, .5f, .5f, 1f) }, hover = { background = OptionButtonOffTexture, textColor = new Color(.6f, .6f, .6f, 1f)} };
            
            // HideEntriesButtonOn = new GUIStyle(OptionButtonOn) { normal = { background = HideEntriesButtonOnTexture } };
            HideEntriesButtonOff = new GUIStyle() { normal = { background = HideEntriesButtonOffTexture }, hover = { background = HideEntriesButtonHoverTexture }};
            
            OptionLinkButtonOn = new GUIStyle(OptionButtonOn)
            {
                normal = { background = OptionLinkButtonOnTexture }, 
                hover = { background = OptionLinkButtonHoverTexture }, 
                active = { background = OptionLinkButtonOnTexture },
                border = new RectOffset(9, 9, 9, 9)
            };
            OptionLinkButtonOff = new GUIStyle(OptionLinkButtonOn) { normal = { background = OptionLinkButtonOffTexture }, hover = { background = OptionLinkButtonOffHoverTexture}};
            
            SectionButtonOn = new GUIStyle(OptionButtonOn) { normal = { background = SectionButtonOnTexture, textColor = Color.white } };
            SectionButtonOff = new GUIStyle(OptionButtonOff) { normal = { background = SectionButtonOffTexture }, hover = { background = SectionButtonHoverTexture } };
            
            SectionLinkButtonOn = new GUIStyle(OptionLinkButtonOn)
            {
                normal = { background = SectionLinkButtonOnTexture }, 
                hover = { background = SectionLinkButtonHoverTexture }, 
                active = { background = SectionLinkButtonOnTexture }
            };
            SectionLinkButtonOff = new GUIStyle(SectionLinkButtonOn) { normal = { background = SectionLinkButtonOffTexture } };
            
            CriticalActionButtonHover = new GUIStyle(OptionButtonOff) { hover = { background = CriticalButtonHoverTexture } };
            CriticalActionButton = new GUIStyle(OptionButtonOn) { normal = { background = CriticalButtonTexture }, hover = { background = CriticalButtonHoverTexture } };
            InputField = new GUIStyle() { fontSize = versionSpecificFontSize, normal = { background = InputFieldTexture, textColor = Color.white }, border = new RectOffset(8, 8, 8, 8), alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, };

            RectBorder = new GUIStyle() { normal = { background = RectBorderTexture }, border = new RectOffset(4, 4, 4, 4) };
            RectBorderAccent = new GUIStyle() { normal = { background = RectBorderAccentTexture }, border = new RectOffset(4, 4, 4, 4) };
            RectBorderDisabled = new GUIStyle() { normal = { background = RectBorderDisabledTexture }, border = new RectOffset(4, 4, 4, 4) };
            
            Separator = new GUIStyle() { normal = { background = SeparatorTexture } };
            MenuArrow = new GUIStyle() { normal = { background = MenuArrowTexture } };
            LockIconHint = new GUIStyle() { hover = { background = LockHintIconTexture } };
            LockIconEnabled = new GUIStyle() { normal = { background = LockIconTexture } };
            LockIconDisabled = new GUIStyle() { normal = { background = LockHintIconTexture } };
            UnlockedIcon = new GUIStyle() { normal = { background = UnlockedIconTexture } };
            PickCountsLinkedStyle = new GUIStyle() { normal = { background = LinkIconTexture } };
            PickCountsUnlinkedStyle = new GUIStyle() { normal = { background = LinkHintIconTexture }, hover = { background = LinkIconTexture } };
            SpreadValueStyle = new GUIStyle() { alignment = TextAnchor.MiddleRight, fontSize = 11, normal = { textColor = Color.white } };
            
            ModifierRectColor = new Color(1f, 1f, 1f, .6f);
            ProbabilityRectHighlightColor = new Color(1f, 1f, 1f, .35f);
            CollectionSeparatorColor = EditorGUIUtility.isProSkin ? new Color(.7f, .7f, .7f, 1f) : new Color(.35f, .35f, .35f, 1f);
            
            ElementAltColor = EditorGUIUtility.isProSkin ? new Color(.29f, .29f, .29f, 1f) : new Color(.73f, .73f, .73f, 1f);
            SliderBackgroundColor = new Color(.30f, .36f, .42f, 1f);
            SliderDisabledBackgroundColor = new Color(.25f, .29f, .33f, 1f);
            SliderHandleColor = new Color(.54f, .8f, .88f, 1f);
            SliderHandleDisabledColor = new Color(.54f, .8f, .88f, .5f);

            DimBackgroundColor = EditorGUIUtility.isProSkin ? new Color(.22f, .22f, .22f, 1f) : new Color(.35f, .35f, .35f, 1f);
            DepletableStripeBackgroundColor = new Color(0.58f, 0.58f, 0.54f);
            
            PreviewIndicatorColor = Color.white;
            PreviewIndicatorInvertedColor = new Color(0f, 1f, 1f);
        }
    }
}