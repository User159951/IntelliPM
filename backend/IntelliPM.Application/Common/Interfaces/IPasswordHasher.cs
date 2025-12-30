namespace IntelliPM.Application.Common.Interfaces;

/// <summary>
/// Interface for password hashing operations
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Hashes a password and returns both the hash and salt
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <returns>A tuple containing the password hash and salt</returns>
    (string Hash, string Salt) HashPassword(string password);
    
    /// <summary>
    /// Verifies a password against a stored hash and salt
    /// </summary>
    /// <param name="password">The password to verify</param>
    /// <param name="hash">The stored password hash</param>
    /// <param name="salt">The stored password salt</param>
    /// <returns>True if the password matches, false otherwise</returns>
    bool VerifyPassword(string password, string hash, string salt);
}

