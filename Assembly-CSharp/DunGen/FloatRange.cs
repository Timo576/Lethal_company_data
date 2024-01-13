// Decompiled with JetBrains decompiler
// Type: DunGen.FloatRange
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen
{
  [Serializable]
  public class FloatRange
  {
    public float Min;
    public float Max;

    public FloatRange()
    {
    }

    public FloatRange(float min, float max)
    {
      this.Min = min;
      this.Max = max;
    }

    public float GetRandom(RandomStream random)
    {
      if ((double) this.Min > (double) this.Max)
      {
        float min = this.Min;
        this.Min = this.Max;
        this.Max = min;
      }
      float num = this.Max - this.Min;
      return this.Min + (float) random.NextDouble() * num;
    }
  }
}
