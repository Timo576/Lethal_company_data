﻿// Decompiled with JetBrains decompiler
// Type: DunGen.ProxyDoorwayConnection
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace DunGen
{
  public struct ProxyDoorwayConnection
  {
    public DoorwayProxy A { get; private set; }

    public DoorwayProxy B { get; private set; }

    public ProxyDoorwayConnection(DoorwayProxy a, DoorwayProxy b)
    {
      this.A = a;
      this.B = b;
    }
  }
}
