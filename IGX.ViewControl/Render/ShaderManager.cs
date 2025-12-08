using System.Collections.Concurrent;

namespace IGX.ViewControl.Render
{
    public static class ShaderManager
    {
        private static readonly ConcurrentDictionary<string, Shader> _cache = new();
        public static bool useCache = true;
        public static Shader Create(string name, string vertexSrc, string fragmentSrc, bool isFile, bool useCache)
        {
            return _cache.GetOrAdd(name, _ =>
            {
                return new Shader(name, vertexSrc, fragmentSrc, isFile);
            });
        }
        public static Shader? Get(string name)
        {
            _cache.TryGetValue(name, out Shader? shader);
            return shader;
        }
        public static bool Remove(string name)
        {
            return _cache.TryRemove(name, out _);
        }
        public static void Clear()
        {
            foreach (var shader in _cache.Values)
            {
                shader.Dispose();
            }
            _cache.Clear();
        }
    }
}