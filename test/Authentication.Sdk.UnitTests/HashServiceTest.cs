using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Authentication.Sdk.Logic;
using Nexus.Link.Contracts.Misc.Sdk.Authentication;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Shouldly;

namespace Authentication.Sdk.UnitTests
{
    [TestClass]
    public class HashServiceTest
    {
        private IHashTable _hashTable;
        private IHashService _serviceUnderTest;

        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(HashServiceTest));
            _hashTable = new ReentryTokenTableMemory();
            _serviceUnderTest = new HashService(_hashTable);
        }

        [TestMethod]
        public async Task CreateReentry_Given_Token_Gives_SavesData()
        {
            // Arrange
            var token = Guid.NewGuid().ToGuidString();

            // Act
            var key = await _serviceUnderTest.CreateAsync(token, DateTimeOffset.Now);

            // Assert
            key.ShouldNotBeNull();
            var hashInformation = await _hashTable.ReadAsync(key.ToGuid());
            hashInformation.ShouldNotBeNull();
            FulcrumAssert.IsValidated(hashInformation);
        }

        [TestMethod]
        public async Task CreateReentry_Given_Same_Gives_True()
        {
            // Arrange
            var savedToken = Guid.NewGuid().ToGuidString();
            var key = await _serviceUnderTest.CreateAsync(savedToken, DateTimeOffset.Now);
            key.ShouldNotBeNull();
            var hashInformation = await _hashTable.ReadAsync(key.ToGuid());
            var reentryAuthentication = hashInformation.Id.ToGuidString();
            var hash = savedToken;

            // Act
            var validated = await _serviceUnderTest.IsSameAsync(reentryAuthentication, hash);

            // Assert
            validated.ShouldBeTrue();
        }

        [TestMethod]
        public async Task CreateReentry_Given_Same_Gives_TimeExtended()
        {
            // Arrange
            var savedToken = Guid.NewGuid().ToGuidString();
            var key = await _serviceUnderTest.CreateAsync(savedToken, DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(1)));
            key.ShouldNotBeNull();
            var hashInformation1 = await _hashTable.ReadAsync(key.ToGuid());
            var reentryAuthentication = hashInformation1.Id.ToGuidString();
            var hash = savedToken;

            // Act
            var validated = await _serviceUnderTest.IsSameAsync(reentryAuthentication, hash);

            // Assert
            var hashInformation2 = await _hashTable.ReadAsync(key.ToGuid());
            hashInformation2.DeleteAfter.ShouldBeGreaterThan(hashInformation1.DeleteAfter);
        }

        [TestMethod]
        public async Task CreateReentry_Given_NotSame_Gives_False()
        {
            // Arrange
            var savedToken = Guid.NewGuid().ToGuidString();
            var key = await _serviceUnderTest.CreateAsync(savedToken, DateTimeOffset.Now);
            key.ShouldNotBeNull();
            var hashInformation = await _hashTable.ReadAsync(key.ToGuid());
            var reentryAuthentication = hashInformation.Id.ToGuidString();
            var hash = Guid.NewGuid().ToGuidString();

            // Act
            var validated = await _serviceUnderTest.IsSameAsync(reentryAuthentication, hash);

            // Assert
            validated.ShouldBeFalse();
        }

        [TestMethod]
        public async Task CreateReentry_Given_NotSame_Gives_NotExtended()
        {
            // Arrange
            var savedToken = Guid.NewGuid().ToGuidString();
            var key = await _serviceUnderTest.CreateAsync(savedToken, DateTimeOffset.Now);
            key.ShouldNotBeNull();
            var hashInformation1 = await _hashTable.ReadAsync(key.ToGuid());
            var reentryAuthentication = hashInformation1.Id.ToGuidString();
            var hash = Guid.NewGuid().ToGuidString();

            // Act
            var validated = await _serviceUnderTest.IsSameAsync(reentryAuthentication, hash);

            // Assert
            var hashInformation2 = await _hashTable.ReadAsync(key.ToGuid());
            hashInformation2.DeleteAfter.ShouldBe(hashInformation1.DeleteAfter);
        }
    }

    internal class ReentryTokenTableMemory : CrudMemory<HashRecordCreate, HashRecord, Guid>, IHashTable
    {

    }
}
