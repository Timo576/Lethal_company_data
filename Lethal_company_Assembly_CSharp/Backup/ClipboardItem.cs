// Decompiled with JetBrains decompiler
// Type: ClipboardItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class ClipboardItem : GrabbableObject
{
  public int currentPage = 1;
  public Animator clipboardAnimator;
  public AudioClip[] turnPageSFX;
  public AudioSource thisAudio;

  public override void PocketItem()
  {
    if (this.IsOwner && (Object) this.playerHeldBy != (Object) null)
    {
      this.playerHeldBy.equippedUsableItemQE = false;
      this.isBeingUsed = false;
    }
    base.PocketItem();
  }

  public override void ItemInteractLeftRight(bool right)
  {
    int currentPage = this.currentPage;
    this.RequireCooldown();
    this.currentPage = !right ? Mathf.Clamp(this.currentPage - 1, 1, 4) : Mathf.Clamp(this.currentPage + 1, 1, 4);
    if (this.currentPage != currentPage)
      RoundManager.PlayRandomClip(this.thisAudio, this.turnPageSFX);
    this.clipboardAnimator.SetInteger("page", this.currentPage);
  }

  public override void DiscardItem()
  {
    if ((Object) this.playerHeldBy != (Object) null)
      this.playerHeldBy.equippedUsableItemQE = false;
    this.isBeingUsed = false;
    base.DiscardItem();
  }

  public override void EquipItem()
  {
    base.EquipItem();
    this.playerHeldBy.equippedUsableItemQE = true;
    if (!this.IsOwner)
      return;
    HUDManager.Instance.DisplayTip("To read the manual:", "Press Z to inspect closely. Press Q and E to flip the pages.", useSave: true, prefsKey: "LCTip_UseManual");
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (ClipboardItem);
}
