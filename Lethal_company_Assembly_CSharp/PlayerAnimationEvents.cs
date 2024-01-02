// Decompiled with JetBrains decompiler
// Type: PlayerAnimationEvents
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
public class PlayerAnimationEvents : MonoBehaviour
{
  public PlayerControllerB thisPlayerController;

  public void PlayFootstepServer() => this.thisPlayerController.PlayFootstepServer();

  public void PlayFootstepLocal() => this.thisPlayerController.PlayFootstepLocal();

  public void LimpForward() => this.thisPlayerController.LimpAnimationSpeed();

  public void LockArmsToCamera() => this.thisPlayerController.localArmsMatchCamera = true;

  public void UnlockArmsFromCamera() => this.thisPlayerController.localArmsMatchCamera = false;
}
