namespace Guider
{
    internal class GuiderBase
    {
        public static string ToStringFromGuid(Guid id)
        {
            return Convert.ToBase64String(id.ToByteArray())
                .Replace('/', '-')
                .Replace('+', '_')
                .Replace("=", string.Empty);
        }

        public static Guid ToGuidFromString(string urlFriendlyBase64Id)
        {
            var efficientBase64 = Convert.FromBase64String(urlFriendlyBase64Id
                .Replace('-', '/')
                .Replace('_', '+') + "==");

            return new Guid(efficientBase64);
        }
    }
}
