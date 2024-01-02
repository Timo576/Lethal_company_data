// Decompiled with JetBrains decompiler
// Type: animatedSun
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class animatedSun : MonoBehaviour
{
  public Light indirectLight;
  public Light directLight;

  private void Start()
  {
    TimeOfDay objectOfType = Object.FindObjectOfType<TimeOfDay>();
    if (!((Object) objectOfType != (Object) null))
      return;
    objectOfType.sunAnimator = this.gameObject.GetComponent<Animator>();
    objectOfType.sunIndirect = this.indirectLight;
    objectOfType.sunDirect = this.directLight;
  }

  private void Update()
  {
  }
}
