using System.Buffers.Text;
using System.Runtime.InteropServices;

namespace Guider
{
    // Reading Guid through explicit layout struct actually degraded performance
    // So i tried to improve upon GuiderMine_2 version by going from end to beginning
    // to let JIT eliminate bounds checking
    // Writing Guid through explicit layout on the other hand brings performance improvement
    // So I tried to write Guid bytes using 8 byte batches represented as longs
    internal class GuiderMine_4
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
        private static readonly long[] urlSafeBase64DecodingMap = {
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
        // * Went back to reading bytes through MemoryMarshal as it turned out to be more performant
        public static string ToStringFromGuid(Guid id)
        {
            Span<byte> idBytes = stackalloc byte[16];
            Span<char> idHash = stackalloc char[22];

            MemoryMarshal.Write(idBytes, ref id);

            idHash[21] = urlSafeBase64EncodingMap[(idBytes[15] & 0x03) << 4];
            idHash[20] = urlSafeBase64EncodingMap[(idBytes[15] & 0xFC) >> 2];

            idHash[19] = urlSafeBase64EncodingMap[idBytes[14] & 0x3F];
            idHash[18] = urlSafeBase64EncodingMap[(idBytes[13] << 2 & 0x3F) | (idBytes[14] >> 6)];
            idHash[17] = urlSafeBase64EncodingMap[(idBytes[12] << 4 & 0x3F) | (idBytes[13] >> 4)];
            idHash[16] = urlSafeBase64EncodingMap[idBytes[12] >> 2];

            idHash[15] = urlSafeBase64EncodingMap[idBytes[11] & 0x3F];
            idHash[14] = urlSafeBase64EncodingMap[(idBytes[10] << 2 & 0x3F) | (idBytes[11] >> 6)];
            idHash[13] = urlSafeBase64EncodingMap[(idBytes[9] << 4 & 0x3F) | (idBytes[10] >> 4)];
            idHash[12] = urlSafeBase64EncodingMap[idBytes[9] >> 2];

            idHash[11] = urlSafeBase64EncodingMap[idBytes[8] & 0x3F];
            idHash[10] = urlSafeBase64EncodingMap[(idBytes[7] << 2 & 0x3F) | (idBytes[8] >> 6)];
            idHash[9] = urlSafeBase64EncodingMap[(idBytes[6] << 4 & 0x3F) | (idBytes[7] >> 4)];
            idHash[8] = urlSafeBase64EncodingMap[idBytes[6] >> 2];

            idHash[7] = urlSafeBase64EncodingMap[idBytes[5] & 0x3F];
            idHash[6] = urlSafeBase64EncodingMap[(idBytes[4] << 2 & 0x3F) | (idBytes[5] >> 6)];
            idHash[5] = urlSafeBase64EncodingMap[(idBytes[3] << 4 & 0x3F) | (idBytes[4] >> 4)];
            idHash[4] = urlSafeBase64EncodingMap[idBytes[3] >> 2];

            idHash[3] = urlSafeBase64EncodingMap[idBytes[2] & 0x3F];
            idHash[2] = urlSafeBase64EncodingMap[(idBytes[1] << 2 & 0x3F) | (idBytes[2] >> 6)];
            idHash[1] = urlSafeBase64EncodingMap[(idBytes[0] << 4 & 0x3F) | (idBytes[1] >> 4)];
            idHash[0] = urlSafeBase64EncodingMap[idBytes[0] >> 2];

            return new string(idHash);
        }

        // * No loop for performance (hardcore loop unroll ;) )
        // * Going from last to first to help JIT eliminate bounds checking
        // * Byte shifts are used to get relevant part of bytes for base64 decoding
        // * Due to custom urlSafeBase64DecodingMap there's no need to change custom (`_`, `-`) characters later
        // * Writing Guid bytes in 8 byte batches through explicit layout struct
        // * Decoding 84 bits to 8 bytes at a time
        public static Guid ToGuidFromString(ReadOnlySpan<char> idHash)
        {
            // Decode 2 bits from byte 10, bytes [11-20] and 2 bits from byte 21
            var idBitsFrom64To127 =
                  (urlSafeBase64DecodingMap[idHash[21]] & 0x30) << 52
                | urlSafeBase64DecodingMap[idHash[20]] << 58
                | urlSafeBase64DecodingMap[idHash[19]] << 48
                | (urlSafeBase64DecodingMap[idHash[18]] & 0x03) << 54
                | (urlSafeBase64DecodingMap[idHash[18]] & 0x3C) << 38
                | (urlSafeBase64DecodingMap[idHash[17]] & 0x0F) << 44
                | (urlSafeBase64DecodingMap[idHash[17]] & 0x30) << 28
                | urlSafeBase64DecodingMap[idHash[16]] << 34
                | urlSafeBase64DecodingMap[idHash[15]] << 24
                | (urlSafeBase64DecodingMap[idHash[14]] & 0x03) << 30
                | (urlSafeBase64DecodingMap[idHash[14]] & 0x3C) << 14
                | (urlSafeBase64DecodingMap[idHash[13]] & 0x0F) << 20
                | (urlSafeBase64DecodingMap[idHash[13]] & 0x30) << 4
                | urlSafeBase64DecodingMap[idHash[12]] << 10
                | urlSafeBase64DecodingMap[idHash[11]]
                | (urlSafeBase64DecodingMap[idHash[10]] & 0x03) << 6;

            // Decode bytes [0-9] and 4 bits from byte 10
            var idBitsFrom0To63 = (urlSafeBase64DecodingMap[idHash[10]] & 0x3C) << 54
                | (urlSafeBase64DecodingMap[idHash[9]] & 0xF) << 60
                | (urlSafeBase64DecodingMap[idHash[9]] & 0x30) << 44
                | urlSafeBase64DecodingMap[idHash[8]] << 50
                | urlSafeBase64DecodingMap[idHash[7]] << 40
                | (urlSafeBase64DecodingMap[idHash[6]] & 0x3) << 46
                | (urlSafeBase64DecodingMap[idHash[6]] & 0x3C) << 30
                | (urlSafeBase64DecodingMap[idHash[5]] & 0xF) << 36
                | (urlSafeBase64DecodingMap[idHash[5]] & 0x30) << 20
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
                idBitsFrom0To63,
                idBitsFrom64To127);

            return guidMemoryWriter.Guid;
        }

        // Helper to reinterpret bytes as a Guid without casting
        // Guid memory can be written in 8-byte batches represented as longs
        [StructLayout(LayoutKind.Explicit)]
        private readonly ref struct GuidMemoryBatchWriter
        {
            internal GuidMemoryBatchWriter(
                long guidBitsFrom0To63,
                long guidBitsFrom64To127)
                : this()
            {
                GuidBitsFrom0To63 = guidBitsFrom0To63;
                GuidBitsFrom64To127 = guidBitsFrom64To127;
            }

            [FieldOffset(0)]
            internal readonly long GuidBitsFrom0To63;

            [FieldOffset(8)]
            internal readonly long GuidBitsFrom64To127;

            [FieldOffset(0)]
            internal readonly Guid Guid;
        }
    }
}
