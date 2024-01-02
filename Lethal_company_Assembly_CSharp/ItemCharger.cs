// Decompiled with JetBrains decompiler
// Type: ItemCharger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class ItemCharger : MonoBehaviour
{
  public InteractTrigger triggerScript;
  public Animator chargeStationAnimator;
  private Coroutine chargeItemCoroutine;
  public AudioSource zapAudio;
  private float updateInterval;

  public void ChargeItem()
  {
    GrabbableObject heldObjectServer = GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer;
    if ((Object) heldObjectServer == (Object) null || !heldObjectServer.itemProperties.requiresBattery)
      return;
    if (this.chargeItemCoroutine != null)
      this.StopCoroutine(this.chargeItemCoroutine);
    this.chargeItemCoroutine = this.StartCoroutine(this.chargeItemDelayed(heldObjectServer));
  }

  private void Update()
  {
    if ((Object) NetworkManager.Singleton == (Object) null)
      return;
    if ((double) this.updateInterval > 1.0)
    {
      this.updateInterval = 0.0f;
      if (!((Object) GameNetworkManager.Instance != (Object) null) || !((Object) GameNetworkManager.Instance.localPlayerController != (Object) null))
        return;
      this.triggerScript.interactable = (Object) GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer != (Object) null && GameNetworkManager.Instance.localPlayerController.currentlyHeldObjectServer.itemProperties.requiresBattery;
    }
    else
      this.updateInterval += Time.deltaTime;
  }

  private IEnumerator chargeItemDelayed(GrabbableObject itemToCharge)
  {
    this.zapAudio.Play();
    yield return (object) new WaitForSeconds(0.75f);
    this.chargeStationAnimator.SetTrigger("zap");
    itemToCharge.insertedBattery = new Battery(false, 1f);
    itemToCharge.SyncBatteryServerRpc(100);
  }
}
