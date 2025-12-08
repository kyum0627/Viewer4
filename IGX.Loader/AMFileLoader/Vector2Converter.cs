using Newtonsoft.Json;
using OpenTK.Mathematics;

namespace IGX.Loader.AMFileLoader
{
    public class Vector2Converter : JsonConverter<Vector2>
    {
        public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(value.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(value.Y);
            writer.WriteEndObject();
        }

        public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            float x = 0, y = 0;
            while (reader.Read())
            {
                if (reader.TokenType == JsonToken.PropertyName)
                {
                    string propName = (string)reader.Value!;
                    reader.Read();
                    if (propName == "X")
                    {
                        x = Convert.ToSingle(reader.Value);
                    }
                    else if (propName == "Y")
                    {
                        y = Convert.ToSingle(reader.Value);
                    }
                }
                else if (reader.TokenType == JsonToken.EndObject)
                {
                    break;
                }
            }
            return new Vector2(x, y);
        }
    }
}
