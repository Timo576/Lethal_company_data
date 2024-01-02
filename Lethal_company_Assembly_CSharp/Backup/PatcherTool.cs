// Decompiled with JetBrains decompiler
// Type: PatcherTool
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class PatcherTool : GrabbableObject
{
  [Space(15f)]
  public float gunAnomalyDamage = 1f;
  public bool isShocking;
  public IShockableWithGun shockedTargetScript;
  [Space(15f)]
  public Light flashlightBulb;
  public Light flashlightBulbGlow;
  public AudioSource mainAudio;
  public AudioSource shockAudio;
  public AudioSource gunAudio;
  public AudioClip[] activateClips;
  public AudioClip[] beginShockClips;
  public AudioClip[] overheatClips;
  public AudioClip[] finishShockClips;
  public AudioClip outOfBatteriesClip;
  public AudioClip detectAnomaly;
  public AudioClip scanAnomaly;
  public Material bulbLight;
  public Material bulbDark;
  public Animator effectAnimator;
  public Animator gunAnimator;
  public ParticleSystem overheatParticle;
  private Coroutine scanGunCoroutine;
  private Coroutine beginShockCoroutine;
  public Transform aimDirection;
  private int anomalyMask = 524296;
  private int roomMask = 256;
  private RaycastHit hit;
  private Ray ray;
  public GameObject lightningObject;
  public Transform lightningDest;
  public Transform lightningBend1;
  public Transform lightningBend2;
  private Vector3 shockVectorMidpoint;
  [Header("Shock difficulty variables")]
  public float bendStrengthCap = 3f;
  public float endStrengthCap = 4.25f;
  private float currentEndStrengthCap;
  public float bendChangeSpeedMultiplier = 10f;
  public float endChangeSpeedMultiplier = 17f;
  private float currentEndChangeSpeedMultiplier;
  public float pullStrength;
  public float endPullStrength = 4.25f;
  private float currentEndPullStrength;
  public float maxChangePerFrame = 0.15f;
  public float endChangePerFrame = 2.5f;
  private float currentEndChangePerFrame;
  [HideInInspector]
  public float bendMultiplier;
  [HideInInspector]
  private float bendRandomizerShift;
  [HideInInspector]
  private Vector3 bendVector;
  public float gunOverheat;
  [HideInInspector]
  private bool sentStopShockingRPC;
  [HideInInspector]
  private bool wasShockingPreviousFrame;
  private LightningSplineScript lightningScript;
  private System.Random gunRandom;
  private int timesUsed;
  private bool lightningVisible;
  private float minigameChecksInterval;
  private float timeSpentShocking;
  private float makeAudibleNoiseTimer;
  public static int finishedShockMinigame;
  private RaycastHit[] raycastEnemies;
  private bool isScanning;
  private float currentDifficultyMultiplier;
  private PlayerControllerB previousPlayerHeldBy;

  public override void Start()
  {
    base.Start();
    this.raycastEnemies = new RaycastHit[12];
  }

  public override void OnDestroy()
  {
    base.OnDestroy();
    if (!((UnityEngine.Object) this.lightningDest != (UnityEngine.Object) null) || !((UnityEngine.Object) this.lightningDest.gameObject != (UnityEngine.Object) null))
      return;
    UnityEngine.Object.Destroy((UnityEngine.Object) this.lightningDest.gameObject);
  }

  public override void ItemActivate(bool used, bool buttonDown = true)
  {
    base.ItemActivate(used, buttonDown);
    this.gunOverheat = 0.0f;
    if ((UnityEngine.Object) this.playerHeldBy == (UnityEngine.Object) null)
      return;
    if (this.scanGunCoroutine != null)
    {
      this.StopCoroutine(this.scanGunCoroutine);
      this.scanGunCoroutine = (Coroutine) null;
    }
    if (this.beginShockCoroutine != null)
    {
      this.StopCoroutine(this.beginShockCoroutine);
      this.beginShockCoroutine = (Coroutine) null;
    }
    if (this.isShocking)
    {
      Debug.Log((object) "Stop shocking gun");
      this.StopShockingAnomalyOnClient(true);
    }
    else if (this.isScanning)
    {
      this.SwitchFlashlight(false);
      this.gunAudio.Stop();
      this.currentUseCooldown = 0.5f;
      if (this.scanGunCoroutine != null)
      {
        this.StopCoroutine(this.scanGunCoroutine);
        this.scanGunCoroutine = (Coroutine) null;
      }
      this.isScanning = false;
    }
    else
    {
      Debug.Log((object) "Start scanning gun");
      this.isScanning = true;
      this.sentStopShockingRPC = false;
      this.scanGunCoroutine = this.StartCoroutine(this.ScanGun());
      this.currentUseCooldown = 0.5f;
      Debug.Log((object) "Use patcher tool");
      this.PlayRandomAudio(this.mainAudio, this.activateClips);
      this.SwitchFlashlight(true);
    }
  }

  private void PlayRandomAudio(AudioSource audioSource, AudioClip[] audioClips)
  {
    if (audioClips.Length == 0)
      return;
    audioSource.PlayOneShot(audioClips[UnityEngine.Random.Range(0, audioClips.Length)]);
  }

  private bool GunMeetsConditionsToShock(
    PlayerControllerB playerUsingGun,
    Vector3 targetPosition,
    float maxAngle = 80f)
  {
    Debug.Log((object) string.Format("Target position: {0}", (object) targetPosition));
    Vector3 position = playerUsingGun.gameplayCamera.transform.position with
    {
      y = targetPosition.y
    };
    if ((double) Vector3.Angle(playerUsingGun.transform.forward, targetPosition - position) > (double) maxAngle)
      return false;
    if ((double) this.gunOverheat <= 2.0 && (double) Vector3.Distance(position, targetPosition) >= 0.699999988079071 && (double) Vector3.Distance(position, targetPosition) <= 13.0 && !Physics.Linecast(position, targetPosition, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
      return true;
    RaycastHit hitInfo;
    if (Physics.Linecast(position, targetPosition, out hitInfo, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
    {
      Debug.Log((object) hitInfo.transform.name);
      Debug.Log((object) hitInfo.transform.gameObject.name);
      Debug.DrawLine(position, targetPosition, Color.green, 25f);
    }
    Debug.Log((object) string.Format("Gun not meeting conditions to zap; {0}; {1}; {2}", (object) ((double) this.gunOverheat > 2.0), (object) ((double) Vector3.Distance(position, targetPosition) < 0.699999988079071), (object) Physics.Linecast(position, targetPosition, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore)));
    return false;
  }

  public override void LateUpdate()
  {
    base.LateUpdate();
    if (!this.lightningVisible)
      return;
    if (this.isShocking && this.shockedTargetScript != null && (UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null && !this.playerHeldBy.isPlayerDead && !this.insertedBattery.empty)
    {
      this.timeSpentShocking += Time.deltaTime / 8f;
      if ((double) this.makeAudibleNoiseTimer <= 0.0)
      {
        this.makeAudibleNoiseTimer = 0.8f;
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, 20f, 0.92f, noiseIsInsideClosedShip: this.isInShipRoom && StartOfRound.Instance.hangarDoorsClosed, noiseID: 11);
      }
      else
        this.makeAudibleNoiseTimer -= Time.deltaTime;
      Vector3 shockablePosition = this.shockedTargetScript.GetShockablePosition();
      this.lightningDest.position = shockablePosition + new Vector3(UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(-0.3f, 0.3f), UnityEngine.Random.Range(-0.3f, 0.3f));
      this.shockVectorMidpoint = Vector3.Normalize(this.shockedTargetScript.GetShockableTransform().position - this.aimDirection.position);
      this.bendVector = this.playerHeldBy.transform.right * this.bendMultiplier;
      this.lightningBend1.position = this.aimDirection.position + 0.3f * this.shockVectorMidpoint + this.bendVector;
      this.lightningBend2.position = this.aimDirection.position + 0.6f * this.shockVectorMidpoint + this.bendVector;
      this.bendMultiplier = (double) this.bendRandomizerShift >= 0.0 ? Mathf.Clamp(this.bendMultiplier + Mathf.Clamp(this.RandomFloatInRange(this.gunRandom, -1f, 1f + Mathf.Round(this.bendRandomizerShift)) * (this.bendChangeSpeedMultiplier * Time.deltaTime), -this.maxChangePerFrame, this.maxChangePerFrame), -this.bendStrengthCap, this.bendStrengthCap) : Mathf.Clamp(this.bendMultiplier + Mathf.Clamp(this.RandomFloatInRange(this.gunRandom, Mathf.Round(this.bendRandomizerShift) - 1f, 1f) * (this.bendChangeSpeedMultiplier * Time.deltaTime), -this.maxChangePerFrame, this.maxChangePerFrame), -this.bendStrengthCap, this.bendStrengthCap);
      this.ShiftBendRandomizer();
      this.AdjustDifficultyValues();
      if (!this.IsOwner)
        return;
      this.wasShockingPreviousFrame = true;
      float num = Mathf.Abs(Mathf.Clamp(this.bendMultiplier * 0.5f, -0.5f, 0.5f) - this.playerHeldBy.shockMinigamePullPosition * 2f);
      this.playerHeldBy.turnCompass.Rotate(Vector3.up * 100f * this.bendMultiplier * this.pullStrength * Time.deltaTime);
      RoundManager.Instance.tempTransform.eulerAngles = new Vector3(0.0f, this.playerHeldBy.gameplayCamera.transform.eulerAngles.y, this.playerHeldBy.gameplayCamera.transform.eulerAngles.z);
      if ((double) Vector3.Angle(RoundManager.Instance.tempTransform.forward, new Vector3(this.lightningDest.position.x, this.playerHeldBy.gameplayCamera.transform.position.y, this.lightningDest.position.z) - this.playerHeldBy.gameplayCamera.transform.position) > 90.0)
        this.gunOverheat += Time.deltaTime * 10f;
      if ((double) this.bendMultiplier < -0.30000001192092896)
      {
        if ((double) this.playerHeldBy.shockMinigamePullPosition < 0.0)
          this.gunOverheat = Mathf.Clamp(this.gunOverheat - Time.deltaTime * 3f, 0.0f, 10f);
        else
          this.gunOverheat += Time.deltaTime * (num * 2f);
        HUDManager.Instance.SetTutorialArrow(2);
      }
      else if ((double) this.bendMultiplier > 0.30000001192092896)
      {
        if ((double) this.playerHeldBy.shockMinigamePullPosition > 0.0)
          this.gunOverheat = Mathf.Clamp(this.gunOverheat - Time.deltaTime * 3f, 0.0f, 10f);
        else
          this.gunOverheat += Time.deltaTime * (num * 2f);
        HUDManager.Instance.SetTutorialArrow(1);
      }
      else
        HUDManager.Instance.SetTutorialArrow(0);
      this.minigameChecksInterval -= Time.deltaTime;
      if ((double) this.minigameChecksInterval <= 0.0)
      {
        this.minigameChecksInterval = 0.15f;
        if (this.shockedTargetScript == null || !this.GunMeetsConditionsToShock(this.playerHeldBy, shockablePosition))
        {
          this.StopShockingAnomalyOnClient(true);
          return;
        }
      }
      if ((double) this.gunOverheat > 0.75)
      {
        this.gunAudio.volume = Mathf.Lerp(this.gunAudio.volume, 1f, 13f * Time.deltaTime);
        this.gunAnimator.SetBool("Overheating", true);
      }
      else
      {
        this.gunAudio.volume = Mathf.Lerp(this.gunAudio.volume, 0.0f, 7f * Time.deltaTime);
        this.gunAnimator.SetBool("Overheating", false);
      }
    }
    else
    {
      if (!this.wasShockingPreviousFrame)
        return;
      this.wasShockingPreviousFrame = false;
      this.timeSpentShocking = 0.0f;
      if (!this.IsOwner)
        return;
      this.StopShockingAnomalyOnClient();
    }
  }

  private void AdjustDifficultyValues()
  {
    this.bendStrengthCap = Mathf.Lerp(0.4f, this.currentEndStrengthCap, this.timeSpentShocking * this.currentDifficultyMultiplier);
    this.bendChangeSpeedMultiplier = Mathf.Lerp(3.5f, this.currentEndChangeSpeedMultiplier, this.timeSpentShocking * this.currentDifficultyMultiplier);
    this.pullStrength = Mathf.Lerp(0.4f, this.currentEndPullStrength, this.timeSpentShocking * this.currentDifficultyMultiplier);
    this.maxChangePerFrame = Mathf.Lerp(0.13f, this.currentEndChangePerFrame, this.timeSpentShocking * this.currentDifficultyMultiplier);
    this.lightningScript.Forkedness = Mathf.Lerp(0.11f, 0.45f, this.timeSpentShocking * this.currentDifficultyMultiplier);
    this.lightningScript.ForkLengthMultiplier = Mathf.Lerp(0.11f, 1.1f, this.timeSpentShocking * this.currentDifficultyMultiplier);
    this.lightningScript.ForkLengthVariance = Mathf.Lerp(0.08f, 4f, this.timeSpentShocking * this.currentDifficultyMultiplier);
    this.shockAudio.volume = Mathf.Lerp(0.1f, 1f, this.timeSpentShocking * this.currentDifficultyMultiplier);
  }

  private void InitialDifficultyValues()
  {
    this.currentEndStrengthCap = this.SetCurrentDifficultyValue(this.endStrengthCap, 1.4f);
    this.currentEndChangeSpeedMultiplier = this.SetCurrentDifficultyValue(this.endChangeSpeedMultiplier, 7f);
    this.currentEndPullStrength = this.SetCurrentDifficultyValue(this.endPullStrength, 0.4f);
    this.currentEndChangePerFrame = this.SetCurrentDifficultyValue(this.endChangePerFrame, 0.12f);
  }

  private float SetCurrentDifficultyValue(float max, float min)
  {
    return this.shockedTargetScript.GetDifficultyMultiplier() * (max - min) + min;
  }

  public void ShiftBendRandomizer()
  {
    if ((double) this.bendMultiplier < 0.0)
    {
      if ((double) this.bendMultiplier < -0.5)
        this.bendRandomizerShift += 1f * Time.deltaTime;
      else
        this.bendRandomizerShift -= 1f * Time.deltaTime;
    }
    else if ((double) this.bendMultiplier > 0.5)
      this.bendRandomizerShift -= 1f * Time.deltaTime;
    else
      this.bendRandomizerShift += 1f * Time.deltaTime;
  }

  private void OnEnable() => this.StartCoroutine(this.waitForStartOfRoundInstance());

  private IEnumerator waitForStartOfRoundInstance()
  {
    PatcherTool patcherTool = this;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null && StartOfRound.Instance.CameraSwitchEvent != null && (UnityEngine.Object) StartOfRound.Instance.activeCamera != (UnityEngine.Object) null));
    StartOfRound.Instance.CameraSwitchEvent.AddListener(new UnityAction(patcherTool.OnSwitchCamera));
    if ((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null && (UnityEngine.Object) StartOfRound.Instance.activeCamera != (UnityEngine.Object) null)
    {
      patcherTool.lightningScript = patcherTool.lightningObject.GetComponent<LightningSplineScript>();
      patcherTool.lightningScript.Camera = StartOfRound.Instance.activeCamera;
    }
  }

  private void OnDisable()
  {
    StartOfRound.Instance.CameraSwitchEvent.RemoveListener(new UnityAction(this.OnSwitchCamera));
  }

  private void OnSwitchCamera()
  {
    this.lightningObject.GetComponent<LightningSplineScript>().Camera = StartOfRound.Instance.activeCamera;
  }

  private IEnumerator ScanGun()
  {
    PatcherTool patcherTool = this;
    patcherTool.effectAnimator.SetTrigger("Scan");
    patcherTool.gunAudio.PlayOneShot(patcherTool.scanAnomaly);
    patcherTool.lightningScript = patcherTool.lightningObject.GetComponent<LightningSplineScript>();
    patcherTool.lightningDest.SetParent((Transform) null);
    patcherTool.lightningBend1.SetParent((Transform) null);
    patcherTool.lightningBend2.SetParent((Transform) null);
    Debug.Log((object) "Scan A");
    for (int i = 0; i < 12; ++i)
    {
      if (patcherTool.IsOwner)
      {
        Debug.Log((object) "Scan B");
        if (patcherTool.isPocketed)
        {
          yield break;
        }
        else
        {
          patcherTool.ray = new Ray(patcherTool.playerHeldBy.gameplayCamera.transform.position - patcherTool.playerHeldBy.gameplayCamera.transform.forward * 3f, patcherTool.playerHeldBy.gameplayCamera.transform.forward);
          Debug.DrawRay(patcherTool.playerHeldBy.gameplayCamera.transform.position - patcherTool.playerHeldBy.gameplayCamera.transform.forward * 3f, patcherTool.playerHeldBy.gameplayCamera.transform.forward * 6f, Color.red, 5f);
          int num = Physics.SphereCastNonAlloc(patcherTool.ray, 5f, patcherTool.raycastEnemies, 5f, patcherTool.anomalyMask, QueryTriggerInteraction.Collide);
          patcherTool.raycastEnemies = ((IEnumerable<RaycastHit>) patcherTool.raycastEnemies).OrderBy<RaycastHit, float>((Func<RaycastHit, float>) (x => x.distance)).ToArray<RaycastHit>();
          for (int index = 0; index < num; ++index)
          {
            if (index < patcherTool.raycastEnemies.Length)
            {
              patcherTool.hit = patcherTool.raycastEnemies[index];
              IShockableWithGun component;
              if (!((UnityEngine.Object) patcherTool.hit.transform == (UnityEngine.Object) null) && patcherTool.hit.transform.gameObject.TryGetComponent<IShockableWithGun>(out component) && component.CanBeShocked())
              {
                Vector3 shockablePosition = component.GetShockablePosition();
                Debug.Log((object) ("Got shockable transform name : " + component.GetShockableTransform().gameObject.name));
                if (patcherTool.GunMeetsConditionsToShock(patcherTool.playerHeldBy, shockablePosition, 60f))
                {
                  patcherTool.gunAudio.Stop();
                  patcherTool.BeginShockingAnomalyOnClient(component);
                  yield break;
                }
              }
            }
          }
        }
      }
      yield return (object) new WaitForSeconds(0.125f);
    }
    Debug.Log((object) "Zap gun light off!!!");
    patcherTool.SwitchFlashlight(false);
    patcherTool.isScanning = false;
  }

  public void BeginShockingAnomalyOnClient(IShockableWithGun shockableScript)
  {
    ++this.timesUsed;
    this.sentStopShockingRPC = false;
    this.gunRandom = new System.Random(this.playerHeldBy.playersManager.randomMapSeed + this.timesUsed);
    this.gunOverheat = 0.0f;
    this.shockedTargetScript = shockableScript;
    this.currentDifficultyMultiplier = shockableScript.GetDifficultyMultiplier();
    this.InitialDifficultyValues();
    this.bendMultiplier = 0.0f;
    this.bendRandomizerShift = 0.0f;
    if (this.beginShockCoroutine != null)
      this.StopCoroutine(this.beginShockCoroutine);
    this.beginShockCoroutine = this.StartCoroutine(this.beginShockGame(shockableScript));
  }

  private IEnumerator beginShockGame(IShockableWithGun shockableScript)
  {
    PatcherTool patcherTool = this;
    if (shockableScript == null || (UnityEngine.Object) shockableScript.GetNetworkObject() == (UnityEngine.Object) null)
    {
      Debug.LogError((object) string.Format("Zap gun: The shockable script was null when starting the minigame! ; {0}; {1}", (object) (shockableScript == null), (object) ((UnityEngine.Object) shockableScript.GetNetworkObject() == (UnityEngine.Object) null)));
      patcherTool.isScanning = false;
    }
    else
    {
      patcherTool.effectAnimator.SetTrigger("Shock");
      patcherTool.gunAudio.PlayOneShot(patcherTool.detectAnomaly);
      patcherTool.isShocking = true;
      patcherTool.isScanning = false;
      patcherTool.playerHeldBy.inShockingMinigame = true;
      Transform shockableTransform = shockableScript.GetShockableTransform();
      patcherTool.playerHeldBy.shockingTarget = shockableTransform;
      patcherTool.playerHeldBy.isCrouching = false;
      patcherTool.playerHeldBy.playerBodyAnimator.SetBool("crouching", false);
      patcherTool.playerHeldBy.turnCompass.LookAt(shockableTransform);
      Vector3 zero = Vector3.zero with
      {
        y = patcherTool.playerHeldBy.turnCompass.localEulerAngles.y
      };
      patcherTool.playerHeldBy.turnCompass.localEulerAngles = zero;
      yield return (object) new WaitForSeconds(0.55f);
      patcherTool.StartShockAudios();
      patcherTool.isBeingUsed = true;
      patcherTool.shockedTargetScript.ShockWithGun(patcherTool.playerHeldBy);
      patcherTool.playerHeldBy.inSpecialInteractAnimation = true;
      patcherTool.playerHeldBy.playerBodyAnimator.SetBool("HoldPatcherTool", true);
      patcherTool.SwitchFlashlight(false);
      patcherTool.gunAnimator.SetTrigger("Shock");
      patcherTool.lightningObject.SetActive(true);
      patcherTool.lightningVisible = true;
      patcherTool.ShockPatcherToolServerRpc((NetworkObjectReference) shockableScript.GetNetworkObject());
    }
  }

  private void StartShockAudios()
  {
    this.PlayRandomAudio(this.mainAudio, this.beginShockClips);
    this.gunAudio.Play();
    this.mainAudio.Play();
    this.mainAudio.volume = 1f;
    this.shockAudio.Play();
    this.shockAudio.volume = 0.0f;
  }

  public void StopShockingAnomalyOnClient(bool failed = false)
  {
    if (this.scanGunCoroutine != null)
    {
      this.StopCoroutine(this.scanGunCoroutine);
      this.scanGunCoroutine = (Coroutine) null;
    }
    this.timeSpentShocking = 0.0f;
    this.wasShockingPreviousFrame = false;
    this.lightningVisible = false;
    this.lightningObject.SetActive(false);
    this.isBeingUsed = false;
    this.SwitchFlashlight(false);
    this.gunAnimator.SetBool("Overheating", false);
    this.gunAnimator.SetBool("Shock", false);
    if (this.shockedTargetScript != null)
      this.shockedTargetScript.StopShockingWithGun();
    this.gunOverheat = 0.0f;
    this.gunAudio.Stop();
    this.gunAudio.volume = 1f;
    this.mainAudio.Stop();
    this.shockAudio.Stop();
    if (this.IsOwner && (UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null && !this.sentStopShockingRPC)
    {
      HUDManager.Instance.SetTutorialArrow(0);
      this.sentStopShockingRPC = true;
      this.StopShockingServerRpc();
      this.playerHeldBy.playerBodyAnimator.SetTrigger("Overheat");
      this.playerHeldBy.thisPlayerBody.localEulerAngles = this.playerHeldBy.thisPlayerBody.localEulerAngles with
      {
        x = 0.0f,
        z = 0.0f
      };
    }
    if (failed)
    {
      this.PlayRandomAudio(this.gunAudio, this.overheatClips);
      this.overheatParticle.Play();
      this.currentUseCooldown = 5f;
      this.effectAnimator.SetTrigger("FailGame");
      if ((double) this.timeSpentShocking > 0.75)
        this.SetFinishedShockMinigameTutorial();
    }
    else
    {
      this.currentUseCooldown = 0.25f;
      this.effectAnimator.SetTrigger("FinishGame");
      if (this.IsOwner)
        this.playerHeldBy.playerBodyAnimator.SetTrigger("Overheat");
      this.SetFinishedShockMinigameTutorial();
    }
    this.playerHeldBy.PlayQuickSpecialAnimation(3f);
    this.PlayRandomAudio(this.mainAudio, this.finishShockClips);
    if (this.IsOwner)
    {
      if ((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null)
      {
        this.playerHeldBy.playerBodyAnimator.SetBool("HoldPatcherTool", false);
        this.StartCoroutine(this.stopShocking(this.playerHeldBy));
      }
      else
        Debug.LogError((object) "Error: playerHeldBy is null for owner of zap gun when stopping shock, in client rpc");
    }
    else
    {
      this.isShocking = false;
      if (!((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null))
        return;
      this.playerHeldBy.inSpecialInteractAnimation = false;
      this.playerHeldBy.inShockingMinigame = false;
    }
  }

  private void SetFinishedShockMinigameTutorial()
  {
    if (!HUDManager.Instance.setTutorialArrow)
      return;
    ++PatcherTool.finishedShockMinigame;
    if (PatcherTool.finishedShockMinigame < 2)
      return;
    HUDManager.Instance.setTutorialArrow = false;
  }

  private IEnumerator stopShocking(PlayerControllerB playerController)
  {
    yield return (object) new WaitForSeconds(0.4f);
    this.isShocking = false;
    playerController.inSpecialInteractAnimation = false;
    playerController.inShockingMinigame = false;
  }

  [ServerRpc(RequireOwnership = false)]
  public void ShockPatcherToolServerRpc(NetworkObjectReference netObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2303694898U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in netObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 2303694898U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ShockPatcherToolClientRpc(netObject);
    Debug.Log((object) "Patcher tool server rpc received");
  }

  [ClientRpc]
  public void ShockPatcherToolClientRpc(NetworkObjectReference netObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4275427213U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in netObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 4275427213U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) "Shock patcher tool client rpc received");
    if (this.IsOwner || (UnityEngine.Object) this.previousPlayerHeldBy == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
      return;
    Debug.Log((object) "Running shock patcher tool function");
    ++this.timesUsed;
    this.gunRandom = new System.Random(this.playerHeldBy.playersManager.randomMapSeed + this.timesUsed);
    if (this.scanGunCoroutine != null)
      this.StopCoroutine(this.scanGunCoroutine);
    NetworkObject networkObject;
    if (!netObject.TryGet(out networkObject))
      return;
    this.isShocking = true;
    this.isScanning = false;
    this.shockedTargetScript = networkObject.gameObject.GetComponentInChildren<IShockableWithGun>();
    if (this.shockedTargetScript != null)
    {
      this.shockedTargetScript.ShockWithGun(this.playerHeldBy);
      this.StartShockAudios();
      this.lightningObject.SetActive(true);
      this.SwitchFlashlight(false);
      this.gunAnimator.SetTrigger("UseGun");
      this.effectAnimator.SetTrigger("Shock");
      this.lightningVisible = true;
      this.playerHeldBy.inShockingMinigame = true;
      this.playerHeldBy.inSpecialInteractAnimation = true;
    }
    else
      Debug.LogError((object) "Zap gun: Unable to get IShockableWithGun interface from networkobject on client rpc!");
  }

  [ServerRpc(RequireOwnership = false)]
  public void StopShockingServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3351579778U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3351579778U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StopShockingClientRpc();
  }

  [ClientRpc]
  public void StopShockingClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(75402723U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 75402723U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) "Running client rpc stopping shock");
    if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || this.IsOwner || (UnityEngine.Object) this.previousPlayerHeldBy == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
      return;
    Debug.Log((object) string.Format("{0} ; {1}", (object) this.IsOwner, (object) this.previousPlayerHeldBy));
    this.StopShockingAnomalyOnClient();
  }

  public override void UseUpBatteries()
  {
    base.UseUpBatteries();
    this.SwitchFlashlight(false);
    this.gunAudio.PlayOneShot(this.outOfBatteriesClip, 1f);
  }

  public override void PocketItem()
  {
    this.isBeingUsed = false;
    if ((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null)
      this.DisablePatcherGun();
    else
      Debug.Log((object) "Could not find what player was holding this item");
    base.PocketItem();
  }

  public override void DiscardItem()
  {
    this.DisablePatcherGun();
    base.DiscardItem();
  }

  private void DisablePatcherGun()
  {
    this.SwitchFlashlight(false);
    if (this.scanGunCoroutine != null)
    {
      this.StopCoroutine(this.scanGunCoroutine);
      this.scanGunCoroutine = (Coroutine) null;
    }
    if (this.beginShockCoroutine != null)
    {
      this.StopCoroutine(this.beginShockCoroutine);
      this.beginShockCoroutine = (Coroutine) null;
    }
    if ((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null && this.isShocking)
      this.StopShockingAnomalyOnClient(true);
    this.isBeingUsed = false;
    this.wasShockingPreviousFrame = false;
  }

  public override void EquipItem()
  {
    base.EquipItem();
    if (!((UnityEngine.Object) this.playerHeldBy != (UnityEngine.Object) null))
      return;
    this.previousPlayerHeldBy = this.playerHeldBy;
  }

  public void SwitchFlashlight(bool on)
  {
    this.flashlightBulb.enabled = on;
    this.flashlightBulbGlow.enabled = on;
  }

  private float RandomFloatInRange(System.Random rand, float min, float max)
  {
    return (float) (rand.NextDouble() * ((double) max - (double) min)) + min;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_PatcherTool()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2303694898U, new NetworkManager.RpcReceiveHandler(PatcherTool.__rpc_handler_2303694898)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4275427213U, new NetworkManager.RpcReceiveHandler(PatcherTool.__rpc_handler_4275427213)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3351579778U, new NetworkManager.RpcReceiveHandler(PatcherTool.__rpc_handler_3351579778)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(75402723U, new NetworkManager.RpcReceiveHandler(PatcherTool.__rpc_handler_75402723)));
  }

  private static void __rpc_handler_2303694898(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference netObject;
    reader.ReadValueSafe<NetworkObjectReference>(out netObject, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((PatcherTool) target).ShockPatcherToolServerRpc(netObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4275427213(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference netObject;
    reader.ReadValueSafe<NetworkObjectReference>(out netObject, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((PatcherTool) target).ShockPatcherToolClientRpc(netObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3351579778(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((PatcherTool) target).StopShockingServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_75402723(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((PatcherTool) target).StopShockingClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (PatcherTool);
}
