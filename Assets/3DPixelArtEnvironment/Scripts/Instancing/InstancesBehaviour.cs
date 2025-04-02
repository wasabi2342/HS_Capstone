namespace Environment.Instancing
{
    using UnityEngine;

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Environment.Utilities;
    using UnityEngine.Rendering;

    [Serializable]
    public class InstancingSettings
    {
        [Header("Rendering")]
        public Mesh Mesh;
        public Material Material;

        [Header("Transformations")]
        public float Probability = 1f;
        public float Scale = 1f;
        public float NormalOffset = 0.1f;
    };

    /// <summary>
    /// The abstract class for instancing behaviours.
    /// </summary>
    public abstract class InstancesBehaviour : MonoBehaviour
    {
        List<InstancingConfiguration> instancingConfigurations;
        void Update()
        {
            if (this.instancingConfigurations != null)
            {
                if (this.transform.hasChanged)
                {
                    // For some reason unity's DrawMeshInstancedIndirect bounds center transform the instances position. 
                    // We remove the position away and only update the rotation and scale. 
                    var localWithoutPositionToWorld = this.transform.localToWorldMatrix;
                    localWithoutPositionToWorld.m03 = 0;
                    localWithoutPositionToWorld.m13 = 0;
                    localWithoutPositionToWorld.m23 = 0;

                    foreach (var configuration in this.instancingConfigurations)
                    {
                        configuration.MaterialPropertyBlock.SetMatrix("_LocalToWorld", localWithoutPositionToWorld);
                    }

                    this.transform.hasChanged = false;
                }

                var bounds = this.CalculateInstancesBounds();
                foreach (var configuration in this.instancingConfigurations)
                {
                    Graphics.DrawMeshInstancedIndirect(
                        configuration.Mesh,
                        0,
                        configuration.Material,
                        bounds,
                        configuration.CommandBuffer,
                        0,
                        configuration.MaterialPropertyBlock,
                        ShadowCastingMode.Off,
                        false,
                        LayerMask.NameToLayer("Default"));
                }
            }
        }

        /// <summary>
        /// Gets instance data for the base implementation.
        /// </summary>
        /// <returns>The instance data related to the transformation settings</returns>
        public abstract Dictionary<InstancingSettings, List<InstanceData>> GetInstanceData();

        /// <summary>
        /// Calculates the bounds for the instances used for culling by Unity.
        /// </summary>
        public virtual Bounds CalculateInstancesBounds()
        {
            return new Bounds(this.transform.position, Vector3.one * 100000);
        } 

        /// <summary>
        /// Helper for dividing instancing data between settings.
        /// </summary>
        public static Dictionary<InstancingSettings, List<InstanceData>> DivideInstanceData(InstanceData[] instanceData, InstancingSettings[] instancingSettings)
        {
            if (instanceData == null || instanceData.Length == 0 || instancingSettings == null || instancingSettings.Length == 0)
            {
                return null;
            }

            var result = new Dictionary<InstancingSettings, List<InstanceData>>();
            foreach (var settings in instancingSettings)
            {
                result.Add(settings, new List<InstanceData>());
            }

            var weights = instancingSettings.Select(configuration => configuration.Probability).ToArray();
            var weightedSampler = new WeightedDistribution(weights);

            for (var i = 0; i < instanceData.Length; i++)
            {
                var index = weightedSampler.Sample();
                var settings = instancingSettings[index];

                instanceData[i].TRS *= Matrix4x4.TRS(settings.NormalOffset * instanceData[i].Normal, Quaternion.identity, new Vector3(settings.Scale, settings.Scale, settings.Scale));

                result[settings].Add(instanceData[i]);
            }

            return result;
        }

        /// <summary>
        /// Enables the instancing by loading up the data from provided configurations and settings.
        /// </summary>
        void OnEnable()
        {
            var instanceData = this.GetInstanceData();

            if (instanceData == null || instanceData.Count == 0)
            {
                Debug.LogWarning("No objects to be instanced.");
                return;
            }

            if (this.instancingConfigurations != null) 
            {
                Debug.LogError("Instances are already loaded."); 
                return; 
            }

            this.instancingConfigurations = new List<InstancingConfiguration>();
            foreach (var (settings, data) in instanceData)
            {
                if (data.Count == 0)
                {
                    Debug.LogWarning("The generated data is empty for mesh: " + settings.Mesh.name + ". Maybe the probability weight is too low?");
                    continue;
                }

                var configuration = new InstancingConfiguration(settings, data, "_InstanceData");
                this.instancingConfigurations.Add(configuration);
            }
        }

        /// <summary>
        /// Disables the instancing behaviour and releases memory.
        /// </summary>
        void OnDisable()
        {
            if (this.instancingConfigurations is null)
            {
                return;
            }

            foreach (var configuration in this.instancingConfigurations)
            {
                configuration.FreeMemory();
            }

            this.instancingConfigurations = null;
        }
    }
}