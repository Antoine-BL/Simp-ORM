using System;
using System.Collections.Generic;
using System.Text;
using SimpORM;

namespace SimpORM_Test.Utils
{
    [TableMapping("Entities")]
    public class ExampleEntity
    {
        [Id]
        [ColumnMapping]
        public int? Property { get; set; }
    }

    [TableMapping("OtherEntities")]
    public class AnotherExampleEntity
    {
        [Id]
        [ColumnMapping]
        public int? Property { get; set; }
        [ColumnMapping]
        public string AnotherProperty { get; set; }
    }
}
