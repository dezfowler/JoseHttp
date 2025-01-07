
var builder = DistributedApplication.CreateBuilder(args);

var mappingsPath = Path.Combine(Directory.GetCurrentDirectory(), "WireMockMappings");

var wiremock = builder
    .AddWireMock("wiremock", WireMockServerArguments.DefaultPort)
    .WithMappingsPath(mappingsPath)
    .WithReadStaticMappings();

var nodeapi = builder.AddDockerfile("nodeapi", "../TestApiNode")
    .WaitFor(wiremock)
    .WithHttpEndpoint(port: 16363, targetPort: 80, env: "PORT", name: "nodeapi")
    .WithExternalHttpEndpoints();

var nodeEndpoint = nodeapi.GetEndpoint("nodeapi");

var testapi = builder.AddProject<Projects.TestApi>("testapi")
    .WithExternalHttpEndpoints()
    .WithReference(wiremock)
    .WithReference(nodeEndpoint);

nodeapi
    .WithReference(wiremock)
    .WithReference(testapi);

builder.Build().Run();
