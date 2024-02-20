// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.RangeOfIntegers
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  [Serializable]
  public struct RangeOfIntegers
  {
    [Tooltip("Minimum value (inclusive)")]
    public int Minimum;
    [Tooltip("Maximum value (inclusive)")]
    public int Maximum;

    public int Random() => UnityEngine.Random.Range(this.Minimum, this.Maximum + 1);

    public int Random(System.Random r) => r.Next(this.Minimum, this.Maximum + 1);
  }
}
