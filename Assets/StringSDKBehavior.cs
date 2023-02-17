using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetafabSdk;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;
using System.Linq;
using Proyecto26;
using RSG;

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
}

