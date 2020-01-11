using Konscious.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CookedRabbit.Core.Utils
{
    public static class ArgonHash
    {
        public static async Task<byte[]> GetHashKeyAsync(string passphrase, string salt, int size)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passphrase))
            {
                DegreeOfParallelism = 4,
                MemorySize = 2048,
                Salt = Encoding.UTF8.GetBytes(salt),
                Iterations = 12
            };

            return await argon2.GetBytesAsync(size).ConfigureAwait(false);
        }

        public static async Task<byte[]> GetHashKeyAsync(string passphrase, byte[] salt, int size)
        {
            using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(passphrase))
            {
                DegreeOfParallelism = 4,
                MemorySize = 2048,
                Salt = salt,
                Iterations = 12
            };

            return await argon2.GetBytesAsync(size).ConfigureAwait(false);
        }

        public static async Task<byte[]> GetHashKeyAsync(byte[] passphrase, byte[] salt, int size)
        {
            using var argon2 = new Argon2id(passphrase)
            {
                DegreeOfParallelism = 4,
                MemorySize = 2048,
                Salt = salt,
                Iterations = 12
            };

            return await argon2.GetBytesAsync(size).ConfigureAwait(false);
        }
    }
}
