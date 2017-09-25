# Remute
Remute .NET tool is intended to produce new immutable object applying lambda expressions to existing immutable object.

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

Generic method `With` returns new object where first name in update:
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

## Performance notes
Remute does not use reflection for object creation. Instead cached lambda expressions are used that demonstrates great performance.

## Get it
Remute available as .Net Standard assembly via Nuget

`Install-Package Remute`
