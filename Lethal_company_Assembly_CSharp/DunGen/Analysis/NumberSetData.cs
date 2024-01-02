// Decompiled with JetBrains decompiler
// Type: DunGen.Analysis.NumberSetData
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen.Analysis
{
  public sealed class NumberSetData
  {
    public float Min { get; private set; }

    public float Max { get; private set; }

    public float Average { get; private set; }

    public float StandardDeviation { get; private set; }

    public NumberSetData(IEnumerable<float> values)
    {
      this.Min = values.Min();
      this.Max = values.Max();
      this.Average = values.Sum() / (float) values.Count<float>();
      float[] source = new float[values.Count<float>()];
      for (int index = 0; index < source.Length; ++index)
        source[index] = Mathf.Pow(values.ElementAt<float>(index) - this.Average, 2f);
      this.StandardDeviation = Mathf.Sqrt(((IEnumerable<float>) source).Sum() / (float) source.Length);
    }

    public override string ToString()
    {
      return string.Format("[ Min: {0}, Max: {1}, Average: {2}, Standard Deviation: {3} ]", (object) this.Min, (object) this.Max, (object) this.Average, (object) this.StandardDeviation);
    }
  }
}
