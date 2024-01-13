// Decompiled with JetBrains decompiler
// Type: DunGen.LocalPropSet
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Random Props/Local Prop Set")]
  public class LocalPropSet : RandomProp
  {
    private static readonly Dictionary<LocalPropSetCountMode, GetPropCountDelegate> GetCountMethods = new Dictionary<LocalPropSetCountMode, GetPropCountDelegate>();
    [AcceptGameObjectTypes(GameObjectFilter.Scene)]
    public GameObjectChanceTable Props = new GameObjectChanceTable();
    public IntRange PropCount = new IntRange(1, 1);
    public LocalPropSetCountMode CountMode;
    public AnimationCurve CountDepthCurve = AnimationCurve.Linear(0.0f, 0.0f, 1f, 1f);

    public override void Process(RandomStream randomStream, Tile tile)
    {
      GameObjectChanceTable objectChanceTable = this.Props.Clone();
      GetPropCountDelegate propCountDelegate;
      if (!LocalPropSet.GetCountMethods.TryGetValue(this.CountMode, out propCountDelegate))
        throw new NotImplementedException("LocalPropSet count mode \"" + this.CountMode.ToString() + "\" is not yet implemented");
      int capacity = propCountDelegate(this, randomStream, tile);
      List<GameObject> gameObjectList = new List<GameObject>(capacity);
      for (int index = 0; index < capacity; ++index)
      {
        GameObjectChance random = objectChanceTable.GetRandom(randomStream, tile.Placement.IsOnMainPath, tile.Placement.NormalizedDepth, (GameObject) null, true, true);
        if (random != null && (UnityEngine.Object) random.Value != (UnityEngine.Object) null)
          gameObjectList.Add(random.Value);
      }
      foreach (GameObjectChance weight in this.Props.Weights)
      {
        if (!gameObjectList.Contains(weight.Value))
          UnityUtil.Destroy((UnityEngine.Object) weight.Value);
      }
    }

    static LocalPropSet()
    {
      LocalPropSet.GetCountMethods[LocalPropSetCountMode.Random] = new GetPropCountDelegate(LocalPropSet.GetCountRandom);
      LocalPropSet.GetCountMethods[LocalPropSetCountMode.DepthBased] = new GetPropCountDelegate(LocalPropSet.GetCountDepthBased);
      LocalPropSet.GetCountMethods[LocalPropSetCountMode.DepthMultiply] = new GetPropCountDelegate(LocalPropSet.GetCountDepthMultiply);
    }

    private static int GetCountRandom(LocalPropSet propSet, RandomStream randomStream, Tile tile)
    {
      return Mathf.Clamp(propSet.PropCount.GetRandom(randomStream), 0, propSet.Props.Weights.Count);
    }

    private static int GetCountDepthBased(
      LocalPropSet propSet,
      RandomStream randomStream,
      Tile tile)
    {
      float t = Mathf.Clamp(propSet.CountDepthCurve.Evaluate(tile.Placement.NormalizedPathDepth), 0.0f, 1f);
      return Mathf.RoundToInt(Mathf.Lerp((float) propSet.PropCount.Min, (float) propSet.PropCount.Max, t));
    }

    private static int GetCountDepthMultiply(
      LocalPropSet propSet,
      RandomStream randomStream,
      Tile tile)
    {
      float num = Mathf.Clamp(propSet.CountDepthCurve.Evaluate(tile.Placement.NormalizedPathDepth), 0.0f, 1f);
      return Mathf.RoundToInt((float) LocalPropSet.GetCountRandom(propSet, randomStream, tile) * num);
    }
  }
}
