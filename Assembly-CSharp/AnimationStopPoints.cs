// Decompiled with JetBrains decompiler
// Type: AnimationStopPoints
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class AnimationStopPoints : MonoBehaviour
{
  public bool canAnimationStop;
  public int animationPosition = 1;

  public void SetAnimationStopPosition1()
  {
    this.canAnimationStop = true;
    this.animationPosition = 1;
  }

  public void SetAnimationGo() => this.canAnimationStop = false;

  public void SetAnimationStopPosition2()
  {
    this.canAnimationStop = true;
    this.animationPosition = 2;
  }
}
