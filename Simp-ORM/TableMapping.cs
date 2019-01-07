using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace SimpORM
{
    [AttributeUsage(AttributeTargets.Class)]
    public class TableMapping : Attribute
    {
        public string TableName { get; }

        public TableMapping(string tableName)
        {
            this.TableName = tableName;
        }
    }
}
