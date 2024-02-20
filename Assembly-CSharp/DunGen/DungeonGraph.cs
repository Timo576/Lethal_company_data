// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonGraph
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;

#nullable disable
namespace DunGen
{
  public class DungeonGraph
  {
    public readonly List<DungeonGraphNode> Nodes = new List<DungeonGraphNode>();
    public readonly List<DungeonGraphConnection> Connections = new List<DungeonGraphConnection>();

    public DungeonGraph(Dungeon dungeon)
    {
      Dictionary<Tile, DungeonGraphNode> dictionary = new Dictionary<Tile, DungeonGraphNode>();
      foreach (Tile allTile in dungeon.AllTiles)
      {
        DungeonGraphNode dungeonGraphNode = new DungeonGraphNode(allTile);
        dictionary[allTile] = dungeonGraphNode;
        this.Nodes.Add(dungeonGraphNode);
      }
      foreach (DoorwayConnection connection in dungeon.Connections)
        this.Connections.Add(new DungeonGraphConnection(dictionary[connection.A.Tile], dictionary[connection.B.Tile], connection.A, connection.B));
    }
  }
}
