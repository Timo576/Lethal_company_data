// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonArchetypeValidator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public sealed class DungeonArchetypeValidator
  {
    public DungeonFlow Flow { get; private set; }

    public DungeonArchetypeValidator(DungeonFlow flow) => this.Flow = flow;

    public bool IsValid()
    {
      if ((UnityEngine.Object) this.Flow == (UnityEngine.Object) null)
      {
        this.LogError("No Dungeon Flow is assigned");
        return false;
      }
      DungeonArchetype[] usedArchetypes = this.Flow.GetUsedArchetypes();
      TileSet[] usedTileSets = this.Flow.GetUsedTileSets();
      foreach (GraphLine line in this.Flow.Lines)
      {
        if (line.DungeonArchetypes.Count == 0)
        {
          this.LogError("One or more line segments in your dungeon flow graph have no archetype applied. Each line segment must have at least one archetype assigned to it.");
          return false;
        }
        foreach (UnityEngine.Object dungeonArchetype in line.DungeonArchetypes)
        {
          if (dungeonArchetype == (UnityEngine.Object) null)
          {
            this.LogError("One or more of the archetypes in your dungeon flow graph have an unset archetype value.");
            return false;
          }
        }
      }
      foreach (GraphNode node in this.Flow.Nodes)
      {
        if (node.TileSets.Count == 0)
        {
          this.LogError("The \"{0}\" node in your dungeon flow graph have no tile sets applied. Each node must have at least one tile set assigned to it.", (object) node.Label);
          return false;
        }
      }
      foreach (DungeonArchetype dungeonArchetype in usedArchetypes)
      {
        if ((UnityEngine.Object) dungeonArchetype == (UnityEngine.Object) null)
        {
          this.LogError("An Archetype in the Dungeon Flow has not been assigned a value");
          return false;
        }
        foreach (TileSet tileSet in dungeonArchetype.TileSets)
        {
          foreach (GameObject gameObject in tileSet.TileWeights.Weights.Select<GameObjectChance, GameObject>((Func<GameObjectChance, GameObject>) (x => x.Value)))
          {
            if (!((UnityEngine.Object) gameObject == (UnityEngine.Object) null))
            {
              int num = ((IEnumerable<Doorway>) gameObject.GetComponentsInChildren<Doorway>(true)).Count<Doorway>();
              if (num <= 1)
                this.LogWarning("The Tile \"{0}\" in TileSet \"{1}\" has {2} doorways. Tiles in an archetype should have more than 1 doorway.", (object) gameObject.name, (object) tileSet.name, (object) num);
            }
          }
        }
      }
      foreach (TileSet tileSet in usedTileSets)
      {
        if ((UnityEngine.Object) tileSet == (UnityEngine.Object) null)
        {
          this.LogError("A TileSet in the Dungeon Flow has not been assigned a value");
          return false;
        }
        if (tileSet.TileWeights.Weights.Count == 0)
        {
          this.LogError("TileSet \"{0}\" contains no Tiles", (object) tileSet.name);
          return false;
        }
        foreach (GameObjectChance weight in tileSet.TileWeights.Weights)
        {
          if ((UnityEngine.Object) weight.Value == (UnityEngine.Object) null)
            this.LogWarning("TileSet \"{0}\" contains an entry with no Tile", (object) tileSet.name);
          if ((double) weight.MainPathWeight <= 0.0 && (double) weight.BranchPathWeight <= 0.0)
            this.LogWarning("TileSet \"{0}\" contains an entry with an invalid weight. Both weights are below zero, resulting in no chance for this tile to spawn in the dungeon. Either MainPathWeight or BranchPathWeight can be zero, not both.", (object) tileSet.name);
        }
      }
      return true;
    }

    private void LogError(string format, params object[] args)
    {
      Debug.LogError((object) string.Format("[ArchetypeValidator] Error: " + format, args));
    }

    private void LogWarning(string format, params object[] args)
    {
      Debug.LogWarning((object) string.Format("[ArchetypeValidator] Warning: " + format, args));
    }
  }
}
