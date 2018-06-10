using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Repository for all music clips used in this stage for all objects to reference
/// </summary>
public class MusicDatabase : MonoBehaviour {
	public AudioClip youWinClip, youLoseClip, displayBoxClip, gameStartClip, wooshClip;
	public AudioClip[] greenTowerClips;
	public AudioClip[] blueTowerClips;
	public AudioClip[] goldTowerClips;
	public AudioClip[] scannerClips;
}
