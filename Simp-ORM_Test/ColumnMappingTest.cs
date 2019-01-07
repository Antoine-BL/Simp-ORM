using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpORM_Test.Utils;

namespace SimpORM_Test
{
    [TestClass]
    public class ColumnMappingTest
    {
        private ColumnMapping IdMapping => getColumMappingForProp("id");

        private ColumnMapping NomMapping => getColumMappingForProp("nom");

        private ColumnMapping ClientMapping => getColumMappingForProp("client");

        private ColumnMapping CustomerMapping => getColumMappingForProp("customer");

        private ColumnMapping getColumMappingForProp(string propName)
        {
            return (ColumnMapping)typeof(ColumnMappingExample).GetProperty(propName).GetCustomAttribute(typeof(ColumnMapping));
        }

        [TestMethod]
        public void WhenOnProp_ThenHasNameBasedOnProp()
        {
            string nameBasedOnProp = "id";
            ColumnMapping idMapping = IdMapping;

            Assert.AreEqual(nameBasedOnProp, idMapping.ColumnName);
        }

        [TestMethod]
        public void WhenOnOtherProp_ThenHasNameBasedOnOtherProp()
        {
            string nameBasedOnProp = "nom";
            ColumnMapping nomMapping = NomMapping;

            Assert.AreEqual(nameBasedOnProp, nomMapping.ColumnName);
        }

        [TestMethod]
        public void GivenANameOverride_WhenOnProp_ThenHasNameOverride()
        {
            string nameOverride = "ID_CUST";

            Assert.AreEqual(nameOverride, CustomerMapping.ColumnName);
        }

        [TestMethod]
        public void GivenAnotherNameOverride_WhenOnProp_ThenHasOtherNameOverride()
        {
            string nameOverride = "ID_CLIENT";

            Assert.AreEqual(nameOverride, ClientMapping.ColumnName);
        }
    }
}
