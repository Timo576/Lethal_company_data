// Decompiled with JetBrains decompiler
// Type: DunGen.Graph.DungeonFlowBuilder
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace DunGen.Graph
{
  public sealed class DungeonFlowBuilder
  {
    private List<GraphLine> lines = new List<GraphLine>();
    private List<GraphNode> nodes = new List<GraphNode>();
    private float currentPosition;

    public DungeonFlow Flow { get; private set; }

    public DungeonFlowBuilder(DungeonFlow flow) => this.Flow = flow;

    public DungeonFlowBuilder AddLine(
      DungeonArchetype archetype,
      float length = 1f,
      IEnumerable<KeyLockPlacement> locks = null,
      IEnumerable<KeyLockPlacement> keys = null)
    {
      return this.AddLine((IEnumerable<DungeonArchetype>) new DungeonArchetype[1]
      {
        archetype
      }, length, locks, keys);
    }

    public DungeonFlowBuilder AddLine(
      IEnumerable<DungeonArchetype> archetypes,
      float length = 1f,
      IEnumerable<KeyLockPlacement> locks = null,
      IEnumerable<KeyLockPlacement> keys = null)
    {
      if ((double) length <= 0.0)
        throw new ArgumentOutOfRangeException("Length must be grater than zero");
      GraphLine graphLine = new GraphLine(this.Flow);
      graphLine.Position = this.currentPosition;
      graphLine.Length = length;
      if (archetypes != null && archetypes.Any<DungeonArchetype>())
        graphLine.DungeonArchetypes.AddRange(archetypes);
      if (locks != null && locks.Any<KeyLockPlacement>())
        graphLine.Locks.AddRange(locks);
      if (keys != null && keys.Any<KeyLockPlacement>())
        graphLine.Keys.AddRange(keys);
      this.lines.Add(graphLine);
      this.currentPosition += length;
      return this;
    }

    public DungeonFlowBuilder ContinueLine(float length = 1f)
    {
      if (this.lines.Count == 0)
        throw new Exception("Cannot call ContinueLine(..) before AddLine(..)");
      this.lines.Last<GraphLine>().Length += length;
      this.currentPosition += length;
      return this;
    }

    public DungeonFlowBuilder AddNode(
      TileSet tileSet,
      string label = null,
      bool allowLocksOnEntrance = false,
      bool allowLocksOnExit = false,
      IEnumerable<KeyLockPlacement> locks = null,
      IEnumerable<KeyLockPlacement> keys = null)
    {
      return this.AddNode((IEnumerable<TileSet>) new TileSet[1]
      {
        tileSet
      }, label, allowLocksOnEntrance, allowLocksOnExit, locks, keys);
    }

    public DungeonFlowBuilder AddNode(
      IEnumerable<TileSet> tileSets,
      string label = null,
      bool allowLocksOnEntrance = false,
      bool allowLocksOnExit = false,
      IEnumerable<KeyLockPlacement> locks = null,
      IEnumerable<KeyLockPlacement> keys = null)
    {
      GraphNode graphNode = new GraphNode(this.Flow);
      graphNode.Label = label == null ? "Node" : label;
      graphNode.Position = this.currentPosition;
      graphNode.NodeType = NodeType.Normal;
      if (allowLocksOnEntrance)
        graphNode.LockPlacement |= NodeLockPlacement.Entrance;
      if (allowLocksOnExit)
        graphNode.LockPlacement |= NodeLockPlacement.Exit;
      if (tileSets != null && tileSets.Any<TileSet>())
        graphNode.TileSets.AddRange(tileSets);
      if (locks != null && locks.Any<KeyLockPlacement>())
        graphNode.Locks.AddRange(locks);
      if (keys != null && keys.Any<KeyLockPlacement>())
        graphNode.Keys.AddRange(keys);
      this.nodes.Add(graphNode);
      return this;
    }

    public DungeonFlowBuilder Complete()
    {
      if (this.lines.Count == 0)
        throw new Exception("DungeonFlowBuilder must have at least one line added before finalizing");
      if (this.nodes.Count < 2)
        throw new Exception("DungeonFlowBuilder must have at least two nodes added before finalizing");
      float currentPosition = this.currentPosition;
      this.currentPosition = 1f;
      foreach (GraphLine line in this.lines)
      {
        line.Position /= currentPosition;
        line.Length /= currentPosition;
      }
      foreach (GraphNode node in this.nodes)
        node.Position /= currentPosition;
      this.nodes.First<GraphNode>().NodeType = NodeType.Start;
      this.nodes.Last<GraphNode>().NodeType = NodeType.Goal;
      this.Flow.Lines.Clear();
      this.Flow.Nodes.Clear();
      this.Flow.Lines.AddRange((IEnumerable<GraphLine>) this.lines);
      this.Flow.Nodes.AddRange((IEnumerable<GraphNode>) this.nodes);
      return this;
    }
  }
}
