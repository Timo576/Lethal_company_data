// Decompiled with JetBrains decompiler
// Type: SoundManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;

#nullable disable
public class SoundManager : NetworkBehaviour
{
  private System.Random SoundsRandom;
  public float soundFrequencyServer = 10f;
  public float soundRarityServer = 0.25f;
  public float soundTimerServer;
  private int serverSoundsPlayedInARow;
  public float soundFrequency = 8f;
  public float soundRarity = 0.6f;
  private float soundTimer;
  private int localSoundsPlayedInARow;
  public AudioSource ambienceAudio;
  public AudioSource ambienceAudioNonDiagetic;
  [Header("Outside Music")]
  public AudioSource musicSource;
  public AudioClip[] DaytimeMusic;
  public AudioClip[] EveningMusic;
  private float timeSincePlayingLastMusic;
  public bool playingOutsideMusic;
  [Space(5f)]
  private bool isAudioPlaying;
  private PlayerControllerB localPlayer;
  private bool isInsanityMusicPlaying;
  private List<int> audioClipProbabilities = new List<int>();
  private int lastSoundTypePlayed = -1;
  private int lastServerSoundTypePlayed = -1;
  private bool playingInsanitySoundClip;
  private bool playingInsanitySoundClipOnServer;
  private float localPlayerAmbientMusicTimer;
  [Header("Audio Mixer")]
  public AudioMixerSnapshot[] mixerSnapshots;
  public AudioMixer diageticMixer;
  public AudioMixerGroup[] playerVoiceMixers;
  [Space(3f)]
  public float[] playerVoicePitchTargets;
  public float[] playerVoicePitches;
  public float[] playerVoicePitchLerpSpeed;
  [Space(3f)]
  public float[] playerVoiceVolumes;
  public int currentMixerSnapshotID;
  private bool overridingCurrentAudioMixer;
  [Header("Background music")]
  public AudioSource highAction1;
  private bool highAction1audible;
  public AudioSource highAction2;
  private bool highAction2audible;
  public AudioSource lowAction;
  private bool lowActionAudible;
  public AudioSource heartbeatSFX;
  public float currentHeartbeatInterval;
  public float heartbeatTimer;
  public AudioClip[] heartbeatClips;
  private int currentHeartbeatClip;
  private bool playingHeartbeat;
  public float earsRingingTimer;
  public float timeSinceEarsStartedRinging;
  private bool earsRinging;
  public AudioSource ringingEarsAudio;
  public AudioSource tempAudio1;
  public AudioSource tempAudio2;
  public AudioClip[] syncedAudioClips;
  private System.Random audioRandom;

  public static SoundManager Instance { get; private set; }

  public void ResetRandomSeed()
  {
    this.audioRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 113);
  }

  private void Awake()
  {
    if ((UnityEngine.Object) SoundManager.Instance == (UnityEngine.Object) null)
      SoundManager.Instance = this;
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) SoundManager.Instance.gameObject);
  }

  private void Start()
  {
    this.InitializeRandom();
    this.SetDiageticMixerSnapshot();
    this.playerVoicePitchLerpSpeed = new float[4]
    {
      3f,
      3f,
      3f,
      3f
    };
    this.playerVoicePitchTargets = new float[4]
    {
      1f,
      1f,
      1f,
      1f
    };
    this.playerVoicePitches = new float[4]{ 1f, 1f, 1f, 1f };
    this.playerVoiceVolumes = new float[4]
    {
      0.5f,
      0.5f,
      0.5f,
      0.5f
    };
    AudioListener.volume = 0.0f;
    this.StartCoroutine(this.fadeVolumeBackToNormalDelayed());
    if (this.audioRandom != null)
      return;
    this.ResetRandomSeed();
  }

  private IEnumerator fadeVolumeBackToNormalDelayed()
  {
    yield return (object) new WaitForSeconds(0.5f);
    float targetVolume = IngamePlayerSettings.Instance.settings.masterVolume;
    for (int i = 0; i < 40; ++i)
    {
      AudioListener.volume += 0.025f;
      if ((double) AudioListener.volume < (double) targetVolume)
        yield return (object) new WaitForSeconds(0.016f);
      else
        break;
    }
    AudioListener.volume = targetVolume;
  }

  public void InitializeRandom()
  {
    this.SoundsRandom = new System.Random(StartOfRound.Instance.randomMapSeed - 33);
    this.ResetValues();
  }

  public void ResetValues()
  {
    this.SetDiageticMixerSnapshot();
    this.lastSoundTypePlayed = -1;
    this.lastServerSoundTypePlayed = -1;
    this.localSoundsPlayedInARow = 0;
    this.soundFrequency = 0.8f;
    this.soundRarity = 0.6f;
    this.soundTimer = 0.0f;
    this.isInsanityMusicPlaying = false;
  }

  public void SetPlayerPitch(float pitch, int playerObjNum)
  {
    this.diageticMixer.SetFloat(string.Format("PlayerPitch{0}", (object) playerObjNum), pitch);
  }

  public void SetPlayerVoiceFilters()
  {
    int num = 0;
    while (num < StartOfRound.Instance.allPlayerScripts.Length)
      ++num;
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
    {
      if (!StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled && !StartOfRound.Instance.allPlayerScripts[index].isPlayerDead)
      {
        this.playerVoicePitches[index] = 1f;
        this.playerVoiceVolumes[index] = 1f;
      }
      else
      {
        this.diageticMixer.SetFloat(string.Format("PlayerVolume{0}", (object) index), 16f * this.playerVoiceVolumes[index]);
        if ((double) Mathf.Abs(this.playerVoicePitches[index] - this.playerVoicePitchTargets[index]) > 0.02500000037252903)
        {
          this.playerVoicePitches[index] = Mathf.Lerp(this.playerVoicePitches[index], this.playerVoicePitchTargets[index], 3f * Time.deltaTime);
          this.diageticMixer.SetFloat(string.Format("PlayerPitch{0}", (object) index), this.playerVoicePitches[index]);
        }
        else if ((double) this.playerVoicePitches[index] != (double) this.playerVoicePitchTargets[index])
        {
          this.playerVoicePitches[index] = this.playerVoicePitchTargets[index];
          this.diageticMixer.SetFloat(string.Format("PlayerPitch{0}", (object) index), this.playerVoicePitches[index]);
        }
      }
    }
  }

  private void Update()
  {
    this.localPlayer = GameNetworkManager.Instance.localPlayerController;
    if ((UnityEngine.Object) this.localPlayer == (UnityEngine.Object) null || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
    {
      Debug.Log((object) string.Format("soumdmanager: {0}; {1}", (object) ((UnityEngine.Object) this.localPlayer == (UnityEngine.Object) null), (object) ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)));
    }
    else
    {
      ++this.timeSincePlayingLastMusic;
      this.SetPlayerVoiceFilters();
      this.SetAudioFilters();
      this.SetOutsideMusicValues();
      this.PlayNonDiageticSound();
      this.SetFearAudio();
      this.SetEarsRinging();
      if (StartOfRound.Instance.inShipPhase || this.ambienceAudio.isPlaying)
        return;
      this.ServerSoundTimer();
      this.LocalPlayerSoundTimer();
    }
  }

  private void SetAudioFilters()
  {
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead && this.currentMixerSnapshotID != 0)
      this.SetDiageticMixerSnapshot(transitionTime: 0.2f);
    else if ((double) StartOfRound.Instance.drunknessSideEffect.Evaluate(this.localPlayer.drunkness) > 0.60000002384185791 && !this.overridingCurrentAudioMixer)
    {
      this.overridingCurrentAudioMixer = true;
      this.mixerSnapshots[4].TransitionTo(6f);
    }
    else
    {
      if ((double) StartOfRound.Instance.drunknessSideEffect.Evaluate(this.localPlayer.drunkness) >= 0.40000000596046448 || !this.overridingCurrentAudioMixer)
        return;
      this.overridingCurrentAudioMixer = false;
      this.ResumeCurrentMixerSnapshot(8f);
    }
  }

  public void PlayRandomOutsideMusic(bool eveningMusic = false)
  {
    if ((double) this.timeSincePlayingLastMusic < 200.0)
      return;
    int index = UnityEngine.Random.Range(0, this.DaytimeMusic.Length);
    if (eveningMusic)
    {
      if (this.EveningMusic.Length == 0)
        return;
      this.musicSource.clip = this.EveningMusic[index];
    }
    else
      this.musicSource.clip = this.DaytimeMusic[index];
    this.musicSource.Play();
    this.playingOutsideMusic = true;
    this.timeSincePlayingLastMusic = 0.0f;
  }

  private void SetOutsideMusicValues()
  {
    if (this.playingOutsideMusic)
    {
      this.musicSource.volume = Mathf.Lerp(this.musicSource.volume, 0.85f, 2f * Time.deltaTime);
      if (!GameNetworkManager.Instance.localPlayerController.isInsideFactory && (double) StartOfRound.Instance.fearLevel <= 0.075000002980232239 && (!GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom || !((UnityEngine.Object) StartOfRound.Instance.mapScreen.targetedPlayer != (UnityEngine.Object) null) || !StartOfRound.Instance.mapScreen.targetedPlayer.isInsideFactory))
        return;
      this.playingOutsideMusic = false;
    }
    else
    {
      this.musicSource.volume = Mathf.Lerp(this.musicSource.volume, 0.0f, 2f * Time.deltaTime);
      if ((double) this.musicSource.volume > 0.004999999888241291)
        return;
      this.musicSource.Stop();
    }
  }

  private void SetEarsRinging()
  {
    if ((double) this.earsRingingTimer > 0.0 && !GameNetworkManager.Instance.localPlayerController.isPlayerDead)
    {
      this.timeSinceEarsStartedRinging = 0.0f;
      if (!this.earsRinging)
      {
        this.earsRinging = true;
        this.SetDiageticMixerSnapshot(2);
        this.ringingEarsAudio.Play();
      }
      this.ringingEarsAudio.volume = Mathf.Lerp(this.ringingEarsAudio.volume, this.earsRingingTimer, Time.deltaTime * 2f);
      this.earsRingingTimer -= Time.deltaTime * 0.1f;
    }
    else
    {
      this.timeSinceEarsStartedRinging += Time.deltaTime;
      if (!this.earsRinging)
        return;
      this.earsRinging = false;
      this.SetDiageticMixerSnapshot();
      this.ringingEarsAudio.Stop();
    }
  }

  private void SetFearAudio()
  {
    if (GameNetworkManager.Instance.localPlayerController.isPlayerDead)
    {
      this.highAction1.volume = 0.0f;
      this.highAction1.Stop();
      this.highAction2.volume = 0.0f;
      this.highAction2.Stop();
      this.heartbeatSFX.volume = 0.0f;
      this.heartbeatSFX.Stop();
      this.lowAction.volume = 0.0f;
      this.lowAction.Stop();
    }
    else
    {
      if (!this.highAction2.isPlaying)
      {
        this.highAction1.Play();
        this.highAction2.Play();
        this.heartbeatSFX.Play();
        this.lowAction.Play();
      }
      float fearLevel = StartOfRound.Instance.fearLevel;
      if ((double) fearLevel > 0.40000000596046448)
      {
        this.highAction1.volume = Mathf.Lerp(this.highAction1.volume, fearLevel - 0.2f, 0.75f * Time.deltaTime);
        this.highAction1audible = true;
      }
      else
      {
        this.highAction1.volume = Mathf.Lerp(this.highAction1.volume, 0.0f, Time.deltaTime);
        if ((double) this.highAction1.volume < 0.0099999997764825821 && this.highAction1audible)
        {
          this.highAction1audible = false;
          this.highAction1.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
        }
      }
      if ((double) fearLevel > 0.699999988079071)
      {
        this.highAction2.volume = Mathf.Lerp(this.highAction2.volume, fearLevel, 2f * Time.deltaTime);
        this.highAction2audible = true;
      }
      else
      {
        this.highAction2.volume = Mathf.Lerp(this.highAction2.volume, 0.0f, 0.75f * Time.deltaTime);
        if ((double) this.highAction2.volume < 0.0099999997764825821 && this.highAction2audible)
        {
          this.highAction2audible = false;
          this.highAction2.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
        }
      }
      if ((double) fearLevel > 0.10000000149011612 && (double) fearLevel < 0.67000001668930054)
      {
        this.lowAction.volume = Mathf.Lerp(this.lowAction.volume, fearLevel + 0.2f, 2f * Time.deltaTime);
        this.lowActionAudible = true;
      }
      else
      {
        this.lowAction.volume = Mathf.Lerp(this.lowAction.volume, 0.0f, 2f * Time.deltaTime);
        if ((double) this.lowAction.volume < 0.0099999997764825821 && this.lowActionAudible)
        {
          this.lowActionAudible = false;
          this.lowAction.pitch = UnityEngine.Random.Range(0.87f, 1.1f);
        }
      }
      float target = (double) GameNetworkManager.Instance.localPlayerController.drunkness <= 0.30000001192092896 ? Mathf.Abs(fearLevel - 1.4f) : Mathf.Abs(StartOfRound.Instance.drunknessSideEffect.Evaluate(GameNetworkManager.Instance.localPlayerController.drunkness) - 1.6f);
      this.currentHeartbeatInterval = Mathf.MoveTowards(this.currentHeartbeatInterval, target, 0.3f * Time.deltaTime);
      if ((double) this.currentHeartbeatInterval > 1.3)
        this.playingHeartbeat = false;
      if ((double) fearLevel <= 0.5 && (double) GameNetworkManager.Instance.localPlayerController.drunkness <= 0.30000001192092896 && !this.playingHeartbeat)
        return;
      this.playingHeartbeat = true;
      this.heartbeatSFX.volume = Mathf.Clamp(Mathf.Abs(target - 1f) + 0.55f, 0.0f, 1f);
      this.heartbeatTimer += Time.deltaTime;
      if ((double) this.heartbeatTimer < (double) this.currentHeartbeatInterval)
        return;
      this.heartbeatTimer = 0.0f;
      int index = UnityEngine.Random.Range(0, this.heartbeatClips.Length);
      if (index == this.currentHeartbeatClip)
        index = (index + 1) % this.heartbeatClips.Length;
      this.currentHeartbeatClip = index;
      this.heartbeatSFX.clip = this.heartbeatClips[index];
      this.heartbeatSFX.Play();
    }
  }

  private void PlayNonDiageticSound()
  {
    if ((UnityEngine.Object) StartOfRound.Instance.currentLevel.levelAmbienceClips == (UnityEngine.Object) null)
      return;
    if (this.localPlayer.isPlayerDead || !this.localPlayer.isInsideFactory || (double) this.localPlayer.insanityLevel < (double) this.localPlayer.maxInsanityLevel * 0.20000000298023224)
    {
      this.ambienceAudioNonDiagetic.volume = Mathf.Lerp(this.ambienceAudioNonDiagetic.volume, 0.0f, Time.deltaTime);
      this.isInsanityMusicPlaying = false;
    }
    else
    {
      this.ambienceAudioNonDiagetic.volume = Mathf.Lerp(this.ambienceAudioNonDiagetic.volume, this.localPlayer.insanityLevel / this.localPlayer.maxInsanityLevel, Time.deltaTime);
      if (!this.isInsanityMusicPlaying)
      {
        if ((double) this.localPlayerAmbientMusicTimer < 13.0)
        {
          this.localPlayerAmbientMusicTimer += Time.deltaTime;
        }
        else
        {
          this.localPlayerAmbientMusicTimer = 0.0f;
          if ((double) UnityEngine.Random.Range(0, 45) < (double) this.localPlayer.insanityLevel)
          {
            this.isInsanityMusicPlaying = true;
            this.ambienceAudioNonDiagetic.clip = StartOfRound.Instance.currentLevel.levelAmbienceClips.insanityMusicAudios[UnityEngine.Random.Range(0, StartOfRound.Instance.currentLevel.levelAmbienceClips.insanityMusicAudios.Length)];
            this.ambienceAudioNonDiagetic.Play();
          }
        }
      }
      if (this.ambienceAudioNonDiagetic.isPlaying)
        return;
      this.isInsanityMusicPlaying = false;
    }
  }

  private void ServerSoundTimer()
  {
    if (!this.IsServer)
      return;
    int num = 0;
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
    {
      if (StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled && StartOfRound.Instance.allPlayerScripts[index].isPlayerAlone)
        ++num;
    }
    if (num == GameNetworkManager.Instance.connectedPlayers)
      return;
    this.soundTimerServer += Time.deltaTime;
    if ((double) this.soundTimerServer <= (double) this.soundFrequencyServer)
      return;
    this.soundTimerServer = 0.0f;
    if ((double) UnityEngine.Random.Range(0.0f, 1f) < (double) this.soundRarityServer)
    {
      ++this.localSoundsPlayedInARow;
      this.PlayAmbientSound(true, this.playingInsanitySoundClipOnServer);
    }
    else
      this.serverSoundsPlayedInARow = 0;
    this.SetServerSoundRandomizerVariables();
  }

  private void LocalPlayerSoundTimer()
  {
    if (this.localPlayer.isPlayerDead || !this.localPlayer.isPlayerAlone)
      return;
    this.soundTimer += Time.deltaTime;
    if ((double) this.soundTimer <= (double) this.soundFrequency)
      return;
    this.soundTimer = 0.0f;
    if ((double) UnityEngine.Random.Range(0.0f, 1f) < (double) this.soundRarity)
    {
      ++this.localSoundsPlayedInARow;
      this.PlayAmbientSound(playInsanitySounds: this.playingInsanitySoundClip);
    }
    else
      this.localSoundsPlayedInARow = 0;
    this.SetLocalSoundRandomizerVariables();
  }

  public void SetServerSoundRandomizerVariables()
  {
    this.playingInsanitySoundClipOnServer = (double) TimeOfDay.Instance.normalizedTimeOfDay <= 0.85000002384185791 ? ((double) TimeOfDay.Instance.normalizedTimeOfDay <= 0.60000002384185791 ? UnityEngine.Random.Range(0, 400) < 4 : UnityEngine.Random.Range(0, 400) < 12) : UnityEngine.Random.Range(0, 400) < 20;
    this.soundFrequencyServer = UnityEngine.Random.Range(0, 100) >= 30 ? UnityEngine.Random.Range((float) (10.0 + (double) this.serverSoundsPlayedInARow * 3.0), 15f) : UnityEngine.Random.Range(0.5f, 15f);
    if (this.serverSoundsPlayedInARow > 0)
      this.soundRarityServer /= 3f;
    else
      this.soundRarityServer *= 1.2f;
  }

  public void SetLocalSoundRandomizerVariables()
  {
    this.playingInsanitySoundClip = false;
    bool flag = (double) this.localPlayer.insanityLevel > (double) this.localPlayer.maxInsanityLevel * 0.75;
    if (flag && (double) UnityEngine.Random.Range(0, 100) > 50.0 && this.localSoundsPlayedInARow < 2)
      this.playingInsanitySoundClip = true;
    this.soundFrequency = Mathf.Clamp((float) (10.0 / ((double) this.localPlayer.insanityLevel * 0.039999999105930328)), 2f, 13f);
    if (!flag)
      this.soundFrequency += (float) this.localSoundsPlayedInARow * 2f;
    this.soundFrequency += UnityEngine.Random.Range(-3f, 3f);
    if (this.localSoundsPlayedInARow > 0)
    {
      if (flag && StartOfRound.Instance.connectedPlayersAmount + 1 > 1)
        this.soundRarity /= 3f;
      else
        this.soundRarity /= 5f;
    }
    else
      this.soundRarity *= 1.2f;
    this.soundRarity = Mathf.Clamp(this.soundRarity, 0.02f, 0.98f);
  }

  public void PlayAmbientSound(bool syncedForAllPlayers = false, bool playInsanitySounds = false)
  {
    if ((UnityEngine.Object) StartOfRound.Instance.currentLevel.levelAmbienceClips == (UnityEngine.Object) null)
      return;
    RandomAudioClip[] randomAudioClipArray = (RandomAudioClip[]) null;
    int soundType;
    int num;
    if (this.localPlayer.isInsideFactory)
    {
      soundType = 0;
      if (playInsanitySounds)
      {
        if (StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbienceInsanity.Length == 0)
          return;
        randomAudioClipArray = StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbienceInsanity;
        num = UnityEngine.Random.Range(0, StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbienceInsanity.Length);
      }
      else
      {
        if (StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbience.Length == 0)
          return;
        num = UnityEngine.Random.Range(0, StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbience.Length);
      }
    }
    else if (!this.localPlayer.isInHangarShipRoom)
    {
      soundType = 1;
      if (playInsanitySounds)
      {
        if (StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbienceInsanity.Length == 0)
          return;
        randomAudioClipArray = StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbienceInsanity;
        num = UnityEngine.Random.Range(0, StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbienceInsanity.Length);
      }
      else
      {
        if (StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbience.Length == 0)
          return;
        num = UnityEngine.Random.Range(0, StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbience.Length);
      }
    }
    else
    {
      soundType = 2;
      if (playInsanitySounds)
      {
        if (StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbienceInsanity.Length == 0)
          return;
        randomAudioClipArray = StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbienceInsanity;
        num = UnityEngine.Random.Range(0, StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbienceInsanity.Length);
      }
      else
      {
        if (StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbience.Length == 0)
          return;
        num = UnityEngine.Random.Range(0, StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbience.Length);
      }
    }
    if (randomAudioClipArray != null)
    {
      Debug.Log((object) string.Format("soundtype: {0}; lastSound: {1}", (object) soundType, (object) this.lastSoundTypePlayed));
      if (soundType != this.lastSoundTypePlayed || this.audioClipProbabilities.Count <= 0)
      {
        Debug.Log((object) string.Format("adding to sound probabilities list; array length: {0}", (object) randomAudioClipArray.Length));
        this.audioClipProbabilities.Clear();
        for (int index = 0; index < randomAudioClipArray.Length; ++index)
          this.audioClipProbabilities.Add(randomAudioClipArray[index].chance);
      }
      Debug.Log((object) this.audioClipProbabilities.Count);
      num = RoundManager.Instance.GetRandomWeightedIndexList(this.audioClipProbabilities, this.audioRandom);
      Debug.Log((object) num);
    }
    if (syncedForAllPlayers)
      this.lastServerSoundTypePlayed = soundType;
    else
      this.lastSoundTypePlayed = soundType;
    float soundVolume = (double) UnityEngine.Random.Range(0.0f, 1f) >= 0.40000000596046448 ? UnityEngine.Random.Range(0.7f, 0.9f) : UnityEngine.Random.Range(0.3f, 0.8f);
    if (syncedForAllPlayers)
      this.PlayAmbienceClipServerRpc(soundType, num, soundVolume, playInsanitySounds);
    else
      this.PlayAmbienceClipLocal(soundType, num, soundVolume, playInsanitySounds);
  }

  public void ResetSoundType()
  {
    this.lastSoundTypePlayed = -1;
    this.lastServerSoundTypePlayed = -1;
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayAmbienceClipServerRpc(
    int soundType,
    int clipIndex,
    float soundVolume,
    bool playInsanitySounds)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(274078295U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, soundType);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      bufferWriter.WriteValueSafe<float>(in soundVolume, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in playInsanitySounds, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 274078295U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayAmbienceClipClientRpc(soundType, clipIndex, soundVolume, playInsanitySounds);
  }

  [ClientRpc]
  public void PlayAmbienceClipClientRpc(
    int soundType,
    int clipIndex,
    float soundVolume,
    bool playInsanitySounds)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(580761520U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, soundType);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      bufferWriter.WriteValueSafe<float>(in soundVolume, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in playInsanitySounds, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 580761520U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    try
    {
      Debug.Log((object) string.Format("clip index: {0}; current planet: {1}", (object) clipIndex, (object) StartOfRound.Instance.currentLevel.PlanetName));
      switch (soundType)
      {
        case 0:
          Debug.Log((object) string.Format("Current inside ambience clips length: {0}", (object) StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbience.Length));
          if (playInsanitySounds)
          {
            this.PlaySoundAroundPlayersAsGroup(StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbienceInsanity[clipIndex].audioClip, soundVolume);
            break;
          }
          this.PlaySoundAroundPlayersAsGroup(StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbience[clipIndex], soundVolume);
          break;
        case 1:
          Debug.Log((object) string.Format("Current outside ambience clips length: {0}", (object) StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbience.Length));
          if (playInsanitySounds)
          {
            this.PlaySoundAroundPlayersAsGroup(StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbienceInsanity[clipIndex].audioClip, soundVolume);
            break;
          }
          this.PlaySoundAroundPlayersAsGroup(StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbience[clipIndex], soundVolume);
          break;
        case 2:
          Debug.Log((object) string.Format("Current ship ambience clips length: {0}", (object) StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbience.Length));
          if (playInsanitySounds)
          {
            this.PlaySoundAroundPlayersAsGroup(StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbienceInsanity[clipIndex].audioClip, soundVolume);
            break;
          }
          this.PlaySoundAroundPlayersAsGroup(StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbience[clipIndex], soundVolume);
          break;
      }
    }
    catch (Exception ex)
    {
      Debug.Log((object) ex);
    }
  }

  public void PlayAmbienceClipLocal(
    int soundType,
    int clipIndex,
    float soundVolume,
    bool playInsanitySounds)
  {
    Debug.Log((object) string.Format("clip index: {0}; soundType: {1}; insanity sounds: {2}; vol: {3}", (object) clipIndex, (object) soundType, (object) playInsanitySounds, (object) soundVolume));
    switch (soundType)
    {
      case 0:
        if (playInsanitySounds)
        {
          this.PlaySoundAroundLocalPlayer(StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbienceInsanity[clipIndex].audioClip, soundVolume);
          break;
        }
        this.PlaySoundAroundLocalPlayer(StartOfRound.Instance.currentLevel.levelAmbienceClips.insideAmbience[clipIndex], soundVolume);
        break;
      case 1:
        if (playInsanitySounds)
        {
          this.PlaySoundAroundLocalPlayer(StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbienceInsanity[clipIndex].audioClip, soundVolume);
          break;
        }
        this.PlaySoundAroundLocalPlayer(StartOfRound.Instance.currentLevel.levelAmbienceClips.outsideAmbience[clipIndex], soundVolume);
        break;
      case 2:
        if (playInsanitySounds)
        {
          this.PlaySoundAroundLocalPlayer(StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbienceInsanity[clipIndex].audioClip, soundVolume);
          break;
        }
        this.PlaySoundAroundLocalPlayer(StartOfRound.Instance.currentLevel.levelAmbienceClips.shipAmbience[clipIndex], soundVolume);
        break;
    }
  }

  public void PlaySoundAroundPlayersAsGroup(AudioClip clipToPlay, float vol)
  {
    this.ambienceAudio.transform.position = RoundManager.Instance.GetRandomPositionInRadius(RoundManager.AverageOfLivingGroupedPlayerPositions(), 10f, 15f, this.SoundsRandom);
    this.ambienceAudio.volume = vol;
    this.ambienceAudio.clip = clipToPlay;
    this.ambienceAudio.Play();
  }

  public void PlaySoundAroundLocalPlayer(AudioClip clipToPlay, float vol)
  {
    this.ambienceAudio.transform.position = RoundManager.Instance.GetRandomPositionInRadius(GameNetworkManager.Instance.localPlayerController.transform.position, 6f, 11f);
    this.ambienceAudio.volume = vol;
    this.ambienceAudio.clip = clipToPlay;
    this.ambienceAudio.Play();
  }

  public void SetDiageticMixerSnapshot(int snapshotID = 0, float transitionTime = 1f)
  {
    if (this.currentMixerSnapshotID == snapshotID)
      return;
    this.currentMixerSnapshotID = snapshotID;
    if (this.overridingCurrentAudioMixer)
      return;
    this.mixerSnapshots[snapshotID].TransitionTo(transitionTime);
  }

  public void ResumeCurrentMixerSnapshot(float time)
  {
    this.mixerSnapshots[this.currentMixerSnapshotID].TransitionTo(time);
  }

  public void PlayAudio1AtPositionForAllClients(Vector3 audioPosition, int clipIndex)
  {
    this.PlayAudio1AtPositionServerRpc(audioPosition, clipIndex);
  }

  [ServerRpc(RequireOwnership = false)]
  public void PlayAudio1AtPositionServerRpc(Vector3 audioPos, int clipIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2837950577U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in audioPos);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      this.__endSendServerRpc(ref bufferWriter, 2837950577U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PlayAudio1AtPositionClientRpc(audioPos, clipIndex);
  }

  [ClientRpc]
  public void PlayAudio1AtPositionClientRpc(Vector3 audioPos, int clipIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4269719820U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in audioPos);
      BytePacker.WriteValueBitPacked(bufferWriter, clipIndex);
      this.__endSendClientRpc(ref bufferWriter, 4269719820U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.tempAudio1.transform.position = audioPos;
    this.tempAudio1.PlayOneShot(this.syncedAudioClips[clipIndex], 1f);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_SoundManager()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(274078295U, new NetworkManager.RpcReceiveHandler(SoundManager.__rpc_handler_274078295)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(580761520U, new NetworkManager.RpcReceiveHandler(SoundManager.__rpc_handler_580761520)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2837950577U, new NetworkManager.RpcReceiveHandler(SoundManager.__rpc_handler_2837950577)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4269719820U, new NetworkManager.RpcReceiveHandler(SoundManager.__rpc_handler_4269719820)));
  }

  private static void __rpc_handler_274078295(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int soundType;
    ByteUnpacker.ReadValueBitPacked(reader, out soundType);
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    float soundVolume;
    reader.ReadValueSafe<float>(out soundVolume, new FastBufferWriter.ForPrimitives());
    bool playInsanitySounds;
    reader.ReadValueSafe<bool>(out playInsanitySounds, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((SoundManager) target).PlayAmbienceClipServerRpc(soundType, clipIndex, soundVolume, playInsanitySounds);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_580761520(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int soundType;
    ByteUnpacker.ReadValueBitPacked(reader, out soundType);
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    float soundVolume;
    reader.ReadValueSafe<float>(out soundVolume, new FastBufferWriter.ForPrimitives());
    bool playInsanitySounds;
    reader.ReadValueSafe<bool>(out playInsanitySounds, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SoundManager) target).PlayAmbienceClipClientRpc(soundType, clipIndex, soundVolume, playInsanitySounds);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2837950577(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 audioPos;
    reader.ReadValueSafe(out audioPos);
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((SoundManager) target).PlayAudio1AtPositionServerRpc(audioPos, clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4269719820(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 audioPos;
    reader.ReadValueSafe(out audioPos);
    int clipIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SoundManager) target).PlayAudio1AtPositionClientRpc(audioPos, clipIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (SoundManager);
}
