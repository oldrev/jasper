using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.Jasper.Tests.Models;

public record class Department(int Id, string Name, Department? Parent, Employee? Director);

public record class Employee(int Id, string Name, DateTime BirthDate, Employee? Manager, Department Department);
