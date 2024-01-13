// Decompiled with JetBrains decompiler
// Type: PowerSwitchable
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class PowerSwitchable : MonoBehaviour
{
  public OnSwitchPowerEvent powerSwitchEvent;

  public void OnPowerSwitch(bool switchedOn) => this.powerSwitchEvent.Invoke(switchedOn);

  private void OnEnable()
  {
    RoundManager.Instance.onPowerSwitch.AddListener(new UnityAction<bool>(this.OnPowerSwitch));
  }

  private void OnDisable()
  {
    RoundManager.Instance.onPowerSwitch.RemoveListener(new UnityAction<bool>(this.OnPowerSwitch));
  }
}
