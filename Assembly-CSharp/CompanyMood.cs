// Decompiled with JetBrains decompiler
// Type: CompanyMood
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/CompanyMoodPreset", order = 2)]
public class CompanyMood : ScriptableObject
{
  public float timeToWaitBeforeGrabbingItem = 10f;
  public float irritability = 1f;
  public float judgementSpeed = 1f;
  public float startingPatience = 3f;
  public bool desiresSilence;
  public bool mustBeWokenUp;
  public int maximumItemsToAnger = -1;
  public float sensitivity = 1f;
  [Space(3f)]
  public AudioClip noiseBehindWallSFX;
  [Space(5f)]
  public AudioClip[] grabItemsSFX;
  public AudioClip[] angerSFX;
  public AudioClip[] attackSFX;
  public AudioClip wallAttackSFX;
  public AudioClip insideWindowSFX;
  public AudioClip behindWallSFX;
  public bool stopWallSFXWhenOpening;
  [Space(5f)]
  public CompanyMonster manifestation;
  public int maxPlayersToKillBeforeSatisfied = 1;
  public int[] enableMonsterAnimationIndex;
  public float grabPlayerAnimationTime = 2f;
}
