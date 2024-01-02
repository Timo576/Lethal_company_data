// Decompiled with JetBrains decompiler
// Type: DunGen.DoorwayPairFinder
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public sealed class DoorwayPairFinder
  {
    public static List<TileConnectionRule> CustomConnectionRules = new List<TileConnectionRule>();
    public RandomStream RandomStream;
    public List<GameObjectChance> TileWeights;
    public TileProxy PreviousTile;
    public bool IsOnMainPath;
    public float NormalizedDepth;
    public DungeonArchetype Archetype;
    public bool? AllowRotation;
    public Vector3 UpVector;
    public TileMatchDelegate IsTileAllowedPredicate;
    public GetTileTemplateDelegate GetTileTemplateDelegate;
    public DungeonFlow DungeonFlow;
    private List<GameObjectChance> tileOrder;

    public Queue<DoorwayPair> GetDoorwayPairs(int? maxCount)
    {
      this.tileOrder = this.CalculateOrderedListOfTiles();
      List<DoorwayPair> potentialPairs = this.PreviousTile != null ? this.GetPotentialDoorwayPairsForNonFirstTile().ToList<DoorwayPair>() : this.GetPotentialDoorwayPairsForFirstTile().ToList<DoorwayPair>();
      int num = potentialPairs.Count;
      if (maxCount.HasValue)
        num = Math.Min(num, maxCount.Value);
      Queue<DoorwayPair> doorwayPairs = new Queue<DoorwayPair>(num);
      foreach (DoorwayPair orderDoorwayPair in this.OrderDoorwayPairs(potentialPairs, num))
        doorwayPairs.Enqueue(orderDoorwayPair);
      return doorwayPairs;
    }

    private int CompareDoorwaysTileWeight(DoorwayPair x, DoorwayPair y)
    {
      return y.TileWeight.CompareTo(x.TileWeight);
    }

    private IEnumerable<DoorwayPair> OrderDoorwayPairs(List<DoorwayPair> potentialPairs, int count)
    {
      potentialPairs.Sort(new Comparison<DoorwayPair>(this.CompareDoorwaysTileWeight));
      for (int index1 = 0; index1 < potentialPairs.Count - 1; ++index1)
      {
        for (int index2 = 0; index2 < potentialPairs.Count - 1; ++index2)
        {
          DoorwayPair potentialPair1 = potentialPairs[index2];
          double tileWeight1 = (double) potentialPair1.TileWeight;
          potentialPair1 = potentialPairs[index2 + 1];
          double tileWeight2 = (double) potentialPair1.TileWeight;
          if (tileWeight1 == tileWeight2)
          {
            potentialPair1 = potentialPairs[index2];
            double doorwayWeight1 = (double) potentialPair1.DoorwayWeight;
            potentialPair1 = potentialPairs[index2 + 1];
            double doorwayWeight2 = (double) potentialPair1.DoorwayWeight;
            if (doorwayWeight1 < doorwayWeight2)
            {
              DoorwayPair potentialPair2 = potentialPairs[index2];
              potentialPairs[index2] = potentialPairs[index2 + 1];
              potentialPairs[index2 + 1] = potentialPair2;
            }
          }
        }
      }
      return potentialPairs.Take<DoorwayPair>(count);
    }

    private List<GameObjectChance> CalculateOrderedListOfTiles()
    {
      List<GameObjectChance> orderedListOfTiles = new List<GameObjectChance>(this.TileWeights.Count);
      GameObjectChanceTable objectChanceTable = new GameObjectChanceTable();
      objectChanceTable.Weights.AddRange((IEnumerable<GameObjectChance>) this.TileWeights);
      while (objectChanceTable.Weights.Any<GameObjectChance>((Func<GameObjectChance, bool>) (x => (UnityEngine.Object) x.Value != (UnityEngine.Object) null && (double) x.GetWeight(this.IsOnMainPath, this.NormalizedDepth) > 0.0)))
        orderedListOfTiles.Add(objectChanceTable.GetRandom(this.RandomStream, this.IsOnMainPath, this.NormalizedDepth, (GameObject) null, true, true));
      return orderedListOfTiles;
    }

    private IEnumerable<DoorwayPair> GetPotentialDoorwayPairsForNonFirstTile()
    {
      foreach (DoorwayProxy unusedDoorway in this.PreviousTile.UnusedDoorways)
      {
        DoorwayProxy previousDoor = unusedDoorway;
        if ((this.PreviousTile.Exit == null ? 0 : (!this.PreviousTile.UsedDoorways.Contains<DoorwayProxy>(this.PreviousTile.Exit) ? 1 : 0)) == 0 || this.PreviousTile.Exit == previousDoor)
        {
          foreach (GameObjectChance tileWeight1 in this.TileWeights)
          {
            GameObjectChance tileWeight = tileWeight1;
            if (this.tileOrder.Contains(tileWeight))
            {
              TileProxy nextTile = this.GetTileTemplateDelegate(tileWeight.Value);
              float weight = (float) (this.tileOrder.Count - this.tileOrder.IndexOf(tileWeight));
              if (this.IsTileAllowedPredicate == null || this.IsTileAllowedPredicate(this.PreviousTile, nextTile, ref weight))
              {
                foreach (DoorwayProxy doorway in nextTile.Doorways)
                {
                  if (((nextTile == null ? 0 : (nextTile.Entrance != null ? 1 : 0)) == 0 || nextTile.Entrance == doorway) && (nextTile == null || nextTile.Exit != doorway))
                  {
                    float weight1 = 0.0f;
                    if (this.IsValidDoorwayPairing(previousDoor, doorway, this.PreviousTile, nextTile, ref weight1))
                      yield return new DoorwayPair(this.PreviousTile, previousDoor, nextTile, doorway, tileWeight.TileSet, weight, weight1);
                  }
                }
                nextTile = (TileProxy) null;
                tileWeight = (GameObjectChance) null;
              }
            }
          }
          previousDoor = (DoorwayProxy) null;
        }
      }
    }

    private IEnumerable<DoorwayPair> GetPotentialDoorwayPairsForFirstTile()
    {
      foreach (GameObjectChance tileWeight1 in this.TileWeights)
      {
        GameObjectChance tileWeight = tileWeight1;
        if (this.tileOrder.Contains(tileWeight))
        {
          TileProxy nextTile = this.GetTileTemplateDelegate(tileWeight.Value);
          float weight = tileWeight.GetWeight(this.IsOnMainPath, this.NormalizedDepth) * (float) this.RandomStream.NextDouble();
          if (this.IsTileAllowedPredicate == null || this.IsTileAllowedPredicate(this.PreviousTile, nextTile, ref weight))
          {
            foreach (DoorwayProxy doorway in nextTile.Doorways)
            {
              float doorwayWeight = this.CalculateDoorwayWeight(doorway);
              yield return new DoorwayPair((TileProxy) null, (DoorwayProxy) null, nextTile, doorway, tileWeight.TileSet, weight, doorwayWeight);
            }
            nextTile = (TileProxy) null;
            tileWeight = (GameObjectChance) null;
          }
        }
      }
    }

    private bool IsValidDoorwayPairing(
      DoorwayProxy a,
      DoorwayProxy b,
      TileProxy previousTile,
      TileProxy nextTile,
      ref float weight)
    {
      if (!this.DungeonFlow.CanDoorwaysConnect(this.PreviousTile.PrefabTile, nextTile.PrefabTile, a.DoorwayComponent, b.DoorwayComponent))
        return false;
      Vector3? nullable = new Vector3?();
      bool flag = this.AllowRotation.HasValue && !this.AllowRotation.Value || nextTile != null && !nextTile.PrefabTile.AllowRotation;
      if ((double) Vector3.Angle(a.Forward, this.UpVector) < 1.0)
        nullable = new Vector3?(-this.UpVector);
      else if ((double) Vector3.Angle(a.Forward, -this.UpVector) < 1.0)
        nullable = new Vector3?(this.UpVector);
      else if (flag)
        nullable = new Vector3?(-a.Forward);
      if (nullable.HasValue && (double) Vector3.Angle(nullable.Value, b.Forward) > 1.0)
        return false;
      weight = this.CalculateDoorwayWeight(b);
      return true;
    }

    private float CalculateDoorwayWeight(DoorwayProxy doorway)
    {
      float doorwayWeight = (float) this.RandomStream.NextDouble();
      float num = (UnityEngine.Object) this.Archetype == (UnityEngine.Object) null ? 0.0f : this.Archetype.StraightenChance;
      if (((double) num <= 0.0 || !this.IsOnMainPath || this.PreviousTile.UsedDoorways.Count<DoorwayProxy>() != 1 ? 0 : (this.PreviousTile.UsedDoorways.First<DoorwayProxy>().Forward == -doorway.Forward ? 1 : 0)) != 0 && this.RandomStream.NextDouble() < (double) num)
        doorwayWeight *= 100f;
      return doorwayWeight;
    }
  }
}
