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

public class E_CardsTest
{
    [UnityTest]
    public IEnumerator GetCards() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);

        await common.DoLogin();
        await Task.Delay(2000);
        await common.VerifyEmail();
        await Task.Delay(2000);
        
        var response = await StringXYZ.GetCards();
        await Task.Delay(2000);

        Debug.Log(response.ToString());

        // Assert.AreNotEqual("", response.txId);
    });
}
