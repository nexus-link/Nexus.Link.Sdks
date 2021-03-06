﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Nexus.Link.KeyTranslator.Sdk;
using Nexus.Link.KeyTranslator.Sdk.Models;
using Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Clients;
using Nexus.Link.Libraries.Core.Application;
using TranslateRequest = Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models.TranslateRequest;
using TranslateResponse = Nexus.Link.KeyTranslator.Sdk.RestClients.Facade.Models.TranslateResponse;

namespace Xlent.Lever.KeyTranslator.Sdk.Test
{
    [TestClass]
    public class TestBatchTranslate
    {
        private readonly Mock<ITranslateClient> _translateClient = new Mock<ITranslateClient>();

        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup("sdk tests");
            BatchTranslate.ResetCache();
        }

        [TestMethod]
        public async Task ValueAndActionAsync()
        {
            var translateResponse = new TranslateResponse
            {
                Value = "b1",
                Request = new TranslateRequest("(concept!!a1)")
            };
            // TODO: How to deal with async?
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse }));
            var personGender = "FAIL";
            var batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse.Value, personGender);
        }

        [TestMethod]
        public async Task TwoDifferentValuesFromSameConcept()
        {
            var translateResponse1 = new TranslateResponse
            {
                Value = "b1",
                Request = new TranslateRequest("(concept!!a1)")
            };
            var translateResponse2 = new TranslateResponse
            {
                Value = "b2",
                Request = new TranslateRequest("(concept!!a2)")
            };
            // TODO: How to deal with async?
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse1, translateResponse2 }));
            var personGender1 = "FAIL";
            var personGender2 = "FAIL";
            var batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender1 = value)
                .Add("concept", "a2", value => personGender2 = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse1.Value, personGender1);
            Assert.AreEqual(translateResponse2.Value, personGender2);
        }

        [TestMethod]
        public async Task TwoEqualValuesFromSameConcept()
        {
            var translateResponse1 = new TranslateResponse
            {
                Value = "b1",
                Request = new TranslateRequest("(concept!!a1)")
            };
            // TODO: How to deal with async?
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse1 }));
            var personGender1 = "FAIL1";
            var personGender2 = "FAIL2";
            var batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender1 = value)
                .Add("concept", "a1", value => personGender2 = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse1.Value, personGender1);
            Assert.AreEqual(translateResponse1.Value, personGender2);
        }

        [TestMethod]
        public async Task ValueAndActionContextsAsync()
        {
            var translateResponse = new TranslateResponse
            {
                Value = "b1",
                Request = new TranslateRequest
                {
                    SourceInstancePath = "(concept!source-context!a1)",
                    TargetContextPath = "(concept!target-context)"
                }
            };
            _translateClient
                .Setup(mock => mock.TranslateBatchAsync(new[] { translateResponse.Request }, CancellationToken.None))
                .ReturnsAsync(new[] { translateResponse });
            var personGender = "FAIL";
            var batch = new BatchTranslate(_translateClient.Object);
            await batch
                .AddWithContexts("concept", "source-context", "a1", "target-context", value => personGender = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse.Value, personGender);
        }


        [TestMethod]
        public async Task ValueAndActionClientsAsync()
        {
            var translateResponse = new TranslateResponse
            {
                Value = "b1",
                Request = new TranslateRequest
                {
                    SourceInstancePath = "(concept!~sourceClient!a1)",
                    TargetContextPath = "(concept!~targetClient)"
                }
            };
            _translateClient
                .Setup(mock => mock.TranslateBatchAsync(new[] { translateResponse.Request }, CancellationToken.None))
                .ReturnsAsync(new[] { translateResponse });
            var personGender = "FAIL";
            var batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse.Value, personGender);
        }

        [TestMethod]
        public async Task ValuesAndActionAsync()
        {
            var translateResponse1 = new TranslateResponse { Value = "b1", Request = new TranslateRequest("(concept!!a1)") };
            var translateResponse2 = new TranslateResponse { Value = "b2", Request = new TranslateRequest("(concept!!a2)") };
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse2, translateResponse1 }));
            var personGender1 = "FAIL";
            var personGender2 = "FAIL";
            var batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender1 = value)
                .Add("concept", "a2", value => personGender2 = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse1.Value, personGender1);
            Assert.AreEqual(translateResponse2.Value, personGender2);
        }

        [TestMethod]
        public async Task ValueAndManualAsync()
        {
            var translateResponse = new TranslateResponse
            {
                Value = "b1",
                Request = new TranslateRequest("(concept!!a1)")
            };
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse }));
            var translations = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await translations
                .Add("concept", "a1")
                .ExecuteAsync();
            var personGender = translations["concept", "a1"];
            Assert.AreEqual(translateResponse.Value, personGender);
        }

        [TestMethod]
        public async Task ValueFromCacheAsync()
        {
            var translateResponse = new TranslateResponse
            {
                Value = "b1",
                Request = new TranslateRequest("(concept!!a1)")
            };
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse }));
            var personGender = "FAIL";
            var batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse.Value, personGender);

            // Now get from cache
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Throws<ApplicationException>();
            personGender = "FAIL";
            batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse.Value, personGender);
        }

        [TestMethod]
        public async Task ValueFromBothCacheAndCallAsync()
        {
            var translateResponse1 = new TranslateResponse { Value = "b1", Request = new TranslateRequest("(concept!!a1)") };

            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse1 }));
            var personGender1 = "FAIL";
            var batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender1 = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse1.Value, personGender1);

            // Now get from cache

            var translateResponse2 = new TranslateResponse { Value = "b2", Request = new TranslateRequest("(concept!!a2)") };
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse2 }));
            personGender1 = "FAIL";
            var personGender2 = "FAIL";
            batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await batch
                .Add("concept", "a1", value => personGender1 = value)
                .Add("concept", "a2", value => personGender2 = value)
                .ExecuteAsync();
            Assert.AreEqual(translateResponse1.Value, personGender1);
            Assert.AreEqual(translateResponse2.Value, personGender2);
        }

        [TestMethod]
        public async Task TestArrayTranslation()
        {
            var batch = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            var translated = new Dictionary<string, string>();

            var values = new[] { "a1", "b1", "c1" };
            var responses = new List<TranslateResponse>();
            foreach (var value in values)
            {
                batch.Add("concept", value, x => translated.Add(value, x));
                responses.Add(new TranslateResponse { Value = $"{value}-translated", Request = new TranslateRequest($"(concept!!{value})") });
            }

            _translateClient.Setup(x => x.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)responses));
            await batch.ExecuteAsync();

            foreach (var value in values)
            {
                Assert.IsTrue(translated.ContainsKey(value));
                Assert.AreEqual($"{value}-translated", translated[value]);
            }
        }

        [TestMethod]
        public async Task NoCachingOfUntranslatedValues()
        {
            // Setup
            var request = new TranslateRequest("(concept!!a1)");
            var translateResponse = new TranslateResponse
            {
                Value = request.SourceInstancePath, // Simulate no translation found
                Request = request
                
            };
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse }));

            // Expect no translation at first
            var translations1 = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await translations1.Add("concept", "a1").ExecuteAsync();
            var translation = translations1["concept", "a1"];
            var instancePath = InstanceInfo.Parse(translation);
            Assert.IsNotNull(instancePath, $"Translation '{translation}' expected to be instance path when no translation was found");

            // Simulate an association
            translateResponse = new TranslateResponse
            {
                Value = "b1",
                Request = request

            };
            _translateClient.Setup(mock => mock.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult((IEnumerable<TranslateResponse>)new[] { translateResponse }));

            // Expect translation (especially, that untranslated value is not cached)
            var translations2 = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await translations2.Add("concept", "a1").ExecuteAsync();
            translation = translations2["concept", "a1"];
            Assert.AreEqual("b1", translation);
        }

        [TestMethod]
        public async Task NoCallToKeyTranslatorForEmptyBatch()
        {
            _translateClient
                .Setup(x => x.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            // No translations
            var translator = new BatchTranslate(_translateClient.Object, "sourceClient", "targetClient");
            await translator.ExecuteAsync();

            _translateClient.Verify(x => x.TranslateBatchAsync(It.IsAny<IEnumerable<TranslateRequest>>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
