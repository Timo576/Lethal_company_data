// Decompiled with JetBrains decompiler
// Type: DunGen.Graph.GraphNode
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace DunGen.Graph
{
  [Serializable]
  public class GraphNode
  {
    public DungeonFlow Graph;
    public List<TileSet> TileSets = new List<TileSet>();
    public NodeType NodeType;
    public float Position;
    public string Label;
    public List<KeyLockPlacement> Keys = new List<KeyLockPlacement>();
    public List<KeyLockPlacement> Locks = new List<KeyLockPlacement>();
    public NodeLockPlacement LockPlacement;

    public GraphNode(DungeonFlow graph) => this.Graph = graph;
  }
}
