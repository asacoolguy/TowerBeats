using System;
using UnityEngine;

public class LevelDatabase : MonoBehaviour{
    // general sound effects
    public AudioClip takeDamageClip, gameStartClip, cameraPullUpClip, powerdownClip;
    public LevelData[] levelData;
}


[Serializable]
public class LevelData {
    public MusicDatabase musicData;
    public string[] spawnPatterns;
    public EnemyPathData[] enemyPaths;
    public Vector3[] platformData;
    public int startingMoney;
}


[Serializable]
public class EnemyPathData {
    public Vector3 spawnPosition;
    public Vector3[] enemyPaths;
}


[Serializable]
public class MusicDatabase {
    public TowerMusicClips[] shockTowerClips;
    public TowerMusicClips[] laserTowerClips;
    public TowerMusicClips[] plasmaTowerClips;
    public AudioClip[] scannerClips;
    public AudioClip[] enemyDeathClips;
    public int[] measurePerSong;
    public AudioClip ending;
}


[Serializable]
public class TowerMusicClips {
    public AudioClip[] clips;
}