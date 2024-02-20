// Decompiled with JetBrains decompiler
// Type: MatchLocalPlayerPosition
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class MatchLocalPlayerPosition : MonoBehaviour
{
  private void LateUpdate()
  {
    if (!((Object) GameNetworkManager.Instance != (Object) null) || !((Object) GameNetworkManager.Instance.localPlayerController != (Object) null))
      return;
    this.transform.position = GameNetworkManager.Instance.localPlayerController.transform.position;
  }
}
