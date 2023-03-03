using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MetafabSdk;
using Cysharp.Threading.Tasks;
using System;
using System.Linq;
using StringSDK;
using MetaMask.Unity;
using MetaMask.Models;
using System.Text;

public class GameLogicBehavior : MonoBehaviour
{
    // Used to determine where we derive the wallet data from
    private bool usingMetaFab;

    // These Metafab vars will be passed into our String package
    private string playerWallet;
    private string playerWalletID;
    private string playerDecryptKey;

    // Storing data from our String package for convenience
    private string stringPlayerID;
    private TransactionRequest lastQuote;
    private TransactionResponse lastTransaction;

    // Metamask vars, not required
    private MetaMask.MetaMaskWallet metaMaskWallet;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the string SDK with our API key
        StringXYZ.ApiKey = "str.808cdf201d1a4b8189e4b1be1bdedef6";
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnWalletConnected(object sender, EventArgs e)
    {
        Debug.Log($"MetaMask Connected to Player Wallet");
    }

    void OnWalletAuthorized(object sender, EventArgs e)
    {
        Debug.Log($"MetaMask Wallet was Authorized by the Player");
    }

    public void InitializeMetaMask()
    {
        usingMetaFab = false;

        // Init singleton client
        MetaMaskUnity.Instance.Initialize();

        // Get handle to wallet
        metaMaskWallet = MetaMaskUnity.Instance.Wallet;

        // Connect wallet
        metaMaskWallet.Connect();

        // Subscribe our handler to wallet connection event
        metaMaskWallet.WalletConnected += OnWalletConnected;

        // Subscribe our handler to wallet authorization event
        metaMaskWallet.WalletAuthorized += OnWalletAuthorized;
    }

    public async UniTaskVoid InitializeMetaFab()
    {
        usingMetaFab = true;

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
        if (players.Count == 0)
        {
            // Create a player if one doesn't exist
            CreatePlayerRequest createRequest = new("sample-player", "password");
            await Metafab.PlayersApi.CreatePlayer(createRequest, 0);

            players = await Metafab.PlayersApi.GetPlayers();
            player = players[players.Count - 1];
            Debug.Log($"Created first player: {player}");
        }
        else
        {
            player = players[players.Count - 1];
            Debug.Log($"Got player: {player}");
        }

        // Simulate that player logging in with their credentials
        Debug.Log($"Authing player...");
        var auth = await Metafab.PlayersApi.AuthPlayer("sample-player", "password", 0);
        Debug.Log($"Got auth {auth}");

        // Store the information we need about the player to be passed into our String package
        // AuthPlayer response will be updated tomorrow, we can get the decrypt key then.
        playerWallet = auth.wallet.address;
        playerWalletID = auth.wallet.id;
        playerDecryptKey = auth.walletDecryptKey;
    }

    public async void LoginPlayerToString()
    {
        string walletAddr;
        if (usingMetaFab)
        {
            walletAddr = playerWallet;
        }
        else
        {
            walletAddr = metaMaskWallet.SelectedAddress;
        }

        // Real

        //LoginPayload payloadToSign = await StringXYZ.RequestLogin(playerWallet);
        //Debug.Log($"Wallet Login Payload: {payloadToSign}");

        //////Metafab.PlayerDecryptKey = playerDecryptKey; // This doesn't work yet.
        //Metafab.Password = "password"; // Temporary hack to get around SDK missing header
        //CreateWalletSignatureRequest signatureRequest = new(payloadToSign.nonce);
        //string signedPayload = await Metafab.WalletsApi.CreateWalletSignature(playerWalletID, signatureRequest);

        //LoginRequest login = new LoginRequest(
        //    nonce: payloadToSign.nonce,
        //    signature: signedPayload,
        //    visitorId: "dle6eqRHxjPEj4H3WLoC",
        //    requestId: "1671054875232.EcrKjS"
        //);

        //var response = await StringXYZ.Login(login);
        //Debug.Log($"Login response = {response}");
        //StringXYZ.Authorization = response.authToken.token;

        // TESTING
        LoginPayload payloadToSign = await StringXYZ.RequestLogin(walletAddr);
        Debug.Log($"Wallet Login Payload: {payloadToSign}");

        var base64decode = Encoding.UTF8.GetString(Convert.FromBase64String(payloadToSign.nonce));


        string sig = "";
        if (usingMetaFab)
        {
            sig = "forget about it";
        }
        else
        {
            var parameters = new string[] { walletAddr, base64decode };

            var request = new MetaMaskEthereumRequest
            {
                Method = "personal_sign",
                Parameters = parameters
            };
            var res = await metaMaskWallet.Request(request);
            Debug.Log($"SIGNATURE RESPONSE = {res}");
            sig = res.GetString();
        }

        LoginRequest login = new LoginRequest(
            nonce: payloadToSign.nonce,
            signature: sig,
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
            chainID: 43113,
            contractAddress: "0x861aF9Ed4fEe884e5c49E9CE444359fe3631418B",
            contractFunction: "mintTo(address)",
            contractReturn: "uint256",
            contractParameters: new string[] { "0x44a4b9E2A69d86BA382a511f845CbF2E31286770" },
            txValue: "0.08 eth",
            gasLimit: "800000");
        var quoteResponse = await StringXYZ.Quote(quoteRequest);
        Debug.Log($"Quote Response: {quoteResponse}");
        lastQuote = quoteResponse;
    }

    public async void ExecuteLastQuote()
    {
        var txResponse = await StringXYZ.Transact(lastQuote);
        Debug.Log($"TX Response: {txResponse}");
        lastTransaction = txResponse;
    }

}
