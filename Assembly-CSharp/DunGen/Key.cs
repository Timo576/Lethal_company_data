// Decompiled with JetBrains decompiler
// Type: DunGen.Key
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [Serializable]
  public sealed class Key
  {
    public GameObject Prefab;
    public Color Colour;
    public IntRange KeysPerLock = new IntRange(1, 1);
    [SerializeField]
    private int id;
    [SerializeField]
    private string name;

    public int ID
    {
      get => this.id;
      set => this.id = value;
    }

    public string Name
    {
      get => this.name;
      internal set => this.name = value;
    }

    internal Key(int id) => this.id = id;
  }
}
