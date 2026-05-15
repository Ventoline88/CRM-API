using System;
using System.Collections.Generic;
using CRMFunction.Data.Models;
using CRMFunction.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace CRMFunction;

public class CustomerModifiedFunction
{
    private readonly ILogger<CustomerModifiedFunction> _logger;
    private readonly EmailService _emailService;

    public CustomerModifiedFunction(
        ILogger<CustomerModifiedFunction> logger,
        EmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    [Function("CustomerModifiedFunction")]
    public async Task Run([CosmosDBTrigger(
        databaseName: "crm-db",
        containerName: "customers",
        Connection = "ConnectionStrings:CosmosDBConnection",
        LeaseContainerName = "leases",
        CreateLeaseContainerIfNotExists = true)] IReadOnlyList<Customer> customers)
    {
        if (customers != null && customers.Count > 0)
        {
            foreach(var customer in customers)
            {
                _logger.LogInformation($"Customer changed: {customer.Name}");
                _logger.LogInformation($"Responsible salesperson: {customer.ResponsibleSalesPerson.Name}");
                _logger.LogInformation($"Salesperson email: {customer.ResponsibleSalesPerson.Email}");

                await _emailService.SendCustomerAssignedEmail(
                    customer.ResponsibleSalesPerson.Email,
                    customer.ResponsibleSalesPerson.Name,
                    customer.Name,
                    customer.Phone,
                    customer.Address);
            }
        }
    }
}