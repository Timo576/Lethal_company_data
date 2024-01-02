// Decompiled with JetBrains decompiler
// Type: HoarderBugItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class HoarderBugItem
{
  public GrabbableObject itemGrabbableObject;
  public Vector3 itemNestPosition;
  public HoarderBugItemStatus status;

  public HoarderBugItem(
    GrabbableObject newObject,
    HoarderBugItemStatus newStatus,
    Vector3 bugNestPosition)
  {
    this.itemGrabbableObject = newObject;
    this.status = newStatus;
    this.itemNestPosition = bugNestPosition;
  }
}
