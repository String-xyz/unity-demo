using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetafabSdk;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using Proyecto26;

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

public class GameLogicBehavior : MonoBehaviour
{
    // These will be passed into our String package
    private string playerWallet;
    private string playerDecryptKey;

    // Start is called before the first frame update
    async UniTaskVoid Start()
    {
        // Simulate Authing the Game with Metafab
        Debug.Log("Authing game with MetaFab...");
        var response = await Metafab.GamesApi.AuthGame(Config.Email, Config.Password, default);
        Debug.Log($"MetaFab Auth Response: {response}");

        // Metafab needs us to copy the response data
        Config.PublishedKey = response.publishedKey;
		Metafab.PublishedKey = Config.PublishedKey;
		Metafab.SecretKey = response.secretKey;
		Metafab.Password = Config.Password;

        // Simulate there being a player
        // get player and authenticate
		Debug.Log($"Getting player...");
		var players = await Metafab.PlayersApi.GetPlayers();
        PublicPlayer player;
        if (players.Count == 0) {
            // Create a player if one doesn't exist
            CreatePlayerRequest createRequest = new("sample-player", "password");
            await Metafab.PlayersApi.CreatePlayer(createRequest);

            players = await Metafab.PlayersApi.GetPlayers();
            player = players[players.Count - 1];
            Debug.Log($"Created first player: {player}");
        } else {
            player = players[players.Count - 1];
            Debug.Log($"Got player: {player}");
        }

        // Simulate that player logging in with their credentials
		Debug.Log($"Authing player...");
		var auth = await Metafab.PlayersApi.AuthPlayer("sample-player", "password");
		Debug.Log($"Got auth {auth}");

        // Store the information we need about the player to be passed into our String package
        // AuthPlayer response will be updated tomorrow, we can get the decrypt key then.
        playerWallet = auth.wallet.address;
        //playerDecryptKey = auth.walletDecryptKey;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Tomorrow this can be updated to fully log in, by requesting the payload, signing it and then storing the JWT

    //async UniTaskVoid GetStringLogin(string walletAddr)
    public void GetStringLogin()
    {
        // We might pass this in instead
        string walletAddr = playerWallet;

        // Set API key
        RestClient.DefaultRequestHeaders["X-Api-Key"] = "str.384be86c18d64b7783c2c4c9132bbd89";

        string base_url = "http://localhost:5555";
        RestClient.Get<LoginPayload>($"{base_url}/login?walletAddress={walletAddr}").Then(response =>
        {
            Debug.Log($"Wallet Login Payload: {response.nonce}");
        });
    }
}
