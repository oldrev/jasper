# Sandwych.Jasper

A Lisp S-Expression style JSON predicate parser for LINQ.

It works on all LINQ implementation, included: 

* LINQ to Objects
* LINQ to XML
* NHibernate
* EntityFramework Core
* Dapper
* etc...

# Usage

```csharp
record class Employee(string Name, int Age);

var employees = new Employee[] {
	new Employee("Alice", 40),
	new Employee("Bob", 41),
	new Employee("Charlie", 42),
	new Employee("Ethan", 60),
};

var json = @"

[
	""and"", 
    ["">="", ""Age"", 35], 
    [""or"", [""="", ""Name"", ""Alice""], ["">"", ""Age"", 45]]
]

";

var filteredEmployees = employees.AsQueryable().WhereByJson(json);

// Equals to LINQ lambda: 
// employees.AsQueryable().Where(x => x.Age >= 35 && (x.Name == "Alice" || x.Age > 45))

```

See the demostration project `Sandwych.Jasper.Demo` for details.

# Status

PreAlpha, working in progress.
