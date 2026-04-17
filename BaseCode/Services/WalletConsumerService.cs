using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using BaseCode.Models.Requests;
using BaseCode.Models.Responses;

namespace BaseCode.Services
{
    public class WalletConsumerService
    {
        private readonly HttpClient _httpClient;
        private readonly string _walletApiBaseUrl;

        public WalletConsumerService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _walletApiBaseUrl = configuration["WalletApi:BaseUrl"] ?? "http://localhost:5001";
            _httpClient.BaseAddress = new Uri(_walletApiBaseUrl);
        }

        // Generic method to call wallet API
        private async Task<TResponse> CallWalletApi<TRequest, TResponse>(string endpoint, TRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseJson = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<TResponse>(responseJson);
                }
                else
                {
                    var errorResponse = JsonConvert.DeserializeObject<TResponse>(responseJson);
                    return errorResponse;
                }
            }
            catch (Exception ex)
            {
                var errorResp = Activator.CreateInstance<TResponse>();
                var prop = errorResp.GetType().GetProperty("isSuccess");
                if (prop != null) prop.SetValue(errorResp, false);
                var msgProp = errorResp.GetType().GetProperty("Message");
                if (msgProp != null) msgProp.SetValue(errorResp, $"Error calling wallet service: {ex.Message}");
                return errorResp;
            }
        }

        // Request Wallet Code
        public async Task<RequestWalletCodeResponse> RequestWalletCodeAsync(RequestWalletCodeRequest request)
        {
            return await CallWalletApi<RequestWalletCodeRequest, RequestWalletCodeResponse>(
                "/api/Wallet/RequestWalletCode", request);
        }

        // Create Wallet
        public async Task<CreateWalletResponse> CreateWalletAsync(CreateWalletRequest request)
        {
            return await CallWalletApi<CreateWalletRequest, CreateWalletResponse>(
                "/api/Wallet/CreateWallet", request);
        }

        // Deposit to Wallet
        public async Task<WalletDepositResponse> DepositToWalletAsync(WalletDepositRequest request)
        {
            return await CallWalletApi<WalletDepositRequest, WalletDepositResponse>(
                "/api/Wallet/DepositToWallet", request);
        }

        // Withdraw from Wallet
        public async Task<WalletWithdrawResponse> WithdrawFromWalletAsync(WalletWithdrawRequest request)
        {
            return await CallWalletApi<WalletWithdrawRequest, WalletWithdrawResponse>(
                "/api/Wallet/WithdrawFromWallet", request);
        }

        // Get Wallet Balance
        public async Task<GetWalletBalanceResponse> GetWalletBalanceAsync(GetWalletBalanceRequest request)
        {
            return await CallWalletApi<GetWalletBalanceRequest, GetWalletBalanceResponse>(
                "/api/Wallet/GetWalletBalance", request);
        }

        // Get Wallet Transactions
        public async Task<GetWalletTransactionsResponse> GetWalletTransactionsAsync(GetWalletTransactionsRequest request)
        {
            return await CallWalletApi<GetWalletTransactionsRequest, GetWalletTransactionsResponse>(
                "/api/Wallet/GetWalletTransactions", request);
        }
    }
}
