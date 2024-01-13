// Decompiled with JetBrains decompiler
// Type: DunGen.Graph.GraphLine
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace DunGen.Graph
{
  [Serializable]
  public class GraphLine
  {
    public DungeonFlow Graph;
    public List<DungeonArchetype> DungeonArchetypes = new List<DungeonArchetype>();
    public float Position;
    public float Length;
    public List<KeyLockPlacement> Keys = new List<KeyLockPlacement>();
    public List<KeyLockPlacement> Locks = new List<KeyLockPlacement>();

    public GraphLine(DungeonFlow graph) => this.Graph = graph;

    public DungeonArchetype GetRandomArchetype(
      RandomStream randomStream,
      IList<DungeonArchetype> usedArchetypes)
    {
      IEnumerable<DungeonArchetype> source = this.DungeonArchetypes.Where<DungeonArchetype>((Func<DungeonArchetype, bool>) (a => !a.Unique || !usedArchetypes.Contains(a)));
      if (!source.Any<DungeonArchetype>())
        source = (IEnumerable<DungeonArchetype>) this.DungeonArchetypes;
      int index = randomStream.Next(0, source.Count<DungeonArchetype>());
      return source.ElementAt<DungeonArchetype>(index);
    }
  }
}
