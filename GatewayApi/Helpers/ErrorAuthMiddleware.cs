using System.Text.Json;

public class ErrorAuthMiddleware(RequestDelegate next, IServiceScopeFactory scopeFactory)
{
    private readonly RequestDelegate _next = next;
    private readonly IServiceScopeFactory _scopeFactory = scopeFactory;

    public async Task InvokeAsync(HttpContext context)
    {
        var originalBody = context.Response.Body;
        using var memStream = new MemoryStream();
        context.Response.Body = memStream;

        await _next(context);

        var isAuthApi = context.Request.Path.StartsWithSegments("/connect");

        if (isAuthApi && (context.Response.StatusCode == 400))
        {
            memStream.Seek(0, SeekOrigin.Begin);
            var originalContent = await new StreamReader(memStream).ReadToEndAsync();
            string errorDescription = "Error desconocido";
            try
            {
                var json = JsonDocument.Parse(originalContent);
                if (json.RootElement.TryGetProperty("error_description", out var desc))
                    errorDescription = desc.GetString() ?? errorDescription;
            }
            catch
            {
                // Si el contenido no es JSON válido, usa el mensaje por defecto
            }

            context.Response.Body = originalBody;
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var errorJson = $@"
            {{
                ""Titulo"": ""Error"",
                ""Mensaje"": ""{errorDescription}"",
                ""Fecha"": ""{DateTime.UtcNow}"",
                ""Detalle"": """"
            }}
            ";
            await context.Response.WriteAsync(errorJson);
        }
        else
        {
            memStream.Seek(0, SeekOrigin.Begin);
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
    }
}