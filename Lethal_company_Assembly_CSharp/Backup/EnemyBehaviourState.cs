// Decompiled with JetBrains decompiler
// Type: EnemyBehaviourState
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
