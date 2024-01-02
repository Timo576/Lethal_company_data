// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonGraphConnection
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace DunGen
{
  public sealed class DungeonGraphConnection : DungeonGraphObject
  {
    public DungeonGraphNode A { get; private set; }

    public DungeonGraphNode B { get; private set; }

    public Doorway DoorwayA { get; private set; }

    public Doorway DoorwayB { get; private set; }

    public DungeonGraphConnection(
      DungeonGraphNode a,
      DungeonGraphNode b,
      Doorway doorwayA,
      Doorway doorwayB)
    {
      this.A = a;
      this.B = b;
      this.DoorwayA = doorwayA;
      this.DoorwayB = doorwayB;
      a.AddConnection(this);
      b.AddConnection(this);
    }
  }
}
