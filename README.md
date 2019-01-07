# Simp-ORM

## Introduction
A simple ORM for .NET/T-SQL I made for my Web Server Programming II final project ([repo here](https://github.com/CGGTeam/GGFlix).

## How it works
The library works by using C# Attributes to map Tables, Columns and ID Columns in SQL to a class' properties and fields. The GenericDao class can then be used to retrieve table data and convert into domain classes that have been properly annotated.

Another feature of the library is to allow users to add order by and where clauses to queries using LINQ Expressions. These Expressions are then converted into SQL and integrated into the query.

## Sample Code

### 
