using System.IO;

namespace ClimbGames
{
    public abstract class TableRecord
    {
        public abstract void Write(BinaryWriter bw);
        public abstract TableRecord Read(BinaryReader br);
    }
}