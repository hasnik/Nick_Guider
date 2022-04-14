using System.Runtime.InteropServices;

namespace Guider
{
    // Custom encoding and decoding map
    internal class GuiderMine_1
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

        // * Going from last to first to help JIT eliminate bounds checking
        // * Byte shifts are used to get relevant part of bytes for base64 encoding
        // * Due to custom urlSafeBase64EncodingMap there's no need to change invalid URL characters later
        public static string ToStringFromGuid(Guid id)
        {
            Span<byte> idBytes = stackalloc byte[16];
            Span<char> idHash = stackalloc char[22];

            MemoryMarshal.Write(idBytes, ref id);

            // Encode remainder Guid byte
            idHash[21] = urlSafeBase64EncodingMap[(idBytes[15] & 0x03) << 4];
            idHash[20] = urlSafeBase64EncodingMap[(idBytes[15] & 0xfc) >> 2];

            // Encode Guid bytes
            var idHashIndex = 19;
            var idBytesIndex = 14;

            while (idHashIndex >= 3)
            {
                idHash[idHashIndex--] = urlSafeBase64EncodingMap[idBytes[idBytesIndex] & 0x3f];
                idHash[idHashIndex--] = urlSafeBase64EncodingMap[(idBytes[idBytesIndex - 1] & 0x0f) << 2 | (idBytes[idBytesIndex] & 0xc0) >> 6];
                idHash[idHashIndex--] = urlSafeBase64EncodingMap[(idBytes[idBytesIndex - 2] & 0x03) << 4 | (idBytes[idBytesIndex - 1] & 0xf0) >> 4];
                idHash[idHashIndex--] = urlSafeBase64EncodingMap[(idBytes[idBytesIndex - 2] & 0xfc) >> 2];

                idBytesIndex -= 3;
            }

            return new string(idHash);
        }

        // * Going from last to first to help JIT eliminate bounds checking
        // * Byte shifts are used to get relevant part of bytes for base64 decoding
        // * Due to custom urlSafeBase64DecodingMap there's no need to change custom (`_`, `-`) characters later
        // * Decoding to single bytes
        public static Guid ToGuidFromString(ReadOnlySpan<char> idHash)
        {
            Span<byte> idBytes = stackalloc byte[16];

            // Decode idHash chars [20-21]
            idBytes[15] = (byte)(
                urlSafeBase64DecodingMap[idHash[20]] << 2
                | urlSafeBase64DecodingMap[idHash[21]] >> 4
            );

            // Decode idHash chars [0-19]
            var idByteIndex = 14;
            var idHashIndex = 19;

            while (idByteIndex >= 2)
            {
                idBytes[idByteIndex--] = (byte)(
                      urlSafeBase64DecodingMap[idHash[idHashIndex]] >> 0
                    | urlSafeBase64DecodingMap[idHash[idHashIndex - 1]] << 6
                );

                idBytes[idByteIndex--] = (byte)(
                      urlSafeBase64DecodingMap[idHash[idHashIndex - 1]] >> 2
                    | urlSafeBase64DecodingMap[idHash[idHashIndex - 2]] << 4
                );

                idBytes[idByteIndex--] = (byte)(
                      urlSafeBase64DecodingMap[idHash[idHashIndex - 2]] >> 4
                    | urlSafeBase64DecodingMap[idHash[idHashIndex - 3]] << 2
                );

                idHashIndex -= 4;
            }

            return new Guid(idBytes);
        }
    }
}
