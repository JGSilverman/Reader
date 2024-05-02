using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Reader.Services
{
    public class GoogleBookHttpService
    {
        private readonly IHttpClientFactory _clientFactory;
        private readonly IConfiguration _configuration;
        private string Uri;
        private string ApiKey;

        public GoogleBookHttpService(IHttpClientFactory clientFactory, IConfiguration configuration)
        {
            _clientFactory = clientFactory;
            _configuration = configuration;

            Uri = _configuration.GetSection("GoogleAPISettings").GetValue<string>("BaseUrl");
            ApiKey = _configuration.GetSection("GoogleAPISettings").GetValue<string>("ApiKey");
        }

        public async Task<GoogleBookAPIResponse> SearchGoogleBooks(string searchTerm)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, BuildGoogleSearchEndpoint(searchTerm));
            request.Headers.Add("ContentType", "application/json");
            request.Headers.Add("Authorization", ApiKey);

            var client = _clientFactory.CreateClient();
            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var stringResponse = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<GoogleBookAPIResponse>(stringResponse,
                    new JsonSerializerOptions() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            }
            else
            {
                return null;
            }
        }

        private string BuildGoogleSearchEndpoint(string term)
        {
            if (string.IsNullOrWhiteSpace(term)) throw new ArgumentNullException(nameof(term));
            return string.Format("{0}?q={1}&maxResults=40&printType=books&key={2}", Uri, term, ApiKey);
        }
    }
}
