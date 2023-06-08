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

public class B_UserTest
{
    [UnityTest]
    public IEnumerator CreateUser() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup(true);
        await Task.Delay(2000);


        var payload = await StringXYZ.RequestLogin(common.playerWallet);
        await Task.Delay(3000);

        var base64decode = Encoding.UTF8.GetString(Convert.FromBase64String(payload.nonce));
        var sig = await common.Sign(base64decode);
        await Task.Delay(2000);

        LoginRequest authentication = new LoginRequest(
            nonce: payload.nonce,
            signature: sig
        );

        var response = await StringXYZ.CreateUser(authentication);
        await Task.Delay(3000);

        Assert.AreNotEqual("", response.authToken.token);
    });

    [UnityTest]
    public IEnumerator RequestEmailAuth() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);

        await common.DoLogin();
        
        var response = await StringXYZ.RequestEmailAuth("fake_unique_email_" + common.playerID + "@example.com", common.playerID);
        await Task.Delay(3000);
        Debug.Log(response.ToString());

        Assert.AreEqual(200, response.status);
    });

    [UnityTest]
    public IEnumerator SetUserName() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);

        await common.DoLogin();

        var newName = new UserNameRequest(
            common.playerWallet,
            "fake",
            "name",
            "test"
        );
        
        var response = await StringXYZ.SetUserName(newName, common.playerID);
        await Task.Delay(3000);

        Assert.AreEqual(newName.firstName, response.firstName);
        Assert.AreEqual(newName.middleName, response.middleName);
        Assert.AreEqual(newName.lastName, response.lastName);
    });

    [UnityTest]
    public IEnumerator GetUserStatus() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup();
        await Task.Delay(2000);

        await common.DoLogin();
        
        var response = await StringXYZ.GetUserStatus(common.playerID);
        await Task.Delay(3000);

        Assert.AreNotEqual("", response.status);
    });

    [UnityTest]
    public IEnumerator PreValidateEmail() => UniTask.ToCoroutine(async () =>
    {
        var common = new StringTest();
        common.Setup(true);
        await Task.Delay(2000);

        await common.DoLogin();
        
        var response = await common.PreValidateEmail("albert123@mailinator.com", common.playerID);
        await Task.Delay(3000);

        Assert.AreNotEqual(0, response.status);
    });
}
