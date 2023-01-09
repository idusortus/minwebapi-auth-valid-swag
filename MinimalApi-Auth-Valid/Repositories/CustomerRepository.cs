using MinimalApi_Auth_Valid.Models;

namespace MinimalApi_Auth_Valid.Repositories;

class CustomerRepository
{
    private readonly Dictionary<Guid, Customer> _customers = new();

    public void Create(Customer? customer)
    {
        if (customer is null)
        {
            return;
        }

        _customers[customer.Id] =  customer;
    }
    
    public Customer? GetById(Guid id)
    {
        return _customers.GetValueOrDefault(id);
        //return _customers.TryGetValue(id, out var customer) ? customer : null;
    }

    public IEnumerable<Customer> GetAll()
    {
        return _customers.Values;
    }

    public void Update(Customer? customer)
    {
        // ! bang prevents compiler nag re: possible null reference but does not handle it
        if (customer is not null)           
            _customers[customer.Id] = customer;
        
    }

    public void Delete(Guid id)
    {
        _customers.Remove(id);
    }
}