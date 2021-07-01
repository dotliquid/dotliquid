using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

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
        /// <param name="input"/>
        /// <returns></returns>
        public static string Md5(string input)
        {
            return GetHash(MD5.Create(), input);
        }

        /// <summary> 
        /// Converts a string into a SHA-1 hash.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#sha1" />
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Sha1(string input)
        {
            return GetHash(SHA1.Create(), input);
        }

        /// <summary>
        /// Converts a string into a SHA-256 hash.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#sha256" />
        /// <param name="input"></param>
        /// <returns></returns>
        public static string Sha256(string input)
        {
            return GetHash(SHA256.Create(), input);
        }

        /// <summary>
        /// Converts a string into a SHA-1 hash using a hash message authentication code (HMAC).
        /// Pass the secret key for the message as a parameter to the filter.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#hmac_sha1" />
        /// <param name="input" />
        /// <param name="secretKey" />
        /// <returns></returns>
        public static string HmacSha1(string input, string secretKey)
        {
            if (input.IsNullOrWhiteSpace() || secretKey.IsNullOrWhiteSpace())
                return input;

            HMACSHA1 hmac = new HMACSHA1(Encoding.UTF8.GetBytes(secretKey));
            hmac.Initialize();
            return GetHash(hmac, input);
        }

        /// <summary>
        /// Converts a string into a SHA-256 hash using a hash message authentication code (HMAC).
        /// Pass the secret key for the message as a parameter to the filter.
        /// </summary>
        /// <see href="https://shopify.dev/docs/themes/liquid/reference/filters/string-filters#hmac_sha256" />
        /// <param name="input" />
        /// <param name="secretKey" />
        /// <returns></returns>
        public static string HmacSha256(string input, string secretKey)
        {
            if (input.IsNullOrWhiteSpace() || secretKey.IsNullOrWhiteSpace())
                return input;

            HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey));
            hmac.Initialize();
            return GetHash(hmac, input);
        }

        /// <summary>
        /// Generates a Hexadecimal hash of the provided input string.
        /// </summary>
        /// <param name="hashAlgorithm" />
        /// <param name="input" />
        /// <returns></returns>
        private static string GetHash(HashAlgorithm hashAlgorithm, string input)
        {
            if (input.IsNullOrWhiteSpace())
                return input;

            // Convert the input string to a byte array and compute the hash.
            byte[] data = hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(input));

            // Create a new Stringbuilder to collect the bytes when converted to Hex
            var sBuilder = new StringBuilder();

            // Format each byte of the hashed data as a hexadecimal.
            for (int i = 0; i < data.Length; i++)
                sBuilder.Append(data[i].ToString(format: "x2", provider: CultureInfo.InvariantCulture));

            return sBuilder.ToString(); // Return the hexadecimal string.
        }

        /// <summary>
        /// Converts a string into JSON format.
        /// </summary>
        /// <see href="https://shopify.dev/api/liquid/filters/additional-filters#json"/>
        /// <param name="input" />
        /// <param name="writeIndented" />
        /// <returns></returns>
        public static object Json(object input, bool writeIndented = false)
        {
            if (input == null)
                return input;

            var options = new JsonSerializerOptions
            {
                WriteIndented = writeIndented
            };
            return JsonSerializer.Serialize(input, options);
        }
    }
}