Console.WriteLine("Hello, World!");
Console.WriteLine("Once all services have started, press 'q' to exit or any other key to run requests...");

var input = Console.ReadKey();

while (input.KeyChar != 'q')
{
    Console.WriteLine();

    using (var client = new HttpClient())
    {
        client.BaseAddress = new Uri("https://localhost:7070/");

        await CallService(client, Service.Instrumented, false);
        await CallService(client, Service.Instrumented, true);
        await CallService(client, Service.NonInstrumented, false);
        await CallService(client, Service.NonInstrumented, true);
    }

    Console.WriteLine("All calls completed. Press 'q' to exit or any other key to run requests again.");
    input = Console.ReadKey();
}

async Task CallService(HttpClient client, Service service, bool malformedTracestate)
{
    var instrumentedEndpoint = "/instrumented/{player}";
    var noninstrumentedEndpoint = "/noninstrumented/{player}";
    var endpoint = service == Service.Instrumented ? instrumentedEndpoint : noninstrumentedEndpoint;

    var tracestate = malformedTracestate ? "@nr=0-2-1723647-1588678844-ba24a74731449699--0--1692836661343" : "123@nr=0-2-1723647-1588678844-ba24a74731449699--0--1692836661343";
    var traceparent = "00-2aa46cb79f90455e3088e54d98496555-3c51d679897ba899-00";
    var baggage = $"SessionId = {Guid.NewGuid()}, instrumented = {service == Service.Instrumented}";

    // Set the required headers for each endpoint
    var headers = new Dictionary<string, string>
    {
        { "tracestate", tracestate },
        { "traceparent", traceparent },
        { "Baggage",  baggage }
    };

    // Build the endpoint URL based on the player parameter
    var url = endpoint.Replace("{player}", service.ToString());

    // Create the request with the specified headers
    var request = new HttpRequestMessage(HttpMethod.Get, url);
    foreach (var header in headers)
    {
        request.Headers.Add(header.Key, header.Value);
    }

    // Send the request and retrieve the response
    try
    {
        var response = await client.SendAsync(request);

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Read the response content as a string
        var responseContent = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"Response: {responseContent}");
    }
    catch (Exception ex)
    {
        Console.Write(ex.ToString());
        throw;
    }
}

enum Service
{
    Instrumented,
    NonInstrumented
}