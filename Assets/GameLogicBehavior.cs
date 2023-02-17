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
        StringXYZ.ApiKey = "str.384be86c18d64b7783c2c4c9132bbd89";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void LoginPlayerToString()
    {
        StringSDK.LoginPayload payloadToSign = await StringXYZ.GetStringLogin(playerWallet);
        Debug.Log($"Wallet Login Payload: {payloadToSign.nonce}");

        //string signedPayload = Metafab.GenerateSignature(..., payloadToSign);
        //stringSDK.Login(signedPayload);
    }

}
