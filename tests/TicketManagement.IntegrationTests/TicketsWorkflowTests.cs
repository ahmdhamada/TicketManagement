using System.Net;
using System.Net.Http.Json;
using TicketManagement.Application.Tickets.Dtos;

namespace TicketManagement.IntegrationTests;

public class TicketsWorkflowTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TicketsWorkflowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Customer_CanCreateTicket_ThenSeeItInTheirList()
    {
        var client = await _factory.CreateClient().AsUserAsync("customer1@invento.sa");

        var createResponse = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("Printer is on fire", "Literally smoking, please help"));
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await createResponse.Content.ReadFromJsonAsync<TicketDetailDto>();
        created!.Status.Should().Be(TicketManagement.Domain.Enums.TicketStatus.Open);

        var getResponse = await client.GetAsync($"/api/tickets/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task InvalidStatusTransition_ReturnsConflict()
    {
        var client = await _factory.CreateClient().AsUserAsync("admin@invento.sa");

        var createResponse = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("Some issue", "Description of the issue"));
        var created = (await createResponse.Content.ReadFromJsonAsync<TicketDetailDto>())!;

        // Open -> Resolved is not a valid direct transition.
        var response = await client.PatchAsJsonAsync($"/api/tickets/{created.Id}/status", new UpdateTicketStatusRequest(TicketManagement.Domain.Enums.TicketStatus.Resolved, created.RowVersion));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task StaleRowVersion_OnConcurrentUpdate_ReturnsConflict()
    {
        var client = await _factory.CreateClient().AsUserAsync("admin@invento.sa");

        var createResponse = await client.PostAsJsonAsync("/api/tickets", new CreateTicketRequest("Concurrency test", "Testing optimistic concurrency"));
        var created = (await createResponse.Content.ReadFromJsonAsync<TicketDetailDto>())!;

        // First update succeeds and advances the RowVersion.
        var firstUpdate = await client.PatchAsJsonAsync($"/api/tickets/{created.Id}/status", new UpdateTicketStatusRequest(TicketManagement.Domain.Enums.TicketStatus.InProgress, created.RowVersion));
        firstUpdate.StatusCode.Should().Be(HttpStatusCode.OK);

        // Reusing the now-stale RowVersion from the original fetch must be rejected.
        var staleUpdate = await client.PatchAsJsonAsync($"/api/tickets/{created.Id}/priority", new UpdateTicketPriorityRequest(TicketManagement.Domain.Enums.TicketPriority.Critical, created.RowVersion));
        staleUpdate.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
