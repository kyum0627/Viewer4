using IGX.ViewControl.GLDataStructure;
using OpenTK.Mathematics;

namespace IGX.ViewControl.Buffer
{
    /// <summary>
    /// 인스턴스 생성을 위한 팩토리 인터페이스
    /// </summary>
    public interface IInstanceFactory<NST> where NST : unmanaged
    {
        /// <summary>
        /// 기본 인스턴스 생성
        /// </summary>
        NST CreateDefault();
    }

    /// <summary>
    /// MeshInstanceGL용 팩토리
    /// </summary>
    public class MeshInstanceFactory : IInstanceFactory<MeshInstanceGL>
    {
        public MeshInstanceGL CreateDefault() => new MeshInstanceGL 
        { 
            Model = Matrix4.Identity 
        };
    }

    /// <summary>
    /// BasicInstance용 팩토리
    /// </summary>
    public class BasicInstanceFactory : IInstanceFactory<BasicInstance>
    {
        public BasicInstance CreateDefault() => new BasicInstance 
        { 
            Model = Matrix4.Identity, 
            Color = Vector4.One 
        };
    }

    /// <summary>
    /// 인스턴스 팩토리 제공자 (정적 헬퍼)
    /// </summary>
    public static class InstanceFactory
    {
        /// <summary>
        /// 타입에 맞는 기본 인스턴스 생성
        /// </summary>
        public static NST CreateDefault<NST>() where NST : unmanaged
        {
            if (typeof(NST) == typeof(MeshInstanceGL))
            {
                return (NST)(object)new MeshInstanceGL { Model = Matrix4.Identity };
            }
            
            if (typeof(NST) == typeof(BasicInstance))
            {
                return (NST)(object)new BasicInstance 
                { 
                    Model = Matrix4.Identity, 
                    Color = Vector4.One 
                };
            }
            
            // 알려지지 않은 타입은 default 반환
            return default;
        }

        /// <summary>
        /// 여러 개의 기본 인스턴스 생성
        /// </summary>
        public static NST[] CreateDefaults<NST>(int count) where NST : unmanaged
        {
            if (count <= 0) 
                throw new ArgumentException("개수는 1 이상이어야 합니다.", nameof(count));

            var instances = new NST[count];
            for (int i = 0; i < count; i++)
            {
                instances[i] = CreateDefault<NST>();
            }
            
            return instances;
        }

        /// <summary>
        /// 변환 행렬을 적용한 인스턴스 생성
        /// </summary>
        public static NST CreateWithTransform<NST>(Matrix4 transform) where NST : unmanaged
        {
            if (typeof(NST) == typeof(MeshInstanceGL))
            {
                return (NST)(object)new MeshInstanceGL { Model = transform };
            }
            
            if (typeof(NST) == typeof(BasicInstance))
            {
                return (NST)(object)new BasicInstance 
                { 
                    Model = transform, 
                    Color = Vector4.One 
                };
            }
            
            return default;
        }

        /// <summary>
        /// 변환 행렬 배열로부터 인스턴스 배열 생성
        /// </summary>
        public static NST[] CreateFromTransforms<NST>(params Matrix4[] transforms) where NST : unmanaged
        {
            if (transforms == null || transforms.Length == 0)
                throw new ArgumentException("변환 행렬이 비어있습니다.", nameof(transforms));

            var instances = new NST[transforms.Length];
            for (int i = 0; i < transforms.Length; i++)
            {
                instances[i] = CreateWithTransform<NST>(transforms[i]);
            }
            
            return instances;
        }
    }
}
