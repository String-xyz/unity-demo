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

public class C_QuoteTest
{
    [UnityTest]
    public IEnumerator Quote() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);

        await common.DoLogin();
        
        TransactionRequest request = new TransactionRequest(
            userAddress: common.playerWallet,
            assetName: "String Avalanche NFT",
            chainId: 43113,
            contractAddress: "0xea1ffe2cf6630a20e1ba397e95358daf362c8781",
            contractFunction: "mintTo(address)",
            contractReturn: "uint256",
            contractParameters: new string[] { common.playerWallet },
            txValue: "0.08 eth",
            gasLimit: "800000");

        var response = await StringXYZ.Quote(request);
        await Task.Delay(5000);

        Assert.AreNotEqual("", response.signature);
    });
}
