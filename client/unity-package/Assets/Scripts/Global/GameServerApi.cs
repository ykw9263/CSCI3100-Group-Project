using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

public static class GameServerApi
{

    public class ServerRequestPayload
    {
        public string method;
#nullable enable
        public string? username;
        public string? email;
        public string? pwd;
        public string? newpwd;
        public string? vericode;
        public string? accessToken;
        public string? refreshToken;
        public string? licenseKey;
#nullable disable
    }

    public class ServerResponse
    {
        public string message;
#nullable enable
        public long? statusCode;
        public string? username;
        public string? accessToken;
        public string? refreshToken;
        public string? activated;
#nullable disable

    }


    const string SERVER_HOST = "http://localhost:8000";

    static void Example() {
        // VerifyEmail("User123", "email@mail.com", VerifyEmailCallback);
        // VerifyCode("User123", "123456", VerifyCodeCallback); // get access token
        // Register("User123", "Password123", "email@mail.com", "accessToken", RegisterCallback);
        // Login("User123", "Password123", LoginCallback);
    }
    static void ExampleUseInExternalClass() {
        // In Unity Scene put GameServerApi script in a gameobj (we call it GameObj_A)

        // In other script, add class field:
        // [SerializeField] GameServerApi gameServerApi;
        // drag and drop the GameObj_A into the script field in Unity

        // calling function
        // 
        // // this will be called after the request has been processed.
        // void SomeCallback(GameServerApi.ServerResponse resObj, bool result)
        // {
        //    // Do something
        // }
        // // send the request
        // gameServerApi.Login("User123", "Password123", SomeCallback);

    }


    static public IEnumerator GetUpstreamVersion()
    {
        Debug.Log("sending getver");
        using (UnityWebRequest res = UnityWebRequest.Get(SERVER_HOST + "/version"))
        {
            yield return res.SendWebRequest();

            if (res.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("getver success" +res.downloadHandler.text);
            }
            else
            {
                
                Debug.Log("getver errored: " + res.error);
            }
        }
    }

    static private IEnumerator PostMessage(string path, ServerRequestPayload reqObj, System.Action<ServerResponse, bool> callback = null, bool tryRefresah = true) {

        string reqjson = JsonUtility.ToJson(reqObj);

        Debug.Log("Request payload: " + reqjson);
        ;
        using (UnityWebRequest res = UnityWebRequest.Post(SERVER_HOST + path, reqjson, "application/json; charset=utf-8"))
        {
            yield return res.SendWebRequest();
            ServerResponse resobj = null;
            bool isSuccess = res.result == UnityWebRequest.Result.Success;
            string jsontext = res.downloadHandler.text;
            Debug.Log("Server responsed: "+jsontext);
            if (jsontext.Length>0) {
                resobj = JsonUtility.FromJson<ServerResponse>(jsontext);
                resobj.statusCode = res.responseCode;
            }
            if (resobj?.statusCode == 403 && tryRefresah && UserData.GetRefreshToken()?.Length>0)
            {
                // try to refresh access token;
                string refreshjson = $"{{" +
                    $"\"method\": \"refresh\", " +
                    $"\"username\": \"{reqObj.username}\", " +
                    $"\"refreshToken\": \"{UserData.GetRefreshToken()}\"" +
                    $"}}";
                
                UnityWebRequest refreshRes = UnityWebRequest.Post(SERVER_HOST + "/auth", refreshjson, "application/json; charset=utf-8");
                yield return refreshRes.SendWebRequest();

                if (refreshRes.result == UnityWebRequest.Result.Success) {
                    ServerResponse refreshobj = JsonUtility.FromJson<ServerResponse>(refreshRes.downloadHandler.text);
                    UserData.SetAccessToken(refreshobj.accessToken);
                    reqObj.accessToken = refreshobj.accessToken;
                    yield return PostMessage(path, reqObj, callback, false);
                }
                else
                {
                    UserData.SetRefreshToken("");
                    callback?.Invoke(resobj, isSuccess);
                }


            }
            else
            {
                callback?.Invoke(resobj, isSuccess);
            }


        }
    }

    /**
     *  Request registration by verifing the email address. Sends one-time-password to email.
     */
    static public IEnumerator VerifyEmail(string username, string email, System.Action<ServerResponse, bool> VerifyEmailCallback) {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "verifyEmail";
        reqObj.email = email;
        reqObj.username = username;

        yield return PostMessage("/account", reqObj, VerifyEmailCallback);
    }


    /**
     *  Finish registration
     *  Requires an registration access token
     */
    static public IEnumerator Register(string username, string pwd, string email, string accessToken, System.Action<ServerResponse, bool> RegisterCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "register";
        reqObj.username = username;
        reqObj.email = email;
        reqObj.pwd = pwd;
        reqObj.accessToken = accessToken;

        yield return PostMessage("/account", reqObj, RegisterCallback);
    }


    /**
     *  Reset user's password
     *  Requires an access token
     */
    static public IEnumerator ResetPW(string username, string pwd, string newpwd, string accessToken, System.Action<ServerResponse, bool> ResetPWCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "reset";
        reqObj.username = username;
        reqObj.pwd = pwd;
        reqObj.newpwd = newpwd;
        reqObj.accessToken = accessToken;

        yield return PostMessage("/account", reqObj, ResetPWCallback);
    }

    /**
     *  Request account recovery. Sends an email containing a one-time-password 
     */
    static public IEnumerator RequestRestore(string username, string email, System.Action<ServerResponse, bool> RequestRestoreCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "requestRestore";
        reqObj.username = username;
        reqObj.email = email;

        yield return PostMessage("/account", reqObj, RequestRestoreCallback);
    }


    /**
     *  Finish Account Restoration 
     *  Requires a restoration access token
     */
    static public IEnumerator FinishRestore(string username, string newpwd, string accessToken, System.Action<ServerResponse, bool> FinishRestoreCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "finishRestore";
        reqObj.username = username;
        reqObj.newpwd = newpwd;
        reqObj.accessToken = accessToken;

        yield return PostMessage("/account", reqObj, FinishRestoreCallback);
    }

    /**
     *  Activate user with a license key
     */
    static public IEnumerator Activate(string username, string newpwd, string accessToken, System.Action<ServerResponse, bool> ActivateCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "activate";
        reqObj.username = username;
        reqObj.licenseKey = newpwd;
        reqObj.accessToken = accessToken;

        yield return PostMessage("/account", reqObj, ActivateCallback);
    }

    /**
     *  Verify one-time-password
     *  Response contains an Access Token used for registration/restoration
     */
    static public IEnumerator VerifyCode(string username, string vericode, System.Action<ServerResponse, bool> VerifyCodeCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "verifycode";
        reqObj.username = username;
        reqObj.vericode = vericode;

        yield return PostMessage("/auth", reqObj, VerifyCodeCallback);
    }

    /**
     *  Refresh access token with refresh token
     *  Response contains a new Access Token
     */
    static public IEnumerator RefreshSession(string username, string refreshToken, System.Action<ServerResponse, bool> RefreshSessionCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "refresh";
        reqObj.username = username;
        reqObj.refreshToken = refreshToken;

        yield return PostMessage("/auth", reqObj, RefreshSessionCallback);
    }

    /**
     *  Login with username and password
     *  Response contains a new access token and a refresh token
     */
    static public IEnumerator Login(string username, string pwd, System.Action<ServerResponse, bool> LoginCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "login";
        reqObj.username = username;
        reqObj.pwd = pwd;

        yield return PostMessage("/auth", reqObj, LoginCallback);
    }

    /**
     *  Logout with username and refresh token
     *  Response contains a new access token and a refresh token
     */
    static public IEnumerator Logout(string username, string refreshToken, System.Action<ServerResponse, bool> LogoutCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "logout";
        reqObj.username = username;
        reqObj.refreshToken = refreshToken;

        yield return PostMessage("/auth", reqObj, LogoutCallback);
    }
}
