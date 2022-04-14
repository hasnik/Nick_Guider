using System.Runtime.InteropServices;

namespace Guider
{
    // Eliminate loops
    internal class GuiderMine_2
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
        // * Byte shifts are used to get relevant part of bytes for base64 encoding
        // * Due to custom urlSafeBase64EncodingMap there's no need to change invalid URL characters later
        public static string ToStringFromGuid(Guid id)
        {
            Span<byte> idBytes = stackalloc byte[16];
            Span<char> idHash = stackalloc char[22];

            MemoryMarshal.Write(idBytes, ref id);

            idHash[0] = urlSafeBase64EncodingMap[(idBytes[0] & 0xfc) >> 2];
            idHash[1] = urlSafeBase64EncodingMap[(idBytes[0] & 0x03) << 4 | (idBytes[1] & 0xf0) >> 4];
            idHash[2] = urlSafeBase64EncodingMap[(idBytes[1] & 0x0f) << 2 | (idBytes[2] & 0xc0) >> 6];
            idHash[3] = urlSafeBase64EncodingMap[idBytes[2] & 0x3f];

            idHash[4] = urlSafeBase64EncodingMap[(idBytes[3] & 0xfc) >> 2];
            idHash[5] = urlSafeBase64EncodingMap[(idBytes[3] & 0x03) << 4 | (idBytes[4] & 0xf0) >> 4];
            idHash[6] = urlSafeBase64EncodingMap[(idBytes[4] & 0x0f) << 2 | (idBytes[5] & 0xc0) >> 6];
            idHash[7] = urlSafeBase64EncodingMap[idBytes[5] & 0x3f];

            idHash[8] = urlSafeBase64EncodingMap[(idBytes[6] & 0xfc) >> 2];
            idHash[9] = urlSafeBase64EncodingMap[(idBytes[6] & 0x03) << 4 | (idBytes[7] & 0xf0) >> 4];
            idHash[10] = urlSafeBase64EncodingMap[(idBytes[7] & 0x0f) << 2 | (idBytes[8] & 0xc0) >> 6];
            idHash[11] = urlSafeBase64EncodingMap[idBytes[8] & 0x3f];

            idHash[12] = urlSafeBase64EncodingMap[(idBytes[9] & 0xfc) >> 2];
            idHash[13] = urlSafeBase64EncodingMap[(idBytes[9] & 0x03) << 4 | (idBytes[10] & 0xf0) >> 4];
            idHash[14] = urlSafeBase64EncodingMap[(idBytes[10] & 0x0f) << 2 | (idBytes[11] & 0xc0) >> 6];
            idHash[15] = urlSafeBase64EncodingMap[idBytes[11] & 0x3f];

            idHash[16] = urlSafeBase64EncodingMap[(idBytes[12] & 0xfc) >> 2];
            idHash[17] = urlSafeBase64EncodingMap[(idBytes[12] & 0x03) << 4 | (idBytes[13] & 0xf0) >> 4];
            idHash[18] = urlSafeBase64EncodingMap[(idBytes[13] & 0x0f) << 2 | (idBytes[14] & 0xc0) >> 6];
            idHash[19] = urlSafeBase64EncodingMap[idBytes[14] & 0x3f];

            idHash[20] = urlSafeBase64EncodingMap[(idBytes[15] & 0xfc) >> 2];
            idHash[21] = urlSafeBase64EncodingMap[(idBytes[15] & 0x03) << 4];

            return new string(idHash);
        }

        // * No loop for performance (hardcore loop unroll ;) )
        // * Byte shifts are used to get relevant part of bytes for base64 decoding
        // * Due to custom urlSafeBase64DecodingMap there's no need to change custom (`_`, `-`) characters later
        // * Decoding to single bytes
        public static Guid ToGuidFromString(ReadOnlySpan<char> idHash)
        {
            Span<byte> idBytes = stackalloc byte[16];

            // Decode idHash chars [0-3]
            idBytes[0] = (byte)(
                  urlSafeBase64DecodingMap[idHash[1]] >> 4
                | urlSafeBase64DecodingMap[idHash[0]] << 2
            );
            idBytes[1] = (byte)(
                  urlSafeBase64DecodingMap[idHash[2]] >> 2
                | urlSafeBase64DecodingMap[idHash[1]] << 4
            );
            idBytes[2] = (byte)(
                  urlSafeBase64DecodingMap[idHash[3]] >> 0
                | urlSafeBase64DecodingMap[idHash[2]] << 6
            );

            // Decode idHash chars [4-7]
            idBytes[3] = (byte)(
                  urlSafeBase64DecodingMap[idHash[5]] >> 4
                | urlSafeBase64DecodingMap[idHash[4]] << 2
            );
            idBytes[4] = (byte)(
                  urlSafeBase64DecodingMap[idHash[6]] >> 2
                | urlSafeBase64DecodingMap[idHash[5]] << 4
            );
            idBytes[5] = (byte)(
                  urlSafeBase64DecodingMap[idHash[7]] >> 0
                | urlSafeBase64DecodingMap[idHash[6]] << 6
            );

            // Decode idHash chars [8-11]
            idBytes[6] = (byte)(
                  urlSafeBase64DecodingMap[idHash[9]] >> 4
                | urlSafeBase64DecodingMap[idHash[8]] << 2
            );
            idBytes[7] = (byte)(
                  urlSafeBase64DecodingMap[idHash[10]] >> 2
                | urlSafeBase64DecodingMap[idHash[9]] << 4
            );
            idBytes[8] = (byte)(
                  urlSafeBase64DecodingMap[idHash[11]] >> 0
                | urlSafeBase64DecodingMap[idHash[10]] << 6
            );

            // Decode idHash chars [12-15]
            idBytes[9] = (byte)(
                  urlSafeBase64DecodingMap[idHash[13]] >> 4
                | urlSafeBase64DecodingMap[idHash[12]] << 2
            );
            idBytes[10] = (byte)(
                  urlSafeBase64DecodingMap[idHash[14]] >> 2
                | urlSafeBase64DecodingMap[idHash[13]] << 4
            );
            idBytes[11] = (byte)(
                  urlSafeBase64DecodingMap[idHash[15]] >> 0
                | urlSafeBase64DecodingMap[idHash[14]] << 6
            );

            // Decode idHash chars [16-19]
            idBytes[12] = (byte)(
                  urlSafeBase64DecodingMap[idHash[17]] >> 4
                | urlSafeBase64DecodingMap[idHash[16]] << 2
            );
            idBytes[13] = (byte)(
                  urlSafeBase64DecodingMap[idHash[18]] >> 2
                | urlSafeBase64DecodingMap[idHash[17]] << 4
            );
            idBytes[14] = (byte)(
                  urlSafeBase64DecodingMap[idHash[19]] >> 0
                | urlSafeBase64DecodingMap[idHash[18]] << 6
            );

            // Decode idHash chars [20-21]
            idBytes[15] = (byte)(
                  urlSafeBase64DecodingMap[idHash[21]] >> 4
                | urlSafeBase64DecodingMap[idHash[20]] << 2
            );

            return new Guid(idBytes);
        }
    }
}
