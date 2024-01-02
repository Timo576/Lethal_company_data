// Decompiled with JetBrains decompiler
// Type: DunGen.TileSet
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [CreateAssetMenu(menuName = "DunGen/Tile Set", order = 700)]
  [Serializable]
  public sealed class TileSet : ScriptableObject
  {
    public GameObjectChanceTable TileWeights = new GameObjectChanceTable();
    public List<LockedDoorwayAssociation> LockPrefabs = new List<LockedDoorwayAssociation>();

    public void AddTile(GameObject tilePrefab, float mainPathWeight, float branchPathWeight)
    {
      this.TileWeights.Weights.Add(new GameObjectChance(tilePrefab, mainPathWeight, branchPathWeight, this));
    }

    public void AddTiles(
      IEnumerable<GameObject> tilePrefab,
      float mainPathWeight,
      float branchPathWeight)
    {
      foreach (GameObject tilePrefab1 in tilePrefab)
        this.AddTile(tilePrefab1, mainPathWeight, branchPathWeight);
    }
  }
}
