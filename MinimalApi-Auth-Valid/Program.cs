using MinimalApi_Auth_Valid.Models;
using MinimalApi_Auth_Valid.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using FluentValidation.AspNetCore;
using FluentValidation;
// dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer for auth

var builder = WebApplication.CreateBuilder(args);
// Configure Services
builder.Services.AddSingleton<CustomerRepository>(); // excluding this results in: System.InvalidOperationException: 'Failure to infer one or more parameters.'
builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();// basic swagger
// This wordy swager implementation allows for swagger auth (top-right, but what does it auth against?)
builder.Services.AddSwaggerGen(x => 
{
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] { }
        }
    });        
});
//builder.Services.AddFluentValidation(fv => fv.RegisterValidatorsFromAssemblyContaining<Customer>());//deprecated
builder.Services.AddFluentValidationAutoValidation().AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<Customer>();

// show options version
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
//{
//    options.Authority = "https://localhost:5001";
//    options.Audience = "minimalapi";
//});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();

// using options to make authentication required for *all* endpoints
builder.Services.AddAuthorization(options => 
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
       .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
       .RequireAuthenticatedUser()
       .Build();
});

var app = builder.Build(); // any builder.service.* calls must be made before this line


// Configure the Application & Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// API Routing, ugly approach //// I don't like this stuff being here

app.MapGet("/customers", (CustomerRepository repository) =>
    Results.Ok(repository.GetAll())) // invokes the data retrieval method
    .Produces<Customer>(StatusCodes.Status200OK, "application/json") // describes the response    
    .AllowAnonymous();

app.MapGet("/customers/{id}",
//[ProducesResponseType(200, Type = (typeof(Customer)))] // can also describe response with decorator. MVC thing
(CustomerRepository repo, Guid id) =>
{
    var customer = repo.GetById(id);
    return customer is not null 
        ? Results.Ok(customer) 
        : Results.NotFound();
});

app.MapPost("/customers", 
(CustomerRepository repository, IValidator<Customer> validator, Customer customer) =>
{
    var validationResult = validator.Validate(customer);
    if (!validationResult.IsValid)
    {
        var errors = validationResult.Errors.Select(x => new { Errors = x.ErrorMessage });
        return Results.BadRequest(errors);
    }

    repository.Create(customer);
    return Results.Created($"/customers/{customer.Id}", customer);
}).AllowAnonymous(); // anyone can add a user

app.MapDelete("/customers/{id}", (CustomerRepository repository, Guid id) =>
{
    repository.Delete(id);
    return Results.Ok($"/customers/{id}");
});

app.MapPut("/customers/{id}", (CustomerRepository repo, Guid id, Customer updatedCustomer) =>
{
    var customer = repo.GetById(id);
    if (customer is null) return Results.NotFound($"Customer with id {id} not found");

    repo.Update(updatedCustomer);
    return Results.Ok(updatedCustomer);     
});


// Endpoints must be described before app.Run()
app.Run();