using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Nexus.Link.Contracts.Misc.Sdk.Authentication;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.Misc;

namespace Nexus.Link.Authentication.Sdk.Logic
{
    public class HashService : IHashService
    {
        private readonly IHashTable _hashTable;

        public HashService(IHashTable hashTable)
        {
            _hashTable = hashTable;
        }

        /// <inheritdoc />
        public async Task<bool> IsSameAsync(string hashId, string text, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            if (string.IsNullOrWhiteSpace(hashId)) return false;
            var id = hashId.ToGuid();
            var hashRecord = await _hashTable.ReadAsync(id, cancellationToken);
            if (hashRecord == null) return false;
            var hash = CreateHash(text, hashRecord.Salt);
            var isSame = hash == hashRecord.Hash;
            if (isSame)
            {
                // Extend the time before we allow a delete of this record
                var timeSinceLast = DateTimeOffset.UtcNow.Subtract(hashRecord.RecordUpdatedAt);
                hashRecord.DeleteAfter = hashRecord.DeleteAfter.Add(timeSinceLast);
                try
                {
                    await _hashTable.UpdateAsync(id, hashRecord, cancellationToken);
                }
                catch (FulcrumConflictException)
                {
                    // Someone else just updated the record, so we don't need to
                }
            }
            return isSame;
        }

        /// <inheritdoc />
        public async Task<string> CreateAsync(string text, DateTimeOffset purgeAfter, CancellationToken cancellationToken = default)
        {
            InternalContract.RequireNotNullOrWhiteSpace(text, nameof(text));

            var salt = CreateSaltAsBase64String();
            var hash = new HashRecordCreate
            {
                Salt = salt,
                Hash = CreateHash(text, salt),
                DeleteAfter = purgeAfter
            };
            var id = await _hashTable.CreateAsync(hash, cancellationToken);
            return id.ToGuidString();
        }

        /// <summary>
        /// Crate a unique salt and return it as a base 64 string. To be used in concert with <see cref="CreateHash"/>.
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
        /// </remarks>
        private static string CreateSaltAsBase64String()
        {
            var salt = new byte[128 / 8];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetNonZeroBytes(salt);
            }

            var saltAsString = Convert.ToBase64String(salt);
            return saltAsString;
        }

        /// <summary>
        /// Hashes a <paramref name="valueToHash"/> after adding a salt (<paramref name="saltAsBase64String"/>).
        /// Use <see cref="CreateSaltAsBase64String"/> to generate a new salt for every hash.
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/consumer-apis/password-hashing
        /// </remarks>
        private static string CreateHash(string valueToHash, string saltAsBase64String)
        {
            var salt = Convert.FromBase64String(saltAsBase64String);
            // derive a 256-bit sub key (use HMACSHA256 with 100,000 iterations)
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: valueToHash,
                salt: salt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}