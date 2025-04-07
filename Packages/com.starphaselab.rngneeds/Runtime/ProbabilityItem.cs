using System;
using System.Linq;
using StarphaseTools.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RNGNeeds
{
    /// <summary>
    /// This class represents an item in the <see cref="ProbabilityList{T}"/>, carrying the value of the item and its associated probabilities.
    /// </summary>
    [Serializable]
    public class ProbabilityItem<T> : IProbabilityItem
    {
        [SerializeField] private T m_Value;
        [SerializeField] private float m_BaseProbability;
        [SerializeField] private bool m_Enabled;
        [SerializeField] private bool m_Locked;
        [SerializeField] private Vector2 m_InfluenceSpread = new Vector2(0f, 1f);
        [SerializeReference] private Object m_InfluenceProvider;
        [SerializeField] private bool m_InvertInfluence;
        
        [SerializeField] private bool m_DepletableItem = true;
        [SerializeField] private int m_Units = 1;
        [SerializeField] private int m_MaxUnits = 1;

        [SerializeField] private int m_Weight = 100;
        
        private IProbabilityInfluenceProvider m_CachedInfluenceProvider;
        private bool m_ValueIsInfluenceProvider;
        private bool m_IsInfluencedItem;
        private bool PropertiesUpdated { get; set; }
        private string cachedProvider;
        
        public ProbabilityItem(T value, float baseProbability, bool enabled = true, bool locked = false)
        {
            Value = value;
            BaseProbability = baseProbability;
            Enabled = enabled;
            Locked = locked;
        }

        #region Internal

        void IProbabilityItem.ConsumeUnit()
        {
            if(m_Units > 0) m_Units--;
        }
        
        void IProbabilityItem.UpdateProperties()
        {
            m_ValueIsInfluenceProvider = GetProviderTypes().HasAny(ItemProviderType.InfluenceProvider);
            m_CachedInfluenceProvider = m_ValueIsInfluenceProvider
                ? m_Value as IProbabilityInfluenceProvider
                : m_InfluenceProvider as IProbabilityInfluenceProvider;

            cachedProvider = m_CachedInfluenceProvider?.ToString();
            m_IsInfluencedItem = m_CachedInfluenceProvider != null;
            PropertiesUpdated = true;
        }
        
        private void ClampSpreadToProbability()
        {
            var probability = BaseProbability;
            m_InfluenceSpread.x = Mathf.Clamp(m_InfluenceSpread.x, 0f, probability);
            m_InfluenceSpread.y = Mathf.Clamp(m_InfluenceSpread.y, probability, 1f);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the value held by this ProbabilityItem.
        /// </summary>
        public T Value
        {
            get => m_Value;
            set
            {
                m_Value = value;
                ((IProbabilityItem)this).UpdateProperties();
            }
        }
        
        /// <summary>
        /// Gets or sets the base probability of the item, a value between 0 and 1. 
        /// If set to a value outside this range, it will be clamped. 
        /// This base probability can be influenced by modifiers but this property represents the unaltered probability.
        /// </summary>
        public float BaseProbability { get => m_BaseProbability;
            set
            {
                m_BaseProbability = Mathf.Clamp(value, 0f, 1f); 
                ClampSpreadToProbability();
            }
        }
        
        /// <summary>
        /// Gets the current probability of the item, taking into account any applied modifiers. 
        /// This is the actual probability used during the selection process. 
        /// If the item has no modifiers, it simply returns the base probability.
        /// </summary>
        public float Probability => IsInfluencedItem ? GetInfluencedProbability(InfluenceProvider.ProbabilityInfluence) : m_BaseProbability;
        
        /// <summary>
        /// Calculates and returns the influenced probability of the item being selected, based on the given influence value.
        /// If the item is not influenced, it simply returns the base probability.
        /// </summary>
        public float GetInfluencedProbability(float influence)
        {
            return IsInfluencedItem ? 
                ProbabilityTools.CalculateInfluencedProbability(BaseProbability, InfluenceSpread.x, InfluenceSpread.y, 
                    m_InvertInfluence ? 
                        influence * -1 : 
                        influence) : 
                m_BaseProbability;
        }
        
        /// <summary>
        /// Gets or sets whether the item is enabled for selection. If set to false, the item is ignored during the selection process.
        /// </summary>
        public bool Enabled
        {
            get => m_Enabled;
            set => m_Enabled = value;
        }

        /// <summary>
        /// Gets or sets whether the item's base probability is locked from being altered. If set to true, the base probability cannot be changed.
        /// </summary>
        public bool Locked
        {
            get => m_Locked;
            set => m_Locked = value;
        }

        public int Weight
        {
            get => m_Weight;
            internal set => m_Weight = Mathf.Clamp(value, 0, int.MaxValue);
        }
        
        #endregion
        
        #region Probability Influence Provider

        /// <summary>
        /// Gets or sets the degree to which influence can affect the probability of this item. The spread is specified as a Vector2, 
        /// where x represents the lowest possible probability and y represents the highest possible probability due to influence.
        /// </summary>
        public Vector2 InfluenceSpread
        {
            get => m_InfluenceSpread;
            set
            {
                m_InfluenceSpread = value;
                ClampSpreadToProbability();
            }
        }
        
        /// <summary>
        /// Gets or sets whether the incoming probability influence should be inverted for this item. When set to true,
        /// the effect of influence is reversed, meaning positive influence decreases probability and negative influence increases it.
        /// </summary>
        public bool InvertInfluence
        {
            get => m_InvertInfluence;
            set => m_InvertInfluence = value;
        }
        
        /// <summary>
        /// Gets or sets an external provider of influence for this item. Note that if the item's value itself is an influence provider, 
        /// it will be preferred over this external provider.
        /// </summary>
        /// <remarks>
        /// <para>The external influence provider should be either MonoBehaviour or ScriptableObject inheriting from <see cref="IProbabilityInfluenceProvider"/> interface.</para>
        /// </remarks>
        public IProbabilityInfluenceProvider InfluenceProvider
        {
            get
            {
                if (IsInfluencedItem == false || m_CachedInfluenceProvider != null) return m_CachedInfluenceProvider;
                ((IProbabilityItem)this).UpdateProperties();
                return m_CachedInfluenceProvider;
            } 
            set
            {
                m_InfluenceProvider = value as Object;
                ((IProbabilityItem)this).UpdateProperties();
            }
        }
        
        /// <summary>
        /// Gets whether the item's value also serves as an influence provider. If the item's value type implements 
        /// the <see cref="IProbabilityInfluenceProvider"/> interface, it can provide its own influence, overriding any external provider. 
        /// </summary>
        public bool ValueIsInfluenceProvider
        {
            get
            {
                if(PropertiesUpdated == false) ((IProbabilityItem)this).UpdateProperties();
                return m_ValueIsInfluenceProvider;
            }
        }

        /// <summary>
        /// Gets whether this item is influenced by any source (either an external influence provider or the item's value itself, if it's an influence provider). 
        /// </summary>
        public bool IsInfluencedItem
        {
            get
            {
                if (PropertiesUpdated == false) ((IProbabilityItem)this).UpdateProperties();
                return m_IsInfluencedItem;
            }
        }
        
        #endregion
        
        #region Depletable Item

        /// <summary>
        /// Specifies the depletable state of the item. If set to true, picking the item will consume one unit until it's depleted. Depleted items will not be selectable until refilled.
        /// </summary>
        /// <remarks>Units will only be consumed if the item is part of a Depletable List. See ProbabilityList.<see cref="ProbabilityList{T}.IsDepletable"/></remarks>
        public bool IsDepletable
        {
            get => m_DepletableItem;
            set => m_DepletableItem = value;
        }
        
        /// <summary>
        /// Specifies the number of units remaining for this item. If the item is not depletable, this value is ignored.
        /// </summary>
        public int Units
        {
            get => m_Units;
            set => m_Units = value;
        }
        
        /// <summary>
        /// Specifies the maximum number of units this item can have. If the item is not depletable, this value is ignored.
        /// </summary>
        public int MaxUnits
        {
            get => m_MaxUnits;
            set => m_MaxUnits = value;
        }

        /// <summary>
        /// Checks if the item is depleted, meaning it has no units left.
        /// </summary>
        public bool IsDepleted => m_DepletableItem && m_Units <= 0;
        
        /// <summary>
        /// Checks if the item is selectable, meaning it's enabled and not depleted (if depletable).
        /// </summary>
        public bool IsSelectable => m_DepletableItem ? m_Enabled && m_Units > 0 : m_Enabled;
        
        /// <summary>
        /// Sets the item's units to the maximum amount specified by <see cref="MaxUnits"/>.
        /// </summary>
        public void Refill()
        {
            m_Units = m_MaxUnits;
        }
        
        /// <summary>
        /// Specifies the depletable state of the item along with the number of units and the maximum number of units.
        /// </summary>
        /// <param name="depletable">Should the item be depletable?</param>
        /// <param name="units">How many units should the item have?</param>
        /// <param name="maxUnits">What is the maximum number of units the item can have?</param>
        /// <remarks>Units will only be consumed if the item is part of a Depletable List. See ProbabilityList.<see cref="ProbabilityList{T}.IsDepletable"/></remarks>
        public void SetDepletable(bool depletable = true, int units = 1, int maxUnits = 1)
        {
            m_DepletableItem = depletable;
            m_Units = units;
            m_MaxUnits = maxUnits;
        }
        
        #endregion
        
        /// <summary>
        /// Determines the types of interfaces implemented by the 'Value' held by this ProbabilityItem.
        /// It checks if the value implements any of the following interfaces: 
        /// <see cref="IProbabilityItemInfoProvider"/>, <see cref="IProbabilityInfluenceProvider"/>, or <see cref="IProbabilityItemColorProvider"/>.
        /// </summary>
        /// <returns>A flag enumeration <see cref="ItemProviderType"/> indicating which of the aforementioned interfaces are implemented by the 'Value'.</returns>
        public ItemProviderType GetProviderTypes()
        {
            var providerTypes = ItemProviderType.None;
            if (Value == null) return providerTypes;
            var interfaces = Value.GetType().GetInterfaces();
            if (interfaces.Contains(typeof(IProbabilityItemInfoProvider))) providerTypes |= ItemProviderType.InfoProvider;
            if (interfaces.Contains(typeof(IProbabilityInfluenceProvider))) providerTypes |= ItemProviderType.InfluenceProvider;
            if (interfaces.Contains(typeof(IProbabilityItemColorProvider))) providerTypes |= ItemProviderType.ColorProvider;
            return providerTypes;
        }
    }
}