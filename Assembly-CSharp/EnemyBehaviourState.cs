// Decompiled with JetBrains decompiler
// Type: EnemyBehaviourState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class EnemyBehaviourState
{
  public string name;
  [Space(5f)]
  public AudioClip VoiceClip;
  public bool playOneShotVoice;
  public AudioClip SFXClip;
  public bool playOneShotSFX;
  [Space(5f)]
  public bool IsAnimTrigger;
  public string parameterString;
  public bool boolValue;
}
