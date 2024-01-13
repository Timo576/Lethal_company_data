// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonArchetype
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [CreateAssetMenu(fileName = "New Archetype", menuName = "DunGen/Dungeon Archetype", order = 700)]
  [Serializable]
  public sealed class DungeonArchetype : ScriptableObject
  {
    public List<TileSet> TileSets = new List<TileSet>();
    public List<TileSet> BranchCapTileSets = new List<TileSet>();
    public BranchCapType BranchCapType = BranchCapType.AsWellAs;
    public IntRange BranchingDepth = new IntRange(2, 4);
    public IntRange BranchCount = new IntRange(0, 2);
    public float StraightenChance;
    public bool Unique;

    public bool GetHasValidBranchCapTiles()
    {
      if (this.BranchCapTileSets.Count == 0)
        return false;
      foreach (TileSet branchCapTileSet in this.BranchCapTileSets)
      {
        if (branchCapTileSet.TileWeights.Weights.Count > 0)
          return true;
      }
      return false;
    }
  }
}
