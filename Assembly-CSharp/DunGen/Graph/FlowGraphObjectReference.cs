// Decompiled with JetBrains decompiler
// Type: DunGen.Graph.FlowGraphObjectReference
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DunGen.Graph
{
  [Serializable]
  public abstract class FlowGraphObjectReference
  {
    [SerializeField]
    protected DungeonFlow flow;
    [SerializeField]
    protected int index;

    public DungeonFlow Flow => this.flow;
  }
}
