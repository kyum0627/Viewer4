using IGX.Loader;
using IGX.Loader.AMFileLoader; // AM 파일 로더 관련 기능을 포함.
using Newtonsoft.Json; // JSON 직렬화/역직렬화를 위한 라이브러리.

namespace IGX.ViewControl.Render
{
    /// <summary>
    /// Model3D 객체의 JSON 직렬화/역직렬화 및 파일 입출력을 전담.
    /// </summary>
    public static class ModelSerializer
    {
        private static readonly JsonSerializerSettings _settings = new()
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            Converters = { new Vector2Converter(), new Vector3Converter(), new Vector4Converter() }
        };

        public static string ToJson(Model3D model, bool indented = true)
        {
            _settings.Formatting = indented ? Formatting.Indented : Formatting.None;
            return JsonConvert.SerializeObject(model, _settings);
        }

        public static Model3D? FromJson(string json)
        {
            return JsonConvert.DeserializeObject<Model3D>(json, _settings);
        }

        public static void SaveToFile(Model3D model, string filePath)
        {
            _settings.Formatting = Formatting.None;
            using var writer = new JsonTextWriter(new StreamWriter(filePath, false))
            {
                Formatting = _settings.Formatting
            };
            JsonSerializer serializer = JsonSerializer.Create(_settings);
            serializer.Serialize(writer, model);
        }

        public static Model3D? LoadFromFile(string filePath)
        {
            using var reader = new JsonTextReader(new StreamReader(filePath));
            JsonSerializer serializer = JsonSerializer.Create(_settings);
            return serializer.Deserialize<Model3D>(reader);
        }
    }
}