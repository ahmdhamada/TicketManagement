using System.Net;
using System.Net.Http.Json;
using TicketManagement.Application.Common.Models;
using TicketManagement.Application.Tickets.Dtos;

namespace TicketManagement.IntegrationTests;

/// <summary>
/// Verifies the core security requirement: a Customer can never read or act on
/// another Customer's tickets, even by guessing ids directly against the API.
/// </summary>
public class TicketsDataIsolationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TicketsDataIsolationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Customer_CannotSeeAnotherCustomersTicket_ViaListOrDirectId()
    {
        var customer2Client = await _factory.CreateClient().AsUserAsync("customer2@invento.sa");

        // Seed data includes tickets created by customer1 (ids 1 and 3). customer2 must not see them.
        var listResponse = await customer2Client.GetAsync("/api/tickets?pageSize=50");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await listResponse.Content.ReadFromJsonAsync<PagedResult<TicketListItemDto>>();
        page!.Items.Should().OnlyContain(t => t.Title != "Cannot reset my password");

        var directGet = await customer2Client.GetAsync("/api/tickets/1");
        directGet.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customer_CannotAddCommentToAnotherCustomersTicket()
    {
        var customer2Client = await _factory.CreateClient().AsUserAsync("customer2@invento.sa");

        var response = await customer2Client.PostAsJsonAsync("/api/tickets/1/comments", new { body = "sneaky comment" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Agent_CannotSeeTicketNotAssignedToThem()
    {
        // Ticket 1 ("Cannot reset my password") is unassigned in seed data.
        var agentClient = await _factory.CreateClient().AsUserAsync("agent1@invento.sa");

        var response = await agentClient.GetAsync("/api/tickets/1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Customer_CannotAssignTickets()
    {
        var customerClient = await _factory.CreateClient().AsUserAsync("customer1@invento.sa");

        var response = await customerClient.PatchAsJsonAsync("/api/tickets/1/assign", new { assignedToUserId = 1, rowVersion = "" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
