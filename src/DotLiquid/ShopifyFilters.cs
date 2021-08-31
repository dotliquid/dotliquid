using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace DotLiquid
{
    /// <summary>
    /// Shopify filters implemented by DotLiquid
    /// </summary>
    /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters"/>
    public static class ShopifyFilters
    {
        /// <summary>
        /// Converts a string into an MD5 hash.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#md5"/>
        /// <param name="input">The input to be encoded</param>
        public static string Md5(string input)
        {
            if (input == null)
                return input;

            using (var hashAlgorithm = MD5.Create())
            {
                return ComputeHash(hashAlgorithm, input);
            }
        }

        /// <summary> 
        /// Converts a string into a SHA-1 hash.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#sha1" />
        /// <param name="input">The input to be encoded</param>
        public static string Sha1(string input)
        {
            if (input == null)
                return input;

            using (var hashAlgorithm = SHA1.Create())
            {
                return ComputeHash(hashAlgorithm, input);
            }
        }

        /// <summary>
        /// Converts a string into a SHA-256 hash.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#sha256" />
        /// <param name="input">The input to be encoded</param>
        public static string Sha256(string input)
        {
            if (input == null)
                return input;

            using (var hashAlgorithm = SHA256.Create())
            {
                return ComputeHash(hashAlgorithm, input);
            }
        }

        /// <summary>
        /// Converts a string into a SHA-1 hash using a hash message authentication code (HMAC).
        /// Pass the secret key for the message as a parameter to the filter.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#hmac_sha1" />
        /// <param name="input">The input to be encoded</param>
        /// <param name="secretKey">The secret key</param>
        public static string HmacSha1(string input, string secretKey)
        {
            if (input == null || secretKey == null)
                return input;

            using (var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey)))
            {
                hmac.Initialize();
                return ComputeHash(hmac, input);
            }
        }

        /// <summary>
        /// Converts a string into a SHA-256 hash using a hash message authentication code (HMAC).
        /// Pass the secret key for the message as a parameter to the filter.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#hmac_sha256" />
        /// <param name="input">The input to be encoded</param>
        /// <param name="secretKey">The secret key</param>
        public static string HmacSha256(string input, string secretKey)
        {
            if (input == null || secretKey == null)
                return input;

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                hmac.Initialize();
                return ComputeHash(hmac, input);
            }
        }

        /// <summary>
        /// Generates a Hexadecimal hash of the provided input string.
        /// </summary>
        /// <param name="hashAlgorithm" >The algorithm to be used for encoding</param>
        /// <param name="input">The input to be encoded</param>
        private static string ComputeHash(HashAlgorithm hashAlgorithm, string input)
        {
            // Convert the input string to a byte array and compute the hash.
            var hash = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes when converted to Hex
            var stringBuilder = new StringBuilder();

            // Format each byte of the hashed data as a hexadecimal (x2).
            for (int i = 0; i < hash.Length; i++)
                stringBuilder.Append(hash[i].ToString(format: "x2", provider: CultureInfo.InvariantCulture));

            return stringBuilder.ToString(); // Return the hexadecimal string.
        }
    }
}