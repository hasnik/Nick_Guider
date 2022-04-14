using System.Buffers.Text;
using System.Runtime.InteropServices;

namespace Guider
{
    internal class GuiderNick
    {
        public static string ToStringFromGuid(Guid id)
        {
            Span<byte> idBytes = stackalloc byte[16];
            Span<byte> base64Bytes = stackalloc byte[24];

            MemoryMarshal.TryWrite(idBytes, ref id);
            Base64.EncodeToUtf8(idBytes, base64Bytes, out _, out _);

            Span<char> finalChars = stackalloc char[22];

            for (var i = 0; i < 22; i++)
            {
                finalChars[i] = base64Bytes[i] switch
                {
                    (byte)'/' => '-',
                    (byte)'+' => '_',
                    _ => (char)base64Bytes[i]
                };
            }

            return new string(finalChars);
        }

        public static Guid ToGuidFromString(ReadOnlySpan<char> urlFriendlyBase64Id)
        {
            Span<char> base64Chars = stackalloc char[24];

            for (var i = 0; i < 22; i++)
            {
                base64Chars[i] = urlFriendlyBase64Id[i] switch
                {
                    '-' => '/',
                    '_' => '+',
                    _ => urlFriendlyBase64Id[i]
                };
            }

            base64Chars[22] = '=';
            base64Chars[23] = '=';

            Span<byte> idBytes = stackalloc byte[16];

            Convert.TryFromBase64Chars(base64Chars, idBytes, out _);

            return new Guid(idBytes);
        }
    }
}
