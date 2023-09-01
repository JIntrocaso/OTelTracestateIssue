using System.Diagnostics;

public class LogHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LogHeadersMiddleware> _logger;

    public LogHeadersMiddleware(RequestDelegate next, ILogger<LogHeadersMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        // Log the incoming response headers
        _logger.LogInformation("Incoming request headers: {headers}", context.Request.Headers);

        // Checking Activity Baggage for value
        var baggageValue = Activity.Current?.Baggage;

        _logger.LogInformation("Activity Baggage values: {baggage}", baggageValue);

        // Call the next middleware in the pipeline
        await _next(context);
    }
}