using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.Jasper.Tests.Models;

public class DataSource {

    public List<Employee> Employees { get; } = new List<Employee>();
    public List<Department> Departments { get; } = new List<Department>();

    public DataSource() {
        this.Departments.Add(new Department(1, "Accounting", null, null));
        this.Departments.Add(new Department(2, "HR", null, null));
        this.Departments.Add(new Department(3, "Sales", null, null));
        this.Departments.Add(new Department(4, "QA", null, null));
        this.Departments.Add(new Department(5, "Purchase", null, null));
        this.Departments.Add(new Department(6, "Support", null, null));
        this.Departments.Add(new Department(7, "Warehouse", null, null));

        this.Employees.Add(new Employee(
            1, "Bob", new DateTime(1977, 7, 7), null, this.Departments.First()));
        this.Employees.Add(new Employee(
            2, "Alice", new DateTime(1988, 8, 8), null, this.Departments.Single(x => x.Id == 2)));
        this.Employees.Add(new Employee(
            3, "Charlie", new DateTime(1988, 9, 9), null, this.Departments.Single(x => x.Id == 2)));
    }

}
