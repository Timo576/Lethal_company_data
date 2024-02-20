// Decompiled with JetBrains decompiler
// Type: DunGen.Door
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [Serializable]
  public class Door : MonoBehaviour
  {
    [HideInInspector]
    public Dungeon Dungeon;
    [HideInInspector]
    public Doorway DoorwayA;
    [HideInInspector]
    public Doorway DoorwayB;
    [HideInInspector]
    public Tile TileA;
    [HideInInspector]
    public Tile TileB;
    [SerializeField]
    private bool dontCullBehind;
    [SerializeField]
    private bool isOpen;

    public bool DontCullBehind
    {
      get => this.dontCullBehind;
      set
      {
        if (this.dontCullBehind == value)
          return;
        this.dontCullBehind = value;
        this.SetDoorState(this.isOpen);
      }
    }

    public bool ShouldCullBehind => !this.DontCullBehind && !this.isOpen;

    public virtual bool IsOpen
    {
      get => this.isOpen;
      set
      {
        if (this.isOpen == value)
          return;
        this.SetDoorState(value);
      }
    }

    public event Door.DoorStateChangedDelegate OnDoorStateChanged;

    private void OnDestroy() => this.OnDoorStateChanged = (Door.DoorStateChangedDelegate) null;

    public void SetDoorState(bool isOpen)
    {
      this.isOpen = isOpen;
      if (this.OnDoorStateChanged == null)
        return;
      this.OnDoorStateChanged(this, isOpen);
    }

    public delegate void DoorStateChangedDelegate(Door door, bool isOpen);
  }
}
