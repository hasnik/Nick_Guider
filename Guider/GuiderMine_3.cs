using System.Runtime.InteropServices;

namespace Guider
{
    // Writing/reading Guid bytes through explicit layout struct in 4 byte batches represent as ints
    internal class GuiderMine_3
    {
        // Used to encode to base64 without characters illegal in URL
        // This is normal base64 with following exceptions
        // byte 62 is encoded as `_` instead of `+`
        // byte 63 is encoded as `-` instead of `/`
        private static readonly char[] urlSafeBase64EncodingMap = new char[64] {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P',
            'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'a', 'b', 'c', 'd', 'e', 'f',
            'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v',
            'w', 'x', 'y', 'z', '0', '1', '2', '3', '4', '5', '6', '7',  '8', '9', '_', '-'
        };

        // Used to decode from urlSafeBase64 encoding
        // This is normal base64:
        // `A (byte 65) - Z (byte 90)` map to bytes 0-25
        // `a (byte 97) - z (byte 122)` (97-122) map to bytes 26-51
        // `0 (byte 48) - 9 (byte 57)` (48-57) map to bytes 52-61
        // with following exceptions        
        // char `_` (byte 95) maps to byte 62
        // char `-` (byte 45) maps to byte 63
        // char `+` char is invalid
        // char `/` char is invalid
        // if there's a need for input validation -1 means character invalid
        private static readonly sbyte[] urlSafeBase64DecodingMap = {
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 63, -1, -1,
            52, 53, 54, 55, 56, 57, 58, 59, 60, 61, -1, -1, -1, -1, -1, -1,
            -1,  0,  1,  2,  3,  4,  5,  6,  7,  8,  9, 10, 11, 12, 13, 14,
            15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, -1, -1, -1, -1, 62,
            -1, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
            41, 42, 43, 44, 45, 46, 47, 48, 49, 50, 51, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
            -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
        };

        // * No loop for performance (hardcore loop unroll ;) )
        // * Going from last to first to help JIT eliminate bounds checking
        // * Byte shifts are used to get relevant part of bytes for base64 encoding
        // * Due to custom urlSafeBase64EncodingMap there's no need to change invalid URL characters later
        // * Reading Guid bytes in 4 byte batches through explicit layout struct, but perf tests show perf degradation
        public static string ToStringFromGuid(Guid id)
        {
            Span<char> idHash = stackalloc char[22];
            var guidMemoryReader = new GuidMemoryBatchReader(id);

            idHash[21] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom96To127 >> 24 & 0x03) << 4];
            idHash[20] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom96To127 >> 24 & 0xfc) >> 2];

            idHash[19] = urlSafeBase64EncodingMap[guidMemoryReader.GuidBitsFrom96To127 >> 16 & 0x3F];
            idHash[18] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom96To127 >> 8 & 0x0F) << 2 | (guidMemoryReader.GuidBitsFrom96To127 >> 16 & 0xC0) >> 6];
            idHash[17] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom96To127 & 0x03) << 4 | (guidMemoryReader.GuidBitsFrom96To127 >> 8 & 0xF0) >> 4];
            idHash[16] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom96To127 & 0xFC) >> 2];

            idHash[15] = urlSafeBase64EncodingMap[guidMemoryReader.GuidBitsFrom64To95 >> 24 & 0x3f];
            idHash[14] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom64To95 >> 16 & 0x0f) << 2 | (guidMemoryReader.GuidBitsFrom64To95 >> 24 & 0xc0) >> 6];
            idHash[13] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom64To95 >> 8 & 0x03) << 4 | (guidMemoryReader.GuidBitsFrom64To95 >> 16 & 0xf0) >> 4];
            idHash[12] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom64To95 >> 8 & 0xfc) >> 2];

            idHash[11] = urlSafeBase64EncodingMap[guidMemoryReader.GuidBitsFrom64To95 & 0x3f];
            idHash[10] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom32To63 >> 24 & 0x0f) << 2 | (guidMemoryReader.GuidBitsFrom64To95 & 0xc0) >> 6];
            idHash[9] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom32To63 >> 16 & 0x03) << 4 | (guidMemoryReader.GuidBitsFrom32To63 >> 24 & 0xf0) >> 4];
            idHash[8] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom32To63 >> 16 & 0xfc) >> 2];

            idHash[7] = urlSafeBase64EncodingMap[guidMemoryReader.GuidBitsFrom32To63 >> 8 & 0x3f];
            idHash[6] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom32To63 & 0x0f) << 2 | (guidMemoryReader.GuidBitsFrom32To63 >> 8 & 0xc0) >> 6];
            idHash[5] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom0To31 >> 24 & 0x03) << 4 | (guidMemoryReader.GuidBitsFrom32To63 & 0xf0) >> 4];
            idHash[4] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom0To31 >> 24 & 0xfc) >> 2];

            idHash[3] = urlSafeBase64EncodingMap[guidMemoryReader.GuidBitsFrom0To31 >> 16 & 0x3F];
            idHash[2] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom0To31 >> 8 & 0x0F) << 2 | (guidMemoryReader.GuidBitsFrom0To31 >> 16 & 0xC0) >> 6];
            idHash[1] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom0To31 & 0x03) << 4 | (guidMemoryReader.GuidBitsFrom0To31 >> 8 & 0xF0) >> 4];
            idHash[0] = urlSafeBase64EncodingMap[(guidMemoryReader.GuidBitsFrom0To31 & 0xFC) >> 2];

            return new string(idHash);
        }

        // * No loop for performance (hardcore loop unroll ;) )
        // * Going from last to first to help JIT eliminate bounds checking
        // * Byte shifts are used to get relevant part of bytes for base64 decoding
        // * Due to custom urlSafeBase64DecodingMap there's no need to change custom (`_`, `-`) characters later
        // * Writing Guid bytes in 4 byte batches through explicit layout struct
        // * Decoding 42 bits to 4 bytes at a time
        public static Guid ToGuidFromString(ReadOnlySpan<char> idHash)
        {
            // Decode bytes [16-20] and 2 bits from byte 21
            var idBitsFrom96To127 =
                  (urlSafeBase64DecodingMap[idHash[21]] & 0x30) << 20
                | urlSafeBase64DecodingMap[idHash[20]] << 26
                | urlSafeBase64DecodingMap[idHash[19]] << 16
                | (urlSafeBase64DecodingMap[idHash[18]] & 0x3) << 22
                | (urlSafeBase64DecodingMap[idHash[18]] & 0x3C) << 6
                | (urlSafeBase64DecodingMap[idHash[17]] & 0xF) << 12
                | urlSafeBase64DecodingMap[idHash[17]] >> 4
                | urlSafeBase64DecodingMap[idHash[16]] << 2;

            // Decode 2 bits from byte 10, bytes [11-15]
            var idBitsFrom64To95 =
                  urlSafeBase64DecodingMap[idHash[15]] << 24
                | (urlSafeBase64DecodingMap[idHash[14]] & 0x3) << 30
                | (urlSafeBase64DecodingMap[idHash[14]] & 0x3C) << 14
                | (urlSafeBase64DecodingMap[idHash[13]] & 0xF) << 20
                | (urlSafeBase64DecodingMap[idHash[13]] & 0x30) << 4
                | urlSafeBase64DecodingMap[idHash[12]] << 10
                | urlSafeBase64DecodingMap[idHash[11]] << 0
                | (urlSafeBase64DecodingMap[idHash[10]] & 0x3) << 6;

            // Decode 4 bits from byte 5, bytes [6-9] and 4 bits from byte 10
            var idBitsFrom32To63 =
                  (urlSafeBase64DecodingMap[idHash[10]] & 0x3C) << 22
                | (urlSafeBase64DecodingMap[idHash[9]] & 0xF) << 28
                | (urlSafeBase64DecodingMap[idHash[9]] & 0x30) << 12
                | urlSafeBase64DecodingMap[idHash[8]] << 18
                | urlSafeBase64DecodingMap[idHash[7]] << 8
                | (urlSafeBase64DecodingMap[idHash[6]] & 0x3) << 14
                | urlSafeBase64DecodingMap[idHash[6]] >> 2
                | (urlSafeBase64DecodingMap[idHash[5]] & 0xF) << 4;

            // Decode bytes [0-4] and 2 bits from byte 5
            var idBitsFrom0To31 =
                  (urlSafeBase64DecodingMap[idHash[5]] & 0x30) << 20
                | urlSafeBase64DecodingMap[idHash[4]] << 26
                | urlSafeBase64DecodingMap[idHash[3]] << 16
                | (urlSafeBase64DecodingMap[idHash[2]] & 0x3) << 22
                | (urlSafeBase64DecodingMap[idHash[2]] & 0x3C) << 6
                | (urlSafeBase64DecodingMap[idHash[1]] & 0xF) << 12
                | urlSafeBase64DecodingMap[idHash[1]] >> 4
                | urlSafeBase64DecodingMap[idHash[0]] << 2;

            // Use struct with explicit layout
            // to reinterpret two longs as Guid
            var guidMemoryWriter = new GuidMemoryBatchWriter(
                idBitsFrom0To31,
                idBitsFrom32To63,
                idBitsFrom64To95,
                idBitsFrom96To127);

            return guidMemoryWriter.Guid;
        }

        // Helper to reinterpret Guid as bytes without casting
        // Guid memory can be accessed in 4 byte batches represented as ints
        [StructLayout(LayoutKind.Explicit)]
        private readonly ref struct GuidMemoryBatchReader
        {
            internal GuidMemoryBatchReader(Guid guid)
                : this()
            {
                Guid = guid;
            }

            [FieldOffset(0)]
            internal readonly int GuidBitsFrom0To31;

            [FieldOffset(4)]
            internal readonly int GuidBitsFrom32To63;

            [FieldOffset(8)]
            internal readonly int GuidBitsFrom64To95;

            [FieldOffset(12)]
            internal readonly int GuidBitsFrom96To127;

            [FieldOffset(0)]
            internal readonly Guid Guid;
        }

        // Helper to reinterpret bytes as a Guid without casting
        // Guid memory can be written in 8-byte batches represented as longs
        [StructLayout(LayoutKind.Explicit)]
        private readonly ref struct GuidMemoryBatchWriter
        {
            internal GuidMemoryBatchWriter(
                int guidBitsFrom0To31,
                int guidBitsFrom32To63,
                int guidBitsFrom64To95,
                int guidBitsFro96To127)
                : this()
            {
                GuidBitsFrom0To31 = guidBitsFrom0To31;
                GuidBitsFrom32To63 = guidBitsFrom32To63;
                GuidBitsFrom64To95 = guidBitsFrom64To95;
                GuidBitsFrom96To127 = guidBitsFro96To127;
            }

            [FieldOffset(0)]
            internal readonly int GuidBitsFrom0To31;

            [FieldOffset(4)]
            internal readonly int GuidBitsFrom32To63;

            [FieldOffset(8)]
            internal readonly int GuidBitsFrom64To95;

            [FieldOffset(12)]
            internal readonly int GuidBitsFrom96To127;

            [FieldOffset(0)]
            internal readonly Guid Guid;
        }
    }
}
