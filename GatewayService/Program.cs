using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/instrumented/{player}", async (string player) =>
{
    Activity.Current?.AddBaggage("testId", Guid.NewGuid().ToString());
    using (var client = new HttpClient())
    {
        client.BaseAddress = new Uri("http://localhost:8080/");

        // Build the endpoint URL based on the player parameter
        var endpoint = $"rolldice/{player}";

        // Send the GET request and retrieve the response
        var response = await client.GetAsync(endpoint);

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Read the response content as a string
        var responseContent = await response.Content.ReadAsStringAsync();

        return responseContent;
    }
})
.WithName("GetDataFromInstrumentedService")
.WithOpenApi();

app.MapGet("/noninstrumented/{player}", async (string player) =>
{
    Activity.Current?.AddBaggage("testId", Guid.NewGuid().ToString());
    using (var client = new HttpClient())
    {
        client.BaseAddress = new Uri("http://localhost:8082/");

        // Build the endpoint URL based on the player parameter
        var endpoint = $"rolldice/{player}";

        // Send the GET request and retrieve the response
        var response = await client.GetAsync(endpoint);

        // Ensure the request was successful
        response.EnsureSuccessStatusCode();

        // Read the response content as a string
        var responseContent = await response.Content.ReadAsStringAsync();

        return responseContent;
    }
})
.WithName("GetDataFromNonInstrumentedService")
.WithOpenApi();


app.Run();

