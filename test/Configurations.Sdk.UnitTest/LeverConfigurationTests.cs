using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using Nexus.Link.Configurations.Sdk;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Error.Logic;
using Nexus.Link.Libraries.Core.MultiTenant.Model;

namespace Configurations.Sdk.UnitTest
{
    [TestClass]
    public class LeverConfigurationTests
    {
        [TestInitialize]
        public void Initialize()
        {
            FulcrumApplicationHelper.UnitTestSetup(typeof(LeverConfigurationTests).FullName);
        }

        private static LeverConfiguration CreateLeverConfiguration(JObject config)
        {
            return new LeverConfiguration(new Tenant("o", "e"), "test", config);
        }

        [TestMethod]
        public void Ints_Can_Be_Used()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = 1
            }));
            Assert.AreEqual(1, config.Value<int>("x"));
            Assert.AreEqual(1, config.MandatoryValue<int?>("x"));
        }

        [TestMethod]
        public void Ints_Can_Be_Configured_As_Strings()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = "1"
            }));
            Assert.AreEqual(1, config.Value<int>("x"));
            Assert.AreEqual(1, config.MandatoryValue<int?>("x"));
        }

        [TestMethod]
        public void Bools_Can_Be_Used()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = true
            }));
            Assert.AreEqual(true, config.Value<bool>("x"));
            Assert.AreEqual(true, config.MandatoryValue<bool?>("x"));
        }

        [TestMethod]
        public void Bools_Can_Be_Configured_As_Strings()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = "true"
            }));
            Assert.AreEqual(true, config.Value<bool>("x"));
            Assert.AreEqual(true, config.MandatoryValue<bool?>("x"));
        }

        [TestMethod]
        public void Strings_Can_Be_Used()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = "s"
            }));
            Assert.AreEqual("s", config.Value<string>("x"));
            Assert.AreEqual("s", config.MandatoryValue<string>("x"));
        }

        [TestMethod]
        public void Doubles_Can_Be_Used()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = 1.1
            }));
            Assert.AreEqual(1.1, config.Value<double>("x"));
            Assert.AreEqual(1.1, config.MandatoryValue<double?>("x"));
        }

        [TestMethod]
        public void Doubles_Can_Be_Configured_As_String()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = "1.1"
            }));
            Assert.AreEqual(1.1, config.Value<double>("x"));
            Assert.AreEqual(1.1, config.MandatoryValue<double?>("x"));
        }

        [TestMethod]
        public void Dates_Can_Be_Used()
        {
            var now = DateTimeOffset.Now;
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = now
            }));
            Assert.AreEqual(now, config.Value<DateTimeOffset>("x"));
            Assert.AreEqual(now, config.MandatoryValue<DateTimeOffset?>("x"));
        }

        [TestMethod]
        public void Arrays_Of_Ints_Can_Be_Used()
        {
            var array = new[] { 1, 2, 3 };
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = array
            }));
            CollectionAssert.AreEqual(array, config.Value<List<int>>("x"));
            CollectionAssert.AreEqual(array, config.MandatoryValue<List<int>>("x"));
        }

        [TestMethod]
        public void Arrays_Of_Ints_Can_Be_Configured_As_Strings()
        {
            var stringArray = new[] { "1", "2", "3" };
            var intArray = new[] { 1, 2, 3 };
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = stringArray
            }));
            CollectionAssert.AreEqual(intArray, config.Value<List<int>>("x"));
            CollectionAssert.AreEqual(intArray, config.MandatoryValue<List<int>>("x"));
        }

        [TestMethod]
        public void Arrays_Of_Strings_Can_Be_Used()
        {
            var array = new[] { "a", "b", "c" };
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = array
            }));
            CollectionAssert.AreEqual(array, config.Value<List<string>>("x"));
            CollectionAssert.AreEqual(array, config.MandatoryValue<List<string>>("x"));
        }

        [TestMethod]
        public void Arrays_Of_Objects_Can_Be_Used()
        {
            var array = new[]
            {
                new X { Id = "x-1", Count = 1 },
                new X { Id = "x-2", Count = 2 }
            };
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = array
            }));
            CollectionAssert.AreEqual(array, config.Value<List<X>>("x"));
            CollectionAssert.AreEqual(array, config.MandatoryValue<List<X>>("x"));
        }

        [TestMethod]
        public void Dictionary_Of_Primitives_Can_Be_Used()
        {
            var dict = new Dictionary<int, string>
            {
                { 1, "a" },
                { 2, "b" }
            };
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = dict
            }));
            CollectionAssert.AreEqual(dict, config.Value<Dictionary<int, string>>("x"));
            CollectionAssert.AreEqual(dict, config.MandatoryValue<Dictionary<int, string>>("x"));
        }

        [TestMethod]
        public void Dictionary_With_Object_Values_Can_Be_Used()
        {
            var dict = new Dictionary<string, X>
            {
                { "s1", new X { Id = "x-1", Count = 1 } },
                { "s2", new X { Id = "x-2", Count = 2 } }
            };
            var config = CreateLeverConfiguration(JObject.FromObject(new
            {
                x = dict
            }));
            CollectionAssert.AreEqual(dict, config.Value<Dictionary<string, X>>("x"));
            CollectionAssert.AreEqual(dict, config.MandatoryValue<Dictionary<string, X>>("x"));
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Throws_When_Mandatory_Int_Is_Missing()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new { }));
            config.MandatoryValue<int?>("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Throws_When_Mandatory_String_Is_Missing()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new { }));
            config.MandatoryValue<string>("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Throws_When_Mandatory_Double_Is_Missing()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new { }));
            config.MandatoryValue<double?>("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Throws_When_Mandatory_Date_Is_Missing()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new { }));
            config.MandatoryValue<DateTimeOffset?>("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Throws_When_Mandatory_Array_Is_Missing()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new { }));
            config.MandatoryValue<List<int>>("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(FulcrumAssertionFailedException))]
        public void Throws_When_Mandatory_Dictionary_Is_Missing()
        {
            var config = CreateLeverConfiguration(JObject.FromObject(new { }));
            config.MandatoryValue<Dictionary<int, string>>("foo");
        }

        public class X
        {
            public string Id { get; set; }
            public int Count { get; set; }

            public override bool Equals(object obj)
            {
                if (!(obj is X other)) return false;
                return other.Id == Id;
            }

            public override int GetHashCode()
            {
                return Id.GetHashCode();
            }
        }
    }
}