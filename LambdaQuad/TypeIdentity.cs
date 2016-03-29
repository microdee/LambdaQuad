using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

namespace VVVV.Nodes.LambdaQuad
{
    public enum LQTypes
    {
        Bool,
        Uint,
        Float,
        Vec2,
        Vec3,
        Vec4,
        Mat4x4,
        RawBuffer,
        StructuredBuffer,
        Tex1D,
        Tex2D,
        Tex3D,
        RawBufferGeom
    }
    public class TypeIdentity : Dictionary<Type, LQTypes>
    {
        private static TypeIdentity _instance;
        public static TypeIdentity Instance
        {
            get
            {
                if (_instance == null) _instance = new TypeIdentity();
                return _instance;
            }
            private set { throw new NotImplementedException(); }
        }

        public TypeIdentity()
        {
            Add(typeof(bool), LQTypes.Bool);
            Add(typeof(uint), LQTypes.Uint);
            Add(typeof(float), LQTypes.Float);
            
            Add(typeof(Vector2D), LQTypes.Vec2);
            Add(typeof(Vector3D), LQTypes.Vec3);
            Add(typeof(Vector4D), LQTypes.Vec4);
            Add(typeof(Matrix4x4), LQTypes.Mat4x4);
        }

        public Type this[LQTypes t]
        {
            get
            {
                foreach (var kvp in this)
                {
                    if (kvp.Value == t) return kvp.Key;
                }
                return null;
            }
        }
    }
}
