// Decompiled with JetBrains decompiler
// Type: DunGen.Chance`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen
{
  [Serializable]
  public class Chance<T>
  {
    public T Value;
    public float Weight;

    public Chance()
      : this(default (T), 1f)
    {
    }

    public Chance(T value)
      : this(value, 1f)
    {
    }

    public Chance(T value, float weight)
    {
      this.Value = value;
      this.Weight = weight;
    }
  }
}
