namespace HoneyRaesAPIs.Models;

public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Specialty { get; set; }
    public List<ServiceTicket> ServiceTickets { get; set; }

    // constructor with default initialization for ServiceTickets
    // if no value is provided for service ticket it will default to null
    public Employee(int id, string name, string specialty, List<ServiceTicket>? serviceTickets = null)
    {
        Id = id;
        Name = name;
        Specialty = specialty;
        //Initialize service tickt to an empty list to avoid null errors/ Default Intialization
        //if the serive ticket is null than make it an empty list. null-coalescing operator (??), 
        ServiceTickets = serviceTickets ?? new List<ServiceTicket>();
    }
}


