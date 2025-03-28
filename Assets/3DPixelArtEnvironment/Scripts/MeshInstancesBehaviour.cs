namespace Environment.Instancing
{
    using UnityEngine;

    using System.Collections.Generic;
    using Environment.Utilities;

    /// <summary>
    /// Instances behaviour to generate on surfaces of meshes.
    /// </summary>
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshInstancesBehaviour : InstancesBehaviour
    {
        [Header("Sub Mesh Details")]
        public bool UseSubMesh = false;
        public int SubMeshIndex = 0;

        [Header("Instance Settings")]
        public float Density = 1f;
        public InstancingSettings[] InstancingSettings;


        /// <summary>
        /// Calculates the bounds for the instances used for culling by Unity.
        /// </summary>
        public override Bounds CalculateInstancesBounds()
        {
            var rendererBounds = this.GetComponent<MeshRenderer>().bounds;
            var diff = this.transform.position - rendererBounds.center;
            var absDiff = new Vector3(Mathf.Abs(diff.x),
                                      Mathf.Abs(diff.y),
                                      Mathf.Abs(diff.z));

            return new Bounds(this.transform.position, (rendererBounds.extents + absDiff) * 2f);
        }

        /// <summary>
        /// Implementation of instance data logic for instances behaviour.
        /// </summary>
        public override Dictionary<InstancingSettings, List<InstanceData>> GetInstanceData()
        {
            if (this.InstancingSettings == null || this.InstancingSettings.Length == 0)
            {
                Debug.LogWarning("Instancing settings should be defined the Unity Editor.");
                return null;
            }

            foreach (var settings in this.InstancingSettings)
            {
                if (settings.Scale <= 0f || settings.Material == null || settings.Mesh == null)
                {
                    Debug.LogError("The given Instance Configurations should have a material, mesh and a scale larger than 0");
                    return null;
                }
            }

            var mesh = this.GetComponent<MeshFilter>().mesh;
            if (this.UseSubMesh)
            {
                if (!mesh.isReadable)
                {
                    Debug.LogWarning("Mesh is not readable. Provide read permissions for it to work in builds!" );
                }

                if(this.SubMeshIndex >= mesh.subMeshCount)
                {
                    Debug.LogWarning("The sub mesh index does not exist. Using default; using the whole mesh.");
                }
                else
                {
                    var subMesh = new Mesh
                    {
                        vertices = mesh.vertices,
                        triangles = mesh.GetTriangles(this.SubMeshIndex)
                    };
                    mesh = subMesh;
                }
            }

            var instanceData = RandomMeshInstanceData(mesh, this.Density, this.InstancingSettings);
            return DivideInstanceData(instanceData, this.InstancingSettings);
        }

        /// <summary>
        /// Gets random mesh instance data on a mesh.
        /// </summary>
        public static InstanceData[] RandomMeshInstanceData(Mesh mesh, float density, InstancingSettings[] configurations)
        {
            if (mesh == null || density <= 0 || configurations == null || configurations.Length == 0)
            {
                return null;
            }

            var instanceAmount = Mathf.CeilToInt(MeshUtilities.GetMeshArea(mesh) / density);

            var samples = MeshUtilities.RandomMeshPoints(mesh, instanceAmount);

            var data = new List<InstanceData>();

            for (var i = 0; i < samples.Length; i++)
            {
                var (vertex, normal) = samples[i];

                var instance = new InstanceData
                {
                    TRS = Matrix4x4.TRS(
                        vertex,
                        Quaternion.identity,
                        Vector3.one
                    ),
                    Normal = normal
                };
                data.Add(instance);
            }

            return data.ToArray();
        }
    }
}