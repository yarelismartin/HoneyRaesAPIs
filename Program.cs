using HoneyRaesAPIs.Models;
using System.Text.Json.Serialization;

List<Customer> customers = new List<Customer>
{
    new Customer(410, "Debby", "830 James Drive, Nashville TN 37211"),
    new Customer(589, "Mike", "456 Oak St, Nashville TN 37211"),
    new Customer(790, "John", "789 Pine St, Nashville TN 37211"),
    new Customer(888, "Jill", "789 Pine St, Nashville TN 37211"),
};

List<Employee> employees = new List<Employee> 
{
    new Employee(480,"Alice", "Electrical"),
    new Employee(490, "Sam", "Storm Damage Repair"),
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket> 
{ 
    new ServiceTicket(1, 888, 480,"Fix light switch", false, new DateTime(2020, 7, 16)),
    new ServiceTicket(2, 589, null,"Replace outlet", true, null),
    new ServiceTicket(3,410, 480,"Repairing electrical wiring in the kitchen", false, new DateTime(2024, 7, 27)),
    new ServiceTicket(4,790, 480,"Repairing roof after storm damage", true,  null),
    new ServiceTicket(5, 790, 490,"Clearing debris and fixing gutters after storm", true, new DateTime(2024, 6, 2)),
};


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options => options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);


// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//All of the code above is setting up the application to be ready to serve as a web API.



var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


//This code creates an endpoint in the application.
//This is an important concept that is a basic building block of a web API.
//An endpoint is essentially a route (a URL to make a request), and a handler,
//which is a function that determines the logic for what to do when a request is made to that route. 

app.MapGet("/api/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/api/servicetickets/{id}", (int id) =>
{
    ServiceTicket serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
    if (serviceTicket == null)
    {
        return Results.NotFound();
    }
    serviceTicket.Employee = employees.FirstOrDefault(e => e.Id == serviceTicket.EmployeeId);
    serviceTicket.Customer = customers.FirstOrDefault(e => e.Id == serviceTicket.CustomerId);
    return Results.Ok(serviceTicket);
});

app.MapGet("/api/employees", () =>
{
    return employees;
});

app.MapGet("/api/employees/{Id}", (int Id) =>
{
    Employee employee = employees.FirstOrDefault(e => e.Id == Id);
    if (employee == null)
    {
        // creates a 404 response for the endpoint.
        return Results.NotFound();
    }
    employee.ServiceTickets = serviceTickets.Where(st => st.EmployeeId == Id).ToList();

    //return a 200 response with the employee data
    return Results.Ok(employee);
});

app.MapGet("/api/customers", () =>
{
    return customers;
});

app.MapGet("/api/customers/{Id}", (int Id) =>
{
    Customer customer = customers.FirstOrDefault(e => e.Id == Id);
    if (customer == null)
    {
        // creates a 404 response for the endpoint.
        return Results.NotFound();
    }
    customer.ServiceTickets = serviceTickets.Where(st => st.CustomerId == Id).ToList();
    //return a 200 response with the employee data
    return Results.Ok(customer);
});

app.MapPost("/api/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/api/serviceTickets/{id}", (int id) =>
    {
        var serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
        if (serviceTicket == null)
        {
            return Results.NotFound();
        }

        serviceTickets.Remove(serviceTicket);
        return Results.NoContent();
    });

app.MapPut("/api/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
{
    ServiceTicket ticketToUpdate = serviceTickets.FirstOrDefault(st => st.Id == id);
    int ticketIndex = serviceTickets.IndexOf(ticketToUpdate);
    if (ticketToUpdate == null)
    {
        return Results.NotFound();
    }
    //the id in the request route doesn't match the id from the ticket in the request body. That's a bad request!
    if (id != serviceTicket.Id)
    {
        return Results.BadRequest();
    }
    serviceTickets[ticketIndex] = serviceTicket;
    return Results.Ok();
});

app.MapPost("/api/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/api/service-tickets/status=incomplete%emergecny=true", () =>
{
    List<ServiceTicket> queryResult = serviceTickets.Where(st => st.DateCompleted == null && st.Emergency == true).ToList();
    return queryResult;
});

app.MapGet("/api/service-tickets/unassigned",() =>
{
    List<ServiceTicket> unassigned = serviceTickets.Where(st => st.EmployeeId == null).ToList();
    return unassigned;
});

app.MapGet("/api/customers/inactive", () =>
{
    // grabs todays date and subtracts one year to obtain the date one year ago
    var oneYearAgoDate = DateTime.Today.AddYears(-1);

    // grab a list of customer Ids that have had service tickets completed within this year
    var activeCustomers = serviceTickets
        .Where(st => st.DateCompleted == null || st.DateCompleted >= oneYearAgoDate)
        .Select(st => st.CustomerId)
        .Distinct()
        .ToList();

    //look through the customer list and grab the customers who's  CustomerId (c.Id) is NOT in the activeCustomerIds list.
    var inactiveCustomers = customers.Where(c => !activeCustomers.Contains(c.Id));

    return inactiveCustomers;
});

app.MapGet("/api/employees/available", () =>
{
    var completedServiceTicket = serviceTickets
    .Where(st => st.DateCompleted == null)
    .Select(st => st.EmployeeId)
    .Distinct()
    .ToList();

    var availableEmployees = employees.Where(e => !completedServiceTicket.Contains(e.Id));
    return availableEmployees;
});

app.MapGet("/api/employees/{Id}/customers", (int Id) =>
{
    var employeesServiceTicket = serviceTickets
    .Where(st =>  st.EmployeeId == Id)
    .Select(st => st.CustomerId)
    .Distinct()
    .ToList();

    var employeesCustomer = customers.Where(c => employeesServiceTicket.Contains(c.Id));
    return employeesCustomer;
});

app.MapGet("/api/employees/employee-of-month", () =>
{
    var oneMonthAgoToday = DateTime.Today.AddMonths(-1);

    var ticketsCompletedWithinMonth = serviceTickets
    .Where(st => st.DateCompleted >= oneMonthAgoToday && st.EmployeeId != null);

    var employeeTicketCount = ticketsCompletedWithinMonth
    .GroupBy(st => st.EmployeeId)
    .Select(group => new
    {
        Employee = group.Key, 
        CompletedTicket = group.Count()
    })
    .OrderByDescending(g => g.CompletedTicket)
    .FirstOrDefault();

    var employeeOfMonth = employees.First(e => e.Id == employeeTicketCount.Employee);
    return employeeOfMonth;

});

app.MapGet("/api/service-tickets/completion-date", () =>
{
    var ticketCompletionDate = serviceTickets
    .Where(st => st.DateCompleted != null)
    .OrderBy(s => s.DateCompleted)
    .ToList();

    return ticketCompletionDate;

});


app.MapGet("/api/service-tickets/prioritize-ticket", () =>
{
    var completedTicket = serviceTickets.Where(st => st.DateCompleted == null).ToList();
    var ticketsToPrioritize = completedTicket
    .OrderByDescending(st => st.Emergency == true)
    .ThenByDescending(st => st.EmployeeId == null)
    .ToList();
    return ticketsToPrioritize;
});


//This actually starts the app, and should always be at the bottom of this file. 
app.Run();