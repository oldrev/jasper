using Sandwych.Jasper;
using System.Text.Json;
using System.Linq;


public record class Employee(string Name, int Age, int DepartmentId);
public record class Department(int Id, string Name);

class Program {


    static void Main() {

        var departments = new Department[] {
            new Department(1, "Finance"),
            new Department(2, "Sales"),
            new Department(3, "HR"),
        };

        var employees = new Employee[] {
            new Employee("Alice", 40, 1),
            new Employee("Bob", 41, 1),
            new Employee("Charlie", 42, 3),
            new Employee("Ethan", 60, 2),
        };

        var json = 
"""
[
    "and", 
    [">=", "Age", 35], 
    ["=", "DepartmentId", 1],
    ["or", ["=", "Name", "Alice"], [">", "Age", 45]]
]
""";
        Console.WriteLine("The JSON AST:");
        Console.WriteLine(json);

        Console.WriteLine("\n>>>>>>>>>>> All employees:");
        foreach (var employee in employees) {
            Console.WriteLine($"\t* {employee.Name}");
        }

        var filteredEmployees = employees.AsQueryable().WhereByJson(json);

        Console.WriteLine("\n>>>>>>>>>>> Filtered employees:");
        foreach (var employee in filteredEmployees) {
            Console.WriteLine($"\t* {employee.Name}");
        }

    }

}

