﻿// Decompiled with JetBrains decompiler
// Type: switchToAudio
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class switchToAudio
{
  public AudioSource audio;
  public AudioClip changeToClip;
  public bool stopAudio;
  public bool changeAudioVolume;
  public float audioVolume = 1f;
}
