using System;
using UnityEngine;

namespace RNGNeeds.Editor
{
    [Serializable]
    internal class PLDrawerSettings
    {
        [HideInInspector] public string DrawerID;
        [HideInInspector] public long Created;
        [HideInInspector] public long Modified;
        [HideInInspector] public DrawerOptionSection DrawerOptionSection;
        [HideInInspector] public StripeHeight StripeHeight;
        
        [Header("Theme Options")]
        public string PalettePath;
        public bool ReverseColorOrder;
        public bool DimColors;
        public bool Monochrome;
        [HideInInspector] public Color MonochromeColor;
        
        [Header("Info Displayed on the Stripe")]
        public bool ShowIndex;
        public bool ShowInfo;
        public bool ShowPercentage;
        public bool ShowWeights;
        public bool ShowPercentageOrWeights => ShowPercentage || ShowWeights;
        
        [Header("Stripe Options")]
        [Range(24, 100)] public int StripeHeightPixels;
        [Range(0, 2)] public int StripePercentageDigits;
        public bool VisualizeDepletableItems;

        [Header("List Entry Options")] 
        public bool HideListEntries;
        [Range(0, 5)] public int ItemPercentageDigits;
        [Range(0, 5)] public int TestPercentageDigits;
        public bool ColorizePreviewBars;
        public bool NormalizePreviewBars;
        [Range(0f, 10f)] public float TestColorizeSensitivity;
        [Range(0f, 10f)] public float SpreadColorizeSensitivity;
        
        [Range(.2f, 1f)] public float ElementInfoSpace;

        [Header("Probability Influence Options")]
        public bool ShowInfluenceToggle;
        public bool ShowSpreadOnBars;
        [Range(0, 2)] public int SpreadPercentageDigits;
        
        [Header("Depletable List Actions - Defaults")]
        public DepletableListAction DepletableListAction;
        public int SetUnitsValue = 1;
        public int SetMaxUnitsValue = 1;
        public bool SetDepletableValue = true;
        
        public bool ColorizeTestResults => TestColorizeSensitivity > 0;
        public bool ColorizeSpreadDifference => SpreadColorizeSensitivity > 0;

        public void ApplySettings(PLDrawerSettings settings)
        {
            Modified = DateTime.Now.Ticks;
            ShowInfluenceToggle = settings.ShowInfluenceToggle;
            
            PalettePath = settings.PalettePath;
            ShowIndex = settings.ShowIndex;
            ShowInfo = settings.ShowInfo;
            ShowPercentage = settings.ShowPercentage;
            ShowWeights = settings.ShowWeights;
            HideListEntries = settings.HideListEntries;
            ReverseColorOrder = settings.ReverseColorOrder;
            DimColors = settings.DimColors;
            Monochrome = settings.Monochrome;
            MonochromeColor = settings.MonochromeColor;
            StripeHeight = settings.StripeHeight;
            StripeHeightPixels = settings.StripeHeightPixels;

            StripePercentageDigits = settings.StripePercentageDigits;
            ItemPercentageDigits = settings.ItemPercentageDigits;
            SpreadPercentageDigits = settings.SpreadPercentageDigits;
            TestPercentageDigits = settings.TestPercentageDigits;
            
            ElementInfoSpace = settings.ElementInfoSpace;
            
            ColorizePreviewBars = settings.ColorizePreviewBars;
            NormalizePreviewBars = settings.NormalizePreviewBars;

            TestColorizeSensitivity = settings.TestColorizeSensitivity;

            SpreadColorizeSensitivity = settings.SpreadColorizeSensitivity;
            ShowSpreadOnBars = settings.ShowSpreadOnBars;
            
            DepletableListAction = settings.DepletableListAction;
            SetUnitsValue = settings.SetUnitsValue; 
            SetMaxUnitsValue = settings.SetMaxUnitsValue;
            SetDepletableValue = settings.SetDepletableValue;
            VisualizeDepletableItems = settings.VisualizeDepletableItems;
        }
    }
}