// Decompiled with JetBrains decompiler
// Type: DunGen.BranchCountHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Graph;
using System;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public static class BranchCountHelper
  {
    public static void ComputeBranchCounts(
      DungeonFlow dungeonFlow,
      RandomStream randomStream,
      DungeonProxy proxyDungeon,
      ref int[] mainPathBranches)
    {
      switch (dungeonFlow.BranchMode)
      {
        case BranchMode.Local:
          BranchCountHelper.ComputeBranchCountsLocal(randomStream, proxyDungeon, ref mainPathBranches);
          break;
        case BranchMode.Global:
          BranchCountHelper.ComputeBranchCountsGlobal(dungeonFlow, randomStream, proxyDungeon, ref mainPathBranches);
          break;
        default:
          throw new NotImplementedException(string.Format("{0}.{1} is not implemented", (object) typeof (BranchMode).Name, (object) dungeonFlow.BranchMode));
      }
    }

    private static void ComputeBranchCountsLocal(
      RandomStream randomStream,
      DungeonProxy proxyDungeon,
      ref int[] mainPathBranches)
    {
      for (int index = 0; index < mainPathBranches.Length; ++index)
      {
        TileProxy mainPathTile = proxyDungeon.MainPathTiles[index];
        if (!((UnityEngine.Object) mainPathTile.Placement.Archetype == (UnityEngine.Object) null))
        {
          int num = Mathf.Min(mainPathTile.Placement.Archetype.BranchCount.GetRandom(randomStream), mainPathTile.UnusedDoorways.Count<DoorwayProxy>());
          mainPathBranches[index] = num;
        }
      }
    }

    private static void ComputeBranchCountsGlobal(
      DungeonFlow dungeonFlow,
      RandomStream randomStream,
      DungeonProxy proxyDungeon,
      ref int[] mainPathBranches)
    {
      int random = dungeonFlow.BranchCount.GetRandom(randomStream);
      float num1 = (float) random / (float) proxyDungeon.MainPathTiles.Count<TileProxy>((Func<TileProxy, bool>) (t => (UnityEngine.Object) t.Placement.Archetype != (UnityEngine.Object) null));
      float f = num1;
      int num2 = random;
      for (int index = 0; index < mainPathBranches.Length && num2 > 0; ++index)
      {
        TileProxy mainPathTile = proxyDungeon.MainPathTiles[index];
        if (!((UnityEngine.Object) mainPathTile.Placement.Archetype == (UnityEngine.Object) null))
        {
          int num3 = mainPathTile.UnusedDoorways.Count<DoorwayProxy>();
          int num4 = Mathf.Min(new int[4]
          {
            Mathf.FloorToInt(f),
            num3,
            mainPathTile.Placement.Archetype.BranchCount.Max,
            num2
          });
          float num5 = f - (float) num4;
          if (num4 < num3 && num4 < num2 && randomStream.NextDouble() < (double) num5)
          {
            ++num4;
            num5 = 0.0f;
          }
          f = num5 + num1;
          num2 -= num4;
          mainPathBranches[index] = num4;
        }
      }
    }
  }
}
