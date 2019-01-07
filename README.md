# Simp-ORM

## Introduction
A simple ORM for .NET/T-SQL I made for my Web Server Programming II final project ([repo here](https://github.com/CGGTeam/GGFlix).

## How it works
The library works by using C# Attributes to map Tables, Columns and ID Columns in SQL to a class' properties and fields. The GenericDao class can then be used to retrieve table data and convert it into instances of a corresponding domain class that has been properly annotated.

## Sample Code

### Example entity class
```csharp
[TableMapping("Entities")]
public class ExampleEntity
{
    [Id]
    [ColumnMapping]
    public int? Property { get; set; }
    [ColumnMapping]
    public string AnotherProperty { get; set; }
}
```

### Using a Dao

#### Instantiation
```csharp
//dbAdapter would be an implementation of IDbAdapter created for a specific DBMS / Sql dialect. I used Sql Server / T-SQL
GenericDao<ExampleEntity> dao = new GenericDao<ExampleEntity>(dbAdapter);
```

#### Getting table contents
```csharp
IList<ExampleEntity> tableContents = dao.FindAll();
```

#### Getting records from an example
```csharp
ExampleEntity exampleInstance = new ExampleEntity 
{
  Property = 5
};

ExampleEntity entityWithPropValueEqual5 = dao.Find(exampleInstance);
```

#### Creating, Update, Delete
```csharp
ExampleEntity newEntity = new ExampleEntity 
{
  Property = 5,
  AnotherProperty = "value"
};

ExampleEntity savedEntity = dao.Save(newEntity); //Inserts row

savedEntity.AnotherProperty = "new property value";
ExampleEntity modifiedEntity = dao.Save(savedEntity) //Updates row

dao.Delete(modifiedEntity) //Deletes Row
```
