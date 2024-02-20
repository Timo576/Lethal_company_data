// Decompiled with JetBrains decompiler
// Type: StoryLog
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Netcode;
using UnityEngine;

#nullable disable
public class StoryLog : NetworkBehaviour
{
  public int storyLogID = -1;
  private bool collected;

  public void CollectLog()
  {
    if ((Object) NetworkManager.Singleton == (Object) null || (Object) GameNetworkManager.Instance == (Object) null || this.collected || this.storyLogID == -1)
      return;
    this.collected = true;
    this.RemoveLogCollectible();
    if (Object.FindObjectOfType<Terminal>().unlockedStoryLogs.Contains(this.storyLogID))
      return;
    HUDManager.Instance.GetNewStoryLogServerRpc(this.storyLogID);
  }

  private void Start()
  {
    if (!Object.FindObjectOfType<Terminal>().unlockedStoryLogs.Contains(this.storyLogID))
      return;
    this.RemoveLogCollectible();
  }

  private void RemoveLogCollectible()
  {
    foreach (Renderer componentsInChild in this.gameObject.GetComponentsInChildren<MeshRenderer>())
      componentsInChild.enabled = false;
    this.gameObject.GetComponent<InteractTrigger>().interactable = false;
    foreach (Collider componentsInChild in this.GetComponentsInChildren<Collider>())
      componentsInChild.enabled = false;
  }

  public void SetStoryLogID(int logID) => this.storyLogID = logID;

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (StoryLog);
}
