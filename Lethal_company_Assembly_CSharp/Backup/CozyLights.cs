// Decompiled with JetBrains decompiler
// Type: CozyLights
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class CozyLights : MonoBehaviour
{
  private bool cozyLightsOn;
  public Animator cozyLightsAnimator;

  private void Update()
  {
    if ((Object) StartOfRound.Instance == (Object) null || StartOfRound.Instance.shipRoomLights.areLightsOn != this.cozyLightsOn)
      return;
    this.cozyLightsOn = !this.cozyLightsOn;
    this.cozyLightsAnimator.SetBool("on", this.cozyLightsOn);
  }
}
