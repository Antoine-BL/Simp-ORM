using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using SimpORM;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SimpORM_Test.Utils;

namespace SimpORM_Test
{
    [TestClass]
    public class TableMappingTest
    {
        private string GetTableMappingOn<T>()
        {
            return ((TableMapping) typeof(T).GetCustomAttribute(typeof(TableMapping))).TableName;
        }

        [TestMethod]
        public void GivenANameOverride_WhenOnClass_ThenHasNameOverride()
        {
            string nameOverride = "TablesMapping";

            Assert.AreEqual(nameOverride, GetTableMappingOn<TableMappingOverrideExample>());
        }

        [TestMethod]
        public void GivenAnotherNameOverride_WhenOnClass_ThenHasOtherNameOverride()
        {
            string nameOverride = "Tables";

            Assert.AreEqual(nameOverride, GetTableMappingOn<AnotherTableMappingOverrideExample>());
        }
    }
}
