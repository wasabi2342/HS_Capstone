namespace Environment.Utilities
{
    using System.Linq;
    using UnityEngine;

    public class WeightedDistribution
    {
        // Could add a seed to random if ever needed

        readonly float[] cumulativeProbabilities;

        /// <summary>
        /// Class for sampling elements that have weights.
        /// </summary>
        public WeightedDistribution(float[] weights)
        {
            if (weights == null || weights.Length == 0)
            {
                Debug.LogError("Cannot create WeightedDistribution with a null or empty input!");
            }

            var sum = weights.Sum();
            this.cumulativeProbabilities = new float[weights.Length];
            var cumulativeProbability = 0f;
            for (var i = 0; i < weights.Length; i++)
            {
                cumulativeProbability += weights[i] / sum;
                this.cumulativeProbabilities[i] = cumulativeProbability;
            }
        }

        /// <summary>
        /// Samples the weighted distribution.
        /// </summary>
        public int Sample()
        {
            if (this.cumulativeProbabilities.Length == 1) 
            { 
                return 0;
            }

            var random = Random.value;
            for (var i = 0; i < this.cumulativeProbabilities.Length; i++)
            {
                if(random <= this.cumulativeProbabilities[i])
                {
                    return i;
                }
            }

            Debug.LogWarning("Undefined behaviour! Defaulting to last element.");
            return this.cumulativeProbabilities.Length - 1;
        }
    }
}
