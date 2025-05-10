using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class UserData
{
    public static string username;
    private struct UserCred {
        public string refershToken;
        public string accessToken;
    }
    private static UserCred userCred = new();
    public static Dictionary<string, string> gameStats = new();


    public static void SetAccessToken(string accessToken) {
        userCred.accessToken = accessToken;
    }
    public static void SetRefreshToken(string refershToken)
    {
        userCred.refershToken = refershToken;
    }
    public static string GetAccessToken()
    {
        return userCred.accessToken;
    }
    public static string GetRefreshToken()
    {
        return userCred.refershToken ;
    }

}
