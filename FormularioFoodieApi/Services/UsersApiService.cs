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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UsersApiService(HttpClient httpClient, IConfiguration configuration, ILogger<UsersApiService> logger, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthorizationHeader()
        {
            var context = _httpContextAccessor.HttpContext;
            if (context?.Request.Headers.ContainsKey("Authorization") == true)
            {
                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = 
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", authHeader.Replace("Bearer ", ""));
                }
            }
        }

        public async Task<bool> AddRoleToUserAsync(int usuarioId, string roleName)
        {
            try
            {
                var usersApiUrl = _configuration["ApiSettings:UsersApiBaseUrl"];
                var requestUrl = $"{usersApiUrl}/api/users/{usuarioId}/roles";

                var requestBody = new { RoleName = roleName };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(requestUrl, content);
                
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
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
            return await Task.FromResult(string.Empty);
        }

        public async Task<object?> GetCurrentUserAsync()
        {
            try
            {
                SetAuthorizationHeader();
                
                var usersApiUrl = _configuration["ApiSettings:UsersApiBaseUrl"];
                var requestUrl = $"{usersApiUrl}/api/auth/userinfo";

                var response = await _httpClient.GetAsync(requestUrl);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                if (response.IsSuccessStatusCode)
                {
                    var user = JsonSerializer.Deserialize<object>(responseContent);
                    return user;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Excepción al obtener usuario actual");
                return null;
            }
        }
    }
}