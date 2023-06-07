using System;
using StringSDK;
using MetafabSdk;
using Cysharp.Threading.Tasks;
using System.Text;
using UnityEngine;
using UnityEngine.TestTools;

public class StringTest
{
    // These Metafab vars will be passed into our String package
    public string playerWallet;
    public string playerWalletID;

    public string playerID; // String ID for player
    public User stringUser;

    public StringTest()
    {
    }

    public void Setup(bool newPlayer = false)
    {
        // Initialize the string SDK with our API key
        StringXYZ.ApiKey = "str.069b1d4c777c4606af3bf893e49ecf6a";
        StringXYZ.Env = "http://localhost:5555";

        InitMetafab(newPlayer);
    }

    public async void InitMetafab(bool newPlayer)
    {
        var response = await Metafab.GamesApi.AuthGame(MetafabSdk.Config.Email, MetafabSdk.Config.Password, default);

        MetafabSdk.Config.PublishedKey = response.publishedKey;
        Metafab.PublishedKey = MetafabSdk.Config.PublishedKey;
        Metafab.SecretKey = response.secretKey;
        Metafab.Password = MetafabSdk.Config.Password;

        var players = await Metafab.PlayersApi.GetPlayers();
        var testName = "sample-player";
        if (players.Count > 0 && newPlayer)
        {
            testName += "-" + players.Count.ToString();
        }
        PublicPlayer player;
        if (players.Count == 0 || newPlayer)
        {
            // Create a player if one doesn't exist
            CreatePlayerRequest createRequest = new(testName, "password");
            await Metafab.PlayersApi.CreatePlayer(createRequest, 0);

            players = await Metafab.PlayersApi.GetPlayers();
            player = players[players.Count - 1];
        }
        else
        {
            player = players[players.Count - 1];
        }

        var auth = await Metafab.PlayersApi.AuthPlayer(testName, "password", 0);

        // Store the information we need about the player to be passed into our String package
        // AuthPlayer response will be updated tomorrow, we can get the decrypt key then.
        playerWallet = auth.wallet.address;
        playerWalletID = auth.wallet.id;

        Metafab.WalletDecryptKey = auth.walletDecryptKey;
    }

    public async UniTask<string> Sign(string payload)
    {
        var request = new CreateWalletSignatureRequest(payload);
        var res = await Metafab.WalletsApi.CreateWalletSignature(playerWalletID, request);
        return res.signature;
    }

    public async UniTask<int> DoLogin()
    {
        var payload = await StringXYZ.RequestLogin(playerWallet);

        var base64decode = Encoding.UTF8.GetString(Convert.FromBase64String(payload.nonce));
        var sig = await Sign(base64decode);

        LoginRequest authentication = new LoginRequest(
            nonce: payload.nonce,
            signature: sig
        );

        var response = await StringXYZ.Login(authentication, true);

        StringXYZ.Authorization = response.authToken.token;
        playerID = response.user.id;
        stringUser = response.user;

        return 0;
    }

    public async UniTask<int> VerifyEmail()
    {
        if (stringUser.status == "unverified")
        {
            await StringXYZ.PreValidateEmail("albert123@mailinator.com", playerID);
        }

        return 0;
    }
}
