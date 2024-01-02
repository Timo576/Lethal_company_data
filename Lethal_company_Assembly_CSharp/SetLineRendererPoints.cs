﻿// Decompiled with JetBrains decompiler
// Type: SetLineRendererPoints
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class SetLineRendererPoints : MonoBehaviour
{
  private LineRenderer lineRenderer;
  public Transform anchor;
  public Transform target;

  private void Start() => this.lineRenderer = this.gameObject.GetComponent<LineRenderer>();

  private void LateUpdate()
  {
    this.lineRenderer.SetPosition(0, this.anchor.position);
    this.lineRenderer.SetPosition(1, this.target.position);
  }
}
