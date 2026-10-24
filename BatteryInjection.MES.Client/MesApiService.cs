
using BatteryInjection.Models;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BatteryInjection.MES.Client
{
    public class MesApiService
    {
        private readonly string _mesUrl;
        public MesApiService(string mesUrl)
        {
            _mesUrl = mesUrl;     
        }
        private static readonly HttpClient _httpClient = new HttpClient(new HttpClientHandler
        {
            // 忽略本地测试环境的 SSL 证书验证 (至关重要！)
            ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
        });
        public async Task<bool> ReportInjectionAsync(InjectionRecord record)
        {
            try
            {
                string url = _mesUrl;
                string json = JsonSerializer.Serialize(record);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url,content);
                if(response.IsSuccessStatusCode)
                {
                    string result = await response.Content.ReadAsStringAsync();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"上报失败:{ex.Message}");
                return false;

            }
        }
    }

}
