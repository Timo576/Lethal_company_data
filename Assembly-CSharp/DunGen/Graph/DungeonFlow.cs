// Decompiled with JetBrains decompiler
// Type: DunGen.Graph.DungeonFlow
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

#nullable disable
namespace DunGen.Graph
{
  [CreateAssetMenu(fileName = "New Dungeon", menuName = "DunGen/Dungeon Flow", order = 700)]
  [Serializable]
  public class DungeonFlow : ScriptableObject, ISerializationCallbackReceiver
  {
    public const int FileVersion = 1;
    [SerializeField]
    [FormerlySerializedAs("GlobalPropGroupIDs")]
    private List<int> globalPropGroupID_obsolete = new List<int>();
    [SerializeField]
    [FormerlySerializedAs("GlobalPropRanges")]
    private List<IntRange> globalPropRanges_obsolete = new List<IntRange>();
    public IntRange Length = new IntRange(5, 10);
    public BranchMode BranchMode;
    public IntRange BranchCount = new IntRange(1, 5);
    public List<DungeonFlow.GlobalPropSettings> GlobalProps = new List<DungeonFlow.GlobalPropSettings>();
    public KeyManager KeyManager;
    [Range(0.0f, 1f)]
    public float DoorwayConnectionChance;
    public bool RestrictConnectionToSameSection;
    public List<TileInjectionRule> TileInjectionRules = new List<TileInjectionRule>();
    public DungeonFlow.TagConnectionMode TileTagConnectionMode;
    public List<TagPair> TileConnectionTags = new List<TagPair>();
    public DungeonFlow.BranchPruneMode BranchTagPruneMode = DungeonFlow.BranchPruneMode.AllTagsMissing;
    public List<Tag> BranchPruneTags = new List<Tag>();
    public List<GraphNode> Nodes = new List<GraphNode>();
    public List<GraphLine> Lines = new List<GraphLine>();
    [SerializeField]
    private int currentFileVersion;

    public void Reset()
    {
      TileSet[] tileSetArray = new TileSet[0];
      DungeonArchetype[] archetypes = new DungeonArchetype[0];
      new DungeonFlowBuilder(this).AddNode((IEnumerable<TileSet>) tileSetArray, "Start").AddLine((IEnumerable<DungeonArchetype>) archetypes).AddNode((IEnumerable<TileSet>) tileSetArray, "Goal").Complete();
    }

    public GraphLine GetLineAtDepth(float normalizedDepth)
    {
      normalizedDepth = Mathf.Clamp(normalizedDepth, 0.0f, 1f);
      if ((double) normalizedDepth == 0.0)
        return this.Lines[0];
      if ((double) normalizedDepth == 1.0)
        return this.Lines[this.Lines.Count - 1];
      foreach (GraphLine line in this.Lines)
      {
        if ((double) normalizedDepth >= (double) line.Position && (double) normalizedDepth < (double) line.Position + (double) line.Length)
          return line;
      }
      Debug.LogError((object) ("GetLineAtDepth was unable to find a line at depth " + normalizedDepth.ToString() + ". This shouldn't happen."));
      return (GraphLine) null;
    }

    public DungeonArchetype[] GetUsedArchetypes()
    {
      return this.Lines.SelectMany<GraphLine, DungeonArchetype>((Func<GraphLine, IEnumerable<DungeonArchetype>>) (x => (IEnumerable<DungeonArchetype>) x.DungeonArchetypes)).ToArray<DungeonArchetype>();
    }

    public TileSet[] GetUsedTileSets()
    {
      List<TileSet> tileSetList = new List<TileSet>();
      foreach (GraphNode node in this.Nodes)
        tileSetList.AddRange((IEnumerable<TileSet>) node.TileSets);
      foreach (GraphLine line in this.Lines)
      {
        foreach (DungeonArchetype dungeonArchetype in line.DungeonArchetypes)
        {
          tileSetList.AddRange((IEnumerable<TileSet>) dungeonArchetype.TileSets);
          tileSetList.AddRange((IEnumerable<TileSet>) dungeonArchetype.BranchCapTileSets);
        }
      }
      return tileSetList.ToArray();
    }

    public bool ShouldPruneTileWithTags(TagContainer tileTags)
    {
      switch (this.BranchTagPruneMode)
      {
        case DungeonFlow.BranchPruneMode.AnyTagPresent:
          return tileTags.HasAnyTag(this.BranchPruneTags.ToArray());
        case DungeonFlow.BranchPruneMode.AllTagsMissing:
          return !tileTags.HasAnyTag(this.BranchPruneTags.ToArray());
        default:
          throw new NotImplementedException(string.Format("BranchPruneMode {0} is not implemented", (object) this.BranchTagPruneMode));
      }
    }

    public void OnBeforeSerialize() => this.currentFileVersion = 1;

    public void OnAfterDeserialize()
    {
      if (this.currentFileVersion >= 1)
        return;
      for (int index = 0; index < this.globalPropGroupID_obsolete.Count; ++index)
        this.GlobalProps.Add(new DungeonFlow.GlobalPropSettings(this.globalPropGroupID_obsolete[index], this.globalPropRanges_obsolete[index]));
      this.globalPropGroupID_obsolete.Clear();
      this.globalPropRanges_obsolete.Clear();
    }

    public bool CanTilesConnect(Tile tileA, Tile tileB)
    {
      if ((UnityEngine.Object) tileA == (UnityEngine.Object) null || (UnityEngine.Object) tileB == (UnityEngine.Object) null)
        return false;
      if (this.TileConnectionTags.Count == 0)
        return true;
      switch (this.TileTagConnectionMode)
      {
        case DungeonFlow.TagConnectionMode.Accept:
          return this.HasMatchingTagPair(tileA, tileB);
        case DungeonFlow.TagConnectionMode.Reject:
          return !this.HasMatchingTagPair(tileA, tileB);
        default:
          throw new NotImplementedException(string.Format("{0}.{1} is not implemented", (object) typeof (DungeonFlow.TagConnectionMode).Name, (object) this.TileTagConnectionMode));
      }
    }

    public bool CanDoorwaysConnect(Tile tileA, Tile tileB, Doorway doorwayA, Doorway doorwayB)
    {
      foreach (TileConnectionRule tileConnectionRule in (IEnumerable<TileConnectionRule>) DoorwayPairFinder.CustomConnectionRules.OrderByDescending<TileConnectionRule, int>((Func<TileConnectionRule, int>) (r => r.Priority)))
      {
        TileConnectionRule.ConnectionResult connectionResult = tileConnectionRule.Delegate(tileA, tileB, doorwayA, doorwayB);
        if (connectionResult != TileConnectionRule.ConnectionResult.Passthrough)
          return connectionResult == TileConnectionRule.ConnectionResult.Allow;
      }
      return DoorwaySocket.CanSocketsConnect(doorwayA.Socket, doorwayB.Socket) && this.CanTilesConnect(tileA, tileB);
    }

    private bool HasMatchingTagPair(Tile tileA, Tile tileB)
    {
      foreach (TagPair tileConnectionTag in this.TileConnectionTags)
      {
        if (tileA.Tags.HasTag(tileConnectionTag.TagA) && tileB.Tags.HasTag(tileConnectionTag.TagB) || tileB.Tags.HasTag(tileConnectionTag.TagA) && tileA.Tags.HasTag(tileConnectionTag.TagB))
          return true;
      }
      return false;
    }

    [Serializable]
    public sealed class GlobalPropSettings
    {
      public int ID;
      public IntRange Count;

      public GlobalPropSettings()
      {
        this.ID = 0;
        this.Count = new IntRange(0, 1);
      }

      public GlobalPropSettings(int id, IntRange count)
      {
        this.ID = id;
        this.Count = count;
      }
    }

    public enum TagConnectionMode
    {
      Accept,
      Reject,
    }

    public enum BranchPruneMode
    {
      AnyTagPresent,
      AllTagsMissing,
    }
  }
}
