namespace HoneyRaesAPIs.Models;

public class ServiceTicket
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public int? EmployeeId { get; set; }
    public string Description { get; set; }
    public bool Emergency { get; set; }
    public DateTime? DateCompleted { get; set; }

    public Customer? Customer { get; set; }

    public Employee? Employee { get; set; }
    public ServiceTicket( int id, int customerId, int? employeeId, string description, bool emergency, DateTime? dateCompleted, Customer? customer = null, Employee? employee = null)
    {
        Id = id;
        CustomerId = customerId;
        EmployeeId = employeeId;
        Description = description;
        Emergency = emergency;
        DateCompleted = dateCompleted;
        Customer = customer;
        Employee = employee;

    }

}
