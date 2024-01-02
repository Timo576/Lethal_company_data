// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.RangeOfIntegers
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
