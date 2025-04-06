using System;
using System.Linq;
using StarphaseTools.Core;
using UnityEngine;

namespace RNGNeeds.Editor
{
    internal static class PLDrawerUtils
    {
        private static readonly GUIContent ThemeButtonTempContent = new GUIContent();
        private static readonly GUIContent PickButtonTempContent = new GUIContent();
        
        internal static bool ShouldAdjustUnits(Rect rect, Event currentEvent, out bool scrollWheel)
        {
            scrollWheel = false;
            if (rect.Contains(currentEvent.mousePosition) == false) return false;
            if (RNGNStaticData.Settings.AllowScrollWheelUnitsAdjustment && currentEvent.type == EventType.ScrollWheel)
            {
                scrollWheel = true;
                return true;
            }
            return currentEvent.type == EventType.KeyDown;
        }
        
        private static bool ShouldClampUnits(Event currentEvent)
        {
            return RNGNStaticData.WindowsOrLinuxEditor ? currentEvent.modifiers.HasAny(RNGNStaticData.Settings.IgnoreClampingModifiers.First()) : currentEvent.modifiers.HasAny(RNGNStaticData.Settings.IgnoreClampingModifiersMac.First());
        }
        
        internal static int GetUnitsAdjustment(Event currentEvent, int units, int maxUnits, bool scrollWheel, out bool clamp)
        {
            var _settings = RNGNStaticData.Settings;
            clamp = ShouldClampUnits(currentEvent);
            var multiply = currentEvent.modifiers.HasAny(_settings.UnitsMultiplierModifiers.First()) ? _settings.UnitsAdjustmentMultiplier : 1;
            if (scrollWheel) return GetScrollValue(currentEvent) * multiply;
            if (_settings.IncrementUnitsKeys.Contains(currentEvent.keyCode)) return 1 * multiply;
            if (_settings.DecrementUnitsKeys.Contains(currentEvent.keyCode)) return -1 * multiply;
            if (_settings.RefillUnitsKeys.Contains(currentEvent.keyCode)) return maxUnits;
            if (_settings.DepleteUnitsKeys.Contains(currentEvent.keyCode)) return -units;
            return 0;
        }
        
        internal static int GetMaxUnitsAdjustment(Event currentEvent, int maxUnits, bool scrollWheel, out bool clamp)
        {
            var _settings = RNGNStaticData.Settings;
            clamp = ShouldClampUnits(currentEvent);
            var multiply = currentEvent.modifiers.HasAny(_settings.UnitsMultiplierModifiers.First()) ? _settings.UnitsAdjustmentMultiplier : 1;
            if (scrollWheel) return GetScrollValue(currentEvent) * multiply;
            if (RNGNStaticData.Settings.IncrementUnitsKeys.Contains(currentEvent.keyCode)) return 1 * multiply;
            if (RNGNStaticData.Settings.DecrementUnitsKeys.Contains(currentEvent.keyCode)) return -1 * multiply;
            if (RNGNStaticData.Settings.DepleteUnitsKeys.Contains(currentEvent.keyCode)) return -maxUnits;
            return 0;
        }

        internal static int GetScrollValue(Event currentEvent)
        {
            var delta = currentEvent.delta.normalized;
            var scrollValue = delta.x == 0 ? (int)delta.y : (int)delta.x;
            if (scrollValue == 0) return 0;
            return RNGNStaticData.Settings.InvertScrollDirection ? scrollValue : scrollValue * -1;
        }
        
        internal static void ExecuteDepletableListAction(PropertyData propertyData)
        {
            switch (propertyData.DepletableListAction.Action)
            {
                case DepletableListAction.Refill:
                    propertyData.ProbabilityListEditorInterface.RefillItems();
                    break;
                case DepletableListAction.SetDepletable:
                    propertyData.ProbabilityListEditorInterface.SetAllItemsDepletable(propertyData.DrawerSettings.SetDepletableValue);
                    break;
                case DepletableListAction.SetUnits:
                    propertyData.ProbabilityListEditorInterface.SetAllItemsUnits(propertyData.DrawerSettings.SetUnitsValue);
                    break;
                case DepletableListAction.SetMaxUnits:
                    propertyData.ProbabilityListEditorInterface.SetAllItemsMaxUnits(propertyData.DrawerSettings.SetMaxUnitsValue);
                    break;
                case DepletableListAction.Reset:
                    propertyData.ProbabilityListEditorInterface.SetAllItemsDepletableProperties(propertyData.DrawerSettings.SetDepletableValue, propertyData.DrawerSettings.SetUnitsValue, propertyData.DrawerSettings.SetMaxUnitsValue);
                    break;
            }
        }
        
        internal static GUIContent GetDepletableListActionContent(DepletableListAction action, string title, string tooltip, PLDrawerSettings drawerSettings, bool dropdown = false)
        {
            var _tempContent = new GUIContent();
            switch (action)
            {
                case DepletableListAction.Refill:
                    _tempContent.text = title;
                    break;
                case DepletableListAction.SetDepletable:
                    var state = drawerSettings.SetDepletableValue ? "On" : "Off";
                    _tempContent.text = $"{title} ({state})";
                    break;
                case DepletableListAction.SetUnits:
                    _tempContent.text = $"{title} ({drawerSettings.SetUnitsValue})";
                    break;
                case DepletableListAction.SetMaxUnits:
                    _tempContent.text = $"{title} ({drawerSettings.SetMaxUnitsValue})";
                    break;
                case DepletableListAction.Reset:
                    var slash = dropdown ? "\u2044" : "/";
                    var resetState = drawerSettings.SetDepletableValue ? "On" : "Off";
                    _tempContent.text = $"{title} ({resetState} {drawerSettings.SetUnitsValue} {slash} {drawerSettings.SetMaxUnitsValue})";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _tempContent.tooltip = tooltip;
            return _tempContent;
        }
        
        public static float GetSubOptionsRectHeight(PLDrawerSettings drawerSettings, bool withAdvancedOptions)
        {
            switch (drawerSettings.DrawerOptionSection) // Set Sub Options rect height
            {
                case DrawerOptionSection.None:
                    return 0f;
                case DrawerOptionSection.Cog:
                    return withAdvancedOptions ? PLDrawerTheme.AdvancedOptionsTotalHeight + 52f: 58f;
                case DrawerOptionSection.Theme:
                    return 32f;
                case DrawerOptionSection.Picks:
                    return 58f;
            }

            return 0f;
        }

        public static GUIContent GetThemeSectionButtonContent(DrawerOptionsButtons drawerOptionsButtons, PLDrawerSettings drawerSettings)
        {
            switch (drawerOptionsButtons)
            {
                case DrawerOptionsButtons.Compact:
                    return drawerSettings.DrawerOptionSection == DrawerOptionSection.Theme ? PLDrawerContents.ThemeSectionButton : PLDrawerContents.ThemeSectionButtonCompact;
                case DrawerOptionsButtons.Full:
                    return PLDrawerContents.ThemeSectionButton;
                case DrawerOptionsButtons.Informative:
                    if (drawerSettings.Monochrome)
                    {
                        ThemeButtonTempContent.text = "Monochrome";
                        return ThemeButtonTempContent;
                    }
                    
                    var label = drawerSettings.PalettePath;
                    var lastIndex = label.LastIndexOf('/');

                    if (lastIndex != -1 && lastIndex + 1 < label.Length)
                    {
                        ThemeButtonTempContent.text = label.Substring(lastIndex + 1);
                        return ThemeButtonTempContent;
                    }
                    
                    ThemeButtonTempContent.text = label;
                    return ThemeButtonTempContent;
                default:
                    throw new ArgumentOutOfRangeException(nameof(drawerOptionsButtons), drawerOptionsButtons, null);
            }
        }

        public static GUIContent GetPickSectionButtonContent(DrawerOptionsButtons drawerOptionsButtons, PropertyData propertyData)
        {
            switch (drawerOptionsButtons)
            {
                case DrawerOptionsButtons.Compact:
                    return propertyData.DrawerSettings.DrawerOptionSection == DrawerOptionSection.Picks ? PLDrawerContents.PickSectionButton : PLDrawerContents.PickSectionButtonCompact;
                case DrawerOptionsButtons.Full:
                    return PLDrawerContents.PickSectionButton;
                case DrawerOptionsButtons.Informative:
                    var maintainInfo = propertyData.p_MaintainPickCountIfDisabled.boolValue ? "M " : "";
                    var preventRepeatInfo = "";
                    switch (propertyData.p_PreventRepeat.enumValueIndex)
                    {
                        case 0:
                            break;
                        case 1:
                        case 2:
                            preventRepeatInfo = $" {propertyData.p_PreventRepeat.enumDisplayNames[propertyData.p_PreventRepeat.enumValueIndex]}";
                            break;
                        case 3:
                            preventRepeatInfo = $" {propertyData.p_PreventRepeat.enumDisplayNames[propertyData.p_PreventRepeat.enumValueIndex]} ({propertyData.p_ShuffleIterations.intValue.ToString()})";
                            break;
                    }

                    var unitsInfo = "";
                    if(propertyData.p_DepletableList.boolValue)
                    {
                        unitsInfo = $" [{propertyData.TotalUnits.ToString()} / {propertyData.TotalMaxUnits.ToString()}]";
                    }

                    PickButtonTempContent.text = propertyData.p_LinkPickCounts.boolValue
                        ? $"{maintainInfo}{propertyData.p_PickCountMin.intValue.ToString()}{unitsInfo}{preventRepeatInfo}"
                        : $"{maintainInfo}{propertyData.p_PickCountMin.intValue.ToString()} - {propertyData.p_PickCountMax.intValue.ToString()}{unitsInfo}{preventRepeatInfo}";
                    
                    return PickButtonTempContent;
                default:
                    throw new ArgumentOutOfRangeException(nameof(drawerOptionsButtons), drawerOptionsButtons, null);
            }
        }
        
        public static void SetPalette(PropertyData propertyData, string palettePath)
        {
            propertyData.DrawerSettings.PalettePath = palettePath;
            propertyData.DrawerSettings.Monochrome = false;
            propertyData.StripeColors = RNGNStaticData.Settings.GetColorsFromPalette(propertyData.DrawerSettings.PalettePath);
            propertyData.SetupColors();
        }
    }
}