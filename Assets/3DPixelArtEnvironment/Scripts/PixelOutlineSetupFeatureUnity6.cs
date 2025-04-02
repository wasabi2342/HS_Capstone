namespace Environment
{
    // This one is for Unity 6, the one for 2021 LTS + is elsewhere
    using UnityEngine.Rendering;
    using UnityEngine.Rendering.Universal;
    using UnityEngine.Rendering.RenderGraphModule;

    /// <summary>
    /// This render feature creates depth normals and colors after rendering opaques.
    /// </summary>
    public class PixelOutlineSetupFeature : ScriptableRendererFeature
    {
        public class EmptyPass : ScriptableRenderPass
        {
            public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData) { }
        }

        EmptyPass emptyPass;

        public override void Create()
        {
            this.emptyPass = new EmptyPass
            {
                renderPassEvent = RenderPassEvent.AfterRenderingOpaques
            };
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            this.emptyPass.ConfigureInput(ScriptableRenderPassInput.Depth | ScriptableRenderPassInput.Normal | ScriptableRenderPassInput.Color);
            renderer.EnqueuePass(this.emptyPass);
        }
    }
}