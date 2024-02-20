// Decompiled with JetBrains decompiler
// Type: Shovel
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class Shovel : GrabbableObject
{
  public int shovelHitForce = 1;
  public bool reelingUp;
  public bool isHoldingButton;
  private RaycastHit rayHit;
  private Coroutine reelingUpCoroutine;
  private RaycastHit[] objectsHitByShovel;
  private List<RaycastHit> objectsHitByShovelList = new List<RaycastHit>();
  public AudioClip reelUp;
  public AudioClip swing;
  public AudioClip[] hitSFX;
  public AudioSource shovelAudio;
  private PlayerControllerB previousPlayerHeldBy;
  private int shovelMask = 11012424;

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    if ((UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null)
      return;
    Debug.Log((object) string.Format("Is player pressing down button?: {0}", (object) buttonDown));
    this.isHoldingButton = buttonDown;
    Debug.Log((object) ("PLAYER ACTIVATED ITEM TO HIT WITH SHOVEL. Who sent this log: " + GameNetworkManager.Instance.localPlayerController.gameObject.name));
    if (this.reelingUp || !buttonDown)
      return;
    this.reelingUp = true;
    this.previousPlayerHeldBy = this.playerHeldBy;
    Debug.Log((object) string.Format("Set previousPlayerHeldBy: {0}", (object) this.previousPlayerHeldBy));
    if (this.reelingUpCoroutine != null)
      this.StopCoroutine(this.reelingUpCoroutine);
    this.reelingUpCoroutine = this.StartCoroutine(this.reelUpShovel());
  }

  private IEnumerator reelUpShovel()
  {
    Shovel shovel = this;
    shovel.playerHeldBy.activatingItem = true;
    shovel.playerHeldBy.twoHanded = true;
    shovel.playerHeldBy.playerBodyAnimator.ResetTrigger("shovelHit");
    shovel.playerHeldBy.playerBodyAnimator.SetBool("reelingUp", true);
    shovel.shovelAudio.PlayOneShot(shovel.reelUp);
    shovel.ReelUpSFXServerRpc();
    yield return (object) new WaitForSeconds(0.35f);
    // ISSUE: reference to a compiler-generated method
    yield return (object) new WaitUntil(new Func<bool>(shovel.\u003CreelUpShovel\u003Eb__14_0));
    shovel.SwingShovel(!shovel.isHeld);
    yield return (object) new WaitForSeconds(0.13f);
    yield return (object) new WaitForEndOfFrame();
    shovel.HitShovel(!shovel.isHeld);
    yield return (object) new WaitForSeconds(0.3f);
    shovel.reelingUp = false;
    shovel.reelingUpCoroutine = (Coroutine) null;
  }

  [ServerRpc]
  public void ReelUpSFXServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4113335123U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 4113335123U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ReelUpSFXClientRpc();
  }

  [ClientRpc]
  public void ReelUpSFXClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2042054613U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2042054613U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.shovelAudio.PlayOneShot(this.reelUp);
  }

  public override void DiscardItem()
  {
    if ((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null)
      this.playerHeldBy.activatingItem = false;
    base.DiscardItem();
  }

  public void SwingShovel(bool cancel = false)
  {
    this.previousPlayerHeldBy.playerBodyAnimator.SetBool("reelingUp", false);
    if (cancel)
      return;
    this.shovelAudio.PlayOneShot(this.swing);
    this.previousPlayerHeldBy.UpdateSpecialAnimationValue(true, (short) this.previousPlayerHeldBy.transform.localEulerAngles.y, 0.4f);
  }

  public void HitShovel(bool cancel = false)
  {
    if ((UnityEngine.Object) this.previousPlayerHeldBy == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "Previousplayerheldby is null on this client when HitShovel is called.");
    }
    else
    {
      this.previousPlayerHeldBy.activatingItem = false;
      bool flag1 = false;
      bool flag2 = false;
      int hitSurfaceID = -1;
      if (!cancel)
      {
        this.previousPlayerHeldBy.twoHanded = false;
        this.objectsHitByShovel = Physics.SphereCastAll(this.previousPlayerHeldBy.gameplayCamera.transform.position + this.previousPlayerHeldBy.gameplayCamera.transform.right * -0.35f, 0.8f, this.previousPlayerHeldBy.gameplayCamera.transform.forward, 1.5f, this.shovelMask, QueryTriggerInteraction.Collide);
        this.objectsHitByShovelList = ((IEnumerable<RaycastHit>) this.objectsHitByShovel).OrderBy<RaycastHit, float>((Func<RaycastHit, float>) (x => x.distance)).ToList<RaycastHit>();
        for (int index1 = 0; index1 < this.objectsHitByShovelList.Count; ++index1)
        {
          RaycastHit objectsHitByShovel = this.objectsHitByShovelList[index1];
          if (objectsHitByShovel.transform.gameObject.layer != 8)
          {
            objectsHitByShovel = this.objectsHitByShovelList[index1];
            if (objectsHitByShovel.transform.gameObject.layer != 11)
            {
              objectsHitByShovel = this.objectsHitByShovelList[index1];
              IHittable component;
              if (objectsHitByShovel.transform.TryGetComponent<IHittable>(out component))
              {
                objectsHitByShovel = this.objectsHitByShovelList[index1];
                if (!((UnityEngine.Object) objectsHitByShovel.transform == (UnityEngine.Object) this.previousPlayerHeldBy.transform))
                {
                  objectsHitByShovel = this.objectsHitByShovelList[index1];
                  if (!(objectsHitByShovel.point == Vector3.zero))
                  {
                    Vector3 position = this.previousPlayerHeldBy.gameplayCamera.transform.position;
                    objectsHitByShovel = this.objectsHitByShovelList[index1];
                    Vector3 point = objectsHitByShovel.point;
                    RaycastHit raycastHit;
                    ref RaycastHit local = ref raycastHit;
                    int roomMaskAndDefault = StartOfRound.Instance.collidersAndRoomMaskAndDefault;
                    if (Physics.Linecast(position, point, out local, roomMaskAndDefault))
                      continue;
                  }
                  flag1 = true;
                  Vector3 forward = this.previousPlayerHeldBy.gameplayCamera.transform.forward;
                  objectsHitByShovel = this.objectsHitByShovelList[index1];
                  Debug.DrawRay(objectsHitByShovel.point, Vector3.up * 0.25f, Color.green, 5f);
                  try
                  {
                    component.Hit(this.shovelHitForce, forward, this.previousPlayerHeldBy, true);
                    flag2 = true;
                    continue;
                  }
                  catch (Exception ex)
                  {
                    Debug.Log((object) string.Format("Exception caught when hitting object with shovel from player #{0}: {1}", (object) this.previousPlayerHeldBy.playerClientId, (object) ex));
                    continue;
                  }
                }
                else
                  continue;
              }
              else
                continue;
            }
          }
          flag1 = true;
          objectsHitByShovel = this.objectsHitByShovelList[index1];
          string tag = objectsHitByShovel.collider.gameObject.tag;
          for (int index2 = 0; index2 < StartOfRound.Instance.footstepSurfaces.Length; ++index2)
          {
            if (StartOfRound.Instance.footstepSurfaces[index2].surfaceTag == tag)
            {
              hitSurfaceID = index2;
              break;
            }
          }
        }
      }
      if (!flag1)
        return;
      RoundManager.PlayRandomClip(this.shovelAudio, this.hitSFX);
      UnityEngine.Object.FindObjectOfType<RoundManager>().PlayAudibleNoise(this.transform.position, 17f, 0.8f);
      if (!flag2 && hitSurfaceID != -1)
      {
        this.shovelAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
        WalkieTalkie.TransmitOneShotAudio(this.shovelAudio, StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
      }
      this.playerHeldBy.playerBodyAnimator.SetTrigger("shovelHit");
      this.HitShovelServerRpc(hitSurfaceID);
    }
  }

  [ServerRpc]
  public void HitShovelServerRpc(int hitSurfaceID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2096026133U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, hitSurfaceID);
      this.__endSendServerRpc(ref bufferWriter, 2096026133U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.HitShovelClientRpc(hitSurfaceID);
  }

  [ClientRpc]
  public void HitShovelClientRpc(int hitSurfaceID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(275435223U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, hitSurfaceID);
      this.__endSendClientRpc(ref bufferWriter, 275435223U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    RoundManager.PlayRandomClip(this.shovelAudio, this.hitSFX);
    if (hitSurfaceID == -1)
      return;
    this.HitSurfaceWithShovel(hitSurfaceID);
  }

  private void HitSurfaceWithShovel(int hitSurfaceID)
  {
    this.shovelAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
    WalkieTalkie.TransmitOneShotAudio(this.shovelAudio, StartOfRound.Instance.footstepSurfaces[hitSurfaceID].hitSurfaceSFX);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_Shovel()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4113335123U, new NetworkManager.RpcReceiveHandler(Shovel.__rpc_handler_4113335123)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2042054613U, new NetworkManager.RpcReceiveHandler(Shovel.__rpc_handler_2042054613)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2096026133U, new NetworkManager.RpcReceiveHandler(Shovel.__rpc_handler_2096026133)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(275435223U, new NetworkManager.RpcReceiveHandler(Shovel.__rpc_handler_275435223)));
  }

  private static void __rpc_handler_4113335123(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((Shovel) target).ReelUpSFXServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2042054613(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Shovel) target).ReelUpSFXClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2096026133(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      int hitSurfaceID;
      ByteUnpacker.ReadValueBitPacked(reader, out hitSurfaceID);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((Shovel) target).HitShovelServerRpc(hitSurfaceID);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_275435223(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int hitSurfaceID;
    ByteUnpacker.ReadValueBitPacked(reader, out hitSurfaceID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((Shovel) target).HitShovelClientRpc(hitSurfaceID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (Shovel);
}
