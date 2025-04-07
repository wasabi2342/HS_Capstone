using System.Collections.Generic;
using UnityEngine;

namespace RNGNeeds.Editor
{
    internal static class RNGNStaticData
    {
        public static RNGNeedsSettings Settings { get; private set; }
        public static readonly bool WindowsOrLinuxEditor;
        public const string PathToEditorAssets = "Packages/com.starphaselab.rngneeds/Editor/Assets/";
        public static string CurrentVersion { get; private set; }

        static RNGNStaticData()
        {
            CurrentVersion = UnityEditor.PackageManager.PackageInfo.FindForAssetPath("Packages/com.starphaselab.rngneeds/package.json").version;
            SetSettings();
            WindowsOrLinuxEditor = Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.LinuxEditor;
        }

        public static void SetSettings()
        {
            Settings = RNGNeedsSettings.instance;
        }

        public const string OverwritePalettesWindowTitle = "Overwrite Palettes?";
        public const string OverwritePalettesWindowText = "This will remove all palettes and import those found in XML. Any changes to current list of palettes will be lost.";
        public const string OverwritePalettesWindowConfirm = "Load Palettes";
        
        public const string ResetPalettesWindowTitle = "Reset Palettes to Defaults?";
        public const string ResetPalettesWindowText = "This will remove all palettes and import the default list. Any changes to current list of palettes will be lost.";
        public const string ResetPalettesWindowConfirm = "Reset Palettes";

        public const string ResetPreferencesWindowTitle = "Reset Preferences?";
        public const string ResetPreferencesWindowText = "This will reset all RNGNeeds settings and options to defaults. All changes (excluding color palettes) will be lost.";
        public const string ResetPreferencesWindowConfirm = "Reset Preferences";
        
        public const string ConfirmWindowCancel = "Cancel";

        public static readonly Color DefaultMonochromeColor = new Color(1f, .78f, .4f, 1f);
        public static readonly Color DefaultMonochromeColorLight = new Color(1f, .90f, .52f, 1f);
        public static readonly GradientColorKey[] TestGradientColorKeys = new GradientColorKey[] { new GradientColorKey(new Color(1f, .66f, 0f), 0f), new GradientColorKey(Color.white, .5f), new GradientColorKey(new Color(0f, 1f, .62f), 1f) };
        public static readonly GradientColorKey[] TestGradientColorKeysLight = new GradientColorKey[] { new GradientColorKey(new Color(.83f, .37f, 0f), 0f), new GradientColorKey(new Color(.15f, .15f, .15f), .5f), new GradientColorKey(new Color(0f, .52f, .53f), 1f) };
        public static readonly GradientColorKey[] SpreadGradientColorKeys = new GradientColorKey[] { new GradientColorKey(new Color(1f, .45f, .54f), 0f), new GradientColorKey(Color.white, .5f), new GradientColorKey(new Color(0f, 0.9f, 1f), 1f) };
        public static readonly GradientColorKey[] SpreadGradientColorKeysLight = new GradientColorKey[] { new GradientColorKey(new Color(.84f, .09f, .22f), 0f), new GradientColorKey(new Color(.1f, .1f, .1f), .5f), new GradientColorKey(new Color(.17f, .30f, .87f), 1f) };
        public static readonly GradientAlphaKey[] GradientAlphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1, 1f) };

        public static readonly List<Color> DefaultPaletteColors = new List<Color>()
        {
            new Color(0.434f, 0.731f, 0.830f),
            new Color(0.420f, 0.532f, 0.981f),
            new Color(0.653f, 0.476f, 0.981f),
            new Color(0.874f, 0.498f, 0.953f),
            new Color(0.991f, 0.462f, 0.697f),
            new Color(0.962f, 0.448f, 0.491f),
            new Color(0.981f, 0.562f, 0.439f),
            new Color(0.934f, 0.781f, 0.453f),
            new Color(0.955f, 0.962f, 0.530f),
            new Color(0.739f, 1.000f, 0.645f)
        };
        
        public static readonly PLDrawerSettings DefaultDrawerSettings = new PLDrawerSettings()
        {
            DrawerID = "DefaultSettings",
            DrawerOptionSection = DrawerOptionSection.None,
            ShowInfluenceToggle = false,
            
            PalettePath = "Default",
            ShowIndex = false,
            ShowInfo = true,
            ShowPercentage = true,
            ShowWeights = false,
            HideListEntries = false,
            ReverseColorOrder = false,
            DimColors = true,
            Monochrome = false,
            
            StripeHeight = StripeHeight.Normal,
            StripeHeightPixels = 50,
            
            StripePercentageDigits = 0,
            ItemPercentageDigits = 2,
            SpreadPercentageDigits = 1,
            TestPercentageDigits = 2,
            ElementInfoSpace = .5f,
            
            ColorizePreviewBars = true,
            NormalizePreviewBars = false,
        
            TestColorizeSensitivity = 3,
        
            SpreadColorizeSensitivity = 2,
            ShowSpreadOnBars = false,
            
            DepletableListAction = DepletableListAction.Refill,
            SetUnitsValue = 1,
            SetMaxUnitsValue = 1,
            SetDepletableValue = true,
            VisualizeDepletableItems = false,
        };

        internal static readonly (DepletableListAction Action, string Title, string Tooltip)[] DepletableListActions = new (DepletableListAction, string, string)[]
        {
            (DepletableListAction.Refill, "Refill Items", "Refills all items in the list."),
            (DepletableListAction.SetDepletable, "Set Depletable", "Sets depletable state of all items in the list."),
            (DepletableListAction.SetUnits, "Set Units", "Sets the units of all items in the list."),
            (DepletableListAction.SetMaxUnits, "Set Max Units", "Sets the max units of all items in the list."),
            (DepletableListAction.Reset, "Reset", "Sets all properties to this drawer's default values.")
        };
        
        public const string LinkDocumentationManual = "https://docs.rngneeds.com/user-guide/introduction";
        public const string LinkDocsQuickStartGuide = "https://docs.rngneeds.com/user-guide/getting-started#quick-start";
        public const string LinkDocsSamplesOverview = "https://docs.rngneeds.com/samples/samples-overview#exploring-the-samples";
        public const string LinkDocsImportingSamples = "https://docs.rngneeds.com/samples/samples-overview";
        public const string LinkDocsTestingOutcomes = "https://docs.rngneeds.com/documentation/testing-outcomes";
        public const string LinkDocsDisabledItems = "https://docs.rngneeds.com/documentation/selecting-values#disabled-items";
        public const string LinkDocsKeepSeed = "https://docs.rngneeds.com/documentation/seeding-options#keep-seed";
        public const string LinkDocsProbabilityInfluence = "https://docs.rngneeds.com/documentation/probability-influence";
        public const string LinkDocsDepletableLists = "https://docs.rngneeds.com/documentation/depletable-lists";
    }
}