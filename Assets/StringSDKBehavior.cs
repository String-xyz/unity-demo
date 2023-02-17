using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetafabSdk;
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
        static string apiKey;
        static ApiClient apiClient;

        public static string ApiKey
        {
            get => apiClient.headers.ContainsKey("X-Api-Key") ? apiClient.headers["X-Api-Key"] : null;
            set => apiClient.headers["X-Api-Key"] = value;
        }

        static StringXYZ()
        {
            apiClient = new ApiClient(Constants.api_base_url);
        }

        public static async UniTask<LoginPayload> GetStringLogin(string walletAddr, CancellationToken token = default)
        {
            return await apiClient.Get<LoginPayload>($"/login?walletAddress={walletAddr}");
        }

        public static async UniTask<LoginResponse> StringLogin(LoginRequest loginRequest, CancellationToken token = default)
        {
            //return await apiClient.Get<LoginPayload>($"/login?walletAddress={walletAddr}");
            return await apiClient.Post<LoginResponse>($"/login/sign", loginRequest);
        }
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
    }


}

