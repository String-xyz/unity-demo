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
        StringXYZ.ApiKey = "str.1675dac185674375a69b7fabcd3cabc1";
        LoginPayload payloadToSign = await StringXYZ.RequestLogin("0x44A4b9E2A69d86BA382a511f845CbF2E31286770");
        Debug.Log($"Wallet Login Payload: {payloadToSign.nonce}");

        Fingerprint fingerprint = new Fingerprint();
        fingerprint.requestId = "1671054875232.EcrKjS";
        fingerprint.visitorId = "dle6eqRHxjPEj4H3WLoC";

        LoginRequest login = new LoginRequest();
        login.nonce = "VGhhbmsgeW91IGZvciB1c2luZyBTdHJpbmchIEJ5IHNpZ25pbmcgdGhpcyBtZXNzYWdlIHlvdSBhcmU6CgoxKSBBdXRob3JpemluZyBTdHJpbmcgdG8gaW5pdGlhdGUgb2ZmLWNoYWluIHRyYW5zYWN0aW9ucyBvbiB5b3VyIGJlaGFsZiwgaW5jbHVkaW5nIHlvdXIgYmFuayBhY2NvdW50LCBjcmVkaXQgY2FyZCwgb3IgZGViaXQgY2FyZC4KCjIpIENvbmZpcm1pbmcgdGhhdCB0aGlzIHdhbGxldCBpcyBvd25lZCBieSB5b3UuCgpUaGlzIHJlcXVlc3Qgd2lsbCBub3QgdHJpZ2dlciBhbnkgYmxvY2tjaGFpbiB0cmFuc2FjdGlvbiBvciBjb3N0IGFueSBnYXMuCgpOb25jZTogWjBUVlZVUHNOVDNleGcwZFJSM2pOdDlybGRCS0c4QjFEYjQ2cVF6MGowdHhqdzh1WU1SNlpmaWxlcmV0anZsVnBTekhLdUxveC9yVzNhM25ra0hqZENQdTZXQzVQeEtsNGxMUmJxaldEV1YwWkFXbFdveDBJc3FNZE15Qkw1WT0=";
        login.signature = "0xba89b5ef3ee5f2190940f3e9ca4767b161dcc833dfade27f1fdafbf1527c1e711df8b8a140a4c65a2b8e4a1c40aee11ca17f7c60c648092fa303c9fd00faa8dc00";
        login.fingerprint = fingerprint;
        //login.fingerprint.requestId = "1671054875232.EcrKjS";
        //login.fingerprint.visitorId = "dle6eqRHxjPEj4H3WLoC";

        var response = await StringXYZ.Login(login);
        Debug.Log($"Bad login response = {response}");
    }

}
