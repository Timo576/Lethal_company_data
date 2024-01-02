// Decompiled with JetBrains decompiler
// Type: ShowerTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
public class ShowerTrigger : MonoBehaviour
{
  private float cleanInterval = 10f;
  private bool showerOn;
  private int cleanDecalIndex;
  private List<PlayerControllerB> playersInShower = new List<PlayerControllerB>();
  private int playerIndex;
  private bool everyOtherFrame;
  public Collider showerCollider;

  public void ToggleShower(bool on) => this.showerOn = on;

  private void AddPlayerToShower(PlayerControllerB playerScript)
  {
    if (this.playersInShower.Contains(playerScript))
      return;
    Debug.Log((object) string.Format("Added player #{0} to shower", (object) playerScript.playerClientId));
    this.playersInShower.Add(playerScript);
  }

  private void RemovePlayerFromShower(PlayerControllerB playerScript)
  {
    if (!this.playersInShower.Contains(playerScript))
      return;
    this.playersInShower.Remove(playerScript);
  }

  private void CheckBoundsForPlayers()
  {
    if ((double) Time.realtimeSinceStartup - (double) this.cleanInterval < 1.5)
      return;
    this.cleanInterval = Time.realtimeSinceStartup;
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
    {
      if (this.playersInShower.Contains(StartOfRound.Instance.allPlayerScripts[index]))
      {
        if (!StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled || !StartOfRound.Instance.allPlayerScripts[index].isInElevator)
          this.RemovePlayerFromShower(StartOfRound.Instance.allPlayerScripts[index]);
      }
      else if (this.showerCollider.bounds.Contains(StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position))
        this.AddPlayerToShower(StartOfRound.Instance.allPlayerScripts[index]);
    }
  }

  private void Update()
  {
    if (!this.showerOn)
      return;
    this.CheckBoundsForPlayers();
    if (this.playersInShower.Count <= 0 || SprayPaintItem.sprayPaintDecals.Count == 0)
      return;
    Debug.Log((object) "Shower is running with players inside!");
    for (int index1 = 0; index1 < 10; ++index1)
    {
      for (int index2 = 0; index2 < this.playersInShower.Count; ++index2)
      {
        if (!this.playersInShower[index2].isInElevator)
          this.playersInShower.RemoveAt(index2);
        else if (SprayPaintItem.sprayPaintDecals != null && this.cleanDecalIndex < SprayPaintItem.sprayPaintDecals.Count && (Object) SprayPaintItem.sprayPaintDecals[this.cleanDecalIndex] != (Object) null)
        {
          if (SprayPaintItem.sprayPaintDecals[this.cleanDecalIndex].transform.IsChildOf(this.playersInShower[index2].transform))
          {
            Debug.Log((object) string.Format("spray decal #{0} found as child of {1}", (object) this.cleanDecalIndex, (object) this.playersInShower[index2].transform));
            SprayPaintItem.sprayPaintDecals[this.cleanDecalIndex].SetActive(false);
            break;
          }
        }
        else
          this.cleanDecalIndex = 0;
      }
      this.cleanDecalIndex = (this.cleanDecalIndex + 1) % SprayPaintItem.sprayPaintDecals.Count;
    }
  }
}
