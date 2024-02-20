// Decompiled with JetBrains decompiler
// Type: SandSpiderWebTrap
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class SandSpiderWebTrap : MonoBehaviour, IHittable
{
  public SandSpiderAI mainScript;
  private bool hinderingLocalPlayer;
  public PlayerControllerB currentTrappedPlayer;
  public Transform leftBone;
  public Transform rightBone;
  public Transform centerOfWeb;
  public int trapID;
  public float zScale = 1f;
  public AudioSource webAudio;
  private bool webHasBeenBroken;

  public bool Hit(
    int force,
    Vector3 hitDirection,
    PlayerControllerB playerWhoHit = null,
    bool playHitSFX = false)
  {
    if (!this.webHasBeenBroken)
    {
      this.webHasBeenBroken = true;
      this.mainScript.BreakWebServerRpc(this.trapID, (int) playerWhoHit.playerClientId);
    }
    return true;
  }

  private void OnEnable()
  {
    StartOfRound.Instance.playerTeleportedEvent.AddListener(new UnityAction<PlayerControllerB>(this.PlayerLeaveWeb));
  }

  private void OnDisable()
  {
    StartOfRound.Instance.playerTeleportedEvent.RemoveListener(new UnityAction<PlayerControllerB>(this.PlayerLeaveWeb));
    this.PlayerLeaveWeb(GameNetworkManager.Instance.localPlayerController);
  }

  public void Update()
  {
    if ((Object) this.currentTrappedPlayer != (Object) null)
    {
      this.CallPlayerLeaveWebOnDeath();
      Vector3 worldPosition = this.currentTrappedPlayer.transform.position + Vector3.up * 0.6f;
      this.rightBone.LookAt(worldPosition);
      this.leftBone.LookAt(worldPosition);
    }
    else
    {
      this.rightBone.LookAt(this.centerOfWeb);
      this.leftBone.LookAt(this.centerOfWeb);
    }
    this.transform.localScale = Vector3.Lerp(this.transform.localScale, new Vector3(1f, 1f, this.zScale), 8f * Time.deltaTime);
  }

  private void Awake() => this.transform.localScale = new Vector3(0.7f, 0.7f, 0.02f);

  private void CallPlayerLeaveWebOnDeath()
  {
    if (!((Object) NetworkManager.Singleton != (Object) null))
      return;
    if (NetworkManager.Singleton.IsHost && !this.currentTrappedPlayer.isPlayerControlled && !this.currentTrappedPlayer.isPlayerDead)
    {
      this.currentTrappedPlayer = (PlayerControllerB) null;
      this.mainScript.PlayerLeaveWebServerRpc(this.trapID, (int) this.currentTrappedPlayer.playerClientId);
    }
    else
    {
      if (!((Object) GameNetworkManager.Instance.localPlayerController == (Object) this.currentTrappedPlayer) || !GameNetworkManager.Instance.localPlayerController.isPlayerDead)
        return;
      this.currentTrappedPlayer = (PlayerControllerB) null;
      --this.currentTrappedPlayer.isMovementHindered;
      this.currentTrappedPlayer.hinderedMultiplier = Mathf.Clamp(this.currentTrappedPlayer.hinderedMultiplier * 0.4f, 1f, 100f);
      this.hinderingLocalPlayer = false;
      this.mainScript.PlayerLeaveWebServerRpc(this.trapID, (int) this.currentTrappedPlayer.playerClientId);
    }
  }

  private void OnTriggerStay(Collider other)
  {
    if ((Object) GameNetworkManager.Instance == (Object) null || this.hinderingLocalPlayer)
      return;
    PlayerControllerB component = other.GetComponent<PlayerControllerB>();
    if (!((Object) component != (Object) null) || !((Object) component == (Object) GameNetworkManager.Instance.localPlayerController))
      return;
    ++component.isMovementHindered;
    component.hinderedMultiplier *= 2.5f;
    this.hinderingLocalPlayer = true;
    if ((Object) this.currentTrappedPlayer == (Object) null)
      this.currentTrappedPlayer = GameNetworkManager.Instance.localPlayerController;
    if (!((Object) this.mainScript != (Object) null))
      return;
    this.mainScript.PlayerTripWebServerRpc(this.trapID, (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  private void PlayerLeaveWeb(PlayerControllerB playerScript)
  {
    if (!this.hinderingLocalPlayer)
      return;
    this.hinderingLocalPlayer = false;
    --playerScript.isMovementHindered;
    playerScript.hinderedMultiplier *= 0.4f;
    if ((Object) this.currentTrappedPlayer == (Object) playerScript)
      this.currentTrappedPlayer = (PlayerControllerB) null;
    this.webAudio.Stop();
    if (!((Object) this.mainScript != (Object) null))
      return;
    this.mainScript.PlayerLeaveWebServerRpc(this.trapID, (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  private void OnTriggerExit(Collider other)
  {
    if (!this.hinderingLocalPlayer)
      return;
    PlayerControllerB component = other.GetComponent<PlayerControllerB>();
    if (!((Object) component != (Object) null) || !((Object) component == (Object) GameNetworkManager.Instance.localPlayerController))
      return;
    this.PlayerLeaveWeb(component);
  }
}
