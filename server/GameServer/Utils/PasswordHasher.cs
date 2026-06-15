using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Utils
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16;
        private const int HashSize = 32;

        private const int iterations = 100_000;

        public static (string hash, string salt) HashPassword(string password)
        {
            byte[] saltBytes = RandomNumberGenerator.GetBytes(SaltSize);

            byte[] hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, iterations, HashAlgorithmName.SHA256, HashSize);

            return(Convert.ToBase64String(hashBytes), Convert.ToBase64String(saltBytes));
        }
        public static bool VerifyPassword(string password, string hash, string salt)
        {
            byte[] saltBytes = Convert.FromBase64String(salt);

            byte[] inputHashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, iterations, HashAlgorithmName.SHA256, HashSize);

            byte[] storedHashBytes = Convert.FromBase64String(hash);

            return CryptographicOperations.FixedTimeEquals(inputHashBytes, storedHashBytes);

        }
    }
}
