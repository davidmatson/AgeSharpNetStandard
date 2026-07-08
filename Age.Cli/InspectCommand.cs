using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Age.Cli
{
    internal struct InspectOutput
    {
        readonly string _file;
        readonly string _version;
        readonly bool _armored;
        readonly bool _postQuantum;
        readonly InspectRecipient[] _recipients;
        readonly InspectSize _size;

        public InspectOutput(string file, string version, bool armored, bool postQuantum, InspectRecipient[] recipients, InspectSize size)
        {
            _file = file;
            _version = version;
            _armored = armored;
            _postQuantum = postQuantum;
            _recipients = recipients;
            _size = size;
        }

        public string File => _file;
        public string Version => _version;
        public bool Armored => _armored;
        public bool PostQuantum => _postQuantum;
        public InspectRecipient[] Recipients => _recipients;
        public InspectSize Size => _size;
    }

    internal struct InspectRecipient
    {
        readonly int _index;
        readonly string _type;
        readonly string[] _args;

        public InspectRecipient(int index, string type, string[] args)
        {
            _index = index;
            _type = type;
            _args = args;
        }

        public int Index => _index;
        public string Type => _type;
        public string[] Args => _args;
    }

    internal struct InspectSize
    {
        readonly long _header;
        readonly long _overhead;
        readonly long _payload;
        readonly long _total;

        public InspectSize(long header, long overhead, long payload, long total)
        {
            _header = header;
            _overhead = overhead;
            _payload = payload;
            _total = total;
        }

        public long Header => _header;
        public long Overhead => _overhead;
        public long Payload => _payload;
        public long Total => _total;
    }

    internal static class InspectCommand
    {
        public static int Execute(string filePath, bool json)
        {
            var (rawInput, displayName) = filePath != null
                ? (File.OpenRead(filePath), filePath)
                : (Console.OpenStandardInput(), "(stdin)");

            using (rawInput)
            {
                var ms = new MemoryStream();

                rawInput.CopyTo(ms);
                var totalSize = ms.Length;
                ms.Position = 0;

                var header = AgeHeader.Parse(ms);

                if (json)
                    PrintJson(header, displayName, totalSize);
                else
                    PrintHuman(header, displayName, totalSize);
            }

            return 0;
        }

        private const int PayloadNonceSize = 16;
        private const int ChunkSize = 64 * 1024;
        private const int TagSize = 16;
        private const int EncryptedChunkSize = ChunkSize + TagSize;

        private static readonly HashSet<string> PostQuantumTypes = new HashSet<string> { "mlkem768x25519" };

        private static void PrintHuman(AgeHeader header, string displayName, long totalSize)
        {
            Console.WriteLine($"{displayName} is an age file, version \"age-encryption.org/v1\".");
            Console.WriteLine();

            var types = header.Recipients.Select(s => s.Type).Distinct().ToList();
            Console.WriteLine("This file is encrypted to the following recipient types:");

            foreach (var type in types)
                Console.WriteLine($"  - \"{type}\"");

            Console.WriteLine();

            var hasPq = types.Any(t => PostQuantumTypes.Contains(t));
            Console.WriteLine(hasPq
                ? "This file uses post-quantum encryption."
                : "This file does NOT use post-quantum encryption.");

            Console.WriteLine();

            var sizes = ComputeSizes(header, totalSize);
            Console.WriteLine("Size breakdown (assuming it decrypts successfully):");
            Console.WriteLine();
            Console.WriteLine($"    {"Header",-24}{sizes.Header,8} bytes");
            Console.WriteLine($"    {"Encryption overhead",-24}{sizes.Overhead,8} bytes");
            Console.WriteLine($"    {"Payload",-24}{sizes.Payload,8} bytes");
            Console.WriteLine($"    {"",24}-------------------");
            Console.WriteLine($"    {"Total",-24}{sizes.Total,8} bytes");
            Console.WriteLine();

            Console.WriteLine("Tip: for machine-readable output, use --json.");
        }

        private static void PrintJson(AgeHeader header, string displayName, long totalSize)
        {
            var sizes = ComputeSizes(header, totalSize);

            var obj = new InspectOutput(
                file: displayName,
                version: "age-encryption.org/v1",
                armored: header.IsArmored,
                postQuantum: header.Recipients.Any(s => PostQuantumTypes.Contains(s.Type)),
                recipients: header.Recipients.Select((s, i) => new InspectRecipient(i, s.Type, s.Args.ToArray())).ToArray(),
                size: new InspectSize(sizes.Header, sizes.Overhead, sizes.Payload, sizes.Total)
            );

            Console.WriteLine(JsonSerializer.Serialize(obj,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true }));
        }

        private struct SizeBreakdown
        {
            readonly long _header;
            readonly long _overhead;
            readonly long _payload;
            readonly long _total;
            
            public SizeBreakdown(long header, long overhead, long payload, long total)
            {
                _header = header;
                _overhead = overhead;
                _payload = payload;
                _total = total;
            }

            public long Header => _header;
            public long Overhead => _overhead;
            public long Payload => _payload;
            public long Total => _total;
        }

        private static SizeBreakdown ComputeSizes(AgeHeader header, long totalSize)
        {
            var headerSize = header.PayloadOffset;
            var encryptedPayload = totalSize - headerSize;
            var overhead = ComputeOverhead(encryptedPayload);
            var payload = encryptedPayload - overhead;
            return new SizeBreakdown(headerSize, overhead, payload, totalSize);
        }

        private static long ComputeOverhead(long encryptedPayload)
        {
            if (encryptedPayload <= PayloadNonceSize)
                return encryptedPayload;

            var afterNonce = encryptedPayload - PayloadNonceSize;
            var fullChunks = afterNonce / EncryptedChunkSize;
            var remainder = afterNonce % EncryptedChunkSize;
            var totalChunks = fullChunks + (remainder > 0 ? 1 : 0);
            return PayloadNonceSize + totalChunks * TagSize;
        }
    }
}
