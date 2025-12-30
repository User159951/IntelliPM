using System.Security.Cryptography;
using IntelliPM.Application.Common.Interfaces;

namespace IntelliPM.Infrastructure.Identity;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 10000;

    public (string Hash, string Salt) HashPassword(string password)
    {
        using (var rng = RandomNumberGenerator.Create())
        {
            byte[] salt = new byte[SaltSize];
            rng.GetBytes(salt);

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
            }
        }
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            byte[] saltBytes = Convert.FromBase64String(salt);
            using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256))
            {
                byte[] hashBytes = pbkdf2.GetBytes(HashSize);
                byte[] expectedHash = Convert.FromBase64String(hash);
                return CryptographicOperations.FixedTimeEquals(hashBytes, expectedHash);
            }
        }
        catch
        {
            return false;
        }
    }
}

