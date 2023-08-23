﻿namespace Nexus.Archive
{
    public abstract class FilesystemEntry : IArchiveFilesystemEntry
    {
        public string Name { get; internal set; }
        public string Path { get; internal set; }
        public string FileName => System.IO.Path.GetFileName(Path);
        public string FolderPath => System.IO.Path.GetDirectoryName(Path);
    }
}