# Remute
Remute .NET tool to create new immutable object applying lambda expressions to the existing immutable object.

## Examples (flat and nested object structures)

For example define immutable class:
```cs
public class Employee
{
    public string FirstName { get; }

    public string LastName { get; }

    public Employee(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}
```

Apply modification:
```cs
var remute = new Remute();
var expected = new Employee("John", "Doe");
var actual = remute.With(expected, x => x.FirstName, "Foo");
```

Generic method `With` returns new object where first name is updated:
```cs
Assert.AreNotSame(expected.FirstName, actual.FirstName);
Assert.AreSame(expected.LastName, actual.LastName);
```

Remute works with immutable nested object structures.

Define few more immutable classes:

```cs
public class Department
{
    public string Title { get; }

    public Employee Manager { get; }

    public Department(string title, Employee manager)
    {
        Title = title;
        Manager = manager;
    }
}

public class Organization
{
    public string Name { get; }

    public Department DevelopmentDepartment { get; }

    public Organization(string name, Department developmentDepartment)
    {
        Name = name;
        DevelopmentDepartment = developmentDepartment;
    }
}
```

Remute first name of the manager of the development department:

```cs
var expected = 
    new Organization("Organization", 
        new Department("Development Department", 
            new Employee("John", "Doe")));
            
var actual = remute.With(expected, x => x.DevelopmentDepartment.Manager.FirstName, "Foo");
```

Actual object has updated references for `DevelopmentDepartment`, `Manager` and `FirstName`.
All other object references (like `Organization.Name`, `Department.Title` and `Employee.LastName`) remain the same.

```cs
Assert.AreNotSame(expected, actual);
Assert.AreNotSame(expected.DevelopmentDepartment, actual.DevelopmentDepartment);
Assert.AreNotSame(expected.DevelopmentDepartment.Manager, actual.DevelopmentDepartment.Manager);
Assert.AreSame(expected.Name, actual.Name);
Assert.AreSame(expected.DevelopmentDepartment.Title, expected.DevelopmentDepartment.Title);
```

## Multiple constuctors and parameter names matching
By default Remute expects immutable type to have single constructor with parameters matching property names (case-insensetive).
Use `ActivationConfiguration` to change this default behaviour. Check [issue](https://github.com/ababik/Remute/issues/3) for more details.

```cs
var config = new ActivationConfiguration()
    .Configure<User>(x => new User(x.FirstName, x.LastName));

var remute = new Remute(config);
```

## Syntax sugar
There is an extension method enabling chained modifications on any object.
```cs
using Remutable.Extensions;
...
var employee = new Employee(Guid.NewGuid(), "Joe", "Doe");

var actual = employee
    .Remute(x => x.FirstName, "Foo")
    .Remute(x => x.LastName, "Bar");

Assert.AreEqual("Foo", actual.FirstName);
Assert.AreEqual("Bar", actual.LastName);
```

## Performance notes
Remute does not use reflection / Activator.CreateInstance for object creation. Instead cached lambda expressions are used that demonstrates great performance.

## Get it
Remute is available as .Net Standard assembly via Nuget

`dotnet add package Remute` or
`Install-Package Remute`
