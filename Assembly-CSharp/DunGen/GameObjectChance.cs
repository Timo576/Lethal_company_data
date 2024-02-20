// Decompiled with JetBrains decompiler
// Type: DunGen.GameObjectChance
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [Serializable]
  public sealed class GameObjectChance
  {
    public GameObject Value;
    public float MainPathWeight = 1f;
    public float BranchPathWeight = 1f;
    public AnimationCurve DepthWeightScale = AnimationCurve.Linear(0.0f, 1f, 1f, 1f);
    public TileSet TileSet;

    public GameObjectChance()
      : this((GameObject) null, 1f, 1f, (TileSet) null)
    {
    }

    public GameObjectChance(GameObject value)
      : this(value, 1f, 1f, (TileSet) null)
    {
    }

    public GameObjectChance(
      GameObject value,
      float mainPathWeight,
      float branchPathWeight,
      TileSet tileSet)
    {
      this.Value = value;
      this.MainPathWeight = mainPathWeight;
      this.BranchPathWeight = branchPathWeight;
      this.TileSet = tileSet;
    }

    public float GetWeight(bool isOnMainPath, float normalizedDepth)
    {
      return (isOnMainPath ? this.MainPathWeight : this.BranchPathWeight) * this.DepthWeightScale.Evaluate(normalizedDepth);
    }
  }
}
