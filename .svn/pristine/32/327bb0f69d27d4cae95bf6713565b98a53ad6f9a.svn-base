using OpenTK.Mathematics;

namespace IGX.Geometry.DataStructure
{
    public class VolColor
    {
        public uint ColorKey;
        private MyColor color;

        //public VolColor(uint colorKind, uint colorKey, byte red, byte green, byte blue, byte amb = 255)
        public VolColor(uint colorKey, byte red, byte green, byte blue, byte amb = 255)
        {
            //ColorKind = colorKind;
            ColorKey = colorKey;
            color.R = red;
            color.G = green;
            color.B = blue;
            color.A = amb;
        }
        public Color4 ToBuffer()
        {
            return color.ToBuffer();
        }
    }
}