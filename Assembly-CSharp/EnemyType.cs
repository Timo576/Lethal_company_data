// Decompiled with JetBrains decompiler
// Type: EnemyType
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/EnemyType", order = 1)]
public class EnemyType : ScriptableObject
{
  public string enemyName;
  [Header("Spawning logic")]
  [Tooltip("Determines how likely an enemy is to spawn throughout the day.")]
  public AnimationCurve probabilityCurve;
  public bool spawningDisabled;
  [Tooltip("X axis is the number of this enemy type that have spawned, divided by 10; Y axis is a multiplier to probabilityCurve.")]
  public AnimationCurve numberSpawnedFalloff;
  public bool useNumberSpawnedFalloff;
  public GameObject enemyPrefab;
  [Tooltip("Adds to a global counter determining how many enemies can spawn.")]
  public int PowerLevel;
  [Tooltip("An individual counter determining how many of this enemy can spawn, regardless of how many other enemies there are.")]
  public int MaxCount;
  public int numberSpawned;
  public bool isOutsideEnemy;
  [Space(3f)]
  public bool isDaytimeEnemy;
  [Range(0.0f, 1f)]
  public float normalizedTimeInDayToLeave = 1f;
  [Space(3f)]
  [Header("Misc. ingame properties")]
  public float stunTimeMultiplier = 2f;
  public float doorSpeedMultiplier = 1f;
  public float stunGameDifficultyMultiplier = 1f;
  public bool canBeStunned = true;
  public bool canDie = true;
  public bool destroyOnDeath;
  public bool canSeeThroughFog;
  [Space(10f)]
  [Header("Vent Properties")]
  public float timeToPlayAudio = 15f;
  public float loudnessMultiplier = 1f;
  public AudioClip overrideVentSFX;
  [Space(5f)]
  public AudioClip hitBodySFX;
  public AudioClip hitEnemyVoiceSFX;
  public AudioClip deathSFX;
  public AudioClip stunSFX;
  public MiscAnimation[] miscAnimations;
  public AudioClip[] audioClips;
}
