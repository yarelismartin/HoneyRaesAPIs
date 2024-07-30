using HoneyRaesAPIs.Models;
using System.Text.Json.Serialization;

List<Customer> customers = new List<Customer>
{
    new Customer(410, "Debby", "830 James Drive, Nashville TN 37211"),
    new Customer(589, "Mike", "456 Oak St, Nashville TN 37211"),
    new Customer(790, "John", "789 Pine St, Nashville TN 37211"),
};

List<Employee> employees = new List<Employee> 
{
    new Employee(480,"Alice", "Electrical"),
    new Employee(490, "Sam", "Storm Damage Repair"),
};

List<ServiceTicket> serviceTickets = new List<ServiceTicket> 
{ 
    new ServiceTicket(1, 410, 480,"Fix light switch", false, new DateTime(2024, 7, 16)),
    new ServiceTicket(2, 589, null,"Replace outlet", true, null),
    new ServiceTicket(3,410, 480,"Repairing electrical wiring in the kitchen", false, new DateTime(2024, 7, 27)),
    new ServiceTicket(4,790, null,"Repairing roof after storm damage", true,  null),
    new ServiceTicket(5, 790, 490,"Clearing debris and fixing gutters after storm", true, new DateTime(2024, 7, 2)),
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

app.MapGet("/servicetickets", () =>
{
    return serviceTickets;
});

app.MapGet("/servicetickets/{id}", (int id) =>
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

app.MapGet("/employees", () =>
{
    return employees;
});

app.MapGet("/employees/{Id}", (int Id) =>
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

app.MapGet("/customers", () =>
{
    return customers;
});

app.MapGet("/customers/{Id}", (int Id) =>
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

app.MapPost("/servicetickets", (ServiceTicket serviceTicket) =>
{
    // creates a new id (When we get to it later, our SQL database will do this for us like JSON Server did!)
    serviceTicket.Id = serviceTickets.Max(st => st.Id) + 1;
    serviceTickets.Add(serviceTicket);
    return serviceTicket;
});

app.MapDelete("/serviceTickets/{id}", (int id) =>
    {
        var serviceTicket = serviceTickets.FirstOrDefault(st => st.Id == id);
        if (serviceTicket == null)
        {
            return Results.NotFound();
        }

        serviceTickets.Remove(serviceTicket);
        return Results.NoContent();
    });

app.MapPut("/servicetickets/{id}", (int id, ServiceTicket serviceTicket) =>
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

app.MapPost("/servicetickets/{id}/complete", (int id) =>
{
    ServiceTicket ticketToComplete = serviceTickets.FirstOrDefault(st => st.Id == id);
    ticketToComplete.DateCompleted = DateTime.Today;
});

app.MapGet("/servicetickets/status=incomplete%emergecny=true", () =>
{
    List<ServiceTicket> queryResult = serviceTickets.Where(st => st.DateCompleted == null && st.Emergency == true).ToList();
    return queryResult;
});

//This actually starts the app, and should always be at the bottom of this file. 
app.Run();

