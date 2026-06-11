namespace ECommercePoc.Domain.Entities;

/// <summary>
/// Customer entity — simplified for this POC.
/// </summary>
public class Customer
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }

#pragma warning disable CS8618 // EF Core parameterless constructor
    private Customer() { }
#pragma warning restore CS8618

#pragma warning disable CS8618
    public Customer(string firstName, string lastName, string email)
#pragma warning restore CS8618
    {
        Id = Guid.NewGuid();
        SetName(firstName, lastName);
        SetEmail(email);
    }

    public void SetName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required.", nameof(firstName));
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required.", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
    }

    public void SetEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        Email = email;
    }
}
