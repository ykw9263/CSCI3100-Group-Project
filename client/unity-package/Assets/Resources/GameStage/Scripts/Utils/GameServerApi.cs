using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Networking;

public class GameServerApi : MonoBehaviour
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
#nullable disable

    }


    private string active_accessToken = null;
    private string active_refreshToken = null;

    const string SERVER_HOST = "http://localhost:8000";

    void Example() {
        VerifyEmail("User123", "email@mail.com", VerifyEmailCallback);
        VerifyCode("User123", "123456", VerifyCodeCallback); // get access token
        Register("User123", "Password123", "email@mail.com", "accessToken", RegisterCallback);
        Login("User123", "Password123", LoginCallback);
    }


    private void Start()
    {
        
    }

    private void Update()
    {
        
    }

    public IEnumerator GetUpstreamVersion()
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

    private IEnumerator PostMessage(string path,ServerRequestPayload reqObj, System.Action<ServerResponse, bool> callback = null) {

        string reqjson = JsonUtility.ToJson(reqObj);

        Debug.Log("VerifyEmail payload: " + reqjson);
        ;
        using (UnityWebRequest res = UnityWebRequest.Post(SERVER_HOST + path, reqjson, "application/json; charset=utf-8"))
        {
            yield return res.SendWebRequest();
            ServerResponse resobj = null;
            bool isSuccess = res.result == UnityWebRequest.Result.Success;
            if (isSuccess)
            {
                string jsontext = res.downloadHandler.text;
                resobj = JsonUtility.FromJson<ServerResponse>(jsontext);
                resobj.statusCode = res.responseCode;
                Debug.Log("VerifyEmail success" + resobj.message);
            }
            else
            {
                Debug.Log("VerifyEmail errored: " + res.error);
            }
            callback?.Invoke(resobj, isSuccess);

        }
    }

    /**
     *  Request registration by verifing the email address. Sends one-time-password to email.
     */
    public void VerifyEmail(string username, string email, System.Action<ServerResponse, bool> VerifyEmailCallback) {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "verifyEmail";
        reqObj.email = email;
        reqObj.username = username;

        StartCoroutine(PostMessage("/account", reqObj, VerifyEmailCallback));
    }


    /**
     *  Finish registration
     *  Requires an registration access token
     */
    public void Register(string username, string pwd, string email, string accessToken, System.Action<ServerResponse, bool> RegisterCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "register";
        reqObj.username = username;
        reqObj.email = email;
        reqObj.pwd = pwd;
        reqObj.accessToken = accessToken;

        StartCoroutine(PostMessage("/account", reqObj, RegisterCallback));
    }


    /**
     *  Reset user's password
     *  Requires an access token
     */
    public void ResetPW(string username, string pwd, string newpwd, string accessToken, System.Action<ServerResponse, bool> ResetPWCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "reset";
        reqObj.username = username;
        reqObj.pwd = pwd;
        reqObj.pwd = newpwd;
        reqObj.accessToken = accessToken;

        StartCoroutine(PostMessage("/account", reqObj, ResetPWCallback));
    }

    /**
     *  Request account recovery. Sends an email containing a one-time-password 
     */
    public void RequestRestore(string username, string email, System.Action<ServerResponse, bool> RequestRestoreCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "requestRestore";
        reqObj.username = username;
        reqObj.pwd = email;

        StartCoroutine(PostMessage("/account", reqObj, RequestRestoreCallback));
    }


    /**
     *  Finish Account Restoration 
     *  Requires a restoration access token
     */
    public void FinishRestore(string username, string newpwd, string accessToken, System.Action<ServerResponse, bool> FinishRestoreCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "finishRestore";
        reqObj.username = username;
        reqObj.pwd = newpwd;
        reqObj.accessToken = accessToken;

        StartCoroutine(PostMessage("/account", reqObj, FinishRestoreCallback));
    }

    /**
     *  Activate user with a license key
     */
    public void Activate(string username, string newpwd, string accessToken, System.Action<ServerResponse, bool> ActivateCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "activate";
        reqObj.username = username;
        reqObj.pwd = newpwd;
        reqObj.accessToken = accessToken;

        StartCoroutine(PostMessage("/account", reqObj, ActivateCallback));
    }

    /**
     *  Verify one-time-password
     *  Response contains an Access Token used for registration/restoration
     */
    public void VerifyCode(string username, string vericode, System.Action<ServerResponse, bool> VerifyCodeCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "verifyEmail";
        reqObj.username = username;
        reqObj.vericode = vericode;

        StartCoroutine(PostMessage("/auth", reqObj, VerifyCodeCallback));
    }

    /**
     *  Refresh access token with refresh token
     *  Response contains a new Access Token
     */
    public void RefreshSession(string username, string refreshToken, System.Action<ServerResponse, bool> RefreshSessionCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "verifyEmail";
        reqObj.username = username;
        reqObj.refreshToken = refreshToken;

        StartCoroutine(PostMessage("/auth", reqObj, RefreshSessionCallback));
    }

    /**
     *  Login with username and password
     *  Response contains a new access token and a refresh token
     */
    public void Login(string username, string pwd, System.Action<ServerResponse, bool> LoginCallback)
    {
        ServerRequestPayload reqObj = new ServerRequestPayload();
        reqObj.method = "verifyEmail";
        reqObj.username = username;
        reqObj.pwd = pwd;

        StartCoroutine(PostMessage("/auth", reqObj, LoginCallback));
    }



    // TODO: Define behaivour according to server response
    // callback can be defined in other scripts. Make sure to track access and refresh tokens

    public void VerifyEmailCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);
    }

    public void RegisterCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);
    }


    public void ResetPWCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);
    }

    public void RequestRestoreCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);
    }

    public void FinishRestoreCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);
    }



    public void ActivateCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);
        if (result)
        {
            // TODO: 
        }
        else
        {
            // TODO: 
        }
    }

    public void VerifyCodeCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);
        if (result)
        {
            active_accessToken = resobj.accessToken;
            // TODO: next UI
        }
        else
        {
            // TODO: Wrong Code
        }
    }


    public void RefreshSessionCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);

        if (result) { active_accessToken = resobj.accessToken; }
        else
        {
            active_accessToken = null;
            if (resobj.statusCode == 422)
            {
                active_refreshToken = null;
            }
        }

    }
    public void LoginCallback(ServerResponse resobj, bool result)
    {
        Debug.Log("EmailCallback: " + resobj.message);
        if (result) { 
            active_accessToken = resobj.accessToken; 
            active_refreshToken= resobj.refreshToken;
        }
        else
        {
            // TODO: 
        }
    }
}
