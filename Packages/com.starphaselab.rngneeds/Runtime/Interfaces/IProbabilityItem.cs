namespace RNGNeeds
{
    public interface IProbabilityItem
    {
        IProbabilityInfluenceProvider InfluenceProvider { get; }
        bool ValueIsInfluenceProvider { get; }
        bool IsInfluencedItem { get; }
        bool Enabled { get; }
        float Probability { get; }
        void UpdateProperties();
        
        bool IsDepletable { get; set; }
        bool IsDepleted { get; }
        bool IsSelectable { get; }
        void ConsumeUnit();
        int Units { get; set; }
    }
}