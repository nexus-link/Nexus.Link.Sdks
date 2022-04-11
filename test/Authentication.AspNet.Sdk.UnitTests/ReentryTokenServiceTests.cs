#if NETCOREAPP
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nexus.Link.Authentication.AspNet.Sdk.Logic;
using Nexus.Link.Authentication.Sdk.Logic;
using Nexus.Link.Contracts.Misc.AspNet.Sdk;
using Nexus.Link.Contracts.Misc.Sdk.Authentication;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Misc;
using Nexus.Link.Libraries.Crud.MemoryStorage;
using Shouldly;

namespace Authentication.AspNet.Sdk.UnitTests
{
    [TestClass]
    public class ReentryTokenServiceTests
    {
        private IHashTable _hashTable;
        private IReentryAuthenticationService _serviceUnderTest;

        [TestInitialize]
        public void RunBeforeEachTestMethod()
        {
            FulcrumApplicationHelper.UnitTestSetup(nameof(ReentryTokenServiceTests));
            _hashTable = new HashTableMemory();
            var hashService = new HashService(_hashTable);
            _serviceUnderTest = new ReentryAuthenticationService(hashService);
        }

        [TestMethod]
        public async Task CreateReentry_Given_Token_Gives_SavesData()
        {
            // Arrange
            var token = Guid.NewGuid().ToGuidString();
            var context = new DefaultHttpContext();
            context.Request.Headers["Authorization"] = $"Bearer {token}";


            // Act
            var key = await _serviceUnderTest.CreateAsync(context, DateTimeOffset.Now);

            // Assert
            key.ShouldNotBeNull();
            var hashInformation = await _hashTable.ReadAsync(key.ToGuid());
            hashInformation.ShouldNotBeNull();
            FulcrumAssert.IsValidated(hashInformation);
        }

        [DataTestMethod]
        [DataRow(true, true)]
        [DataRow(false, false)]
        public async Task CreateReentry_Given_Conditions_Gives_ExpectedResult(bool sameToken, bool expectedResult)
        {
            // Arrange
            var context = new DefaultHttpContext();
            var savedToken = Guid.NewGuid().ToGuidString();
            context.Request.Headers["Authorization"] = $"Bearer {savedToken}";
            var key = await _serviceUnderTest.CreateAsync(context, DateTimeOffset.Now);
            key.ShouldNotBeNull();
            var hashInformation = await _hashTable.ReadAsync(key.ToGuid());
            var reentryAuthentication = hashInformation.Id.ToGuidString();
            var token = sameToken ? savedToken : Guid.NewGuid().ToGuidString();
            context.Request.Headers["Authorization"] = $"Bearer {token}";

            // Act
            var validated = await _serviceUnderTest.ValidateAsync(reentryAuthentication, context);

            // Assert
            validated.ShouldBe(expectedResult);
        }
    }

    internal class HashTableMemory : CrudMemory<HashRecordCreate, HashRecord, Guid>, IHashTable
    {

    }
}
#endif