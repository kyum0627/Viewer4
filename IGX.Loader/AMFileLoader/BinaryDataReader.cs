using IGX.Geometry.Common;
using OpenTK.Mathematics;
using System.Diagnostics;
using System.Text;

namespace IGX.Loader.AMFileLoader
{
    public static class BinaryDataReader
    {
        // 리틀 엔디안 <--> 빅 엔디안 값 변환
        public static uint EndianConvert(uint value)
        {
            return (uint)((value >> 24) |
                          ((value >> 8) & 0x0000FF00) |
                          ((value << 8) & 0x00FF0000) |
                          ((value << 24) & 0xFF000000));
        }
        public static uint ToUint32(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt32(bytes, 0);
        }

        public static uint ReadUint32(MemoryStream stream)
        {
            try
            {
                byte[] bytes = new byte[4];
                stream!.Read(bytes);
                return bytes.Length != 4 ? throw new IOException($"파일이 손상되어 있음. 위치: {stream.Position} / {stream.Length}") : BinaryDataReader.ToUint32(bytes);
            }
            catch (Exception ex)
            {
                throw new IOException($"{ex.Message}. 파일이 손상되어 있음. 위치: {stream!.Position} / {stream.Length}");
            }
        }

        public static float[] ParseFloatsb(MemoryStream stream, int n)
        {
            if (stream == null)
            {
                throw new InvalidOperationException("스트림이 null.");
            }

            byte[] bytes = new byte[4 * n];
            int bytesRead = stream.Read(bytes, 0, bytes.Length); // 실제로 읽은 바이트 수

            if (bytesRead != bytes.Length)
            {
                if (bytesRead == 0 && stream.Position < stream.Length)
                {
                    throw new IOException($"파일이 손상되어 있음. (읽은 바이트 수: {bytesRead}, 예상 바이트 수: {bytes.Length}) 위치: {stream.Position} / {stream.Length}");
                }
                throw new IOException($"파일이 손상되어 있음. (읽은 바이트 수: {bytesRead}, 예상 바이트 수: {bytes.Length}) 위치: {stream.Position} / {stream.Length}");
            }

            return ToFloats(n, bytes);  // 바이트 배열을 float[]로 변환
        }
        public static float[] ToFloats(int n, byte[] bytes)
        {
            // 시스템의 엔디안이 파일과 일치하는 경우
            if (BitConverter.IsLittleEndian)
            {
                // 엔디안이 다를 경우, 바이트를 제자리에서 반전시키는 것이 가장 효율적.
                for (int i = 0; i < n; i++)
                {
                    // 각 4바이트 묶음을 제자리에서 반전
                    int start = i * 4;
                    byte temp = bytes[start];
                    bytes[start] = bytes[start + 3];
                    bytes[start + 3] = temp;
                    temp = bytes[start + 1];
                    bytes[start + 1] = bytes[start + 2];
                    bytes[start + 2] = temp;
                }
            }

            // MemoryMarshal.Cast를 사용해 바이트 배열을 float 배열로 캐스팅
            return System.Runtime.InteropServices.MemoryMarshal.Cast<byte, float>(bytes).ToArray();
        }

        public static string ReadStringb(MemoryStream stream)
        {
            uint nCharacters = ReadUint32(stream);
            uint length = 4 * nCharacters;
            byte[] bytes = new byte[length];

            // 실제 읽은 바이트 수를 확인
            int bytesRead = stream!.Read(bytes, 0, bytes.Length);
            if (bytesRead > 0)
            {
                // 파일이 UTF-8 인코딩으로 저장되었다고 가정
                Encoding encoding = Encoding.UTF8;

                // 읽어온 바이트 배열의 0번째 인덱스부터 bytesRead만큼을 문자열로 변환
                string text = encoding.GetString(bytes, 0, bytesRead);

                Debug.WriteLine($"읽어온 텍스트 (UTF-8): {text}");

                // 만약 파일이 다른 인코딩이었다면 해당 인코딩을 사용해야 함.
                // 예를 들어, 파일이 Unicode (UTF-16)으로 저장되었다면:
                // Encoding unicodeEncoding = Encoding.Unicode;
                // string unicodeText = unicodeEncoding.GetString(bytes, 0, bytesRead);
                // Debug.WriteLine($"읽어온 텍스트 (Unicode): {unicodeText}");
            }
            if (bytesRead < length)
            {
                // 읽은 바이트가 예상한 크기보다 적을 때
                if (stream.Position == stream.Length)
                {
                    return "EOF";  // 파일 끝에 도달했을 경우
                }
                throw new IOException($"파일이 손상되어 있음. 위치: {stream.Position} / {stream.Length}");
            }

            // 문자열 끝을 찾는 과정
            int end;
            for (end = 0; end < bytesRead; end++)
            {
                if (bytes[end] == 0)
                {
                    break;  // NULL 바이트(0)를 만나면 종료
                }
            }
            return Encoding.UTF8.GetString(bytes, 0, end);
            // 필요한 경우 다른 인코딩을 사용.
            // return Encoding.GetEncoding("euc-kr").GetString(bytes, 0, end);
        }
        public static Vertex[] ParseVerticesb(MemoryStream stream, uint vertexCount)
        {
            float[] p = BinaryDataReader.ParseFloatsb(stream, (int)vertexCount * 6);
            List<Vertex> vertices = new((int)vertexCount); // List로 변경하여 동적 배열을 사용
            HashSet<Vertex> uniqueVertices = new((int)vertexCount); // 중복을 처리할 때 HashSet 사용

            for (int i = 0; i < vertexCount; i++)
            {
                int offset = i * 6;
                Vector3 position = new(p[offset], p[offset + 1], p[offset + 2]);
                Vector3 normal = new(p[offset + 3], p[offset + 4], p[offset + 5]);
                Vertex vertex = new(position, normal);

                if (uniqueVertices.Add(vertex)) // 중복되지 않으면 추가
                {
                    vertices.Add(vertex); // 중복되지 않으면 List에 추가
                }
            }
            return [.. vertices]; // vertices.ToAssay(); //List를 배열로 변환하여 반환
        }
    }
}