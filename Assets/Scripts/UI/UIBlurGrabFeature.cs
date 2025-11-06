// Written with love, light and rainbow :)

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class UIBlurGrabFeature : ScriptableRendererFeature
{
    class GrabPass : ScriptableRenderPass
    {
        static readonly int _UIBlurTexID = Shader.PropertyToID("_UIBlurTex");
        RTHandle _temp;
        string _profilerTag = "UI Blur Grab";
        Material _copyMat;

        public GrabPass()
        {
            // Simple copy; you can use CoreUtils.CreateEngineMaterial("Hidden/BlitCopy") if needed
            _copyMat = CoreUtils.CreateEngineMaterial("Hidden/BlitCopy");
            renderPassEvent = RenderPassEvent.AfterRendering; // after ALL rendering
        }

        public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
        {
            var desc = renderingData.cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;
            RenderingUtils.ReAllocateIfNeeded(ref _temp, desc, name: "_UIBlurTexRT");
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var cmd = CommandBufferPool.Get(_profilerTag);
            // Copy current color target to _UIBlurTex
            Blitter.BlitCameraTexture(cmd, renderingData.cameraData.renderer.cameraColorTargetHandle, _temp, _copyMat, 0);
            cmd.SetGlobalTexture(_UIBlurTexID, _temp);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        public override void OnCameraCleanup(CommandBuffer cmd) { }
        public void Dispose() { _temp?.Release(); CoreUtils.Destroy(_copyMat); }
    }

    GrabPass _pass;

    public override void Create() { _pass = new GrabPass(); }
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(_pass);
    }

    protected override void Dispose(bool disposing) { _pass?.Dispose(); }
}
