using Api.Repositories.IRepositories;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Services.SearchServices.Google
{
    public class GoogleSearchService : ISearchService<GoogleRequest, GoogleResponse?>
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly IUnitOfWork _unit;

        public GoogleSearchService(HttpClient httpClient, IOptions<ApiSettings> apiSettings, IUnitOfWork unit)
        {
            _httpClient = httpClient;
            _apiKey = apiSettings.Value.GoogleApi.ApiKey;
            _httpClient.BaseAddress = new Uri(apiSettings.Value.GoogleApi.BaseUrl);
            _unit = unit;
        }
        public async Task<GoogleResponse?> SearchAsync(GoogleRequest request)
        {
            GoogleResponse googleResponse = new();
            try
            {
                // Xây dựng các tham số từ yêu cầu
                var parameters = request.BuildRequestParams();

                // Chuyển đổi tham số thành JSON
                var requestContent = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");

                // Thêm API key vào header
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

                // Gửi yêu cầu POST
                var response = await _httpClient.PostAsync("", requestContent);

                // Kiểm tra mã trạng thái phản hồi
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    googleResponse = JsonConvert.DeserializeObject<GoogleResponse>(jsonResponse)!;
                    return googleResponse;
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                Console.WriteLine($"Error: {ex.Message}");
            }

            return googleResponse; // Hoặc xử lý lỗi phù hợp
        }
        public async Task<GoogleResponse?> SearchWithUserTokenCofigAsync(GoogleRequest googleRequest, string userIdTokenConfig)
        {
            GoogleResponse googleResponse = new();
            try
            {
                // Xây dựng các tham số từ yêu cầu
                var parameters = googleRequest.BuildRequestParams();

                // Chuyển đổi tham số thành JSON
                var requestContent = new StringContent(JsonConvert.SerializeObject(parameters), Encoding.UTF8, "application/json");


                string? tokenDecrypted = _unit.SecretTokens.GetByIdAsync(userIdTokenConfig).Result?.Token;

                // Thêm API key vào header
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", tokenDecrypted ?? _apiKey);

                // Gửi yêu cầu POST
                var response = await _httpClient.PostAsync("", requestContent);

                // Kiểm tra mã trạng thái phản hồi
                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    googleResponse = JsonConvert.DeserializeObject<GoogleResponse>(jsonResponse)!;
                    return googleResponse;
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi nếu cần thiết
                Console.WriteLine($"Error: {ex.Message}");
            }

            return googleResponse; // Hoặc xử lý lỗi phù hợp
        }
    }
}