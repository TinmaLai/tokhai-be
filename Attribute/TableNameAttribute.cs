using System;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class TableNameAttribute : Attribute
{
    public string Name { get; }

    public TableNameAttribute(string name)
    {
        Name = name;
    }
}
