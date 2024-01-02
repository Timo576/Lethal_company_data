// Decompiled with JetBrains decompiler
// Type: TerminalAccessibleObject
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class TerminalAccessibleObject : NetworkBehaviour
{
  public string objectCode;
  public float codeAccessCooldownTimer;
  private float currentCooldownTimer;
  private bool inCooldown;
  public InteractEvent terminalCodeEvent;
  public InteractEvent terminalCodeCooldownEvent;
  public bool setCodeRandomlyFromRoundManager = true;
  [Space(3f)]
  public MeshRenderer[] codeMaterials;
  public int rows;
  public int columns;
  [Space(3f)]
  public bool isBigDoor = true;
  private TextMeshProUGUI mapRadarText;
  private Image mapRadarBox;
  private Image mapRadarBoxSlider;
  private bool initializedValues;
  private bool playerHitDoorTrigger;
  private bool isDoorOpen;
  private bool isPoweredOn = true;

  public void CallFunctionFromTerminal()
  {
    if (this.inCooldown)
      return;
    this.terminalCodeEvent.Invoke(GameNetworkManager.Instance.localPlayerController);
    if ((double) this.codeAccessCooldownTimer > 0.0)
    {
      this.currentCooldownTimer = this.codeAccessCooldownTimer;
      this.StartCoroutine(this.countCodeAccessCooldown());
    }
    Debug.Log((object) ("calling terminal function for code : " + this.objectCode + "; object name: " + this.gameObject.name));
  }

  public void TerminalCodeCooldownReached()
  {
    this.terminalCodeCooldownEvent.Invoke((PlayerControllerB) null);
    Debug.Log((object) ("cooldown reached for object with code : " + this.objectCode + "; object name: " + this.gameObject.name));
  }

  private IEnumerator countCodeAccessCooldown()
  {
    this.inCooldown = true;
    if (!this.initializedValues)
      this.InitializeValues();
    Image cooldownBar = this.mapRadarBox;
    Image[] componentsInChildren = this.mapRadarText.gameObject.GetComponentsInChildren<Image>();
    for (int index = 0; index < componentsInChildren.Length; ++index)
    {
      if (componentsInChildren[index].type == Image.Type.Filled)
        cooldownBar = componentsInChildren[index];
    }
    cooldownBar.enabled = true;
    this.mapRadarText.color = Color.red;
    this.mapRadarBox.color = Color.red;
    while ((double) this.currentCooldownTimer > 0.0)
    {
      yield return (object) null;
      this.currentCooldownTimer -= Time.deltaTime;
      cooldownBar.fillAmount = this.currentCooldownTimer / this.codeAccessCooldownTimer;
    }
    this.TerminalCodeCooldownReached();
    this.mapRadarText.color = Color.green;
    this.mapRadarBox.color = Color.green;
    this.currentCooldownTimer = 1.5f;
    int frameNum = 0;
    while ((double) this.currentCooldownTimer > 0.0)
    {
      yield return (object) null;
      this.currentCooldownTimer -= Time.deltaTime;
      cooldownBar.fillAmount = Mathf.Abs((float) ((double) this.currentCooldownTimer / 1.5 - 1.0));
      ++frameNum;
      if (frameNum % 7 == 0)
        this.mapRadarText.enabled = !this.mapRadarText.enabled;
    }
    this.mapRadarText.enabled = true;
    cooldownBar.enabled = false;
    this.inCooldown = false;
  }

  public void OnPowerSwitch(bool switchedOn)
  {
    this.isPoweredOn = switchedOn;
    if (!switchedOn)
    {
      this.mapRadarText.color = Color.gray;
      this.mapRadarBox.color = Color.gray;
      if (this.isDoorOpen)
        return;
      this.gameObject.GetComponent<AnimatedObjectTrigger>().SetBoolOnClientOnly(true);
    }
    else if (!this.isDoorOpen)
    {
      this.mapRadarText.color = Color.red;
      this.mapRadarBox.color = Color.red;
      this.gameObject.GetComponent<AnimatedObjectTrigger>().SetBoolOnClientOnly(false);
    }
    else
    {
      this.mapRadarText.color = Color.green;
      this.mapRadarBox.color = Color.green;
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void SetDoorOpenServerRpc(bool open)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1181174413U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in open, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1181174413U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetDoorOpenClientRpc(open);
  }

  [ClientRpc]
  public void SetDoorOpenClientRpc(bool open)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(635686545U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in open, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 635686545U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SetDoorOpen(open);
  }

  public void SetDoorToggleLocalClient()
  {
    if (!this.isPoweredOn)
      return;
    this.SetDoorOpen(!this.isDoorOpen);
    this.SetDoorOpenServerRpc(this.isDoorOpen);
  }

  public void SetDoorLocalClient(bool open)
  {
    this.SetDoorOpen(open);
    this.SetDoorOpenServerRpc(this.isDoorOpen);
  }

  public void SetDoorOpen(bool open)
  {
    if (!this.isBigDoor || this.isDoorOpen == open || !this.isPoweredOn)
      return;
    this.isDoorOpen = open;
    if (open)
    {
      Debug.Log((object) ("Setting door " + this.gameObject.name + " with code " + this.objectCode + " to open"));
      this.mapRadarText.color = Color.green;
      this.mapRadarBox.color = Color.green;
    }
    else
    {
      Debug.Log((object) ("Setting door " + this.gameObject.name + " with code " + this.objectCode + " to closed"));
      this.mapRadarText.color = Color.red;
      this.mapRadarBox.color = Color.red;
    }
    Debug.Log((object) string.Format("setting big door open for door {0}; {1}; {2}", (object) this.gameObject.name, (object) this.isDoorOpen, (object) open));
    this.gameObject.GetComponent<AnimatedObjectTrigger>().SetBoolOnClientOnly(open);
  }

  public void SetCodeTo(int codeIndex)
  {
    if (!this.setCodeRandomlyFromRoundManager)
      return;
    if (codeIndex > RoundManager.Instance.possibleCodesForBigDoors.Length)
    {
      Debug.LogError((object) "Attempted setting code to an index higher than the amount of possible codes in TerminalAccessibleObject");
    }
    else
    {
      this.objectCode = RoundManager.Instance.possibleCodesForBigDoors[codeIndex];
      this.SetMaterialUV(codeIndex);
      if ((Object) this.mapRadarText == (Object) null)
        this.InitializeValues();
      this.mapRadarText.text = this.objectCode;
    }
  }

  private void Start() => this.InitializeValues();

  public void InitializeValues()
  {
    if (this.initializedValues)
      return;
    this.initializedValues = true;
    GameObject gameObject = Object.Instantiate<GameObject>(StartOfRound.Instance.objectCodePrefab, StartOfRound.Instance.mapScreen.mapCameraStationaryUI, false);
    RectTransform component = gameObject.GetComponent<RectTransform>();
    component.position = this.transform.position + Vector3.up * 4.35f;
    RectTransform rectTransform = component;
    rectTransform.position = rectTransform.position + (component.up * 1.2f - component.right * 1.2f);
    this.mapRadarText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
    this.mapRadarText.text = this.objectCode;
    this.mapRadarBox = gameObject.GetComponentInChildren<Image>();
    if (!this.isBigDoor)
      return;
    this.SetDoorOpen(this.gameObject.GetComponent<AnimatedObjectTrigger>().boolValue);
    if (this.gameObject.GetComponent<AnimatedObjectTrigger>().boolValue)
    {
      this.mapRadarText.color = Color.green;
      this.mapRadarBox.color = Color.green;
    }
    else
    {
      this.mapRadarText.color = Color.red;
      this.mapRadarBox.color = Color.red;
    }
  }

  public override void OnDestroy()
  {
    if ((Object) this.mapRadarText != (Object) null && (Object) this.mapRadarText.gameObject != (Object) null)
      Object.Destroy((Object) this.mapRadarText.gameObject);
    base.OnDestroy();
  }

  private void SetMaterialUV(int codeIndex)
  {
    float x = 0.0f;
    float y = 0.0f;
    for (int index = 0; index < codeIndex; ++index)
    {
      x += 1f / (float) this.columns;
      if ((double) x >= 1.0)
      {
        x = 0.0f;
        y += 1f / (float) this.rows;
        if ((double) y >= 1.0)
          y = 0.0f;
      }
    }
    if (this.codeMaterials == null || this.codeMaterials.Length == 0)
      return;
    Material material = this.codeMaterials[0].material;
    material.SetTextureOffset("_BaseColorMap", new Vector2(x, y));
    for (int index = 0; index < this.codeMaterials.Length; ++index)
      this.codeMaterials[index].sharedMaterial = material;
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!this.isBigDoor || this.playerHitDoorTrigger || this.isDoorOpen && this.isPoweredOn || !other.CompareTag("Player") || !((Object) other.gameObject.GetComponent<PlayerControllerB>() == (Object) GameNetworkManager.Instance.localPlayerController))
      return;
    this.playerHitDoorTrigger = true;
    HUDManager.Instance.DisplayTip("TIP:", "Use the ship computer terminal to access secure doors.", useSave: true, prefsKey: "LCTip_SecureDoors");
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_TerminalAccessibleObject()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1181174413U, new NetworkManager.RpcReceiveHandler(TerminalAccessibleObject.__rpc_handler_1181174413)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(635686545U, new NetworkManager.RpcReceiveHandler(TerminalAccessibleObject.__rpc_handler_635686545)));
  }

  private static void __rpc_handler_1181174413(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool open;
    reader.ReadValueSafe<bool>(out open, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((TerminalAccessibleObject) target).SetDoorOpenServerRpc(open);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_635686545(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool open;
    reader.ReadValueSafe<bool>(out open, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((TerminalAccessibleObject) target).SetDoorOpenClientRpc(open);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (TerminalAccessibleObject);
}
