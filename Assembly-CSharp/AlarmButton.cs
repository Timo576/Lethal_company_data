// Decompiled with JetBrains decompiler
// Type: AlarmButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class AlarmButton : MonoBehaviour
{
  private Animator buttonAnimator;
  public float timeSincePushing;

  public void PushAlarmButton()
  {
    if ((double) this.timeSincePushing < 1.0)
      return;
    this.buttonAnimator.SetTrigger("press");
    HUDManager.Instance.TriggerAlarmHornEffect();
  }

  private void Update()
  {
    if ((double) this.timeSincePushing > 5.0)
      return;
    this.timeSincePushing += Time.deltaTime;
  }
}
