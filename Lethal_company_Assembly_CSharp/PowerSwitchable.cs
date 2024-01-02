// Decompiled with JetBrains decompiler
// Type: PowerSwitchable
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class PowerSwitchable : MonoBehaviour
{
  public OnSwitchPowerEvent powerSwitchEvent;

  public void OnPowerSwitch(bool switchedOn)
  {
    Debug.Log((object) "Power switched event invoked by powerswitchable");
    this.powerSwitchEvent.Invoke(switchedOn);
  }

  private void OnEnable()
  {
    RoundManager.Instance.onPowerSwitch.AddListener(new UnityAction<bool>(this.OnPowerSwitch));
    Debug.Log((object) "Added listener to power switch event");
  }

  private void OnDisable()
  {
    RoundManager.Instance.onPowerSwitch.RemoveListener(new UnityAction<bool>(this.OnPowerSwitch));
  }
}
