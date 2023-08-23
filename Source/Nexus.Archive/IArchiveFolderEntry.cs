using System.Collections.Generic;
using System.IO;

namespace Nexus.Archive
{
    public interface IArchiveFolderEntry : IArchiveFilesystemEntry
    {
        IEnumerable<IArchiveFilesystemEntry> EnumerateChildren(bool recurse = false);
        IEnumerable<IArchiveFolderEntry> EnumerateFolders(bool recurse = false);
        IEnumerable<IArchiveFileEntry> EnumerateFiles(bool recurse = false);
        void Write(BinaryWriter writer);
        ulong GetSizeInBytes();
    }
}