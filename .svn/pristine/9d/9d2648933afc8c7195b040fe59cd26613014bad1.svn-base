// 메시 정보를 제공하는 인터페이스
namespace IGX.Geometry.GeometryBuilder
{
    // JSON 직렬화 및 역직렬화 기능을 제공하는 인터페이스
    // 제네릭 타입 T는 직렬화/역직렬화될 객체의 타입을 나타냄.
    public interface IJsonSerializable<T>
    {
        /// <summary>
        /// 현재 객체를 JSON 문자열로 직렬화.
        /// </summary>
        /// <returns>객체의 JSON 표현을 담은 문자열.</returns>
        string ToJson();

        /// <summary>
        /// 주어진 JSON 문자열로부터 객체를 역직렬화.
        /// </summary>
        /// <param name="json">역직렬화할 JSON 문자열.</param>
        /// <returns>JSON 데이터로 채워진 새로운 T 타입 객체 또는 null.</returns>
        T? FromJson(string json);
    }
}
