// Decompiled with JetBrains decompiler
// Type: ReverbPreset
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
[CreateAssetMenu(menuName = "ScriptableObjects/ReverbPreset", order = 1)]
public class ReverbPreset : ScriptableObject
{
  public bool changeDryLevel;
  [Range(-10000f, 0.0f)]
  public float dryLevel;
  public bool changeHighFreq;
  [Range(-10000f, 0.0f)]
  public float highFreq = -270f;
  public bool changeLowFreq;
  [Range(-10000f, 0.0f)]
  public float lowFreq = -244f;
  public bool changeDecayTime;
  [Range(0.0f, 35f)]
  public float decayTime = 1.4f;
  public bool changeRoom;
  [Range(-10000f, 0.0f)]
  public float room = -600f;
}
