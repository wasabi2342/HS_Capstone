using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GlitchEffectPass : ScriptableRenderPass
{
    private Material effectMaterial;
    private RTHandle source;
    private RTHandle tempTexture;
    
    private string profilerTag;

    public GlitchEffectPass(Material material, string tag)
    {
        this.effectMaterial = material;
        this.profilerTag = tag;
        renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing; // UI 이후에 실행
    }

    public void Setup(RTHandle source)
    {
        this.source = source;
    }

    public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // RTHandle 시스템을 사용하여 임시 텍스처 할당
        RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
        descriptor.depthBufferBits = 0; // 깊이 버퍼를 사용하지 않음
        
        // RTHandle 생성하거나 재사용
        RenderingUtils.ReAllocateIfNeeded(ref tempTexture, descriptor, name: "_TempGlitchTexture");
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        if (effectMaterial == null)
            return;

        CommandBuffer cmd = CommandBufferPool.Get(profilerTag);
        
        using (new ProfilingScope(cmd, new ProfilingSampler(profilerTag)))
        {
            // 임시 텍스처에 복사
            Blitter.BlitCameraTexture(cmd, source, tempTexture);
            // 효과 적용하여 다시 카메라 타겟으로 복사
            Blitter.BlitCameraTexture(cmd, tempTexture, source, effectMaterial, 0);
        }
        
        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void OnCameraCleanup(CommandBuffer cmd)
    {
        // tempTexture는 RTHandle 시스템에 의해 관리되므로 여기서 해제할 필요가 없음
    }

    public void Dispose()
    {
        // 리소스 해제
        tempTexture?.Release();
    }
}

public class GlitchEffectFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class GlitchEffectSettings
    {
        public Material glitchMaterial = null;
    }

    public GlitchEffectSettings settings = new GlitchEffectSettings();
    private GlitchEffectPass glitchPass;

    public override void Create()
    {
        if (settings.glitchMaterial != null)
            glitchPass = new GlitchEffectPass(settings.glitchMaterial, name);
        else
            glitchPass = null;
    }

    // URP 13+에서 업데이트된 메서드 시그니처
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (glitchPass == null) return;
        
        // URP 13+에서 카메라 색상 타겟 얻기
        RTHandle cameraColorTarget = renderingData.cameraData.renderer.cameraColorTargetHandle;
        glitchPass.Setup(cameraColorTarget);
        renderer.EnqueuePass(glitchPass);
    }

    protected override void Dispose(bool disposing)
    {
        // 렌더 피처가 삭제될 때 리소스 정리
        if (disposing)
        {
            glitchPass?.Dispose();
        }

        base.Dispose(disposing);
    }
}
