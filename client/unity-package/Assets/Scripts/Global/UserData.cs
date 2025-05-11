using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class UserData
{
    public static string username;
    public struct GameStat {
        public int playcount;
        public string fastestEndTime;
        public int maxHp;
        public int maxAtk;
        public int maxSpeed;
    } 
    
    private struct UserCred {
        public string refershToken ;
        public string accessToken ;
    }
    private static UserCred userCred = new();
    public static  GameStat gameStat= new();
    //public static Dictionary<string, string> gameStats = new();
    public static bool Activated { get; private set; }

    public static void Activate() {
        Activated = true;
    }

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
