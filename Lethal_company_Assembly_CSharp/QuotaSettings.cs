// Decompiled with JetBrains decompiler
// Type: QuotaSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class QuotaSettings
{
  [Tooltip("The starting profit quota")]
  public int startingQuota = 300;
  public int startingCredits = 60;
  public int deadlineDaysAmount = 4;
  [Space(5f)]
  [Tooltip("Higher value means a less steep exponential increase")]
  public float increaseSteepness = 4f;
  [Tooltip("The minimum amount to increase")]
  public float baseIncrease = 200f;
  [Tooltip("A random Y coordinate on this line is picked and multiplied with the increase amount")]
  public AnimationCurve randomizerCurve;
  [Tooltip("This determines the severity of the randomizer curve")]
  public float randomizerMultiplier = 1f;
}
