namespace HoneyRaesAPIs.Models;

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public List<ServiceTicket> ServiceTickets { get; set; }


    public Customer(int id, string name, string address, List<ServiceTicket>? serviceTickets = null)
        {
            Id = id;    
            Name = name;
            Address = address;
            ServiceTickets = serviceTickets ?? new List<ServiceTicket>();
    }
    }

