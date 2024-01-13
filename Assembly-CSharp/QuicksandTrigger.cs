// Decompiled with JetBrains decompiler
// Type: QuicksandTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
public class QuicksandTrigger : MonoBehaviour
{
  public bool isWater;
  public int audioClipIndex;
  [Space(5f)]
  public bool sinkingLocalPlayer;
  public float movementHinderance = 1.6f;
  public float sinkingSpeedMultiplier = 0.15f;

  private void OnTriggerStay(Collider other)
  {
    if (this.isWater)
    {
      if (!other.gameObject.CompareTag("Player"))
        return;
      PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
      if ((Object) component != (Object) GameNetworkManager.Instance.localPlayerController && (Object) component != (Object) null && (Object) component.underwaterCollider != (Object) this)
      {
        component.underwaterCollider = this.gameObject.GetComponent<Collider>();
        return;
      }
    }
    if (GameNetworkManager.Instance.localPlayerController.isInsideFactory || GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom || !this.isWater && !other.gameObject.CompareTag("Player"))
      return;
    PlayerControllerB component1 = other.gameObject.GetComponent<PlayerControllerB>();
    if ((Object) component1 != (Object) GameNetworkManager.Instance.localPlayerController)
      return;
    if (this.isWater && !component1.isUnderwater)
    {
      component1.underwaterCollider = this.gameObject.GetComponent<Collider>();
      component1.isUnderwater = true;
    }
    component1.statusEffectAudioIndex = this.audioClipIndex;
    if (component1.isSinking)
      return;
    if (this.sinkingLocalPlayer)
    {
      if (component1.CheckConditionsForSinkingInQuicksand())
        return;
      this.StopSinkingLocalPlayer(component1);
    }
    else
    {
      if (!component1.CheckConditionsForSinkingInQuicksand())
        return;
      Debug.Log((object) "Set local player to sinking!");
      this.sinkingLocalPlayer = true;
      ++component1.sourcesCausingSinking;
      ++component1.isMovementHindered;
      component1.hinderedMultiplier *= this.movementHinderance;
      if (this.isWater)
        component1.sinkingSpeedMultiplier = 0.0f;
      else
        component1.sinkingSpeedMultiplier = this.sinkingSpeedMultiplier;
    }
  }

  private void OnTriggerExit(Collider other) => this.OnExit(other);

  public void OnExit(Collider other)
  {
    if (!this.sinkingLocalPlayer)
    {
      if (!this.isWater || !other.CompareTag("Player") || (Object) other.gameObject.GetComponent<PlayerControllerB>() == (Object) GameNetworkManager.Instance.localPlayerController)
        return;
      other.gameObject.GetComponent<PlayerControllerB>().isUnderwater = false;
    }
    else
    {
      if (!other.CompareTag("Player"))
        return;
      PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
      if ((Object) component != (Object) GameNetworkManager.Instance.localPlayerController)
        return;
      this.StopSinkingLocalPlayer(component);
    }
  }

  public void StopSinkingLocalPlayer(PlayerControllerB playerScript)
  {
    if (!this.sinkingLocalPlayer)
      return;
    this.sinkingLocalPlayer = false;
    playerScript.sourcesCausingSinking = Mathf.Clamp(playerScript.sourcesCausingSinking - 1, 0, 100);
    playerScript.isMovementHindered = Mathf.Clamp(playerScript.isMovementHindered - 1, 0, 100);
    playerScript.hinderedMultiplier = Mathf.Clamp(playerScript.hinderedMultiplier / this.movementHinderance, 1f, 100f);
    if (playerScript.isMovementHindered != 0 || !this.isWater)
      return;
    playerScript.isUnderwater = false;
  }
}
