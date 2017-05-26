using System;
using NUnit.Framework;

namespace Weblog.DynamicDomainObject.Test
{
    [TestFixture]
    public class DynamicDomainObjectTest
    {
        [Test]
        public void TestGetNonDynamicProperties()
        {
            dynamic testFixture = new TestFixture();

            Assert.AreEqual(0, testFixture.NonDynamicPropertyStruct);
            Assert.IsNull(testFixture.NonDynamicPropertyStruct2);
            Assert.AreEqual("NonDynamicPropertyOne", testFixture.NonDynamicPropertyOne);
            Assert.IsNull(testFixture.NonDynamicPropertyTwo);
            Assert.IsNotNull(testFixture.NonDynamicPropertyThree);
            Assert.That(testFixture.NonDynamicPropertyThree is TestFixtureType);
        }

        [Test]
        public void TestSetNonDynamicProperties()
        {
            dynamic testFixture = new TestFixture();

            testFixture.NonDynamicPropertyOne = "Value changed";
            testFixture.NonDynamicPropertyStruct = 1;
            testFixture.NonDynamicPropertyStruct2 = 1;

            Assert.AreEqual("Value changed", testFixture.NonDynamicPropertyOne);
            Assert.AreEqual(1, testFixture.NonDynamicPropertyStruct);
            Assert.AreEqual(1, testFixture.NonDynamicPropertyStruct2);
        }

        [Test]
        public void null_returned_when_dynamic_property_has_not_been_set()
        {
            dynamic testFixture = new TestFixture();
            Assert.IsNull(testFixture.NonExistsDynamicProperty);
        }

        [Test]
        public void can_set_dynamic_class_property()
        {
            dynamic testFixture = new TestFixture();

            testFixture.DynamicPropertyOne = "test";
            Assert.AreEqual("test", testFixture.DynamicPropertyOne);
        }

        [Test]
        public void can_set_dynamic_struct_property()
        {
            dynamic testFixture = new TestFixture();
            testFixture.DynamicPropertyTwo = 1;
            Assert.AreEqual(1, testFixture.DynamicPropertyTwo);
        }

        [Test]
        public void can_convert_from_dynamic_to_implemented_interface()
        {
            dynamic testFixture = new TypeTestFixture();
            var fixtureInterface = (TestFixtureInterface)testFixture;
            Assert.NotNull(fixtureInterface);
        }

        [Test]
        public void converting_from_dynamic_to_unimplemented_interface_returns_null()
        {
            dynamic testFixture = new TypeTestFixture();
            var fixtureInterface2 = (TestFixtureInterface2)testFixture;
            Assert.IsNull(fixtureInterface2);
        }

        [Test]
        public void can_dispose()
        {
            var t = new TypeTestFixture();
            t.Dispose();
        }

        [Test]
        public void can_dispose_twice()
        {
            var t = new TypeTestFixture();
            t.Dispose();
            t.Dispose();
        }

        private interface TestFixtureInterface
        {
            
        }

        private interface TestFixtureInterface2
        {
            
        }

        private class TypeTestFixture : DynamicDomainObject, TestFixtureInterface
        {
            
        }

        private class TestFixtureType
        {
        }


        private class TestFixture : DynamicDomainObject
        {
            public String NonDynamicPropertyOne { get; set; }
            public TestFixtureType NonDynamicPropertyTwo { get; set; }
            public TestFixtureType NonDynamicPropertyThree { get; set; }
            public Int32 NonDynamicPropertyStruct { get; set; }
            public Int32? NonDynamicPropertyStruct2 { get; set; }

            public TestFixture()
            {
                NonDynamicPropertyOne = "NonDynamicPropertyOne";
                NonDynamicPropertyThree = new TestFixtureType();
            }
        }
    }
}
