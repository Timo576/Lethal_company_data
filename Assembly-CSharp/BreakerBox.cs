// Decompiled with JetBrains decompiler
// Type: BreakerBox
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class BreakerBox : NetworkBehaviour, IShockableWithGun
{
  public int leversSwitchedOff = 2;
  public bool isPowerOn;
  public RoundManager roundManager;
  public Animator[] breakerSwitches;
  public AudioSource thisAudioSource;
  public AudioSource breakerBoxHum;
  public AudioClip switchPowerSFX;

  private void Start() => this.roundManager = Object.FindObjectOfType<RoundManager>();

  public void SetSwitchesOff()
  {
    this.roundManager = Object.FindObjectOfType<RoundManager>();
    if ((Object) this.roundManager == (Object) null)
    {
      Debug.LogError((object) "Could not find round manager from breaker box script!");
    }
    else
    {
      this.leversSwitchedOff = 0;
      int num = this.roundManager.BreakerBoxRandom.Next(2, this.breakerSwitches.Length - 1);
      Debug.Log((object) string.Format("loopLimit: {0}", (object) num));
      for (int index1 = 0; index1 < num; ++index1)
      {
        int index2 = this.roundManager.BreakerBoxRandom.Next(0, this.breakerSwitches.Length);
        Debug.Log((object) string.Format("switch {0}: {1}", (object) index1, (object) index2));
        AnimatedObjectTrigger component = this.breakerSwitches[index2].gameObject.GetComponent<AnimatedObjectTrigger>();
        if (!component.boolValue)
        {
          Debug.Log((object) "switch was already turned off");
        }
        else
        {
          this.breakerSwitches[index2].SetBool("turnedLeft", false);
          component.boolValue = false;
          component.setInitialState = false;
          ++this.leversSwitchedOff;
        }
      }
      Debug.Log((object) "Set lever switches");
    }
  }

  public void SwitchBreaker(bool on)
  {
    Debug.Log((object) "Switch breaker!");
    if ((Object) this.roundManager == (Object) null)
      return;
    if (on)
      --this.leversSwitchedOff;
    else
      ++this.leversSwitchedOff;
    if (this.IsServer)
    {
      Debug.Log((object) "Breaker switched on server.");
      if (this.leversSwitchedOff <= 0 && !this.isPowerOn)
      {
        this.isPowerOn = true;
        this.roundManager.SwitchPower(true);
      }
      else if (this.leversSwitchedOff > 0 && this.isPowerOn)
      {
        this.isPowerOn = false;
        this.roundManager.SwitchPower(false);
      }
    }
    if (this.leversSwitchedOff <= 0)
    {
      this.breakerBoxHum.Play();
    }
    else
    {
      if (this.leversSwitchedOff != 1)
        return;
      this.breakerBoxHum.Stop();
    }
  }

  void IShockableWithGun.ShockWithGun(PlayerControllerB shockedByPlayer)
  {
    this.SetSwitchesOff();
    RoundManager.Instance.FlickerLights();
  }

  void IShockableWithGun.StopShockingWithGun() => RoundManager.Instance.FlickerLights();

  bool IShockableWithGun.CanBeShocked() => true;

  float IShockableWithGun.GetDifficultyMultiplier() => 0.3f;

  Vector3 IShockableWithGun.GetShockablePosition() => this.transform.position;

  Transform IShockableWithGun.GetShockableTransform() => this.transform;

  NetworkObject IShockableWithGun.GetNetworkObject() => this.NetworkObject;

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (BreakerBox);
}
