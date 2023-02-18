using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetafabSdk;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using StringSDK;

public class GameLogicBehavior : MonoBehaviour
{
    // These will be passed into our String package
    private string playerWallet;
    private string playerDecryptKey;
    private string stringPlayerID;

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

        // Initialize the string SDK with our API key
        StringXYZ.ApiKey = "str.1675dac185674375a69b7fabcd3cabc1";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void LoginPlayerToString()
    {
        //LoginPayload payloadToSign = await StringXYZ.RequestLogin(playerWallet);
        //Debug.Log($"Wallet Login Payload: {payloadToSign.nonce}");

        // string signedPayload = await Metafab.GenerateSignature(..., payloadToSign);
        // LoginResponse credentials = await StringXYZ.Login(signedPayload);
        // StringXYZ.Authorization = credentials.authToken.token;
        // stringPlayerID = credentials.user.id;

        // TESTING
        LoginPayload payloadToSign = await StringXYZ.RequestLogin("0x44A4b9E2A69d86BA382a511f845CbF2E31286770");
        Debug.Log($"Wallet Login Payload: {payloadToSign}");

        LoginRequest login = new LoginRequest(
            nonce: "VGhhbmsgeW91IGZvciB1c2luZyBTdHJpbmchIEJ5IHNpZ25pbmcgdGhpcyBtZXNzYWdlIHlvdSBhcmU6CgoxKSBBdXRob3JpemluZyBTdHJpbmcgdG8gaW5pdGlhdGUgb2ZmLWNoYWluIHRyYW5zYWN0aW9ucyBvbiB5b3VyIGJlaGFsZiwgaW5jbHVkaW5nIHlvdXIgYmFuayBhY2NvdW50LCBjcmVkaXQgY2FyZCwgb3IgZGViaXQgY2FyZC4KCjIpIENvbmZpcm1pbmcgdGhhdCB0aGlzIHdhbGxldCBpcyBvd25lZCBieSB5b3UuCgpUaGlzIHJlcXVlc3Qgd2lsbCBub3QgdHJpZ2dlciBhbnkgYmxvY2tjaGFpbiB0cmFuc2FjdGlvbiBvciBjb3N0IGFueSBnYXMuCgpOb25jZTogelNVVmxBNkwwc3F2VU50OU5pYVlmVzJyaG5odWVUcWpOelpGdFpxbU14T2ZRb1R2eTlGQnlCcnh2YTBNWDNvTmFTb1dzQ2JzdXRWQWNvUGZWeWFqWVRNT2pQMm1USUJEblRocWQwT2dIUEdodDVEZURtRFJzQ3RrLzFHWW12OD0=",
            signature: "0xb256447369ac45b1b61caf6f9fa93a66a1c184285bf630bdab26cc36dc0fdda45b569ed26b8fff5a19f53417ac149a51fad869a47e6f9d199c2e5cd644d1a51a01",
            visitorId: "dle6eqRHxjPEj4H3WLoC",
            requestId: "1671054875232.EcrKjS"
        );

        var response = await StringXYZ.Login(login);
        Debug.Log($"Login response = {response}");
        StringXYZ.Authorization = response.authToken.token;
    }

    public async void GetQuoteFromString()
    {
        QuoteRequest quoteRequest = new QuoteRequest(
            userAddress: "0x44A4b9E2A69d86BA382a511f845CbF2E31286770",
            chainId: 43113,
            contractAddress: "0x861aF9Ed4fEe884e5c49E9CE444359fe3631418B",
            contractFunction: "mintTo(address)",
            contractReturn: "uint256",
            contractParameters: new string[] { "0x44a4b9E2A69d86BA382a511f845CbF2E31286770" },
            txValue: "0.08 eth",
            gasLimit: "800000");
        var quoteResponse = await StringXYZ.Quote(quoteRequest);
        Debug.Log($"Quote Response: {quoteResponse}");
    }

}
