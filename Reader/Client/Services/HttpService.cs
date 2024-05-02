using Reader.Client.Helpers;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace Reader.Client.Services
{
    public class HttpService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthStateProvider _authState;
        public HttpService(HttpClient httpClient, AuthStateProvider authState)
        {
            _httpClient = httpClient;
            _authState = authState;
        }

        public async Task<ApiResponse> Get(string url)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/{url}");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync(urlBuiler.ToString());
            var content = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                Success = response.IsSuccessStatusCode,
                Message = content,
                Data = content
            };
        }

        public async Task<ApiResponse> Create(string controller, object obj)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/{controller}/Create");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            StringContent stringContent = new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(urlBuiler.ToString(), stringContent);
            var content = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                Success = response.IsSuccessStatusCode,
                Message = content,
                Data = content
            };
        }

        public async Task<ApiResponse> ChangePassword(object obj)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/users/ChangePassword");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            StringContent stringContent = new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(urlBuiler.ToString(), stringContent);
            var content = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                Success = response.IsSuccessStatusCode,
                Message = content,
                Data = content
            };
        }

        public async Task<ApiResponse> Update(string controller, object obj)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/{controller}/Update");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            StringContent stringContent = new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync(urlBuiler.ToString(), stringContent);
            var content = await response.Content.ReadAsStringAsync();

            return new ApiResponse
            {
                Success = response.IsSuccessStatusCode,
                Message = content,
                Data = content
            };
        }

        public async Task<bool> Delete(string url, long id)
        {
            var token = await _authState.GetToken();
            StringBuilder urlBuiler = new StringBuilder();
            urlBuiler.Append($"api/{url}/{id}");

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.DeleteAsync(urlBuiler.ToString());
            return response.IsSuccessStatusCode;
        }
    }
}
