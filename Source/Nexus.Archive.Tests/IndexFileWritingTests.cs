using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Xunit;

namespace Nexus.Archive.Tests
{
    public class IndexFileWritingTest : IClassFixture<CoreDataArchiveFixture>
    {
        public ArchiveFile CoreData { get; }
        public string PatchPath { get; set; }
        public string DumpPath { get; set; }

        public IndexFileWritingTest(CoreDataArchiveFixture coreDataFixture)
        {
            CoreData = coreDataFixture.CoreData;
            PatchPath = coreDataFixture.BasePath;
            DumpPath = Directory.CreateDirectory("Dump").FullName;
        }

        [Fact]
        public void BasicTest()
        {
            /*foreach(var file in Directory.EnumerateFiles(PatchPath).Where(f => Path.GetExtension(f).Equals(".index", System.StringComparison.InvariantCultureIgnoreCase)))
            {
                IndexFileTest(file);
            }*/
            //IndexFileTest(Path.Combine(PatchPath, "Client64.index"));
            IndexFileTest(Path.Combine(PatchPath, "Patch.index"));
        }

        private void IndexFileTest(string filePath)
        {
            IndexFile index = (IndexFile) IndexFile.FromFile(filePath);
            string testFile = Path.Combine(DumpPath, Path.GetFileName(filePath));
            index.ToFile(testFile);

            using (Stream stream1 = File.OpenRead(filePath))
            using (Stream stream2 = File.OpenRead(testFile))
            {
                areStreamsEqual(stream1, stream2);
            }
        }

        private void areStreamsEqual(Stream stream1, Stream stream2)
        {
            stream1.Position = 0;
            stream2.Position = 0;
            Assert.Equal(stream1.Length, stream2.Length);

            int position = 0;

            byte[] buf1 = new byte[10000];
            byte[] buf2 = new byte[10000];

            while (position < stream1.Length)
            {
                int amount = (int)stream1.Length - position;
                if (amount > 10000)
                {
                    amount = 10000;
                }

                stream1.Read(buf1, 0, amount);
                stream2.Read(buf2, 0, amount);
                position += amount;

                for(int i = 0; i < amount; ++i)
                {
                    Assert.Equal(buf1[i], buf2[i]);
                }

                //Assert.True(Enumerable.SequenceEqual(buf1, buf2));
            }
        }
    }
}