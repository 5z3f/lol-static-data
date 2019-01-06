using System;
using System.IO;
using System.Text;
using ZstdNet;

namespace Riot.StaticData
{
    internal class RiotWadStructure
    {
        public byte MajorVersion, MinorVersion;
        public ulong XXHash, XXHashChecksum = 0;
        public uint CompressedSize, UncompressedSize;

        public uint DataOffset;
        public byte DataEntryType;

        public byte[] ECDSASignature;
        public uint FileCount;
        public bool IsDuplicated;
    }

    internal class RiotWad : RiotWadStructure
    {
        public static byte[] UnpackGlobalFile(string FileName)
        {
            RiotWadStructure wadStructure = new RiotWadStructure();
            MemoryStream memoryStream = new MemoryStream(File.ReadAllBytes(FileName));

            using (BinaryReader binaryReader = new BinaryReader(memoryStream))
            {
                bool IsFileValid = Encoding.ASCII.GetString(binaryReader.ReadBytes(2)) == "RW" ? true : false;

                if (!IsFileValid)
                {
                    Console.WriteLine($"Wad file is not valid");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                wadStructure.MajorVersion = binaryReader.ReadByte();
                wadStructure.MinorVersion = binaryReader.ReadByte();

                if (wadStructure.MajorVersion != 3)
                {
                    Console.WriteLine($"Wad version not supported | Version: {wadStructure.MajorVersion}");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                wadStructure.ECDSASignature = binaryReader.ReadBytes(256);
                wadStructure.XXHashChecksum = binaryReader.ReadUInt64();

                wadStructure.FileCount = binaryReader.ReadUInt32();

                if (wadStructure.FileCount > 1)
                {
                    Console.WriteLine($"Wad file contains more than 1 chunk (riot changes?) | FileCount: {wadStructure.FileCount}");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

                wadStructure.XXHash = binaryReader.ReadUInt64();
                wadStructure.DataOffset = binaryReader.ReadUInt32();
                wadStructure.CompressedSize = binaryReader.ReadUInt32();
                wadStructure.UncompressedSize = binaryReader.ReadUInt32();
                wadStructure.DataEntryType = binaryReader.ReadByte();
                wadStructure.IsDuplicated = binaryReader.ReadBoolean();

                byte[] chunkBuffer = new byte[wadStructure.CompressedSize];

                memoryStream.Seek(wadStructure.DataOffset, SeekOrigin.Begin);
                memoryStream.Read(chunkBuffer, 0, (int)wadStructure.CompressedSize);

                using (var decompressor = new Decompressor())
                    return decompressor.Unwrap(chunkBuffer);
            }
        }
    }
}
