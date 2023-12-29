using TinyURL.Core.Interfaces;

namespace TinyURL.Services
{
    public class Base62Service : IBase62Encoding
    {
        private static readonly string Base62Chars = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public string Encode(long num)
        {
            if (num < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(num), "Number to encode must be non-negative.");
            }

            if (num == 0) return "0";

            string encoded = "";
            while (num > 0)
            {
                encoded = Base62Chars[(int)(num % 62)] + encoded;
                num /= 62;
            }
            return encoded;
        }

        public long Decode(string encoded)
        {
            if (string.IsNullOrEmpty(encoded))
            {
                throw new ArgumentNullException(nameof(encoded), "Encoded string cannot be null or empty.");
            }

            long decoded = 0;
            foreach (var c in encoded)
            {
                int index = Base62Chars.IndexOf(c);
                if (index == -1)
                {
                    throw new ArgumentException("Encoded string contains invalid characters.", nameof(encoded));
                }
                decoded = decoded * 62 + index;
            }
            return decoded;
        }
    }

}
