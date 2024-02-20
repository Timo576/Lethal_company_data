// Decompiled with JetBrains decompiler
// Type: DunGen.ChanceTable`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public class ChanceTable<T>
  {
    [SerializeField]
    public List<Chance<T>> Weights = new List<Chance<T>>();

    public void Add(T value, float weight) => this.Weights.Add(new Chance<T>(value, weight));

    public void Remove(T value)
    {
      for (int index = 0; index < this.Weights.Count; ++index)
      {
        if (this.Weights[index].Value.Equals((object) value))
          this.Weights.RemoveAt(index);
      }
    }

    public T GetRandom(RandomStream random)
    {
      float num1 = this.Weights.Select<Chance<T>, float>((Func<Chance<T>, float>) (x => x.Weight)).Sum();
      float num2 = (float) random.NextDouble() * num1;
      foreach (Chance<T> weight in this.Weights)
      {
        if ((double) num2 < (double) weight.Weight)
          return weight.Value;
        num2 -= weight.Weight;
      }
      return default (T);
    }

    public static TVal GetCombinedRandom<TVal, TChance>(
      RandomStream random,
      params ChanceTable<TVal>[] tables)
    {
      float num1 = ((IEnumerable<ChanceTable<TVal>>) tables).SelectMany<ChanceTable<TVal>, float>((Func<ChanceTable<TVal>, IEnumerable<float>>) (x => x.Weights.Select<Chance<TVal>, float>((Func<Chance<TVal>, float>) (y => y.Weight)))).Sum();
      float num2 = (float) random.NextDouble() * num1;
      foreach (Chance<TVal> chance in ((IEnumerable<ChanceTable<TVal>>) tables).SelectMany<ChanceTable<TVal>, Chance<TVal>>((Func<ChanceTable<TVal>, IEnumerable<Chance<TVal>>>) (x => (IEnumerable<Chance<TVal>>) x.Weights)))
      {
        if ((double) num2 < (double) chance.Weight)
          return chance.Value;
        num2 -= chance.Weight;
      }
      return default (TVal);
    }
  }
}
