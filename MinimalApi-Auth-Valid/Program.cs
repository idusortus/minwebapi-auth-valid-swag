

using MinimalApi_Auth_Valid.Models;
using MinimalApi_Auth_Valid.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<CustomerRepository>(); // excluding this results in: System.InvalidOperationException: 'Failure to infer one or more parameters.'
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapGet("/customers", (CustomerRepository repository) => repository.GetAll());

app.MapGet("/customers/{id}", (CustomerRepository repo, Guid id) =>
{
    var customer = repo.GetById(id);
    return customer is not null 
        ? Results.Ok(customer) 
        : Results.NotFound();
});

app.MapPost("/customers", (CustomerRepository repository, Customer customer) =>
{
    repository.Create(customer);
    return Results.Created($"/customers/{customer.Id}", customer);
});

app.MapDelete("/customers/{id}", (CustomerRepository repository, Guid id) =>
{
    repository.Delete(id);
    return Results.Ok($"/customers/{id}");
});

app.MapPut("/customers/{id}", (CustomerRepository repo, Guid id, Customer updatedCustomer) =>
{
    var customer = repo.GetById(id);
    if (customer is null) return Results.NotFound();

    repo.Update(updatedCustomer);
    return Results.Ok(updatedCustomer);
});
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.Run();