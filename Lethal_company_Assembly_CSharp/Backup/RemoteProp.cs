// Decompiled with JetBrains decompiler
// Type: RemoteProp
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class RemoteProp : GrabbableObject
{
  public AudioSource remoteAudio;

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    this.remoteAudio.PlayOneShot(this.remoteAudio.clip);
    WalkieTalkie.TransmitOneShotAudio(this.remoteAudio, this.remoteAudio.clip, 0.7f);
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, 8f, 0.4f, noiseIsInsideClosedShip: this.isInElevator && StartOfRound.Instance.hangarDoorsClosed);
    Object.FindObjectOfType<ShipLights>().ToggleShipLightsOnLocalClientOnly();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (RemoteProp);
}
