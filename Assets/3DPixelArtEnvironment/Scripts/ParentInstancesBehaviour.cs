namespace Environment.Instancing
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// This script calls each InstanceBehaviour in their child objects using their input variables and combines them into a single call.
    /// If you a large amount of instances behaviours, they will all create a material instance and instancing call. This causes a performance hit.
    /// As a drawback the parent node that the objects rotate and move with is not the gameobject but the parent instead and bounds can not be used to cull individual meshes.
    /// </summary>
    public class ParentInstancesBehaviour : InstancesBehaviour
    {
        /// <summary>
        /// Calculates the bounds for the instances used for culling by Unity.
        /// </summary>
        public override Bounds CalculateInstancesBounds()
        {
            return new Bounds(this.transform.position, Vector3.one * 100000); //this.GetComponent<MeshFilter>().mesh.bounds.max);
        }

        /// <summary>
        /// Implementation of instance data logic for parent instances behaviour.
        /// </summary>
        public override Dictionary<InstancingSettings, List<InstanceData>> GetInstanceData()
        {
            var parentInstanceData = new Dictionary<InstancingSettings, List<InstanceData>>();
            var childrenAndThis = this.transform.GetComponentsInChildren<InstancesBehaviour>(true);

            if (childrenAndThis.Length <= 1)
            {
                Debug.LogWarning("The ParentInstancesBehaviour could not find any InstancesBehaviours in child objects.");
            }

            foreach (var child in childrenAndThis)
            {
                if (child == this)
                {
                    continue;
                }

                var instanceData = child.GetInstanceData();
                if (instanceData == null)
                {
                    Debug.LogError("GetInstanceData should not return null.");
                }

                foreach (var (configuration, data) in instanceData)
                {
                    for (var i = 0; i < data.Count; i++)
                    {
                        var instance = data[i];
                        instance.TRS = this.transform.worldToLocalMatrix * child.transform.localToWorldMatrix * instance.TRS;
                        data[i] = instance;
                    }

                    var found = parentInstanceData.Keys.FirstOrDefault(key => key.Material == configuration.Material && key.Mesh == configuration.Mesh);
                    if (found == null)
                    {
                        parentInstanceData[configuration] = data;
                    }
                    else
                    {
                        parentInstanceData[found].AddRange(data);
                    }
                }

                child.enabled = false;
            }

            return parentInstanceData;
        }
    }
}
