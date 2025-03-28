namespace Environment.Instancing
{
    using UnityEngine;

    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Instances behaviour to generate on surfaces of terrains.
    /// </summary>
    [RequireComponent(typeof(Terrain))]
    public class TerrainInstancesBehaviour : InstancesBehaviour
    {
        [Serializable]
        public class TerrainInstancingInput
        {
            public float Density = 1f;
            public InstancingSettings[] Settings;
        }

        [Header("Terrain Texture Channels")]
        public TerrainInstancingInput FirstLayer;
        public TerrainInstancingInput SecondLayer;
        public TerrainInstancingInput ThirdLayer;
        public TerrainInstancingInput FourthLayer;

        [Header("Variance Parameters")]
        [Range(0f, 0.5f)] public float PositionVariance = 0.5f;
        [Range(0f, 0.9f)] public float ScaleVariance = 0.2f;

        /// <summary>
        /// Calculates the bounds for the instances used for culling by Unity.
        /// </summary>
        public override Bounds CalculateInstancesBounds()
        {
            var bounds = this.GetComponent<Terrain>().terrainData.bounds;

            bounds.center += this.transform.position;

            return bounds;
        }

        /// <summary>
        /// Implementation of instance data logic for instances behaviour.
        /// </summary>
        public override Dictionary<InstancingSettings, List<InstanceData>> GetInstanceData()
        {
            var instancingInput = new TerrainInstancingInput[] { this.FirstLayer, this.SecondLayer, this.ThirdLayer, this.FourthLayer };
            foreach (var input in instancingInput)
            {
                if (input.Settings == null)
                {
                    Debug.LogError("Instancing settings should be defined the Unity Editor.");
                    return null;
                }

                foreach (var settings in input.Settings)
                {
                    if (settings.Scale <= 0f || settings.Material == null || settings.Mesh == null)
                    {
                        Debug.LogError("The instancing input should have a material, mesh, a scale larger than 0 and a density larger than 0.");
                        return null;
                    }
                }
            }

            var instanceData = TerrainInstanceData(this.GetComponent<Terrain>(), this.PositionVariance, this.ScaleVariance, instancingInput).ToArray();

            var result = new Dictionary<InstancingSettings, List<InstanceData>>();
            for (var i = 0; i < instancingInput.Length; i++)
            {
                var dividedInstances = DivideInstanceData(instanceData[i], instancingInput[i].Settings);
                if (dividedInstances != null)
                {
                    foreach (var (configuration, data) in dividedInstances)
                    {
                        result.Add(configuration, data);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates which layer should be picked.
        /// </summary>
        static int LargestColorIndex(Color color) => (new float[] { color.r, color.g, color.b, color.a }).Select((value, index) => (value, index)).Max().index;

        /// <summary>
        /// Calculates the terrains instance data based on settings.
        /// </summary>
        static IEnumerable<InstanceData[]> TerrainInstanceData(Terrain terrain, float positionVariance, float scaleVariance, params TerrainInstancingInput[] input)
        {
            var result = new List<List<InstanceData>>();
            for (var i = 0; i < input.Length; i++)
            {
                result.Add(new List<InstanceData>());
            }

            var controlTexture = terrain.terrainData.alphamapTextureCount > 0 ? terrain.terrainData.alphamapTextures[0] : null;
            if (controlTexture == null)
            {
                Debug.LogWarning("Control texture not defined. Defaulting to layer 1 everywhere!");
            }

            var terrainCenter = terrain.terrainData.bounds.center;
            var size = terrain.terrainData.size;
            float width = size.x, height = size.y, length = size.z;

            for (var layer = 0; layer < input.Length; layer++)
            {
                var density = input[layer].Density;
                var settings = input[layer].Settings;

                if (settings.Length == 0 || (controlTexture == null && layer > 0))
                {
                    // Dont calculate points for empty layer's configurations
                    continue;
                }

                var step = 1f / density;
                var pointWidth = Mathf.RoundToInt(width * density);
                var pointLength = Mathf.RoundToInt(length * density);

                for (var x = 0; x < pointWidth; x++)
                {
                    for (var z = 0; z < pointLength; z++)
                    {
                        var xPosition = (x + 0.5f + UnityEngine.Random.Range(-positionVariance, positionVariance)) * step;
                        var zPosition = (z + 0.5f + UnityEngine.Random.Range(-positionVariance, positionVariance)) * step;
                        var terrainUV = new Vector2(xPosition / width, zPosition / length);

                        var position = new Vector3()
                        {
                            x = xPosition,
                            y = terrain.terrainData.GetInterpolatedHeight(terrainUV.x, terrainUV.y),
                            z = zPosition,
                        } - terrainCenter;

                        var instance = new InstanceData()
                        {
                            TRS = Matrix4x4.TRS(
                                position,
                                Quaternion.identity,
                                Vector3.one * UnityEngine.Random.Range(1 - scaleVariance, 1 + scaleVariance)
                            ),
                            Normal = terrain.terrainData.GetInterpolatedNormal(terrainUV.x, terrainUV.y),
                        };

                        // Choose layer that is most potent. Could roll with values as weights but this is simple.
                        var pointLayer = controlTexture == null ? 0 : LargestColorIndex(controlTexture.GetPixelBilinear(terrainUV.x, terrainUV.y));

                        if (layer == pointLayer)
                        {
                            result[layer].Add(instance);
                        }
                    }
                }
            }

            return result.Select(list => list.ToArray());
        }
    }
}
