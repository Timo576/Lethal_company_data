// Decompiled with JetBrains decompiler
// Type: DunGen.GameObjectChanceTable
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [Serializable]
  public class GameObjectChanceTable
  {
    public List<GameObjectChance> Weights = new List<GameObjectChance>();

    public GameObjectChanceTable Clone()
    {
      GameObjectChanceTable objectChanceTable = new GameObjectChanceTable();
      foreach (GameObjectChance weight in this.Weights)
        objectChanceTable.Weights.Add(new GameObjectChance(weight.Value, weight.MainPathWeight, weight.BranchPathWeight, weight.TileSet)
        {
          DepthWeightScale = weight.DepthWeightScale
        });
      return objectChanceTable;
    }

    public bool ContainsGameObject(GameObject obj)
    {
      foreach (GameObjectChance weight in this.Weights)
      {
        if ((UnityEngine.Object) weight.Value == (UnityEngine.Object) obj)
          return true;
      }
      return false;
    }

    public GameObjectChance GetRandom(
      RandomStream random,
      bool isOnMainPath,
      float normalizedDepth,
      GameObject previouslyChosen,
      bool allowImmediateRepeats,
      bool removeFromTable = false)
    {
      float num1 = 0.0f;
      foreach (GameObjectChance weight in this.Weights)
      {
        if (weight != null && (UnityEngine.Object) weight.Value != (UnityEngine.Object) null && (allowImmediateRepeats || (UnityEngine.Object) previouslyChosen == (UnityEngine.Object) null || (UnityEngine.Object) weight.Value != (UnityEngine.Object) previouslyChosen))
          num1 += weight.GetWeight(isOnMainPath, normalizedDepth);
      }
      float num2 = (float) random.NextDouble() * num1;
      foreach (GameObjectChance weight1 in this.Weights)
      {
        if (weight1 != null && !((UnityEngine.Object) weight1.Value == (UnityEngine.Object) null) && (!((UnityEngine.Object) weight1.Value == (UnityEngine.Object) previouslyChosen) || this.Weights.Count <= 1 || allowImmediateRepeats))
        {
          float weight2 = weight1.GetWeight(isOnMainPath, normalizedDepth);
          if ((double) num2 < (double) weight2)
          {
            if (removeFromTable)
              this.Weights.Remove(weight1);
            return weight1;
          }
          num2 -= weight2;
        }
      }
      return (GameObjectChance) null;
    }

    public static GameObject GetCombinedRandom(
      RandomStream random,
      bool isOnMainPath,
      float normalizedDepth,
      params GameObjectChanceTable[] tables)
    {
      float num1 = ((IEnumerable<GameObjectChanceTable>) tables).SelectMany<GameObjectChanceTable, float>((Func<GameObjectChanceTable, IEnumerable<float>>) (x => x.Weights.Select<GameObjectChance, float>((Func<GameObjectChance, float>) (y => y.GetWeight(isOnMainPath, normalizedDepth))))).Sum();
      float num2 = (float) random.NextDouble() * num1;
      foreach (GameObjectChance gameObjectChance in ((IEnumerable<GameObjectChanceTable>) tables).SelectMany<GameObjectChanceTable, GameObjectChance>((Func<GameObjectChanceTable, IEnumerable<GameObjectChance>>) (x => (IEnumerable<GameObjectChance>) x.Weights)))
      {
        float weight = gameObjectChance.GetWeight(isOnMainPath, normalizedDepth);
        if ((double) num2 < (double) weight)
          return gameObjectChance.Value;
        num2 -= weight;
      }
      return (GameObject) null;
    }

    public static GameObjectChanceTable Combine(params GameObjectChanceTable[] tables)
    {
      GameObjectChanceTable objectChanceTable = new GameObjectChanceTable();
      foreach (GameObjectChanceTable table in tables)
      {
        foreach (GameObjectChance weight in table.Weights)
          objectChanceTable.Weights.Add(new GameObjectChance(weight.Value, weight.MainPathWeight, weight.BranchPathWeight, weight.TileSet)
          {
            DepthWeightScale = weight.DepthWeightScale
          });
      }
      return objectChanceTable;
    }
  }
}
