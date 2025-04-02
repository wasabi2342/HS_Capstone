namespace Environment.Instancing
{
    using UnityEngine;

    using System.Runtime.InteropServices;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Data as seen on GPU. Exact same struct should be defined there.
    /// </summary>
    public struct InstanceData
    {
        public Matrix4x4 TRS;
        public Vector3 Normal;
    }

    /// <summary>
    /// Contains the needed bakend information for instancing with Graphics.RenderMeshIndirect().
    /// </summary>
    public class InstancingConfiguration
    {
        public Mesh Mesh;
        public Material Material;

        public MaterialPropertyBlock MaterialPropertyBlock;
        public ComputeBuffer CommandBuffer;
        public ComputeBuffer DataBuffer;

        /// <summary>
        /// Initializes new instancing information.
        /// </summary>
        public InstancingConfiguration(InstancingSettings settings, IEnumerable<InstanceData> data, string shaderBufferParameter)
        {
            this.Mesh = settings.Mesh;
            this.Material = settings.Material;

            this.DataBuffer = new ComputeBuffer(data.Count(), Marshal.SizeOf<InstanceData>());
            this.DataBuffer.SetData(data.ToArray());

            var commandArguments = new uint[5];
            commandArguments[0] = settings.Mesh.GetIndexCount(0);
            commandArguments[1] = (uint)data.Count();

            this.CommandBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);
            this.CommandBuffer.SetData(commandArguments);

            this.MaterialPropertyBlock = new MaterialPropertyBlock();
            this.MaterialPropertyBlock.SetBuffer(shaderBufferParameter, this.DataBuffer);
        }

        /// <summary>
        /// Frees the memory of this configuration from the GPU.
        /// </summary>
        public void FreeMemory()
        {
            this.CommandBuffer?.Release();
            this.CommandBuffer = null;

            this.DataBuffer?.Release();
            this.DataBuffer = null;
        }
    }
}

/*
 * if Unity ever supports Graphics.RenderMeshIndirect() with Shader Graph shaded instances. This can be used to upgrade.
 namespace Environment.Instancing
{
    using UnityEngine;

    using System.Runtime.InteropServices;

    /// <summary>
    /// Contains the needed bakend information for instancing with Graphics.RenderMeshIndirect().
    /// </summary>
    public class InstancingInfo<T>
    {
        public GraphicsBuffer GraphicsBuffer;
        public MaterialPropertyBlock MaterialPropertyBlock;

        /// <summary>
        /// Initializes new instancing information.
        /// </summary>
        public InstancingInfo(T[] data, string shaderBufferParameter)
        {
            // For structured graphics buffer there has to be an equivalnt struct in hlsl
            this.GraphicsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, data.Length, Marshal.SizeOf<T>());

            this.MaterialPropertyBlock = new MaterialPropertyBlock();
            this.MaterialPropertyBlock.SetBuffer(shaderBufferParameter, this.GraphicsBuffer);
        }
    }
}
 */