// Decompiled with JetBrains decompiler
// Type: StormyWeather
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DigitalRuby.ThunderAndLightning;
using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

#nullable disable
public class StormyWeather : MonoBehaviour
{
  private float randomThunderTime;
  private float timeAtLastStrike;
  private Vector3 lastRandomStrikePosition;
  private System.Random seed;
  public AudioClip[] strikeSFX;
  public AudioClip[] distantThunderSFX;
  public LightningBoltPrefabScript randomThunder;
  public LightningBoltPrefabScript targetedThunder;
  public AudioSource randomStrikeAudio;
  public AudioSource randomStrikeAudioB;
  private bool lastStrikeAudioUsed;
  public AudioSource targetedStrikeAudio;
  private RaycastHit rayHit;
  private GameObject[] outsideNodes;
  private NavMeshHit navHit;
  public ParticleSystem explosionEffectParticle;
  private List<GrabbableObject> metalObjects = new List<GrabbableObject>();
  private GrabbableObject targetingMetalObject;
  private float getObjectToTargetInterval;
  private float strikeMetalObjectTimer;
  private bool hasShownStrikeWarning;
  public ParticleSystem staticElectricityParticle;
  private GameObject setStaticToObject;
  private GrabbableObject setStaticGrabbableObject;
  public AudioClip staticElectricityAudio;
  private float lastGlobalTimeUsed;
  private float globalTimeAtLastStrike;
  private System.Random targetedThunderRandom;

  private void OnEnable()
  {
    this.lastRandomStrikePosition = Vector3.zero;
    this.targetedThunderRandom = new System.Random(StartOfRound.Instance.randomMapSeed);
    TimeOfDay.Instance.onTimeSync.AddListener(new UnityAction(this.OnGlobalTimeSync));
    this.globalTimeAtLastStrike = TimeOfDay.Instance.globalTime;
    this.lastGlobalTimeUsed = 0.0f;
    this.randomThunderTime = TimeOfDay.Instance.globalTime + 7f;
    this.timeAtLastStrike = TimeOfDay.Instance.globalTime;
    this.navHit = new NavMeshHit();
    this.outsideNodes = ((IEnumerable<GameObject>) GameObject.FindGameObjectsWithTag("OutsideAINode")).OrderBy<GameObject, float>((Func<GameObject, float>) (x => x.transform.position.x + x.transform.position.z)).ToArray<GameObject>();
    if (StartOfRound.Instance.spectateCamera.enabled)
      this.SwitchCamera(StartOfRound.Instance.spectateCamera);
    else
      this.SwitchCamera(GameNetworkManager.Instance.localPlayerController.gameplayCamera);
    this.seed = new System.Random(StartOfRound.Instance.randomMapSeed);
    this.DetermineNextStrikeInterval();
    this.StartCoroutine(this.GetMetalObjectsAfterDelay());
  }

  private void OnDisable()
  {
    TimeOfDay.Instance.onTimeSync.RemoveListener(new UnityAction(this.OnGlobalTimeSync));
  }

  private void OnGlobalTimeSync()
  {
    float nearestTen = (float) this.RoundUpToNearestTen(TimeOfDay.Instance.globalTime);
    if ((double) nearestTen == (double) this.lastGlobalTimeUsed)
      return;
    this.lastGlobalTimeUsed = nearestTen;
    this.seed = new System.Random((int) nearestTen + StartOfRound.Instance.randomMapSeed);
    this.timeAtLastStrike = TimeOfDay.Instance.globalTime;
  }

  private IEnumerator GetMetalObjectsAfterDelay()
  {
    yield return (object) new WaitForSeconds(15f);
    GrabbableObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].itemProperties.isConductiveMetal)
        this.metalObjects.Add(objectsOfType[index]);
    }
  }

  public void SwitchCamera(Camera newCamera)
  {
    this.randomThunder.Camera = newCamera;
    this.targetedThunder.Camera = newCamera;
  }

  private void DetermineNextStrikeInterval()
  {
    this.timeAtLastStrike = this.randomThunderTime;
    this.randomThunderTime += Mathf.Clamp((float) this.seed.Next(-5, 110) * 0.25f, 0.6f, 110f) / Mathf.Clamp(TimeOfDay.Instance.currentWeatherVariable, 1f, 100f);
  }

  private int RoundUpToNearestTen(float x) => (int) ((double) x / 10.0) * 10;

  private void Update()
  {
    if (!this.gameObject.activeInHierarchy)
      return;
    if ((double) TimeOfDay.Instance.globalTime > (double) this.randomThunderTime)
    {
      this.LightningStrikeRandom();
      this.DetermineNextStrikeInterval();
    }
    if ((UnityEngine.Object) this.setStaticToObject != (UnityEngine.Object) null && (UnityEngine.Object) this.setStaticGrabbableObject != (UnityEngine.Object) null)
    {
      if (this.setStaticGrabbableObject.isInFactory)
        this.staticElectricityParticle.Stop();
      this.staticElectricityParticle.transform.position = this.setStaticToObject.transform.position;
    }
    if (!RoundManager.Instance.IsOwner)
      return;
    if ((UnityEngine.Object) this.targetingMetalObject == (UnityEngine.Object) null)
    {
      if (this.metalObjects.Count <= 0)
        return;
      if ((double) this.getObjectToTargetInterval <= 4.0)
      {
        this.getObjectToTargetInterval += Time.deltaTime;
      }
      else
      {
        this.hasShownStrikeWarning = false;
        this.strikeMetalObjectTimer = Mathf.Clamp(UnityEngine.Random.Range(1f, 28f), 0.0f, 20f);
        this.getObjectToTargetInterval = 0.0f;
        float num1 = 1000f;
        for (int index1 = 0; index1 < this.metalObjects.Count; ++index1)
        {
          if (!this.metalObjects[index1].isInFactory && !this.metalObjects[index1].isInShipRoom)
          {
            for (int index2 = 0; index2 < StartOfRound.Instance.allPlayerScripts.Length; ++index2)
            {
              if (StartOfRound.Instance.allPlayerScripts[index2].isPlayerControlled)
              {
                float num2 = Vector3.Distance(this.metalObjects[index1].transform.position, StartOfRound.Instance.allPlayerScripts[index2].transform.position);
                if ((double) num2 < (double) num1)
                {
                  this.targetingMetalObject = this.metalObjects[index1];
                  num1 = num2;
                  break;
                }
              }
            }
            if (UnityEngine.Random.Range(0, 100) < 20)
              break;
          }
        }
        if (!((UnityEngine.Object) this.targetingMetalObject != (UnityEngine.Object) null) || !this.targetingMetalObject.isHeld)
          return;
        this.strikeMetalObjectTimer = Mathf.Clamp(this.strikeMetalObjectTimer + Time.deltaTime, 4f, 20f);
      }
    }
    else
    {
      this.strikeMetalObjectTimer -= Time.deltaTime;
      if ((double) this.strikeMetalObjectTimer <= 0.0)
      {
        if (!this.targetingMetalObject.isInFactory)
          RoundManager.Instance.LightningStrikeServerRpc(this.targetingMetalObject.transform.position);
        this.getObjectToTargetInterval = 5f;
        this.targetingMetalObject = (GrabbableObject) null;
      }
      else
      {
        if ((double) this.strikeMetalObjectTimer >= 10.0 || this.hasShownStrikeWarning)
          return;
        this.hasShownStrikeWarning = true;
        float timeLeft = Mathf.Abs(this.strikeMetalObjectTimer - 10f);
        RoundManager.Instance.ShowStaticElectricityWarningServerRpc((NetworkObjectReference) this.targetingMetalObject.gameObject.GetComponent<NetworkObject>(), timeLeft);
      }
    }
  }

  public void SetStaticElectricityWarning(NetworkObject warningObject, float particleTime)
  {
    this.setStaticToObject = warningObject.gameObject;
    GrabbableObject component = warningObject.gameObject.GetComponent<GrabbableObject>();
    if ((UnityEngine.Object) component != (UnityEngine.Object) null)
    {
      this.setStaticGrabbableObject = warningObject.gameObject.GetComponent<GrabbableObject>();
      for (int index = 0; index < GameNetworkManager.Instance.localPlayerController.ItemSlots.Length; ++index)
      {
        if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.ItemSlots[index] == (UnityEngine.Object) component)
          HUDManager.Instance.DisplayTip("ALERT!", "Drop your metallic items now! A static charge has been detected. You have seconds left to live.", true, true, "LC_LightningTip");
      }
    }
    this.staticElectricityParticle.shape.meshRenderer = this.setStaticToObject.GetComponentInChildren<MeshRenderer>();
    this.staticElectricityParticle.time = particleTime;
    this.staticElectricityParticle.Play();
    this.staticElectricityParticle.time = particleTime;
    this.staticElectricityParticle.gameObject.GetComponent<AudioSource>().clip = this.staticElectricityAudio;
    this.staticElectricityParticle.gameObject.GetComponent<AudioSource>().Play();
    this.staticElectricityParticle.gameObject.GetComponent<AudioSource>().time = particleTime;
  }

  private void LightningStrikeRandom()
  {
    Vector3 strikePosition;
    if (this.seed.Next(0, 100) < 60 && ((double) this.randomThunderTime - (double) this.timeAtLastStrike) * (double) TimeOfDay.Instance.currentWeatherVariable < 3.0)
    {
      strikePosition = this.lastRandomStrikePosition;
    }
    else
    {
      int index = this.seed.Next(0, this.outsideNodes.Length);
      if (this.outsideNodes == null || (UnityEngine.Object) this.outsideNodes[index] == (UnityEngine.Object) null)
        this.outsideNodes = ((IEnumerable<GameObject>) GameObject.FindGameObjectsWithTag("OutsideAINode")).OrderBy<GameObject, float>((Func<GameObject, float>) (x => x.transform.position.x + x.transform.position.z)).ToArray<GameObject>();
      strikePosition = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(this.outsideNodes[index].transform.position, 15f, this.navHit, this.seed);
    }
    this.lastRandomStrikePosition = strikePosition;
    this.LightningStrike(strikePosition, false);
  }

  public void LightningStrike(Vector3 strikePosition, bool useTargetedObject)
  {
    System.Random random;
    AudioSource audio;
    LightningBoltPrefabScript boltPrefabScript;
    if (useTargetedObject)
    {
      random = this.targetedThunderRandom;
      this.staticElectricityParticle.Stop();
      this.staticElectricityParticle.GetComponent<AudioSource>().Stop();
      this.setStaticToObject = (GameObject) null;
      audio = this.targetedStrikeAudio;
      boltPrefabScript = this.targetedThunder;
    }
    else
    {
      random = new System.Random(this.seed.Next(0, 10000));
      audio = !this.lastStrikeAudioUsed ? this.randomStrikeAudio : this.randomStrikeAudioB;
      this.lastStrikeAudioUsed = !this.lastStrikeAudioUsed;
      boltPrefabScript = this.randomThunder;
    }
    bool flag = false;
    Vector3 vector3 = Vector3.zero;
    for (int index = 0; index < 7; ++index)
    {
      if (index == 6)
      {
        vector3 = strikePosition + Vector3.up * 80f;
      }
      else
      {
        float x = (float) random.Next(-32, 32);
        float z = (float) random.Next(-32, 32);
        vector3 = strikePosition + Vector3.up * 80f + new Vector3(x, 0.0f, z);
      }
      if (!Physics.Linecast(vector3, strikePosition + Vector3.up * 0.5f, out this.rayHit, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      {
        flag = true;
        break;
      }
    }
    if (!flag)
    {
      if (!Physics.Raycast(vector3, strikePosition - vector3, out this.rayHit, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        return;
      strikePosition = this.rayHit.point;
    }
    boltPrefabScript.Source.transform.position = vector3;
    boltPrefabScript.Destination.transform.position = strikePosition;
    boltPrefabScript.AutomaticModeSeconds = 0.2f;
    audio.transform.position = strikePosition + Vector3.up * 0.5f;
    Landmine.SpawnExplosion(strikePosition + Vector3.up * 0.25f, killRange: 2.4f, damageRange: 5f);
    this.explosionEffectParticle.transform.position = strikePosition + Vector3.up * 0.25f;
    this.explosionEffectParticle.Play();
    this.PlayThunderEffects(strikePosition, audio);
  }

  private void PlayThunderEffects(Vector3 strikePosition, AudioSource audio)
  {
    PlayerControllerB playerControllerB = GameNetworkManager.Instance.localPlayerController;
    if (playerControllerB.isPlayerDead && (UnityEngine.Object) playerControllerB.spectatedPlayerScript != (UnityEngine.Object) null)
      playerControllerB = playerControllerB.spectatedPlayerScript;
    float num = Vector3.Distance(playerControllerB.gameplayCamera.transform.position, strikePosition);
    bool flag = false;
    if ((double) num < 40.0)
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
    else if ((double) num < 110.0)
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
    else
      flag = true;
    AudioClip[] clipsArray = !flag ? this.strikeSFX : this.distantThunderSFX;
    if (!playerControllerB.isInsideFactory)
      RoundManager.PlayRandomClip(audio, clipsArray);
    WalkieTalkie.TransmitOneShotAudio(audio, clipsArray[UnityEngine.Random.Range(0, clipsArray.Length)]);
    if (!StartOfRound.Instance.shipBounds.bounds.Contains(strikePosition))
      return;
    StartOfRound.Instance.shipAnimatorObject.GetComponent<Animator>().SetTrigger("shipShake");
    RoundManager.PlayRandomClip(StartOfRound.Instance.ship3DAudio, StartOfRound.Instance.shipCreakSFX, false);
    StartOfRound.Instance.PowerSurgeShip();
  }
}
