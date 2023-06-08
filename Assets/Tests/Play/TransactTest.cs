using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using StringSDK;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using System.Text;
using System;

public class D_TransactTest
{
    [UnityTest]
    public IEnumerator Transact() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);

        await common.DoLogin();
        
        TransactionRequest requestQuote = new TransactionRequest(
            userAddress: common.playerWallet,
            assetName: "String Avalanche NFT",
            chainId: 43113,
            contractAddress: "0xea1ffe2cf6630a20e1ba397e95358daf362c8781",
            contractFunction: "mintTo(address)",
            contractReturn: "uint256",
            contractParameters: new string[] { common.playerWallet },
            txValue: "0.08 eth",
            gasLimit: "800000");

        var quote = await StringXYZ.Quote(requestQuote);
        await Task.Delay(5000);

        ExecutionRequest request = new ExecutionRequest(
            quote: quote,
            paymentInfo: new PaymentInfo()
        );
        var response = await StringXYZ.Transact(request);
        await Task.Delay(5000);

        Assert.AreNotEqual("", response.txId);
    });
}
