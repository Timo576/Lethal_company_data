// Decompiled with JetBrains decompiler
// Type: TransformAndName
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
