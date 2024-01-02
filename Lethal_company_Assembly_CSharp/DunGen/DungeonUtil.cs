// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonUtil
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public static class DungeonUtil
  {
    public static void AddAndSetupDoorComponent(
      Dungeon dungeon,
      GameObject doorPrefab,
      Doorway doorway)
    {
      Door door = doorPrefab.GetComponent<Door>();
      if ((Object) door == (Object) null)
        door = doorPrefab.AddComponent<Door>();
      door.Dungeon = dungeon;
      door.DoorwayA = doorway;
      door.DoorwayB = doorway.ConnectedDoorway;
      door.TileA = doorway.Tile;
      door.TileB = doorway.ConnectedDoorway.Tile;
      dungeon.AddAdditionalDoor(door);
    }

    public static bool HasAnyViableEntries(this List<GameObjectWeight> weights)
    {
      if (weights == null || weights.Count == 0)
        return false;
      foreach (GameObjectWeight weight in weights)
      {
        if ((Object) weight.GameObject != (Object) null && (double) weight.Weight > 0.0)
          return true;
      }
      return false;
    }

    public static GameObject GetRandom(
      this List<GameObjectWeight> weights,
      RandomStream randomStream)
    {
      float num1 = 0.0f;
      foreach (GameObjectWeight weight in weights)
      {
        if ((Object) weight.GameObject != (Object) null)
          num1 += weight.Weight;
      }
      float num2 = (float) randomStream.NextDouble() * num1;
      foreach (GameObjectWeight weight in weights)
      {
        if (weight != null && !((Object) weight.GameObject == (Object) null))
        {
          if ((double) num2 < (double) weight.Weight)
            return weight.GameObject;
          num2 -= weight.Weight;
        }
      }
      return (GameObject) null;
    }
  }
}
