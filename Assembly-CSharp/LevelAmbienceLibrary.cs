// Decompiled with JetBrains decompiler
// Type: LevelAmbienceLibrary
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/LevelAmbience", order = 2)]
public class LevelAmbienceLibrary : ScriptableObject
{
  public AudioClip[] insanityMusicAudios;
  public AudioClip[] insideAmbience;
  public RandomAudioClip[] insideAmbienceInsanity;
  [Space(15f)]
  public AudioClip[] shipAmbience;
  public RandomAudioClip[] shipAmbienceInsanity;
  [Space(15f)]
  public AudioClip[] outsideAmbience;
  public RandomAudioClip[] outsideAmbienceInsanity;
}
