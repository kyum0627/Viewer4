using System.Diagnostics;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Render
{
    public class RenderPipeline
    {
        private readonly List<IRenderPass> _passes = new();

        public RenderPipeline()
        {
        }
        public RenderPipeline(IEnumerable<IRenderPass> passes)
        {
            if (passes != null)
                _passes.AddRange(passes);
        }

        public void AddPass(IRenderPass pass)
        {
            if (pass == null) return;
            _passes.Add(pass);
            Debug.WriteLine($"[RenderPipeline] Added pass: {pass.Name}, Order: {pass.Order}, Enabled: {pass.Enabled}");
        }

        public void RemovePass(IRenderPass pass)
        {
            if (pass == null) return;
            _passes.Remove(pass);
        }
        public void ClearPasses()
        {
            _passes.Clear();
        }

        public void Initialize(IMyCamera camera)
        {
            Debug.WriteLine($"[RenderPipeline] Initializing {_passes.Count} passes");
            foreach (var pass in _passes)
            {
                try
                {
                    Debug.WriteLine($"[RenderPipeline] Initializing {pass.Name}");
                    pass.Initialize(camera);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RenderPipeline: Initialize failed for pass {pass?.GetType().FullName}: {ex}");
                }
            }
        }

        public void Execute(IMyCamera camera)
        {
            if (camera == null) return;

            Matrix4 view = camera.ViewMatrix;
            Matrix4 projection = camera.ProjectionMatrix;

            Debug.WriteLine($"[RenderPipeline] Executing {_passes.Count} passes");

            // Order로 정렬하여 실행 (낮은 Order가 먼저)
            var passesSnapshot = _passes
                .Where(p => p != null && p.Enabled)
                .OrderBy(p => p.Order)
                .ToArray();

            Debug.WriteLine($"[RenderPipeline] Sorted {passesSnapshot.Length} enabled passes by Order");
            foreach (var pass in passesSnapshot)
            {
                Debug.WriteLine($"  - {pass.Name}: Order={pass.Order}");
            }

            foreach (var pass in passesSnapshot)
            {
                try
                {
                    Debug.WriteLine($"[RenderPipeline] Executing pass: {pass.Name} (Order: {pass.Order})");
                    pass.SetCameraUniforms(view, projection);
                    pass.Execute();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RenderPipeline: Exception in pass {pass.GetType().FullName}: {ex}");
                    // continue with other passes
                }
            }
        }

        public void Resize(int w, int h)
        {
            foreach (var pass in _passes)
            {
                try
                {
                    pass.Resize(w, h);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RenderPipeline: Resize failed for pass {pass?.GetType().FullName}: {ex}");
                }
            }
        }

        public void Dispose()
        {
            foreach (var pass in _passes)
            {
                try
                {
                    pass.Dispose();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RenderPipeline: Dispose failed for pass {pass?.GetType().FullName}: {ex}");
                }
            }

            _passes.Clear();
        }
    }
}