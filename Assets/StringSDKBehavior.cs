using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using MetafabSdk;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;

public static class Constants
{
    public const string api_base_url = "http://localhost:5555";
}

namespace StringSDK
{
    public static class StringXYZ
    {
        // API Client
        static ApiClient apiClient;

        // Headers
        public static string ApiKey
        {
            get => apiClient.headers.ContainsKey("X-Api-Key") ? apiClient.headers["X-Api-Key"] : null;
            set => apiClient.headers["X-Api-Key"] = value;
        }

        public static string Authorization
        {
            get => apiClient.headers.ContainsKey("Authorization") ? apiClient.headers["Authorization"] : null;
            set => apiClient.headers["Authorization"] = value;
        }

        // Constructor
        static StringXYZ()
        {
            apiClient = new ApiClient(Constants.api_base_url);
        }

        // Methods
        public static async UniTask<LoginPayload> RequestLogin(string walletAddr, CancellationToken token = default)
        {
            return await apiClient.Get<LoginPayload>($"/login?walletAddress={walletAddr}");
        }

        public static async UniTask<LoginResponse> Login(LoginRequest loginRequest, CancellationToken token = default)
        {
            try
            {
                return await apiClient.Post<LoginResponse>($"/login/sign", loginRequest);
            }
            catch // (Exception e)
            {
                // Unsure how to check for 400 Bad response without parsing Exception string
                // Skipping for now
                // TODO: Parse exception for status code 400 before Creating user
                // If the status is not 400, throw error up the stack
                return await CreateUser(loginRequest);
            }
        }

        public static async UniTask<LoginResponse> CreateUser(LoginRequest loginRequest, CancellationToken token = default)
        {
            return await apiClient.Post<LoginResponse>($"/users", loginRequest);
        }

        public static async UniTask<HttpResponse> RequestEmailAuth(string emailAddr, string userId, CancellationToken token = default)
        {
            return await apiClient.Get($"/users/{userId}/verify-email?email={emailAddr}");
        }

        public static async UniTask<HttpResponse> Logout(CancellationToken token = default)
        {
            return await apiClient.Post<HttpResponse>($"/login/logout");
        }

        public static async UniTask<User> SetUserName(UserNameRequest userNameRequest, string userId, CancellationToken token = default)
        {
            return await apiClient.Put<User>($"/users/{userId}", userNameRequest);
        }

        public static async UniTask<UserStatusResponse> GetUserStatus(string userId, CancellationToken token = default)
        {
            return await apiClient.Put<UserStatusResponse>($"/users/{userId}/status");
        }

        public static async UniTask<TransactionRequest> Quote(QuoteRequest quoteRequest, CancellationToken token = default)
        {
            return await apiClient.Post<TransactionRequest>($"/quotes", quoteRequest);
        }

        public static async UniTask<TransactionResponse> Transact(TransactionRequest transactionRequest, CancellationToken token = default)
        {
            return await apiClient.Post<TransactionResponse>($"/transactions", transactionRequest);
        }
        // More
    }


    // JSON Serializations
    [Serializable]
    public class QuoteRequest
    {
        public string userAddress;
        public int chainId;
        public string contractAddress;
        public string contractFunction;
        public string contractReturn;
        public string[] contractParameters;
        public string txValue;
        public string gasLimit;
    }

    [Serializable]
    public class TransactionRequest : QuoteRequest
    {
        public int timestamp;
        public float baseUSD;
        public float gasUSD;
        public float tokenUSD;
        public float serviceUSD;
        public float totalUSD;
        public string signature;
        public string cardToken;
    }

    [Serializable]
    public class TransactionResponse
    {
        public string txID;
        public string txUrl;
    }

    [Serializable]
    public class LoginPayload
    {
        public string nonce;
    }

    [Serializable]
    public class LoginRequest
    {
        public string nonce;
        public string signature;
        public Fingerprint fingerprint;
    }

    [Serializable]
    public class Fingerprint
    {
        public string visitorId;
        public string requestId;
    }

    [Serializable]
    public class LoginResponse
    {
        public AuthToken authToken;
        public User user;
    }

    [Serializable]
    public class AuthToken
    {
        public string expAt;
        public string issuedAt;
        public string token;
        public RefreshToken refreshToken;
    }

    [Serializable]
    public class RefreshToken
    {
        public string token;
        public string expAt;
    }

    [Serializable]
    public class User
    {
        public string id;
        public string createdAt;
        public string updatedAt;
        public string type;
        public string status;
        public string[] tags;
        public string firstName;
        public string middleName;
        public string lastName;
    }

    [Serializable]
    public class UserNameRequest
    {
        public string walletAddress;
        public string firstName;
        public string middleName;
        public string lastName;
    }

    [Serializable]
    public class UserStatusResponse
    {
        public string status;
    }

}

