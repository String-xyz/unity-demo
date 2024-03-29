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
    // Used to activate / deactivate buttons
    public UnityEngine.UI.Button buttonInitMetaMask;
    public UnityEngine.UI.Button buttonInitMetaFab;
    public UnityEngine.UI.Button buttonLogin;
    public UnityEngine.UI.Button buttonGetQuote;
    public UnityEngine.UI.Button buttonExecute;
    public UnityEngine.UI.Button buttonSubmitUserData;

    // Used to input Player Name and Email
    public TMPro.TMP_InputField inputFirstName;
    public TMPro.TMP_InputField inputMiddleName;
    public TMPro.TMP_InputField inputLastName;
    public TMPro.TMP_InputField inputEmail;

    // Message box
    public TMPro.TextMeshProUGUI messageBox;

    // Used to determine where we derive the wallet data from
    private bool usingMetaFab;

    // These Metafab vars will be passed into our String package
    private string playerWallet;
    private string playerWalletID;

    // Storing data from our String package for convenience
    private string stringPlayerID;
    private Quote lastQuote;
    private TransactionResponse lastTransaction;

    // Metamask vars, not required
    private MetaMask.MetaMaskWallet metaMaskWallet;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the string SDK with our API key
        StringXYZ.ApiKey = "str.069b1d4c777c4606af3bf893e49ecf6a";
        StringXYZ.Env = "http://localhost:5555";

        // Disable buttons we shouldn't press yet
        buttonLogin.interactable = false;
        buttonGetQuote.interactable = false;
        buttonExecute.interactable = false;

        buttonSubmitUserData.interactable = false;
        inputFirstName.interactable = false;
        inputMiddleName.interactable = false;
        inputLastName.interactable = false;
        inputEmail.interactable = false;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnWalletConnected(object sender, EventArgs e)
    {
        Debug.Log($"MetaMask Connected to Player Wallet");
        messageBox.SetText("Wallet connected!  You will receive a confirmation dialog on your phone to authorize this application.  Please accept it to continue.");
    }

    void OnWalletAuthorized(object sender, EventArgs e)
    {
        // Allow next step
        buttonLogin.interactable = true;

        Debug.Log($"MetaMask Wallet was Authorized by the Player");
        messageBox.SetText("Application was authorized by your MetaMask wallet! You may now Log In to StringPay using the button below.  If you are new to StringPay, an account will automatically be created when you log in.");
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

        messageBox.SetText("MetaMask has been initialized, please open the application on your phone and scan the QR code to continue.");
    }

    public async void InitializeMetaFab()
    {
        usingMetaFab = true;

        // Simulate Authing the Game with Metafab
        Debug.Log("Authing game with MetaFab...");
        var response = await Metafab.GamesApi.AuthGame(MetafabSdk.Config.Email, MetafabSdk.Config.Password, default);
        Debug.Log($"MetaFab Auth Response: {response}");

        // Metafab needs us to copy the response data
        MetafabSdk.Config.PublishedKey = response.publishedKey;
        Metafab.PublishedKey = MetafabSdk.Config.PublishedKey;
        Metafab.SecretKey = response.secretKey;
        Metafab.Password = MetafabSdk.Config.Password;

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

        Metafab.WalletDecryptKey = auth.walletDecryptKey;

        // Allow next step
        buttonLogin.interactable = true;
        messageBox.SetText("MetaFab was initialized, you may now Log In to StringPay using the button below. If you are new to StringPay, an account will automatically be created when you log in.");
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
            messageBox.SetText("Please confirm on the MetaMask app that you authorize your login to StringPay.");
            walletAddr = metaMaskWallet.SelectedAddress;
        }

        LoginPayload payloadToSign = await StringXYZ.RequestLogin(walletAddr);
        Debug.Log($"Wallet Login Payload: {payloadToSign}");

        var base64decode = Encoding.UTF8.GetString(Convert.FromBase64String(payloadToSign.nonce));


        string sig = "";
        if (usingMetaFab)
        {
            var request = new CreateWalletSignatureRequest(base64decode);
            try
            {
                var res = await Metafab.WalletsApi.CreateWalletSignature(playerWalletID, request);
                sig = res.signature;
            }
            catch (Exception e)
            {
                Debug.Log($"ERROR = {e}");
                return;
            }
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
            sig = res.GetString();
        }
        Debug.Log($"SIGNATURE RESPONSE = {sig}");


        LoginRequest login = new LoginRequest(
            nonce: payloadToSign.nonce,
            signature: sig
        );

        var response = await StringXYZ.Login(login);
        Debug.Log($"Login response = {response}");

        StringXYZ.Authorization = response.authToken.token;
        stringPlayerID = response.user.id;
        if (response.user.status == "unverified")
        {
            VerifyUser();
            return;
        }

        PopulateUserData(response.user);

        buttonGetQuote.interactable = true;
        messageBox.SetText("You are now logged in to StringPay!  You may now request a Quote using the button below.");
    }

    public async void GetQuoteFromString()
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

        TransactionRequest transactionRequest = new TransactionRequest(
            userAddress: walletAddr,
            assetName: "String Avalanche NFT",
            chainId: 43113,
            contractAddress: "0xea1ffe2cf6630a20e1ba397e95358daf362c8781",
            contractFunction: "mintTo(address)",
            contractReturn: "uint256",
            contractParameters: new string[] { walletAddr },
            txValue: "0.08 eth",
            gasLimit: "800000");
        var quoteResponse = await StringXYZ.Quote(transactionRequest);
        Debug.Log($"Quote Response: {quoteResponse}");
        lastQuote = quoteResponse;
        var estimate = lastQuote.estimate;

        buttonExecute.interactable = true;
        var quoteString = "\nBase Cost: $" + estimate.baseUSD +
                          "\nToken Cost: $" + estimate.tokenUSD +
                          "\nGas Cost: $" + estimate.gasUSD +
                          "\nService Cost: $" + estimate.serviceUSD +
                          "\nTotal Cost: $" + estimate.totalUSD;
        messageBox.SetText("Got quote: " + quoteString + "\nThis quote expires in 15 seconds.  You may have StringPay execute the transaction using the button below.");
    }

    public async void ExecuteLastQuote()
    {
        ExecutionRequest executionRequest = new ExecutionRequest(
            quote: lastQuote,
            paymentInfo: new PaymentInfo()
        );
        if (StringXYZ.ReadyForPayment())
        {
            executionRequest.paymentInfo.cardToken = StringXYZ.GetPaymentToken();
        }
        else
        {
            Debug.Log("Payment Token Not Available");
            return;
        }
        buttonExecute.interactable = false;
        var txResponse = await StringXYZ.Transact(executionRequest);
        Debug.Log($"TX Response: {txResponse}");
        lastTransaction = txResponse;
        messageBox.SetText("Transaction: [" + lastTransaction.txUrl + "].  Thank you for using StringPay!");
    }

    private void VerifyUser()
    {
        messageBox.SetText("Welcome to StringPay!  Please provide your Name and Email in the fields below and press 'Submit'");
        buttonSubmitUserData.interactable = true;
        inputFirstName.interactable = true;
        inputMiddleName.interactable = true;
        inputLastName.interactable = true;
        inputEmail.interactable = true;
    }

    private void PopulateUserData(User user)
    {
        inputFirstName.text = user.firstName;
        inputMiddleName.text = user.middleName;
        inputLastName.text = user.lastName;
        inputEmail.text = user.status;
    }

    public async void SubmitNameAndEmail()
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

        var usernameRequest = new UserNameRequest(walletAddr, inputFirstName.text, inputMiddleName.text, inputLastName.text);
        var user = await StringXYZ.SetUserName(usernameRequest, stringPlayerID);

        messageBox.SetText("Please check your email inbox for a verification from StringPay and click the link provided to continue!");

        buttonSubmitUserData.interactable = false;

        await StringXYZ.RequestEmailAuth(inputEmail.text, stringPlayerID);

        inputFirstName.interactable = false;
        inputMiddleName.interactable = false;
        inputLastName.interactable = false;
        inputEmail.interactable = false;
        buttonGetQuote.interactable = true;
    }

    public void SubmitCard()
    {
        WebEventManager.SubmitCard();
    }

    public void Msg(string msg)
    {
        messageBox.SetText(msg);
    }
}
