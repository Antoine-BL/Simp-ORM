using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;


[AttributeUsage(AttributeTargets.Property)]
public class ColumnMapping : Attribute
{
    public string ColumnName { get; }

    public ColumnMapping([CallerMemberName] string columnName = "")
    {
        this.ColumnName = columnName;
    }
}