// Decompiled with JetBrains decompiler
// Type: DunGen.GameObjectWeight
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [Serializable]
  public sealed class GameObjectWeight
  {
    public GameObject GameObject;
    public float Weight = 1f;

    public GameObjectWeight()
    {
    }

    public GameObjectWeight(GameObject gameObject, float weight = 1f)
    {
      this.GameObject = gameObject;
      this.Weight = weight;
    }
  }
}
