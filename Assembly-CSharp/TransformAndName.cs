// Decompiled with JetBrains decompiler
// Type: TransformAndName
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
[Serializable]
public class TransformAndName
{
  public Transform transform;
  public string name;
  public bool isNonPlayer;

  public TransformAndName(Transform newTransform, string newName, bool nonPlayer = false)
  {
    this.name = newName;
    this.transform = newTransform;
    this.isNonPlayer = nonPlayer;
  }
}
