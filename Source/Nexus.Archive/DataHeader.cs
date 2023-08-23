using System.IO;
using System.Runtime.InteropServices;

namespace Nexus.Archive
{
    public struct DataHeader
    {
        //public ulong Unknown1;
        public ulong FileSize; // Seems to be related to the size of the associated .archive file.
        public ulong Reserved; // Doesn't seem to matter.
        public ulong BlockTableOffset; // Seems to be related to the size of the associated .archive file.
        public long BlockCount;
        public long RootBlockIndex;
        public ulong ReverseSeekGuard; // Must be zero
        public ulong Unknown1;
        public ulong Unknown2;
        public uint Unknown3;
        public uint Unknown4;
        public ulong Unknown5;
        public ulong Unknown6;

        public static DataHeader Create()
        {
            return new DataHeader();
        }

        public static DataHeader ReadFrom(BinaryReader binaryReader)
        {
            var ret = new DataHeader
            {
                //Unknown1 = binaryReader.ReadUInt64(),
                FileSize = binaryReader.ReadUInt64(),
                Reserved = binaryReader.ReadUInt64(),
                BlockTableOffset = binaryReader.ReadUInt64(),
                BlockCount = binaryReader.ReadInt64(),
                RootBlockIndex = binaryReader.ReadInt64(),
                ReverseSeekGuard = binaryReader.ReadUInt64(),
                Unknown1 = binaryReader.ReadUInt64(),
                Unknown2 = binaryReader.ReadUInt64(),
                Unknown3 = binaryReader.ReadUInt32(),
                Unknown4 = binaryReader.ReadUInt32(),
                Unknown5 = binaryReader.ReadUInt64(),
                Unknown6 = binaryReader.ReadUInt64(),
            };
            return ret;
        }

        public void WriteTo(BinaryWriter binaryWriter)
        {
            binaryWriter.Write(FileSize);
            binaryWriter.Write(Reserved);
            binaryWriter.Write(BlockTableOffset);
            binaryWriter.Write(BlockCount);
            binaryWriter.Write(RootBlockIndex);
            binaryWriter.Write(ReverseSeekGuard);
            binaryWriter.Write(Unknown1);
            binaryWriter.Write(Unknown2);
            binaryWriter.Write(Unknown3);
            binaryWriter.Write(Unknown4);
            binaryWriter.Write(Unknown5);
            binaryWriter.Write(Unknown6);
        }
    }
}