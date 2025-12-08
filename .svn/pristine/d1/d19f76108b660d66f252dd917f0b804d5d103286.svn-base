using OpenTK.Mathematics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace IGX.ViewControl.GLDataStructure
{
    public static class OpenGLStd140Helper
    {
        public static int GetAlignedSize<T>() where T : struct
        {
            var type = typeof(T);
            int currentOffset = 0;
            int maxAlignment = 1;

            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                int fieldSize = GetFieldSize(field.FieldType);
                int fieldAlignment = GetFieldAlignment(field.FieldType);

                currentOffset = (currentOffset + fieldAlignment - 1) / fieldAlignment * fieldAlignment;

                currentOffset += fieldSize;
                maxAlignment = Math.Max(maxAlignment, fieldAlignment);
            }
            return (currentOffset + maxAlignment - 1) / maxAlignment * maxAlignment;
        }

        private static int GetFieldSize(Type fieldType)
        {
            if (fieldType == typeof(Vector2) || fieldType == typeof(Vector3) || fieldType == typeof(Vector4))
            {
                return 4 * fieldType.GenericTypeArguments.Length; // float32 = 4 bytes
            }
            if (fieldType == typeof(Matrix4))
            {
                return 4 * 4 * 4; // 4x4 행렬 * 4바이트
            }
            // 그 외 기본 타입에 대한 크기 반환
            return Marshal.SizeOf(fieldType);
        }

        private static int GetFieldAlignment(Type fieldType)
        {
            if (fieldType == typeof(Vector2)) return 8;
            if (fieldType == typeof(Vector3) || fieldType == typeof(Vector4)) return 16;
            if (fieldType == typeof(Matrix4)) return 16;
            if (fieldType.IsArray) return 16; // 배열은 16바이트 경계
            // 그 외 기본 타입
            return Marshal.SizeOf(fieldType);
        }
    }

    public enum BufferLayout
    {
        Std140,
        Std430 // Std430도 이와 유사한 방식으로 구현 가능
    }
}
