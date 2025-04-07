#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RNGNeeds
{
    [Obsolete("Used only by RNGNeeds ProbabilityList Drawer")]
    public interface IProbabilityListEditorActions
    {
        Type ItemType { get; }
        ItemProviderType GetItemProviderTypes(int index);
        TestResults RunTest();
        List<Vector2> GetInfluencedProbabilitiesLimits { get; }
        
        int IndexOfUnremovableItem { get; }
        int UnlockedItemsCount { get; }
        bool IsListInfluenced { get; }
        string SelectionMethodID { get; set; }
        string ListName { get; set; }

        void AddDefaultItem();
        void AdjustItemBaseProbability(int index, float amount);
        void ResetAllProbabilities();
        bool RemoveItemAtIndex(int index, bool normalize = true);
        bool SetItemBaseProbability(int index, float probability, bool normalize = true);
        float GetItemBaseProbability(int index);
        float NormalizeProbabilities();
        
        void RefillItems();
        void SetAllItemsUnits(int units);
        void SetAllItemsMaxUnits(int maxUnits);
        void SetAllItemsDepletable(bool depletable);
        void SetAllItemsDepletableProperties(bool depletable = true, int units = 1, int maxUnits = 1);

        int TotalWeight { get; }
        int GetItemWeight(int index);
        void SetItemWeight(int index, int weight);
        void RecalibrateWeights();
        void ResetWeights();
        void CalculatePercentageFromWeights();
    }
}
#endif