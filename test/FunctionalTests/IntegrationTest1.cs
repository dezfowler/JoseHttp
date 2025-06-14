namespace FunctionalTests;

public class IntegrationTest1
{
    [Fact]
    public async Task PostSignedRequest()
    {
        // Arrange
        var appHost = await DistributedApplicationTestingBuilder.CreateAsync<Projects.TestAppHost>();
        appHost.Services.ConfigureHttpClientDefaults(clientBuilder =>
        {
            clientBuilder.AddStandardResilienceHandler();
        });
        // To output logs to the xUnit.net ITestOutputHelper, consider adding a package from https://www.nuget.org/packages?q=xunit+logging
    
        await using var app = await appHost.BuildAsync();
        var resourceNotificationService = app.Services.GetRequiredService<ResourceNotificationService>();
        await app.StartAsync();

        // Act
        var httpClient = app.CreateHttpClient("testapi");
        await resourceNotificationService.WaitForResourceAsync("testapi", KnownResourceStates.Running).WaitAsync(TimeSpan.FromSeconds(30));
        var response = await httpClient.PostAsync("/sig", new StringContent(""));

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
