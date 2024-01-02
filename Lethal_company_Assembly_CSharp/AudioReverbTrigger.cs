// Decompiled with JetBrains decompiler
// Type: AudioReverbTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

#nullable disable
public class AudioReverbTrigger : NetworkBehaviour
{
  public PlayerControllerB playerScript;
  public ReverbPreset reverbPreset;
  public int usePreset = -1;
  [Header("CHANGE AUDIO AMBIANCE")]
  public switchToAudio[] audioChanges;
  private bool changingVolumes;
  [Header("MISC")]
  public bool elevatorTriggerForProps;
  public bool setInElevatorTrigger;
  public bool isShipRoom;
  public bool toggleLocalFog;
  public float fogEnabledAmount = 10f;
  public LocalVolumetricFog localFog;
  public Terrain terrainObj;
  [Header("Weather and effects")]
  public bool setInsideAtmosphere;
  public bool insideLighting;
  public int weatherEffect = -1;
  public bool effectEnabled;
  public bool disableAllWeather;
  public bool enableCurrentLevelWeather;
  private bool spectatedClientTriggered;

  private IEnumerator changeVolume(AudioSource aud, float changeVolumeTo)
  {
    int i;
    if ((Object) this.localFog != (Object) null)
    {
      float fogTarget = this.fogEnabledAmount;
      if (!this.toggleLocalFog)
        fogTarget = 200f;
      for (i = 0; i < 40; ++i)
      {
        aud.volume = Mathf.Lerp(aud.volume, changeVolumeTo, (float) i / 40f);
        this.localFog.parameters.meanFreePath = Mathf.Lerp(this.localFog.parameters.meanFreePath, fogTarget, (float) i / 40f);
        yield return (object) new WaitForSeconds(0.004f);
      }
    }
    else
    {
      for (i = 0; i < 40; ++i)
      {
        aud.volume = Mathf.Lerp(aud.volume, changeVolumeTo, (float) i / 40f);
        yield return (object) new WaitForSeconds(0.004f);
      }
    }
    this.playerScript.audioCoroutines.Remove(aud);
    this.playerScript.audioCoroutines2.Remove(aud);
  }

  public void ChangeAudioReverbForPlayer(PlayerControllerB pScript)
  {
    this.playerScript = pScript;
    if ((Object) GameNetworkManager.Instance.localPlayerController == (Object) null || (Object) this.playerScript.currentAudioTrigger == (Object) this || !this.playerScript.isPlayerControlled)
      return;
    if ((Object) NetworkManager.Singleton == (Object) null)
      Debug.Log((object) "Network manager is null");
    if (this.usePreset != -1)
    {
      AudioReverbPresets objectOfType = Object.FindObjectOfType<AudioReverbPresets>();
      if ((Object) objectOfType != (Object) null)
      {
        if (objectOfType.audioPresets.Length <= this.usePreset)
        {
          Debug.LogError((object) ("The audio preset set by " + this.gameObject.name + " is not one allowed by the audioreverbpresets in the scene."));
          return;
        }
        if (objectOfType.audioPresets[this.usePreset].usePreset != -1)
        {
          Debug.LogError((object) "Audio preset AudioReverbTrigger is set to call another audio preset which would crash!");
          return;
        }
        objectOfType.audioPresets[this.usePreset].ChangeAudioReverbForPlayer(pScript);
        return;
      }
    }
    if ((Object) this.reverbPreset != (Object) null)
      this.playerScript.reverbPreset = this.reverbPreset;
    if (this.elevatorTriggerForProps)
    {
      if ((Object) this.playerScript.currentlyHeldObjectServer != (Object) null && this.playerScript.isHoldingObject)
        this.playerScript.SetItemInElevator(this.isShipRoom, this.setInElevatorTrigger, this.playerScript.currentlyHeldObjectServer);
      if (this.playerScript.playersManager.shipDoorsEnabled || this.setInElevatorTrigger)
      {
        this.playerScript.isInElevator = this.setInElevatorTrigger;
        this.playerScript.isInHangarShipRoom = this.isShipRoom;
      }
      this.playerScript.playersManager.SetPlayerSafeInShip();
    }
    if ((Object) this.playerScript != (Object) GameNetworkManager.Instance.localPlayerController)
    {
      if ((Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript != (Object) this.playerScript)
      {
        this.playerScript.currentAudioTrigger = this;
        return;
      }
      this.spectatedClientTriggered = true;
    }
    else
      this.spectatedClientTriggered = false;
    if (this.disableAllWeather)
    {
      TimeOfDay.Instance.DisableAllWeather();
    }
    else
    {
      if (this.weatherEffect != -1)
        TimeOfDay.Instance.effects[this.weatherEffect].effectEnabled = this.effectEnabled;
      if (this.enableCurrentLevelWeather && TimeOfDay.Instance.currentLevelWeather != LevelWeatherType.None)
        TimeOfDay.Instance.effects[(int) TimeOfDay.Instance.currentLevelWeather].effectEnabled = true;
    }
    if (this.setInsideAtmosphere)
      TimeOfDay.Instance.insideLighting = this.insideLighting;
    PlayerControllerB playerScript = this.playerScript;
    this.playerScript = GameNetworkManager.Instance.localPlayerController;
    for (int index = 0; index < this.audioChanges.Length; ++index)
    {
      AudioSource audio = this.audioChanges[index].audio;
      if (this.audioChanges[index].stopAudio)
      {
        audio.Stop();
      }
      else
      {
        if ((Object) this.audioChanges[index].changeToClip != (Object) null && (Object) audio.clip != (Object) this.audioChanges[index].changeToClip)
        {
          bool flag = false;
          if (audio.isPlaying)
            flag = true;
          audio.clip = this.audioChanges[index].changeToClip;
          if (flag)
            audio.Play();
        }
        else if ((Object) this.audioChanges[index].changeToClip == (Object) null && !audio.isPlaying && !this.audioChanges[index].changeAudioVolume)
          audio.Play();
        if (this.audioChanges[index].changeAudioVolume && (Object) this.playerScript.currentAudioTrigger != (Object) this)
        {
          AudioReverbTrigger audioReverbTrigger;
          if (this.playerScript.audioCoroutines.TryGetValue(audio, out audioReverbTrigger))
          {
            audioReverbTrigger.StopAudioCoroutine(audio);
            this.StartCoroutine(this.changeVolume(audio, this.audioChanges[index].audioVolume));
          }
          else
          {
            IEnumerator routine = this.changeVolume(audio, this.audioChanges[index].audioVolume);
            this.StartCoroutine(routine);
            this.playerScript.audioCoroutines.Add(audio, this);
            this.playerScript.audioCoroutines2.Add(audio, routine);
          }
        }
      }
    }
    if (this.spectatedClientTriggered)
      playerScript.currentAudioTrigger = this;
    this.playerScript.currentAudioTrigger = this;
  }

  private void OnTriggerStay(Collider other)
  {
    if (this.elevatorTriggerForProps)
    {
      if (this.setInElevatorTrigger && other.gameObject.CompareTag("Enemy") && this.gameObject.GetComponent<Collider>().bounds.Contains(other.transform.position))
      {
        EnemyAICollisionDetect component = other.gameObject.GetComponent<EnemyAICollisionDetect>();
        if (!((Object) component != (Object) null))
          return;
        bool flag = false;
        if (component.mainScript.isInsidePlayerShip != this.isShipRoom)
          flag = true;
        component.mainScript.isInsidePlayerShip = this.isShipRoom;
        if (!flag)
          return;
        StartOfRound.Instance.SetPlayerSafeInShip();
        return;
      }
      if (other.gameObject.tag.StartsWith("PlayerRagdoll"))
      {
        DeadBodyInfo component = other.gameObject.GetComponent<DeadBodyInfo>();
        if ((Object) component != (Object) null)
        {
          if ((Object) component.attachedTo != (Object) null && (Object) component.attachedLimb != (Object) null)
            return;
          component.parentedToShip = this.setInElevatorTrigger;
          if ((Object) component.attachedLimb == (Object) null || (Object) component.attachedTo == (Object) null)
          {
            if (this.setInElevatorTrigger)
              component.transform.SetParent(StartOfRound.Instance.elevatorTransform);
            else
              component.transform.SetParent((Transform) null);
          }
        }
      }
    }
    if (!other.gameObject.CompareTag("Player") || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null)
      return;
    this.playerScript = other.gameObject.GetComponent<PlayerControllerB>();
    if ((Object) this.playerScript == (Object) null || !this.playerScript.isPlayerControlled)
      return;
    this.ChangeAudioReverbForPlayer(this.playerScript);
  }

  public void StopAudioCoroutine(AudioSource audioKey)
  {
    IEnumerator routine;
    if (!this.playerScript.audioCoroutines2.TryGetValue(audioKey, out routine))
      return;
    this.StopCoroutine(routine);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (AudioReverbTrigger);
}
