// Decompiled with JetBrains decompiler
// Type: facePlayerOnAxis
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
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
