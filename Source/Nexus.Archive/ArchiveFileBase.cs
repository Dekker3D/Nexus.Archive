using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Nexus.Archive
{
    public abstract class ArchiveFileBase : IDisposable
    {
        private delegate ArchiveFileBase ArchiveFactory(IViewableData file, ArchiveHeader header, BlockInfoHeader[] blockTable, RootIndexBlock rootBlock);
        private static readonly Dictionary<ArchiveType, ArchiveFactory> TypeHandlers =
            new Dictionary<ArchiveType, ArchiveFactory>();

        static ArchiveFileBase()
        {
            TypeHandlers.Add(ArchiveType.Index, (file, header, blockTable, rootIndexBlock) => new IndexFile(file, header, blockTable, rootIndexBlock));
            TypeHandlers.Add(ArchiveType.Archive, (file, header, blockTable, rootIndexBlock) => new ArchiveFile(file, header, blockTable, rootIndexBlock));
        }

        protected ArchiveFileBase(IViewableData file, ArchiveHeader header,
            BlockInfoHeader[] blockInfoHeaders, RootIndexBlock rootIndex)
        {
            File = file;
            Header = header;
            BlockPointers = blockInfoHeaders;
            RootIndex = rootIndex;
        }

        public string FileName => File.FileName;
        protected IViewableData File { get; }
        public ArchiveHeader Header { get; }
        public BlockInfoHeader[] BlockPointers { get; }
        public RootIndexBlock RootIndex { get; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static RootIndexBlock ReadRootBlock(IViewableData file, BlockInfoHeader rootBlockInfo)
        {
            using (var reader = new BinaryReader(GetBlockView(rootBlockInfo, file)))
            {
                return RootIndexBlock.FromReader(reader);
            }
        }

        private void WriteRootBlock(IViewableData file, BlockInfoHeader rootBlockInfo)
        {
            using (var writer = new BinaryWriter(GetBlockView(rootBlockInfo, file)))
            {
                RootIndex.Write(writer);
            }
        }

        public ulong GetBlockSizeInBytes()
        {
            ulong ret = 0;
            foreach(BlockInfoHeader header in BlockPointers)
            {
                ret += header.Size;
            }
            return ret;
        }

        internal BinaryReader GetBlockReader(int index, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            return new BinaryReader(GetBlockView(index), encoding, false);
        }

        internal Stream GetBlockView(int index, IViewableData file = null)
        {
            return GetBlockView(BlockPointers[index], file ?? File);
        }

        private static Stream GetBlockView(BlockInfoHeader blockInfo, IViewableData file)
        {
            if (blockInfo.Size == 0) return null;
            return file.CreateView((long)blockInfo.Offset, (long)blockInfo.Size);
        }

        /// <summary>
        /// </summary>
        /// <returns>Index block number</returns>
        private static BlockInfoHeader[] ReadBlockPointers(
            IViewableData file, ArchiveHeader header)
        {
            var blockPointers = new BlockInfoHeader[header.DataHeader.BlockCount];
            var startPosition = header.DataHeader.BlockTableOffset;
            var length = header.DataHeader.BlockCount * Marshal.SizeOf<BlockInfoHeader>();
            using (var reader = new BinaryReader(file.CreateView((long)startPosition, length)))
            {
                for (var x = 0; x < header.DataHeader.BlockCount; x++)
                {
                    blockPointers[x] = BlockInfoHeader.FromReader(reader);
                }
            }

            return blockPointers;
        }

        private void WriteBlockPointers(
            IViewableData file)
        {
            var startPosition = Header.DataHeader.BlockTableOffset;
            var length = Header.DataHeader.BlockCount * Marshal.SizeOf<BlockInfoHeader>();
            using (var writer = new BinaryWriter(file.CreateView((long)startPosition, length)))
            {
                for (var x = 0; x < Header.DataHeader.BlockCount; x++)
                {
                    BlockPointers[x].Write(writer);
                }
            }
        }

        public ulong GetBlockPointersSizeInBytes()
        {
            return (ulong)(Header.DataHeader.BlockCount * Marshal.SizeOf<BlockInfoHeader>());
        }


        private static ArchiveHeader ReadHeader(IViewableData file)
        {
            var length = Marshal.SizeOf<ArchiveHeader>();
            using (var stream = file.CreateView(0, length))
            {
                return ArchiveHeader.ReadFrom(stream);
            }
        }

        private void WriteHeader(IViewableData file)
        {
            var length = Marshal.SizeOf<ArchiveHeader>();
            using (var stream = file.CreateView(0, length))
            {
                Header.WriteTo(stream);
            }
        }

        public ulong GetHeaderSizeInBytes()
        {
            return (ulong)Marshal.SizeOf<ArchiveHeader>();
        }

        private static IViewableData OpenFile(string fileName, FileAccess fileAccess = FileAccess.Read, long capacity = 0)
        {
            return new MemoryMappedViewableData(fileName, fileAccess, capacity);
        }

        public static ArchiveFileBase FromFile(string fileName)
        {
            var file = OpenFile(fileName); //MemoryMappedFile.CreateFromFile(fileName, FileMode.Open);
            try
            {
                var header = ReadHeader(file);
                var blockPointerInfo = ReadBlockPointers(file, header);
                var rootBlock = ReadRootBlock(file,
                    blockPointerInfo[header.DataHeader.RootBlockIndex]);
                if (!TypeHandlers.TryGetValue(rootBlock.ArchiveType, out var creator))
                    throw new InvalidOperationException($"Unknown archive type: {rootBlock.ArchiveType:G}");
                return creator(file, header, blockPointerInfo, rootBlock);
            }
            catch(Exception ex)
            {
                try
                {
                    file.Dispose();
                }
                catch
                {
                    // Ignored.
                }

                throw new InvalidDataException($"Failed to read file {fileName}", ex);
            }
        }

        public void ToFile(string fileName)
        {
            using (var file = OpenFile(fileName, FileAccess.ReadWrite, (long)Header.DataHeader.FileSize/*(long)GetSizeInBytes()*/))
            {
                try
                {
                    WriteHeader(file);
                    WriteBlockPointers(file);
                    WriteRootBlock(file, BlockPointers[Header.DataHeader.RootBlockIndex]);
                    Write(file);
                }
                catch (Exception ex)
                {
                    try
                    {
                        file.Dispose();
                    }
                    catch
                    {
                        // Ignored.
                    }

                    throw new InvalidDataException($"Failed to write file {fileName}", ex);
                }
            }
        }

        protected virtual void Write(IViewableData file)
        {

        }

        public virtual ulong GetSizeInBytes()
        {
            return GetHeaderSizeInBytes() + GetBlockPointersSizeInBytes() + GetBlockSizeInBytes();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) File?.Dispose();
        }
    }
}