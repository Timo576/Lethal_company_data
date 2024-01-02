// Decompiled with JetBrains decompiler
// Type: DunGen.IntRange
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen
{
  [Serializable]
  public class IntRange
  {
    public int Min;
    public int Max;

    public IntRange()
    {
    }

    public IntRange(int min, int max)
    {
      this.Min = min;
      this.Max = max;
    }

    public int GetRandom(RandomStream random)
    {
      if (this.Min > this.Max)
        this.Max = this.Min;
      return random.Next(this.Min, this.Max + 1);
    }

    public override string ToString() => this.Min.ToString() + " - " + this.Max.ToString();
  }
}
