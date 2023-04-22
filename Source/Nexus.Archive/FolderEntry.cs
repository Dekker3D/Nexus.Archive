﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Nexus.Archive
{
    public class FolderEntry : FilesystemEntry, IArchiveFolderEntry
    {
        private const int FolderEntrySize = 8;
        private readonly FileEntry[] _files;
        private readonly FolderPointer[] _folderPointers;
        private readonly IndexFile _index;
        private readonly Lazy<List<IArchiveFilesystemEntry>> _lazyChildrenReader;

        public FolderEntry(string name, IndexFile indexFile, BinaryReader reader)
        {
            _index = indexFile;
            Path = name;
            _lazyChildrenReader = new Lazy<List<IArchiveFilesystemEntry>>(ReadChildren, false);
            Subdirectories = reader.ReadInt32();
            Files = reader.ReadInt32();
            var dataSize = FolderPointer.Size * Subdirectories + FileEntry.Size * Files + 8;
            var stringLength = reader.BaseStream.Length - dataSize;
            _folderPointers = new FolderPointer[Subdirectories];
            _files = new FileEntry[Files];
            for (var x = 0; x < Subdirectories; x++) _folderPointers[x] = FolderPointer.FromReader(reader);

            for (var x = 0; x < Files; x++)
            {
                _files[x] = FileEntry.FromReader(reader);
                if (((int) _files[x].Flags & 1) != 1)
                    Debugger.Break();
            }

            string ReadName(int itemNameOffset)
            {
                reader.BaseStream.Seek(dataSize + itemNameOffset, SeekOrigin.Begin);
                var nameBuilder = new StringBuilder();
                char next;
                while ((next = reader.ReadChar()) != '\0') nameBuilder.Append(next);

                return nameBuilder.ToString();
            }

            foreach (var folder in _folderPointers)
                folder.Name = GetChildPath(ReadName((int) folder.NameOffset));

            foreach (var file in _files)
                file.Path = GetChildPath(ReadName(file.NameOffset));
            //var nameData = reader.ReadBytes((int)nameLength);
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(Subdirectories);
            writer.Write(Files);
            foreach(FolderPointer folder in _folderPointers)
            {
                folder.Write(writer);
            }
            foreach(FileEntry file in _files)
            {
                file.Write(writer);
            }
        }

        public int Subdirectories { get; }
        public int Files { get; }
        public IEnumerable<IArchiveFilesystemEntry> Children => _lazyChildrenReader.Value;

        public IEnumerable<IArchiveFilesystemEntry> EnumerateChildren(bool recurse = false)
        {
            foreach (var child in Children)
            {
                yield return child;
                if (!recurse || !(child is IArchiveFolderEntry folder))
                    continue;

                foreach (var innerChild in folder.EnumerateChildren(true)) yield return innerChild;
            }
        }

        public IEnumerable<IArchiveFolderEntry> EnumerateFolders(bool recurse = false)
        {
            return EnumerateChildren(recurse).OfType<IArchiveFolderEntry>();
        }

        public IEnumerable<IArchiveFileEntry> EnumerateFiles(bool recurse = false)
        {
            return EnumerateChildren(recurse).OfType<IArchiveFileEntry>();
        }

        private string GetChildPath(string name)
        {
            if (string.IsNullOrWhiteSpace(Path)) return name;
            return System.IO.Path.Combine(Path, name);
        }

        public override string ToString()
        {
            return Path;
        }

        private List<IArchiveFilesystemEntry> ReadChildren()
        {
            var allFiles = new List<IArchiveFilesystemEntry>();
            foreach (var folderPointer in _folderPointers)
                allFiles.Add(new FolderEntry(folderPointer.Name, _index,
                    new BinaryReader(_index.GetBlockView(folderPointer.FolderBlock), Encoding.UTF8)));

            allFiles.AddRange(_files);
            return allFiles;
        }
    }
}