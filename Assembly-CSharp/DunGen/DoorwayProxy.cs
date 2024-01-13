// Decompiled with JetBrains decompiler
// Type: DunGen.DoorwayProxy
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DunGen
{
  public sealed class DoorwayProxy
  {
    public bool Used => this.ConnectedDoorway != null;

    public TileProxy TileProxy { get; private set; }

    public int Index { get; private set; }

    public DoorwaySocket Socket { get; private set; }

    public Doorway DoorwayComponent { get; private set; }

    public Vector3 LocalPosition { get; private set; }

    public Quaternion LocalRotation { get; private set; }

    public DoorwayProxy ConnectedDoorway { get; private set; }

    public Vector3 Forward
    {
      get => this.TileProxy.Placement.Rotation * this.LocalRotation * Vector3.forward;
    }

    public Vector3 Up => this.TileProxy.Placement.Rotation * this.LocalRotation * Vector3.up;

    public Vector3 Position => this.TileProxy.Placement.Transform.MultiplyPoint(this.LocalPosition);

    public DoorwayProxy(TileProxy tileProxy, DoorwayProxy other)
    {
      this.TileProxy = tileProxy;
      this.Index = other.Index;
      this.Socket = other.Socket;
      this.DoorwayComponent = other.DoorwayComponent;
      this.LocalPosition = other.LocalPosition;
      this.LocalRotation = other.LocalRotation;
    }

    public DoorwayProxy(
      TileProxy tileProxy,
      int index,
      Doorway doorwayComponent,
      Vector3 localPosition,
      Quaternion localRotation)
    {
      this.TileProxy = tileProxy;
      this.Index = index;
      this.Socket = doorwayComponent.Socket;
      this.DoorwayComponent = doorwayComponent;
      this.LocalPosition = localPosition;
      this.LocalRotation = localRotation;
    }

    public static void Connect(DoorwayProxy a, DoorwayProxy b)
    {
      a.ConnectedDoorway = b;
      b.ConnectedDoorway = a;
    }

    public void Disconnect()
    {
      if (this.ConnectedDoorway == null)
        return;
      this.ConnectedDoorway.ConnectedDoorway = (DoorwayProxy) null;
      this.ConnectedDoorway = (DoorwayProxy) null;
    }
  }
}
