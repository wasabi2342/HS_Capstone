using UnityEngine;

namespace RNGNeeds.Editor
{
    internal static class PLDrawerContents
    {
        private static readonly string DocsHint = "\nShift + Click -> Online Docs";
        public static readonly GUIContent ShowListEntriesButton = new GUIContent() { tooltip = "Show / Hide list entries."};
        public static readonly GUIContent ShowItemIndexButton = new GUIContent() { tooltip = "Show Item index on Stripe." };
        public static readonly GUIContent ShowItemInfoButton = new GUIContent() { tooltip = "Show Item info on Stripe." };
        public static readonly GUIContent CannotShowItemInfoButton = new GUIContent() { tooltip = "This type cannot show info on Stripe." };
        public static readonly GUIContent ShowItemPercentageButton = new GUIContent() { tooltip = "Show Item percentage on Stripe." };
        public static readonly GUIContent DimColorsButton = new GUIContent() { tooltip = "Dim colors by probability."};
        public static readonly GUIContent ReverseColorOrderButton = new GUIContent() { tooltip = "Toggle between normal and reversed color order."};
        public static readonly GUIContent ColorizeBarsButton = new GUIContent() { tooltip = "Colorize probability bars."};
        public static readonly GUIContent NormalizeBarsButton = new GUIContent() { tooltip = "Normalize probability bars."};
        public static readonly GUIContent ShowWeightsButton = new GUIContent() { tooltip = "Show / Hide item weights."};
        // public static readonly GUIContent PaletteDropdown = new GUIContent() { tooltip = "Select color palette from dropdown.\nShift + Scroll -> Change palette."};
        public static readonly GUIContent LinkPickCounts = new GUIContent() { tooltip = "Link Pick Count range to disable random pick counts." };
        public static readonly GUIContent UnlinkPickCounts = new GUIContent() { tooltip = "Unlink Pick Count range to enable random pick count. You can use curve to bias the resulting pick count." };
        public static readonly GUIContent TestButton = new GUIContent() { text = "TEST", tooltip = $"Run test with current settings.{DocsHint}"};
        public static readonly GUIContent ClearTestResultsButton = new GUIContent() { text = "X", tooltip = "Clear test results."};
        public static readonly GUIContent AddItemButton = new GUIContent() { text = "ADD", tooltip = "Add new item to list."};
        public static readonly GUIContent SetAsDefaultSettingsButton = new GUIContent() { text = "Set as Default Settings (!)", tooltip = "All settings of this drawer will be set as Defaults in Preferences/RNGNeeds."};
        public static readonly GUIContent GetNewDrawerIDButton = new GUIContent() { text = "Get New Drawer ID", tooltip = "If you are duplicating objects, they may end up sharing drawer settings. Clicking this button will create new, independent settings for the current drawer."};
        public static readonly GUIContent ResetSettingsToDefaultButton = new GUIContent() { text = "Reset Settings to Default", tooltip = "Will reset settings of this drawer to Defaults found in Preferences/RNGNeeds."};
        public static readonly GUIContent ResetProbabilitiesButton = new GUIContent() { text = "Reset Probabilities", tooltip = "Will even out probabilities of unlocked items in list."};
        
        public static readonly GUIContent StripePercentageDigits = new GUIContent() { text = "Stripe % Digits", tooltip = "Number of percentage digits shown on the Stripe."};
        public static readonly GUIContent ItemPercentageDigits = new GUIContent() { text = "Item % Digits", tooltip = "Number of percentage digits shown for Item probability value."};
        public static readonly GUIContent TestPercentageDigits = new GUIContent() { text = "Test % Digits", tooltip = "Number of percentage digits shown for test results."};
        public static readonly GUIContent TestColorSensitivity = new GUIContent() { text = "Test Color Bias", tooltip = "Colorize strength of test results. Use higher setting to spot small differences."};
        
        public static readonly GUIContent InfluenceProviderButton = new GUIContent() { text = "Show Influence Toggle", tooltip = $"Allow assigning Influence Provider to list items. If any item has provider assigned, the list is considered as 'influenced' and will always show the toggle.{DocsHint}"};
        public static readonly GUIContent SpreadPercentageDigits = new GUIContent() { text = "Spread % Digits", tooltip = "Number of percentage digits in Influence Spread values."};
        public static readonly GUIContent SpreadColorizeSensitivity = new GUIContent() { text = "Spread Color Bias", tooltip = "Colorize strength of Influence Spread values. Use higher setting to spot small differences."};
        public static readonly GUIContent ShowSpreadOnBars = new GUIContent() { text = "Show Spread on Bars", tooltip = "Probability bars of items that are not influenced will show their adjusted probabilities based on influenced items. Top part of bar will represent maximum positive influence, bottom part will show maximum negative influence."};
        
        public static readonly GUIContent ItemUtilitySpace = new GUIContent() { text = "Item Utility Space", tooltip = "The width of the list element reserved for item data and values."};

        public const string SelectionMethodTooltip = "Selection Method\nMathematical approach to selecting random item based on probability distribution. For most cases in game development the default 'Linear Search' is the ideal option.";
        public static readonly GUIContent MaintainPickCountButton = new GUIContent() { text = "Maintain Pick Count", tooltip = $"If list has disabled Items, resulting pick count might be lower than desired. This option will respect pick count by continuing the selection process until the limit is reached. However, resulting probability distribution might be different.{DocsHint}"};
        public static readonly GUIContent SeedReadoutField = new GUIContent() { tooltip = "The last seed used by this list."};
        public static readonly GUIContent KeepSeedButton = new GUIContent() { text = "Keep Seed", tooltip = $"If on, this list will not re-seed on each pick.\nBy default, each ProbabilityList will get new seed before each pick.{DocsHint}" };
        
        public static readonly GUIContent ThemeSectionButton = new GUIContent() { text = "THEME"};
        public static readonly GUIContent ThemeSectionButtonCompact = new GUIContent() { text = "T"};
        public static readonly GUIContent PickSectionButton = new GUIContent() { text = "PICK"};
        public static readonly GUIContent PickSectionButtonCompact = new GUIContent() { text = "P"};
        
        public static readonly GUIContent StripeHeightSlider = new GUIContent() { text = "STRIPE HEIGHT"};
        
        public static readonly GUIContent InfluenceInvertToggle = new GUIContent() { text = "Invert Influence", tooltip = "Reverses the influence effect on this item's probability."};
        
        public static readonly GUIContent CollectionLocked = new GUIContent() { tooltip = "Locked Collection. Unlock to enable removing of lists in editor."};
        public static readonly GUIContent CollectionUnlocked = new GUIContent() { tooltip = "Unlocked Collection. Lock to disable removing of lists in editor."};
        public static readonly GUIContent CollectionAllowReorder = new GUIContent() { tooltip = "Enable / Disable Reordering of lists in editor."};
        public static readonly GUIContent MoveUpButton = new GUIContent() { tooltip = "Move List Up"};
        public static readonly GUIContent MoveDownButton = new GUIContent() { tooltip = "Move List Down"};
        
        public static readonly GUIContent DepletableListButton = new GUIContent() { text = "Depletable", tooltip = $"Set list as depletable. In depletable lists, items have units which are consumed when picked.{DocsHint}"};
        public static readonly GUIContent DepletableActionExecuteButton = new GUIContent() { tooltip = "Executes the selected action."};
        public static readonly GUIContent DepletableStripeButton = new GUIContent() { tooltip = "Visualize remaining units in The Stripe."};
        
        public static readonly GUIContent UnitsAdjustmentMultiplier = new GUIContent() { text = "Units Adjustment Multiplier", tooltip = "When using Multiply Units Modifier, this value will be used as a factor."};
        public static readonly GUIContent AllowScrollWheelUnitsAdjustment = new GUIContent() { text = "Scroll Wheel Units Adjustment", tooltip = "Enable / Disable adjusting units with scroll wheel."};
        public static readonly GUIContent IncrementUnitsKeyDropdown = new GUIContent() { text = "Increment Units Key", tooltip = "Key used to increment units of selected item."};
        public static readonly GUIContent DecrementUnitsKeyDropdown = new GUIContent() { text = "Decrement Units Key", tooltip = "Key used to decrement units of selected item."};
        public static readonly GUIContent UnitsMultiplierKeyDropdown = new GUIContent() { text = "Multiply Units Modifier", tooltip = "Hold down while incrementing or decrementing units to multiply the change by a factor."};
        public static readonly GUIContent RefillUnitsKeyDropdown = new GUIContent() { text = "Refill Units Key", tooltip = "Key used to fill units of selected item to maximum."};
        public static readonly GUIContent DepleteUnitsKeyDropdown = new GUIContent() { text = "Deplete Units Key", tooltip = "Key used to deplete units of selected item to minimum."};
        public static readonly GUIContent IgnoreClampingKeyDropdown = new GUIContent() { text = "Ignore Clamping Modifier", tooltip = "Modifier key to bypass clamping to max units."};
        
        public static readonly GUIContent WeightsPriorityButtonOn = new GUIContent() { tooltip = "Weights Priority is ON\nIn general, weights produce probabilities with lower precision. This can lead to slight inconsistencies between percentile probabilities and weights. With Weights Priority, the probabilities of items will 'snap' to fractions based on weights."};
        public static readonly GUIContent WeightsPriorityButtonOff = new GUIContent() { tooltip = "Weights Priority is OFF\nIn general, weights produce probabilities with lower precision. This can lead to slight inconsistencies between percentile probabilities and weights. With Weights Priority, the probabilities of items will 'snap' to fractions based on weights."};
        public static readonly GUIContent ResetWeightsButton = new GUIContent() { tooltip = "Reset Weights\nWill reset weights of all items in list. The highest percentage will equal to the Base Weight of the list. You can adjust the Base Weight using the field to the left."};
        
        // public static readonly GUIContent Template = new GUIContent() { };
    }
}