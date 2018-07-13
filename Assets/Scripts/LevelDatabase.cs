﻿using System;
using UnityEngine;

public class LevelDatabase : MonoBehaviour{
    // general sound effects
    public AudioClip youWinClip, youLoseClip, displayBoxClip, gameStartClip, wooshClip;
    public LevelData[] levelData;
}


[Serializable]
public class LevelData {
    public MusicDatabase musicData;
    public int totalWaves;
    public string spawnPattern;
    public EnemyPathData[] enemyPaths;
    public Vector3[] platformData;
}


[Serializable]
public class EnemyPathData {
    public Vector3 spawnPosition;
    public Vector3[] enemyPaths;
}


[Serializable]
public class MusicDatabase {
    public AudioClip[] shockTowerClips;
    public AudioClip[] sniperTowerClips;
    public AudioClip[] laserTowerClips;
    public AudioClip[] scannerClips;
    public int[] measurePerSong;
}