// Decompiled with JetBrains decompiler
// Type: DunGen.DoorwayConnection
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace DunGen
{
  public sealed class DoorwayConnection
  {
    public Doorway A { get; private set; }

    public Doorway B { get; private set; }

    public DoorwayConnection(Doorway a, Doorway b)
    {
      this.A = a;
      this.B = b;
    }
  }
}
