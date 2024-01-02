// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.RangeOfFloats
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  [Serializable]
  public struct RangeOfFloats
  {
    [Tooltip("Minimum value (inclusive)")]
    public float Minimum;
    [Tooltip("Maximum value (inclusive)")]
    public float Maximum;

    public float Random() => UnityEngine.Random.Range(this.Minimum, this.Maximum);

    public float Random(System.Random r)
    {
      return this.Minimum + (float) r.NextDouble() * (this.Maximum - this.Minimum);
    }
  }
}
