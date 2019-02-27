using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.KeyTranslator.Sdk;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models;

namespace Xlent.Lever.KeyTranslator.Sdk.Test
{
    [TestClass]
    public class TestTranslatorService
    {
        private readonly Mock<ITranslateClient> _translateClient = new Mock<ITranslateClient>();

        [TestInitialize]
        public void Initialize()
        {
        }

        [TestMethod]
        public async Task SimpleTranslation()
        {
            const string path1 = "(concept!~from!a1)";
            const string value1 = "b1";
            const string path2 = "(concept!~from!a2)";
            const string value2 = "b2";
            var translateResponse1 = new TranslateResponse
            {
                Value = value1,
                Request = new TranslateRequest(path1)
            };
            var translateResponse2 = new TranslateResponse
            {
                Value = value2,
                Request = new TranslateRequest(path2)
            };
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>()))
                .ReturnsAsync(new[] { translateResponse1, translateResponse2 });
            var service = new TranslatorService(_translateClient.Object);

            var conceptValuePaths = new List<string> {path1};
            var translations = await service.TranslateAsync(conceptValuePaths, "to");
            Assert.IsNotNull(translations);
            Assert.AreEqual(2, translations.Count);
            Assert.IsTrue(translations.ContainsKey(path1));
            Assert.AreEqual(value1, translations[path1]);
            Assert.IsTrue(translations.ContainsKey(path2));
            Assert.AreEqual(value2, translations[path2]);
        }
    }
}
