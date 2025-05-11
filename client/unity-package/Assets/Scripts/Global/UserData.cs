using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public static class UserData
{
    public static string username;
    
    public class GameStat : ICloneable
    {
        public int playcount;
        public double fastestEndTime = Double.PositiveInfinity;
        public int maxHp;
        public int maxAtk;
        public int maxSpeed;


         
        public object Clone()
        {
            GameStat newgs = new GameStat();
            newgs.playcount = playcount;
            newgs.fastestEndTime = fastestEndTime;
            newgs.maxHp = maxHp;
            newgs.maxAtk = maxAtk;
            newgs.maxSpeed = maxSpeed;
            return newgs;
        }
    }
    
    private struct UserCred {
        public string refershToken ;
        public string accessToken ;
    }
    public class GameSetting
    {
        public int enemyCount = 2;
        public int seed = 0;
    }
    private static UserCred userCred = new();
    private static GameStat gameStat= new();
    public static GameSetting gameSetting = new();

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


    public static void IncrementPlayCount( )
    {
        gameStat.playcount++;
    }
    public static void SetGameStat(
        double fastestEndTime,
        int maxHp,
        int maxAtk,
        int maxSpeed,
        int? playcount = null
    )
    {
        gameStat.fastestEndTime = (fastestEndTime < gameStat.fastestEndTime) ? fastestEndTime : gameStat.fastestEndTime;
        gameStat.maxHp = (maxHp > gameStat.maxHp) ? maxHp : gameStat.maxHp;
        gameStat.maxAtk = (maxAtk > gameStat.maxAtk)? maxAtk : gameStat.maxAtk;
        gameStat.maxSpeed = (maxSpeed > gameStat.maxSpeed)? maxSpeed : gameStat.maxSpeed;

        if (playcount != null )
        {
            gameStat.playcount = playcount.Value;
        }
    }

    public static void SetGameStat(string jsonfied) {
        if (jsonfied == null) return;
        try {
            JsonUtility.FromJson<GameStat>(jsonfied);
            gameStat = JsonUtility.FromJson<GameStat>(jsonfied);
        }
        catch (Exception ex)
        {

        }
        
    }

    public static GameStat GetGameStat() {
        return (GameStat)gameStat.Clone();
    }
}
