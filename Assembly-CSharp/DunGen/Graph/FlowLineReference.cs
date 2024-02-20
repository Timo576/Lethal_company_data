// Decompiled with JetBrains decompiler
// Type: DunGen.Graph.FlowLineReference
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen.Graph
{
  [Serializable]
  public sealed class FlowLineReference : FlowGraphObjectReference
  {
    public GraphLine Line
    {
      get => this.flow.Lines[this.index];
      set => this.index = this.flow.Lines.IndexOf(value);
    }

    public FlowLineReference(DungeonFlow flowGraph, GraphLine line)
    {
      this.flow = flowGraph;
      this.Line = line;
    }
  }
}
