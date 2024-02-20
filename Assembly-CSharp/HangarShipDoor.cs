// Decompiled with JetBrains decompiler
// Type: HangarShipDoor
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using TMPro;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class HangarShipDoor : MonoBehaviour
{
  public InteractTrigger triggerScript;
  public TextMeshProUGUI doorPowerDisplay;
  private StartOfRound playersManager;
  public Animator shipDoorsAnimator;
  public bool buttonsEnabled = true;
  public float doorPower = 1f;
  public float doorPowerDuration = 20f;
  public bool overheated;
  public bool doorsOpenedInGameOverAnimation;
  public GameObject hydraulicsDisplay;
  private bool hydraulicsScreenDisplayed = true;

  public void Update()
  {
    if ((Object) GameNetworkManager.Instance == (Object) null || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null)
      return;
    this.SetScreenDisplay();
    if (StartOfRound.Instance.hangarDoorsClosed && StartOfRound.Instance.shipHasLanded)
    {
      this.overheated = false;
      this.triggerScript.interactable = true;
      if ((double) this.doorPower > 0.0)
        this.doorPower = Mathf.Clamp(this.doorPower - Time.deltaTime / this.doorPowerDuration, 0.0f, 1f);
      else if (NetworkManager.Singleton.IsServer)
      {
        this.PlayDoorAnimation(false);
        StartOfRound.Instance.SetShipDoorsOverheatServerRpc();
      }
    }
    else
    {
      this.doorPower = Mathf.Clamp(this.doorPower + Time.deltaTime / (this.doorPowerDuration * 0.22f), 0.0f, 1f);
      if (this.overheated && (double) this.doorPower >= 1.0)
      {
        this.overheated = false;
        this.triggerScript.interactable = true;
      }
    }
    this.doorPowerDisplay.text = string.Format("{0}%", (object) Mathf.RoundToInt(this.doorPower * 100f));
  }

  private void SetScreenDisplay()
  {
    bool flag = true;
    if (!GameNetworkManager.Instance.localPlayerController.isPlayerDead)
      flag = GameNetworkManager.Instance.localPlayerController.isInElevator;
    else if ((Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (Object) null)
      flag = GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript.isInElevator;
    if (this.hydraulicsScreenDisplayed == flag)
      return;
    this.hydraulicsScreenDisplayed = flag;
    this.hydraulicsDisplay.SetActive(flag);
  }

  public void PlayDoorAnimation(bool closed)
  {
    if (!this.buttonsEnabled)
      return;
    this.shipDoorsAnimator.SetBool("Closed", closed);
  }

  public void SetDoorClosed() => this.playersManager.SetShipDoorsClosed(true);

  public void SetDoorOpen() => this.playersManager.SetShipDoorsClosed(false);

  public void SetDoorButtonsEnabled(bool doorButtonsEnabled)
  {
    this.buttonsEnabled = doorButtonsEnabled;
  }

  private void Start() => this.playersManager = Object.FindObjectOfType<StartOfRound>();
}
