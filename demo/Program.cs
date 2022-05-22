using Sandwych.Jasper;
using System.Text.Json;
using System.Linq;


public record class Employee(string Name, int Age);

class Program {


    static void Main() {

        var employees = new Employee[] {
            new Employee("Alice", 40),
            new Employee("Bob", 41),
            new Employee("Charlie", 42),
            new Employee("Ethan", 60),
        };

        var json = @"
[""and"", 
    ["">="", ""Age"", 35], 
    [""or"",
        [""="", ""Name"", ""Alice""], 
        ["">"", ""Age"", 45] 
    ]
]";

        Console.WriteLine(">>>>>>>>>>> All employees:");
        foreach (var employee in employees) {
            Console.WriteLine($"\t* {employee.Name}");
        }

        var filteredEmployees = employees.AsQueryable().Where(json);

        Console.WriteLine("\n>>>>>>>>>>> Filtered employees:");
        foreach (var employee in filteredEmployees) {
            Console.WriteLine($"\t* {employee.Name}");
        }

    }

}

