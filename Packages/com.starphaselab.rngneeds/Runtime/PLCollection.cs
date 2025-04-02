using System;
using System.Collections.Generic;
using StarphaseTools.Core;
using UnityEngine;

namespace RNGNeeds
{
#pragma warning disable 0618
    /// <summary>
    /// A collection class for organizing multiple ProbabilityLists into an array.
    /// </summary>
    [Serializable]
    public class PLCollection<T>
        #if UNITY_EDITOR
            : IPLCollectionEditorActions
        #endif
    {
        [SerializeField] private List<ProbabilityList<T>> pl_collection = new List<ProbabilityList<T>>();
 
        /// <summary>
        /// Gets the Type of items stored in this Collection and its Probability Lists.
        /// </summary>
        public Type ItemType() => typeof(T);
        
        /// <summary>
        /// Gets the number of Probability Lists stored in this Collection.
        /// </summary>
        public int Count => pl_collection.Count;
        
        #region Add List
        
        /// <summary>
        /// Adds a provided <see cref="ProbabilityList{T}"/> with its name to the collection.
        /// </summary>
        /// <param name="list">The Probability List to add.</param>
        /// <param name="name">Optional string name to associate with this list.</param>
        /// <remarks>
        /// If name is not provided, the list will get a name automatically based on its resulting index in the collection.
        /// Automatic name format: "#index" (for example "#1")
        /// </remarks>
        public void AddList(ProbabilityList<T> list, string name = "")
        {
            if (list == null)
            {
                throw new ArgumentNullException(nameof(list), "List cannot be null");
            }
            
            if(string.IsNullOrEmpty(list.ListName)) list.ListName = string.IsNullOrEmpty(name) ? $"#{pl_collection.Count}" : name;
            pl_collection.Add(list);
        }
        
        /// <summary>
        /// Creates a new <see cref="ProbabilityList{T}"/> and adds it to the collection.
        /// </summary>
        /// <param name="name">Optional string name to associate with this list.</param>
        /// <remarks>
        /// If name is not provided, the list will get a name automatically based on its resulting index in the collection.
        /// Automatic name format: "#index" (for example "#1")
        /// </remarks>
        public void AddList(string name = "")
        {
            AddList(new ProbabilityList<T>(), name);
        }
        
        #endregion
        
        #region Remove List
        
        /// <summary>
        /// Removes the Probability List at the specified index from the collection.
        /// </summary>
        /// <param name="index">The index of the Probability List to remove.</param>
        /// <returns>True if the list was found and removed, otherwise false.</returns>
        public bool RemoveList(int index)
        {
            if (GetList(index, out _) == false) return false;
            pl_collection.RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Removes the Probability List with the specified name from the collection.
        /// </summary>
        /// <param name="name">The name of the Probability List to remove.</param>
        /// <returns>True if the list was found and removed, otherwise false.</returns>
        public bool RemoveList(string name)
        {
            return GetList(name, out var list) && pl_collection.Remove(list);
        }
        
        #endregion
        
        #region Get List
        
        /// <summary>
        /// Retrieves a Probability List at the specified index from the collection.
        /// </summary>
        /// <param name="index">The index of the Probability List to retrieve.</param>
        /// <returns>Returns the <see cref="ProbabilityList{T}"/> at the specified index or null if the index is out of range.</returns>
        public ProbabilityList<T> GetList(int index)
        {
            if (index < 0 || index > Count - 1) return null;
            return pl_collection[index];
        }
        
        /// <summary>
        /// Retrieves a Probability List with specified name from the collection.
        /// </summary>
        /// <param name="name">The name associated with the Probability List to retrieve.</param>
        /// <returns>Returns the <see cref="ProbabilityList{T}"/> with specified name or null if the list was not found.</returns>
        public ProbabilityList<T> GetList(string name)
        {
            foreach (var probabilityList in pl_collection)
            {
                if (probabilityList.ListName.Equals(name) == false) continue;
                return probabilityList;
            }
            
            return null;
        }
        
        /// <summary>
        /// Tries to retrieve a Probability List at the specified index from the collection.
        /// </summary>
        /// <param name="index">The index of the Probability List to retrieve.</param>
        /// <param name="list">The retrieved <see cref="ProbabilityList{T}"/> if found, otherwise null.</param>
        /// <returns>True if the list was found, otherwise false.</returns>
        public bool GetList(int index, out ProbabilityList<T> list)
        {
            list = null;
            if (index < 0 || index > Count - 1) return false;
            list = pl_collection[index];
            return true;
        }

        /// <summary>
        /// Tries to retrieve a Probability List with specified name from the collection.
        /// </summary>
        /// <param name="name">The name associated with the Probability List to retrieve.</param>
        /// <param name="list">The retrieved <see cref="ProbabilityList{T}"/> if found, otherwise null.</param>
        /// <returns>True if the list was found, otherwise false.</returns>
        public bool GetList(string name, out ProbabilityList<T> list)
        {
            foreach (var probabilityList in pl_collection)
            {
                if (probabilityList.ListName.Equals(name) == false) continue;
                list = probabilityList;
                return true;
            }
            
            list = null;

            return false;
        }
        
        #endregion

        #region List Operations
        
        /// <summary>
        /// Sets the name of the Probability List at the specified index.
        /// </summary>
        /// <param name="index">The index of the Probability List.</param>
        /// <param name="name">The name to associate with the list.</param>
        /// <returns>True if the list was found and renamed, otherwise false.</returns>
        public bool SetListName(int index, string name)
        {
            if (GetList(index, out var list) == false) return false;
            list.ListName = name;
            return true;
        }
        
        /// <summary>
        /// Sets a new name of the Probability List with the specified name.
        /// </summary>
        /// <param name="oldName">The old name of the Probability List.</param>
        /// <param name="newName">The new name to associate with the list.</param>
        /// <returns>True if the list was found and renamed, otherwise false.</returns>
        public bool SetListName(string oldName, string newName)
        {
            if (GetList(oldName, out var list) == false) return false;
            list.ListName = newName;
            return true;
        }

        /// <summary>
        /// Clears the entire collection of Probability Lists.
        /// </summary>
        public void ClearCollection()
        {
            pl_collection.Clear();
        }

        /// <summary>
        /// Clears the Probability List at the specified index.
        /// </summary>
        /// <param name="index">The index of the Probability List to clear.</param>
        /// <returns>True if the list was found and cleared, otherwise false.</returns>
        public bool ClearList(int index)
        {
            if (GetList(index, out var list) == false) return false;
            list.ClearList();
            return true;
        }
        
        /// <summary>
        /// Clears the Probability List with the specified name.
        /// </summary>
        /// <param name="name">The name of the Probability List to clear.</param>
        /// <returns>True if the list was found and cleared, otherwise false.</returns>
        public bool ClearList(string name)
        {
            if (GetList(name, out var list) == false) return false;
            list.ClearList();
            return true;
        }

        /// <summary>
        /// Clears all Probability Lists in the collection.
        /// </summary>
        public void ClearAllLists()
        {
            foreach (var list in pl_collection) list.ClearList();
        }

        /// <summary>
        /// Checks if the Probability List at the specified index is empty.
        /// </summary>
        /// <param name="index">The index of the Probability List to check.</param>
        /// <returns>
        /// A tuple with the following elements:
        /// <list type="bullet">
        /// <item>bool ListFound - Indicates whether the Probability List was found.</item>
        /// <item>bool IsEmpty - Indicates whether the Probability List is empty. This is only valid if ListFound is true.</item>
        /// </list>
        /// </returns>
        public (bool ListFound, bool IsEmpty) IsListEmpty(int index)
        {
            return GetList(index, out var list) == false ? (false, false) : (true, list.ItemCount == 0);
        }
        
        /// <summary>
        /// Checks if the Probability List with the specified name is empty.
        /// </summary>
        /// <param name="name">The name of the Probability List to check.</param>
        /// <returns>
        /// A tuple with the following elements:
        /// <list type="bullet">
        /// <item>bool ListFound - Indicates whether the Probability List was found.</item>
        /// <item>bool IsEmpty - Indicates whether the Probability List is empty. This is only valid if ListFound is true.</item>
        /// </list>
        /// </returns>
        public (bool ListFound, bool IsEmpty) IsListEmpty(string name)
        {
            return GetList(name, out var list) == false ? (false, false) : (true, list.ItemCount == 0);
        }

        /// <summary>
        /// Moves the list up within the collection.
        /// </summary>
        /// <param name="index">The index of the list to move.</param>
        /// <returns>Returns true if the list was successfully moved up. Returns false if the list is already at the top of the collection or if the index is out of range.</returns>
        /// <remarks>The first list in the collection cannot be moved up.</remarks>
        public bool MoveListUp(int index)
        {
            if (index <= 0 || index >= pl_collection.Count) return false;
            pl_collection.Swap(index, index - 1);
            return true;
        }

        /// <summary>
        /// Moves the list down within the collection.
        /// </summary>
        /// <param name="index">The index of the list to move.</param>
        /// <returns>Returns true if the list was successfully moved down. Returns false if the list is already at the bottom of the collection or if the index is out of range.</returns>
        /// <remarks>The last list in the collection cannot be moved down.</remarks>
        public bool MoveListDown(int index)
        {
            if (index < 0 || index >= pl_collection.Count - 1) return false;
            pl_collection.Swap(index, index + 1);
            return true;
        }
        
        #endregion
        
        #region Pick Value From

        /// <summary>
        /// Picks a single value from Probability List at specified index in the collection.
        /// </summary>
        /// <param name="index">The index of the Probability List.</param>
        /// <param name="value">Result of <see cref="ProbabilityList{T}.TryPickValue(out T)"/> method of the list, or default if no value was picked.</param>
        /// <returns>True if a value was picked, otherwise false.</returns>
        /// <remarks>Use this simplified method if you are sure the list exists in the collection.
        /// If no value was picked, it could also mean the list was not found.
        /// If you need to determine whether the list was found as well, use <see cref="TryPickValueFrom(int, out T)"/> method instead.
        /// </remarks>
        public bool PickValueFrom(int index, out T value)
        {
            return TryPickValueFrom(index, out value).ValuePicked;
        }
        
        /// <summary>
        /// Picks a single value from Probability List with specified name in the collection.
        /// </summary>
        /// <param name="name">The name of the Probability List.</param>
        /// <param name="value">Result of <see cref="ProbabilityList{T}.TryPickValue(out T)"/> method of the list, or default if no value was picked.</param>
        /// <returns>True if a value was picked, otherwise false.</returns>
        /// <remarks>Use this simplified method if you are sure the list exists in the collection.
        /// If no value was picked, it could also mean the list was not found.
        /// If you need to determine whether the list was found as well, use <see cref="TryPickValueFrom(string, out T)"/> method instead.
        /// </remarks>
        public bool PickValueFrom(string name, out T value)
        {
            return TryPickValueFrom(name, out value).ValuePicked;
        }

        #endregion

        #region Try Pick Value From
        
        /// <summary>
        /// Picks a single value from Probability List at specified index in the collection.
        /// </summary>
        /// <param name="index">The index of the Probability List.</param>
        /// <param name="value">Result of <see cref="ProbabilityList{T}.TryPickValue(out T)"/> method of the list, or default if the list was not found.</param>
        /// <returns>
        /// A tuple with the following elements:
        /// <list type="bullet">
        /// <item>bool ListFound - Indicates whether the Probability List was found.</item>
        /// <item>bool ValuePicked - Indicates whether a value was successfully picked from the list. This is only valid if ListFound is true.</item>
        /// </list>
        /// </returns>
        public (bool ListFound, bool ValuePicked) TryPickValueFrom(int index, out T value)
        {
            if (GetList(index, out var list))
            {
                if (list.TryPickValue(out value) == false) return (true, false);
                return (true, true);
            }
            
            value = default;
            return (false, false);
        }
        
        /// <summary>
        /// Picks a single value from Probability List with specified name in the collection.
        /// </summary>
        /// <param name="name">The name of the Probability List.</param>
        /// <param name="value">Result of <see cref="ProbabilityList{T}.TryPickValue(out T)"/> method of the list, or default if the list was not found.</param>
        /// <returns>
        /// A tuple with the following elements:
        /// <list type="bullet">
        /// <item>bool ListFound - Indicates whether the Probability List was found.</item>
        /// <item>bool ValuePicked - Indicates whether a value was successfully picked from the list. This is only valid if ListFound is true.</item>
        /// </list>
        /// </returns>
        public (bool ListFound, bool ValuePicked) TryPickValueFrom(string name, out T value)
        {
            if (GetList(name, out var list))
            {
                if (list.TryPickValue(out value) == false) return (true, false);
                return (true, true);
            }
            
            value = default;
            return (false, false);
        }
        
        #endregion
        
        #region Pick Values From
        
        /// <summary>
        /// Picks values from Probability List at specified index in the collection.
        /// </summary>
        /// <param name="index">The index of the Probability List.</param>
        /// <returns>Result of <see cref="ProbabilityList{T}.PickValues()"/> method of the list, or null if the list was not found.</returns>
        public List<T> PickValuesFrom(int index)
        {
            return GetList(index, out var list) ? list.PickValues() : null;
        }
        
        /// <summary>
        /// Picks values from Probability List with specified name in the collection.
        /// </summary>
        /// <param name="name">The name of the Probability List.</param>
        /// <returns>Result of <see cref="ProbabilityList{T}.PickValues()"/> method of the list, or null if the list was not found.</returns>
        public List<T> PickValuesFrom(string name)
        {
            return GetList(name, out var list) ? list.PickValues() : null;
        }
        
        /// <summary>
        /// Picks values from Probability List at specified index in the collection.
        /// </summary>
        /// <param name="index">The index of the Probability List.</param>
        /// <param name="values">Result of <see cref="ProbabilityList{T}.PickValues()"/> method of the list, or null if the list was not found.</param>
        /// <returns>True if the list was found, otherwise false.</returns>
        public bool PickValuesFrom(int index, out List<T> values)
        {
            if (GetList(index, out var list))
            {
                values = list.PickValues();
                return true;
            }
            
            values = null;
            return false;
        }
        
        /// <summary>
        /// Picks values from Probability List with specified name in the collection.
        /// </summary>
        /// <param name="name">The name of the Probability List.</param>
        /// <param name="values">Result of <see cref="ProbabilityList{T}.PickValues()"/> method of the list, or null if the list was not found.</param>
        /// <returns>True if the list was found, otherwise false.</returns>
        public bool PickValuesFrom(string name, out List<T> values)
        {
            if (GetList(name, out var list))
            {
                values = list.PickValues();
                return true;
            }
            
            values = null;
            return false;
        }

        /// <summary>
        /// Picks values from Probability List at specified index in the collection and adds them to a provided list.
        /// </summary>
        /// <param name="index">The index of the Probability List.</param>
        /// <param name="listToFill">The list to add picked values to.</param>
        /// <returns>True if the list was found, otherwise false.</returns>
        /// <remarks>This method adds the result of <see cref="ProbabilityList{T}.PickValues(List{T})"/> to the provided list.</remarks>
        public bool PickValuesFrom(int index, List<T> listToFill)
        {
            if (GetList(index, out var list) == false) return false;
            list.PickValues(listToFill);
            return true;
        }

        /// <summary>
        /// Picks values from Probability List with specified name in the collection and adds them to a provided list.
        /// </summary>
        /// <param name="name">The name of the Probability List.</param>
        /// <param name="listToFill">The list to add picked values to.</param>
        /// <returns>True if the list was found, otherwise false.</returns>
        /// <remarks>This method adds the result of <see cref="ProbabilityList{T}.PickValues()"/> to the provided list.</remarks>
        public bool PickValuesFrom(string name, List<T> listToFill)
        {
            if (GetList(name, out var list) == false) return false;
            list.PickValues(listToFill);
            return true;
        }
        
        #endregion
        
        #region Pick Values From All
        
        /// <summary>
        /// Picks values from all Probability Lists in the collection.
        /// </summary>
        /// <returns>A new list containing the results of <see cref="ProbabilityList{T}.PickValues()"/> method from all lists in the collection.</returns>
        /// <remarks>This method creates and returns a new <see cref="List{T}"/> containing the results.</remarks>
        public List<T> PickValuesFromAll()
        {
            var pickedValues = new List<T>();
            foreach (var probabilityList in pl_collection)
            {
                pickedValues.AddRange(probabilityList.PickValues());
            }

            return pickedValues;
        }

        /// <summary>
        /// Picks a value from each Probability List in the collection.
        /// </summary>
        /// <returns>A new list containing the results of <see cref="ProbabilityList{T}.PickValue()"/> method from all lists in the collection.</returns>
        /// <remarks>This method creates and returns a new <see cref="List{T}"/> containing the results.</remarks>
        public List<T> PickValueFromAll()
        {
            var pickedValues = new List<T>();
            foreach (var probabilityList in pl_collection)
            {
                if(probabilityList.TryPickValue(out var value)) pickedValues.Add(value);
            }

            return pickedValues;
        }
        
        [Obsolete("Method will be removed in v1.0. Use PickValuesFromAll() instead")]
        public List<T> PickFromAll()
        {
            var pickedValues = new List<T>();
            foreach (var probabilityList in pl_collection)
            {
                pickedValues.AddRange(probabilityList.PickValues());
            }

            return pickedValues;
        }
        
        #endregion
        
        #region Refill List
        
        /// <summary>
        /// Refills the units of items in the Probability List at the specified index.
        /// </summary>
        /// <param name="index">The index of the Probability List.</param>
        /// <returns>True if the list was found and refilled, otherwise false.</returns>
        public bool RefillList(int index)
        {
            if (GetList(index, out var list) == false) return false;
            list.RefillItems();
            return true;
        }
        
        /// <summary>
        /// Refills the units of items in the Probability List with specified name.
        /// </summary>
        /// <param name="name">The name of the Probability List to Refill.</param>
        /// <returns>True if the list was found and refilled, otherwise false.</returns>
        public bool RefillList(string name)
        {
            if (GetList(name, out var list) == false) return false;
            list.RefillItems();
            return true;
        }
        
        /// <summary>
        /// Refills the units of items in all Probability Lists in the collection.
        /// </summary>
        public void RefillAllLists()
        {
            foreach (var list in pl_collection) list.RefillItems();
        }
        
        #endregion
    }

#pragma warning restore 0618
}