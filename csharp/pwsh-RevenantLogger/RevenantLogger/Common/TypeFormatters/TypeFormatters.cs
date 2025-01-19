using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RosettaTools.Pwsh.Text.RevenantLogger.Common.TypeFormatters {
    //internal class ConfigDefinition {
    //}



    /*
     * 
       
     from: https://github.com/verticalsoftware/vertical-spectreconsolelogger/blob/dev/docs/formatting.md  
      
       
    Custom Formatting
    Imagine a type called Customer that is located in a 3rd party library and contains sensitive data that cannot be displayed in destructured output, and the ToString method cannot be modified. The obvious way to solve this problem is to log a new object that is a subset of the Customer type like so:

    public async Task SaveCustomerAsync(Customer customer)
    {
        await database.SaveAsync(customer);

        logger.LogInformation("Created new customer record {customer}",
            new { Id = customer.Id, Name = customer.Name });
    }

    This works, but must be repeated every time a Customer needs to be logged. A consistent mechanism for formatting the Customer type for logging would be to provide a ICustomFormatter instance and register it with the logging provider.

public class CustomerFormatter : ICustomFormatter
{
    public override string Format(string? format, object? arg, IFormatProvider? provider)
    {
        var customer = (Customer)arg;
            
        return $"Id={customer.Id}, Name={customer.Name}";
    }
}

// Register the formatter during configuration.

config.ConfigureProfiles(profile => profile.AddTypeFormatter<Customer>(new CustomerFormatter()));
There are additional tools to make custom formatting less burdensome. First, instead of having to define an implementation of ICustomFormatter, you can register a delegate instead. The following example produces the same behavior:

// Using a formatting delegate - note no additional registration
// is necessary.

config.ConfigureProfiles(profile => profile.AddTypeFormatter<Customer>((format, customer, formatProvider) => 
{
    return $"Id={customer.Id}, Name={customer.Name}";
});




      Secondly, you may decorate ICustomFormatter implementations with the TypeFormatter attribute
      and call a single method to register them all with assembly scanning:

    [TypeFormatter(typeof(Customer))]
    public class CustomerFormatter : ICustomFormatter
    {
    // ...
    }

    // Registration

    config.ConfigureProfiles(profile => profile.AddTypeFormatters());

        */
}
