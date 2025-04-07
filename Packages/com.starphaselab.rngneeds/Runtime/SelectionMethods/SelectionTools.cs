using Unity.Collections;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace RNGNeeds
{
    public struct ItemData
    {
        public readonly int Index;
        public readonly bool Enabled;
        public readonly float Probability;
        public readonly bool Depletable;
        public int Units;
        
        public void ConsumeUnit()
        {
            Units--;
        }
        
        public bool IsSelectable => Depletable ? Enabled && Units > 0 : Enabled;

        public ItemData(int index, bool enabled, float probability, bool depletable, int units)
        {
            Index = index;
            Enabled = enabled;
            Probability = probability;
            Depletable = depletable;
            Units = units;
        }
    }

    internal static class SelectionTools
    {
        public static void RemoveItem(ref NativeList<int> indices, int index)
        {
            var currentIndex = indices.IndexOf(index);
            if (currentIndex == -1) return;
            
            indices.RemoveAt(currentIndex);
        }

        public static void ApplyChangesToList(IProbabilityList list, NativeArray<ItemData> itemData)
        {
            foreach (var item in itemData) list.Item(item.Index).Units = item.Units;
        }
        
        public static int SpreadResult(int index, NativeList<int> spreadMap, Random random)
        {
            var currentIndex = spreadMap.IndexOf(index);
            if (currentIndex == -1) return index;
            
            var chooseNext = random.NextFloat() > 0.5f;

            if (chooseNext) index = spreadMap[(currentIndex + 1) % spreadMap.Length];
            else index = spreadMap[(currentIndex - 1 + spreadMap.Length) % spreadMap.Length];
        
            return index;
        }
        
        public static void ShuffleResult(NativeList<int> indices, int iterations, Random random)
        {
            var hasTwoDistinctIndices = false;
            for (var i = 1; i < indices.Length; i++)
            {
                if (indices[i] == indices[i - 1]) continue;
                hasTwoDistinctIndices = true;
                break;
            }
            
            if (hasTwoDistinctIndices == false) return;
            
            for(var iteration = 1; iteration <= iterations; iteration++)
            {
                for (var i = 1; i < indices.Length; i++)
                {
                    if (indices[i] != indices[i - 1]) continue;
                    int randomIndex;
                    do
                    {
                        randomIndex = random.NextInt(0, indices.Length);
                    } while (randomIndex == i || randomIndex == i - 1 || indices[randomIndex] == indices[i]);
        
                    (indices[i], indices[randomIndex]) = (indices[randomIndex], indices[i]);
                }
            }
        }
        
        public static int BinarySearch(NativeList<float> array, float target, float epsilon = 1e-7f)
        {
            var left = 0;
            var right = array.Length - 1;
            
            while (left <= right)
            {
                var middle = left + ((right - left) / 2);

                if (math.abs(array[middle] - target) < epsilon) return middle;

                if (array[middle] < target) left = middle + 1;
                else right = middle - 1;
            }

            return left;
        }

        public static int LinearSearch(float randomValue, NativeArray<ItemData> itemData)
        {
            var cumulativeProbability = 0f;
            foreach (var item in itemData)
            {
                if(item.Probability <= 0f) continue;
                cumulativeProbability += item.Probability;
                if (randomValue <= cumulativeProbability == false) continue;
                return item.Index;
            }
        
            // execution should never reach this point
            return -1;
        }

        public static NativeArray<ItemData> GetItemData(IProbabilityList probabilityList, Allocator allocator)
        {
            var itemsInList = probabilityList.ItemCount;
            var items = new NativeArray<ItemData>(itemsInList, allocator);
            
            for (var i = 0; i < itemsInList; i++)
            {
                var probabilityItem = probabilityList.Item(i);
                items[i] = new ItemData(i, probabilityItem.Enabled, probabilityItem.Probability, probabilityItem.IsDepletable, probabilityItem.Units);
            }

            return items;
        }
    }
}