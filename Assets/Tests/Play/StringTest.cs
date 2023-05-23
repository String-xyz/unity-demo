using System;
using StringSDK;
using MetafabSdk;
using Cysharp.Threading.Tasks;

public class StringTest
{
    // These Metafab vars will be passed into our String package
    public string playerWallet;
    public string playerWalletID;

    public StringTest()
    {
    }

    public void Setup()
    {
        // Initialize the string SDK with our API key
        StringXYZ.ApiKey = "str.953506363c8540b5acd517bd4982fa87";
        StringXYZ.Env = "http://localhost:5555";

        InitMetafab();
    }

    public async void InitMetafab()
    {
        var response = await Metafab.GamesApi.AuthGame(MetafabSdk.Config.Email, MetafabSdk.Config.Password, default);

        MetafabSdk.Config.PublishedKey = response.publishedKey;
        Metafab.PublishedKey = MetafabSdk.Config.PublishedKey;
        Metafab.SecretKey = response.secretKey;
        Metafab.Password = MetafabSdk.Config.Password;

        var players = await Metafab.PlayersApi.GetPlayers();
        PublicPlayer player;
        if (players.Count == 0)
        {
            // Create a player if one doesn't exist
            CreatePlayerRequest createRequest = new("sample-player", "password");
            await Metafab.PlayersApi.CreatePlayer(createRequest, 0);

            players = await Metafab.PlayersApi.GetPlayers();
            player = players[players.Count - 1];
        }
        else
        {
            player = players[players.Count - 1];
        }

        var auth = await Metafab.PlayersApi.AuthPlayer("sample-player", "password", 0);

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
}
