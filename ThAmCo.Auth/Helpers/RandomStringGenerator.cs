using System;
using System.Linq;

namespace ThAmCo.Auth.Helpers
{
    public class RandomStringGenerator
    {
        /// <summary>
        /// Generate a random stirng for a temporary access token
        /// </summary>
        /// <returns>A randomly generated string</returns>
        public string GenerateRandomString(int limit, string text = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789")
        => new string(Enumerable.Repeat(text, limit).Select(s => s[new Random().Next(s.Length)]).ToArray());
    }
}
