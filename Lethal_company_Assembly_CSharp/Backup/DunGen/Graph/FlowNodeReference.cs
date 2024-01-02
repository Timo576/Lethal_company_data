﻿// Decompiled with JetBrains decompiler
// Type: DunGen.Graph.FlowNodeReference
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen.Graph
{
  [Serializable]
  public sealed class FlowNodeReference : FlowGraphObjectReference
  {
    public GraphNode Node
    {
      get => this.flow.Nodes[this.index];
      set => this.index = this.flow.Nodes.IndexOf(value);
    }

    public FlowNodeReference(DungeonFlow flowGraph, GraphNode node)
    {
      this.flow = flowGraph;
      this.Node = node;
    }
  }
}
