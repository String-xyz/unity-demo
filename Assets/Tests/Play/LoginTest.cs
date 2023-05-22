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

public class LoginTest
{
    [UnityTest]
    public IEnumerator RequestLogin() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);

        var result = await StringXYZ.RequestLogin(common.playerWallet);
        await Task.Delay(3000);

        Assert.AreNotEqual("", result.nonce);
    });

    [UnityTest]
    public IEnumerator Login() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);


        var payload = await StringXYZ.RequestLogin(common.playerWallet);
        await Task.Delay(3000);

        var base64decode = Encoding.UTF8.GetString(Convert.FromBase64String(payload.nonce));
        var sig = await common.Sign(base64decode);
        await Task.Delay(2000);

        LoginRequest login = new LoginRequest(
            nonce: payload.nonce,
            signature: sig
        );

        var response = await StringXYZ.Login(login, true); // Bypass device for 
        await Task.Delay(3000);

        Assert.AreNotEqual("", response.authToken.token);
    });

    [UnityTest]
    public IEnumerator Logout() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);

        var payload = await StringXYZ.RequestLogin(common.playerWallet);
        await Task.Delay(3000);

        var base64decode = Encoding.UTF8.GetString(Convert.FromBase64String(payload.nonce));
        var sig = await common.Sign(base64decode);
        await Task.Delay(2000);

        LoginRequest login = new LoginRequest(
            nonce: payload.nonce,
            signature: sig
        );

        var response = await StringXYZ.Login(login, true); // Bypass device for 
        await Task.Delay(3000);

        Assert.AreNotEqual("", response.authToken.token);

        var result = await StringXYZ.Logout();
        await Task.Delay(5000);
        Debug.Log(result);

        Assert.AreEqual(204, result.status);
    });
}