// Decompiled with JetBrains decompiler
// Type: WhoopieCushionItem
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class WhoopieCushionItem : GrabbableObject
{
  public AudioSource whoopieCushionAudio;
  public AudioClip[] fartAudios;
  private float fartDebounce;
  private Vector3 lastPositionAtFart;
  private int timesPlayingInOneSpot;

  public void Fart()
  {
    Debug.Log((object) "Fart called");
    if ((double) Vector3.Distance(this.lastPositionAtFart, this.transform.position) > 2.0)
      this.timesPlayingInOneSpot = 0;
    ++this.timesPlayingInOneSpot;
    this.lastPositionAtFart = this.transform.position;
    RoundManager.PlayRandomClip(this.whoopieCushionAudio, this.fartAudios, audibleNoiseID: -1);
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, 8f, 0.8f, this.timesPlayingInOneSpot, this.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed, 101158);
  }

  public void FartWithDebounce()
  {
    Debug.Log((object) string.Format("Fart with debounce called : {0}; {1}", (object) (float) ((double) Time.realtimeSinceStartup - (double) this.fartDebounce), (object) this.fartDebounce));
    if ((double) Time.realtimeSinceStartup - (double) this.fartDebounce <= 0.20000000298023224)
      return;
    this.fartDebounce = Time.realtimeSinceStartup;
    this.Fart();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (WhoopieCushionItem);
}
