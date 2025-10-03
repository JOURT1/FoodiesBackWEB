using System.Text;
using System.Text.Json;
using FormularioFoodieApi.Services.Interfaces;

namespace FormularioFoodieApi.Services
{
    public class UsersApiService : IUsersApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UsersApiService> _logger;

        public UsersApiService(HttpClient httpClient, IConfiguration configuration, ILogger<UsersApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> AddRoleToUserAsync(int usuarioId, string roleName)
        {
            try
            {
                var usersApiUrl = _configuration["ApiSettings:UsersApiBaseUrl"];
                var requestUrl = $"{usersApiUrl}/api/users/{usuarioId}/roles";

                _logger.LogInformation($"UsersApiUrl configurada: {usersApiUrl}");
                _logger.LogInformation($"URL completa para agregar rol: {requestUrl}");

                var requestBody = new { RoleName = roleName };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                _logger.LogInformation($"Enviando petición POST a {requestUrl} con body: {json}");

                var response = await _httpClient.PostAsync(requestUrl, content);
                
                var responseContent = await response.Content.ReadAsStringAsync();
                _logger.LogInformation($"Response status: {response.StatusCode}, Content: {responseContent}");
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Rol '{roleName}' agregado exitosamente al usuario {usuarioId}");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"Error al agregar rol al usuario {usuarioId}: {response.StatusCode} - {responseContent}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Excepción al agregar rol '{roleName}' al usuario {usuarioId}");
                return false;
            }
        }

        public async Task<bool> ValidateUserExistsAsync(int usuarioId)
        {
            try
            {
                var usersApiUrl = _configuration["ApiSettings:UsersApiBaseUrl"];
                var requestUrl = $"{usersApiUrl}/api/users/{usuarioId}";

                // Agregar token de autorización
                var token = await GetUserTokenAsync();
                if (!string.IsNullOrEmpty(token))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                }

                var response = await _httpClient.GetAsync(requestUrl);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al validar la existencia del usuario {usuarioId}");
                return false;
            }
        }

        public async Task<string> GetUserTokenAsync()
        {
            // Por ahora retornamos vacío ya que la autenticación se maneja a nivel de Gateway
            // En el futuro, aquí podríamos implementar autenticación service-to-service
            return await Task.FromResult(string.Empty);
        }
    }
}