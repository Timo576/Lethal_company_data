// Decompiled with JetBrains decompiler
// Type: facePlayerOnAxis
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class facePlayerOnAxis : MonoBehaviour
{
  private Transform playerCamera;
  public Transform turnAxis;
  private bool gotPlayer;

  private void Update()
  {
    if (!this.gotPlayer)
    {
      if (!((Object) GameNetworkManager.Instance != (Object) null) || !GameNetworkManager.Instance.gameHasStarted)
        return;
      this.playerCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform;
      this.gotPlayer = true;
    }
    else
      this.transform.LookAt(this.playerCamera, this.turnAxis.up);
  }
}
