using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Random = Unity.Mathematics.Random;

namespace RNGNeeds
{
    public class LinearSearchMethodBurst : ISelectionMethod
    {
        public string Identifier => "LSMBurst";
        public string Name => "Linear Search [BURST]";
        
        #if UNITY_EDITOR
        public string EditorTooltip => "A simple linear search method. Fast for small lists, but performance degrades as the list size grows. Burst-compiled Job version, great for selecting large number of items in one pick.";
        #endif
        
        public NativeList<int> SelectItems(IProbabilityList probabilityList, int pickCount)
        {
            var _pickedIndices = new NativeList<int>(pickCount, Allocator.TempJob);
            
            var job = new LinearBurstSelectJob()
            {
                lastPickedIndices = _pickedIndices,
                ItemData = SelectionTools.GetItemData(probabilityList, Allocator.TempJob),
                JobData = SelectJobUtility.GetJobData(probabilityList, pickCount),
                DepletableList = probabilityList.IsDepletable
            };
            
            job.Prep();
            var handle = job.Schedule();
            handle.Complete();
            if(job.DepletableList) SelectionTools.ApplyChangesToList(probabilityList, job.ItemData);
            job.Clean();

            return _pickedIndices;
        }
        
        [BurstCompile(FloatPrecision.High, FloatMode.Strict)]
        private struct LinearBurstSelectJob : IJob
        {
            public NativeList<int> lastPickedIndices;
            public NativeArray<ItemData> ItemData;
            public SelectJobData JobData;
            public bool DepletableList;
            
            private NativeList<int> m_EnabledItemIndicesWithPositiveProb;

            private float totalProbability;
            
            public void Prep()
            {
                m_EnabledItemIndicesWithPositiveProb = new NativeList<int>(JobData.itemCount, Allocator.TempJob);
                
                totalProbability = 0f;
                
                for (var i = 0; i < JobData.itemCount; i++)
                {
                    var probabilityItem = ItemData[i];
                    if (probabilityItem.Probability <= 0f) continue;
                    totalProbability += probabilityItem.Probability;

                    if ((DepletableList && probabilityItem.IsSelectable) || (DepletableList == false && probabilityItem.Enabled))
                    {
                        m_EnabledItemIndicesWithPositiveProb.Add(i);    
                    }
                }
            }

            public void Execute()
            {
                if (m_EnabledItemIndicesWithPositiveProb.Length <= 0) return;
                
                var shouldPreventRepeatsInLoop = 
                    (JobData.preventRepeatMethod == PreventRepeatMethod.Spread || JobData.preventRepeatMethod == PreventRepeatMethod.Repick) && m_EnabledItemIndicesWithPositiveProb.Length > 1;

                var random = new Random(JobData.seed);
                var searchProbability = totalProbability + 0.0000001f;
                
                for (var i = 0; i < JobData.pickCount; i++)
                {
                    if (m_EnabledItemIndicesWithPositiveProb.Length == 0) break;
                    var selectedIndex = SelectionTools.LinearSearch(random.NextFloat(searchProbability), ItemData);
                    
                    switch (DepletableList)
                    {
                        case true:
                            if (ItemData[selectedIndex].IsSelectable == false)
                            {
                                if (JobData.maintainPickCountWhenDisabled) i--;
                                continue;
                            }
                            break;
                        case false:
                            if (ItemData[selectedIndex].Enabled == false)
                            {
                                if (JobData.maintainPickCountWhenDisabled) i--;
                                continue;
                            }
                            break;
                    }

                    if (shouldPreventRepeatsInLoop && JobData.lastPickedIndex == selectedIndex)
                    {
                        switch (JobData.preventRepeatMethod)
                        {
                            case PreventRepeatMethod.Spread:
                                selectedIndex = SelectionTools.SpreadResult(selectedIndex, m_EnabledItemIndicesWithPositiveProb, random);
                                break;
                            case PreventRepeatMethod.Repick:
                                i--;
                                continue;
                        }
                    }
                    
                    if (DepletableList)
                    {
                        var item = ItemData[selectedIndex];
                        if (item.Depletable)
                        {
                            item.ConsumeUnit();
                            ItemData[selectedIndex] = item;
                            if (item.Units == 0)
                            {
                                SelectionTools.RemoveItem(ref m_EnabledItemIndicesWithPositiveProb, selectedIndex);
                            }    
                        }
                    }

                    JobData.lastPickedIndex = selectedIndex;
                    lastPickedIndices.Add(selectedIndex);
                }
                
                if(JobData.preventRepeatMethod == PreventRepeatMethod.Shuffle && m_EnabledItemIndicesWithPositiveProb.Length > 1) SelectionTools.ShuffleResult(lastPickedIndices, JobData.shuffleIterations, random);
            }

            public void Clean()
            {
                ItemData.Dispose();
                m_EnabledItemIndicesWithPositiveProb.Dispose();
            }
        }
    }
}