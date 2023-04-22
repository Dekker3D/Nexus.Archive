using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Nexus.Archive.Extensions;
using Nexus.Archive.Patcher;
using Xunit;
using Xunit.Abstractions;

namespace Nexus.Archive.Tests
{
    public class IndexFileWritingTest : IClassFixture<CoreDataArchiveFixture>
    {
        public ArchiveFile CoreData { get; }
        public string PatchPath { get; set; }

        public IndexFileWritingTest(CoreDataArchiveFixture coreDataFixture)
        {
            CoreData = coreDataFixture.CoreData;
            PatchPath = coreDataFixture.BasePath;
        }

        [Fact]
        public void BasicTest()
        {
            
        }
    }
}