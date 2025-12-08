using IGX.Geometry.DataStructure;

namespace IGX.Loader.AMFileLoader
{
    public static class ChunkParser
    {
        public static FileInformation ChunkHEADb(MemoryStream stream, uint version)
        {
            string info = BinaryDataReader.ReadStringb(stream);
            string note = BinaryDataReader.ReadStringb(stream);
            string date = BinaryDataReader.ReadStringb(stream);
            string user = BinaryDataReader.ReadStringb(stream);
            string encoding;

            if (2 <= version)
            {
                encoding = BinaryDataReader.ReadStringb(stream);
            }
            else
            {
                encoding = string.Intern("");
            }

            return new FileInformation(info, "", date, user, encoding);
        }
        public static (string project, string name) ChunkMODLb(MemoryStream stream)
        {
            string project = BinaryDataReader.ReadStringb(stream);
            string name = BinaryDataReader.ReadStringb(stream);
            return (project, name);
        }

        public static VolColor ChunkCOLRb(MemoryStream stream)
        {
            uint colorIndex = BinaryDataReader.ReadUint32(stream); // Color index
            uint RGBA = BinaryDataReader.ReadUint32(stream); // Color components
            (byte R, byte G, byte B, byte A) = ( // RGBA ??  ARGB ??
                (byte)((RGBA >> 24) & 0xff),
                (byte)((RGBA >> 16) & 0xff),
                (byte)((RGBA >> 8) & 0xff),
                (byte)(RGBA & 0xff));
            return new VolColor(colorIndex, R, G, B);
        }
    }
}