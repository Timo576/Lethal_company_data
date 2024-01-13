// Decompiled with JetBrains decompiler
// Type: DeadBodyInfo
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
public class DeadBodyInfo : MonoBehaviour
{
  public int playerObjectId;
  public PlayerControllerB playerScript;
  public Rigidbody[] bodyParts;
  [Space(3f)]
  public Rigidbody attachedLimb;
  public Transform attachedTo;
  [Space(2f)]
  public Rigidbody secondaryAttachedLimb;
  public Transform secondaryAttachedTo;
  [Space(5f)]
  public int timesOutOfBounds;
  public Vector3 spawnPosition;
  [Space(3f)]
  private Vector3 forceDirection;
  public float maxVelocity;
  public float speedMultiplier;
  public bool matchPositionExactly = true;
  public bool wasMatchingPosition;
  private Rigidbody previousAttachedLimb;
  [Space(3f)]
  public bool bodyBleedingHeavily = true;
  private Vector3 previousBodyPosition;
  private int bloodAmount;
  private int maxBloodAmount = 30;
  public GameObject[] bodyBloodDecals;
  [Space(3f)]
  private bool bodyMovedThisFrame;
  private float syncBodyPositionTimer;
  private bool serverSyncedPositionWithClients;
  public bool seenByLocalPlayer;
  public AudioSource bodyAudio;
  private float velocityLastFrame;
  public Transform radarDot;
  private float timeSinceLastCollisionSFX;
  public bool parentedToShip;
  public bool detachedHead;
  public Transform detachedHeadObject;
  public Vector3 detachedHeadVelocity;
  public ParticleSystem bloodSplashParticle;
  public ParticleSystem beamUpParticle;
  public ParticleSystem beamOutParticle;
  public AudioSource playAudioOnDeath;
  public CauseOfDeath causeOfDeath;
  private float resetBodyPartsTimer;
  public GrabbableObject grabBodyObject;
  private bool bodySetToKinematic;
  public bool lerpBeforeMatchingPosition;
  private float moveToExactPositionTimer;
  public bool canBeGrabbedBackByPlayers;
  public bool isInShip;
  public bool deactivated;
  public bool overrideSpawnPosition;

  private void FloatBodyToWaterSurface()
  {
    for (int index = 0; index < this.bodyParts.Length; ++index)
    {
      float num = this.playerScript.underwaterCollider.transform.position.y + this.playerScript.underwaterCollider.bounds.extents.y - this.bodyParts[index].transform.position.y;
      this.bodyParts[index].AddForce(-Physics.gravity * num * 5f, ForceMode.Force);
      this.bodyParts[index].drag = 2.5f;
      this.bodyParts[index].useGravity = false;
    }
  }

  private void StopFloatingBody()
  {
    this.playerScript.underwaterCollider = (Collider) null;
    for (int index = 0; index < this.bodyParts.Length; ++index)
    {
      this.bodyParts[index].drag = 0.0f;
      this.bodyParts[index].useGravity = true;
    }
  }

  private void FixedUpdate()
  {
    if (this.deactivated || this.wasMatchingPosition || this.causeOfDeath != CauseOfDeath.Drowning || !((Object) this.playerScript != (Object) null) || !((Object) this.playerScript.underwaterCollider != (Object) null) || this.isInShip)
      return;
    this.FloatBodyToWaterSurface();
  }

  private void OnDestroy()
  {
    if (!((Object) this.grabBodyObject != (Object) null))
      return;
    this.grabBodyObject.grabbable = false;
  }

  private void Start()
  {
    this.spawnPosition = this.transform.position;
    this.previousBodyPosition = Vector3.zero;
    if ((Object) StartOfRound.Instance != (Object) null)
    {
      this.playerScript = StartOfRound.Instance.allPlayerScripts[this.playerObjectId];
      this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = StartOfRound.Instance.unlockablesList.unlockables[this.playerScript.currentSuitID].suitMaterial;
      this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().renderingLayerMask = (uint) (513 | 1 << this.playerObjectId + 12);
      for (int index = 0; index < this.playerScript.bodyParts.Length; ++index)
      {
        if (!this.overrideSpawnPosition)
          this.bodyParts[index].position = this.playerScript.bodyParts[index].position;
        if (this.playerObjectId == 0)
          this.bodyParts[index].gameObject.tag = "PlayerRagdoll";
        else
          this.bodyParts[index].gameObject.tag = string.Format("PlayerRagdoll{0}", (object) this.playerObjectId);
      }
    }
    if (this.detachedHead)
    {
      if ((Object) RoundManager.Instance != (Object) null && (Object) RoundManager.Instance.mapPropsContainer != (Object) null)
        this.detachedHeadObject.SetParent(RoundManager.Instance.mapPropsContainer.transform);
      this.detachedHeadObject.GetComponent<Rigidbody>().AddForce(this.detachedHeadVelocity * 350f, ForceMode.Impulse);
    }
    if ((Object) this.bloodSplashParticle != (Object) null)
      this.bloodSplashParticle.main.customSimulationSpace = RoundManager.Instance.mapPropsContainer.transform;
    if (!(bool) (Object) this.playAudioOnDeath)
      return;
    this.playAudioOnDeath.Play();
    WalkieTalkie.TransmitOneShotAudio(this.playAudioOnDeath, this.playAudioOnDeath.clip);
  }

  private void Update()
  {
    if (this.deactivated)
    {
      this.isInShip = false;
      if (!((Object) this.grabBodyObject != (Object) null) || !this.grabBodyObject.grabbable)
        return;
      this.grabBodyObject.grabbable = false;
      this.grabBodyObject.grabbableToEnemies = false;
      this.grabBodyObject.EnablePhysics(false);
      this.GetComponentInChildren<ScanNodeProperties>().GetComponent<Collider>().enabled = false;
    }
    else
    {
      this.isInShip = this.parentedToShip || (Object) this.grabBodyObject != (Object) null && this.grabBodyObject.isHeld && (Object) this.grabBodyObject.playerHeldBy != (Object) null && this.grabBodyObject.playerHeldBy.isInElevator;
      if ((Object) this.attachedLimb != (Object) null && (Object) this.attachedTo != (Object) null && this.matchPositionExactly)
      {
        this.syncBodyPositionTimer = 5f;
        this.ResetBodyPositionIfTooFarFromAttachment();
        this.resetBodyPartsTimer += Time.deltaTime;
        if ((double) this.resetBodyPartsTimer >= 0.25)
        {
          this.resetBodyPartsTimer = 0.0f;
          this.EnableCollisionOnBodyParts();
        }
      }
      if ((Object) GameNetworkManager.Instance == (Object) null || (Object) GameNetworkManager.Instance.localPlayerController == (Object) null)
        return;
      this.DetectIfSeenByLocalPlayer();
      this.DetectBodyMovedDistanceThreshold();
      if (this.bodyMovedThisFrame)
      {
        this.syncBodyPositionTimer = 5f;
        if (this.bodyBleedingHeavily && this.bloodAmount < this.maxBloodAmount)
        {
          ++this.bloodAmount;
          this.playerScript.DropBlood(Vector3.down);
        }
      }
      if ((Object) this.attachedLimb != (Object) null && (Object) this.attachedTo != (Object) null)
        this.syncBodyPositionTimer = 5f;
      else if (GameNetworkManager.Instance.localPlayerController.IsOwnedByServer && !this.serverSyncedPositionWithClients)
      {
        if ((double) this.syncBodyPositionTimer >= 0.0)
        {
          this.syncBodyPositionTimer -= Time.deltaTime;
        }
        else
        {
          if (Physics.CheckSphere(this.transform.position, 30f, StartOfRound.Instance.playersMask))
          {
            for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
            {
              if (StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled && !Physics.Linecast(StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position, this.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
              {
                this.syncBodyPositionTimer = 0.3f;
                return;
              }
            }
          }
          this.serverSyncedPositionWithClients = true;
          this.playerScript.SyncBodyPositionWithClients();
        }
      }
      if ((double) this.timeSinceLastCollisionSFX <= 0.5)
      {
        this.timeSinceLastCollisionSFX += Time.deltaTime;
      }
      else
      {
        this.timeSinceLastCollisionSFX = 0.0f;
        this.velocityLastFrame = this.bodyParts[5].velocity.sqrMagnitude;
      }
    }
  }

  public void DetectIfSeenByLocalPlayer()
  {
    if (this.seenByLocalPlayer)
      return;
    PlayerControllerB playerController = GameNetworkManager.Instance.localPlayerController;
    Rigidbody rigidbody = (Rigidbody) null;
    float num = Vector3.Distance(playerController.gameplayCamera.transform.position, this.transform.position);
    for (int index = 0; index < this.bodyParts.Length; ++index)
    {
      if (!((Object) this.bodyParts[index] == (Object) rigidbody))
      {
        rigidbody = this.bodyParts[index];
        if (playerController.HasLineOfSightToPosition(this.bodyParts[index].transform.position, (float) (30.0 / ((double) num / 5.0))))
        {
          if ((double) num < 10.0)
            GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.9f);
          else
            GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.55f);
          this.seenByLocalPlayer = true;
          break;
        }
      }
    }
  }

  private void LateUpdate()
  {
    if (this.deactivated)
    {
      this.radarDot.gameObject.SetActive(false);
      if (!this.parentedToShip)
        return;
      this.parentedToShip = false;
      this.transform.SetParent((Transform) null, true);
    }
    else
    {
      this.radarDot.eulerAngles = new Vector3(0.0f, this.radarDot.eulerAngles.y, 0.0f);
      if ((Object) this.attachedLimb == (Object) null || (Object) this.attachedTo == (Object) null || (Object) this.attachedTo.parent == (Object) this.transform)
      {
        if ((Object) this.grabBodyObject != (Object) null)
          this.grabBodyObject.grabbable = true;
        this.moveToExactPositionTimer = 0.0f;
        if (!this.wasMatchingPosition)
          return;
        this.wasMatchingPosition = false;
        if (StartOfRound.Instance.shipBounds.bounds.Contains(this.transform.position))
        {
          this.transform.SetParent(StartOfRound.Instance.elevatorTransform);
          this.parentedToShip = true;
          this.StopFloatingBody();
        }
        this.previousAttachedLimb.ResetCenterOfMass();
        this.previousAttachedLimb.ResetInertiaTensor();
        this.previousAttachedLimb.freezeRotation = false;
        this.previousAttachedLimb.isKinematic = false;
        this.EnableCollisionOnBodyParts();
      }
      else
      {
        if ((Object) this.grabBodyObject != (Object) null)
          this.grabBodyObject.grabbable = this.canBeGrabbedBackByPlayers;
        if (this.parentedToShip)
        {
          this.parentedToShip = false;
          this.transform.SetParent((Transform) null, true);
        }
        if (this.matchPositionExactly)
        {
          if (this.lerpBeforeMatchingPosition && (double) this.moveToExactPositionTimer < 0.30000001192092896)
          {
            this.moveToExactPositionTimer += Time.deltaTime;
            this.speedMultiplier = 25f;
          }
          else
          {
            if (!this.wasMatchingPosition)
            {
              this.wasMatchingPosition = true;
              Vector3 vector3 = this.transform.position - this.attachedLimb.position;
              this.transform.GetComponent<Rigidbody>().position = this.attachedTo.position + vector3;
              this.previousAttachedLimb = this.attachedLimb;
              this.attachedLimb.freezeRotation = true;
              this.attachedLimb.isKinematic = true;
              this.attachedLimb.transform.position = this.attachedTo.position;
              this.attachedLimb.transform.rotation = this.attachedTo.rotation;
              for (int index = 0; index < this.bodyParts.Length; ++index)
              {
                this.bodyParts[index].angularDrag = 1f;
                this.bodyParts[index].maxAngularVelocity = 2f;
                this.bodyParts[index].maxDepenetrationVelocity = 0.3f;
                this.bodyParts[index].velocity = Vector3.zero;
                this.bodyParts[index].angularVelocity = Vector3.zero;
                this.bodyParts[index].WakeUp();
              }
              return;
            }
            this.attachedLimb.position = this.attachedTo.position;
            this.attachedLimb.rotation = this.attachedTo.rotation;
            this.attachedLimb.centerOfMass = Vector3.zero;
            this.attachedLimb.inertiaTensorRotation = Quaternion.identity;
            return;
          }
        }
        this.forceDirection = Vector3.Normalize(this.attachedTo.position - this.attachedLimb.position);
        this.attachedLimb.AddForce(this.forceDirection * this.speedMultiplier * Mathf.Clamp(Vector3.Distance(this.attachedTo.position, this.attachedLimb.position), 0.2f, 2.5f), ForceMode.VelocityChange);
        Vector3 velocity = this.attachedLimb.velocity;
        if ((double) velocity.sqrMagnitude > (double) this.maxVelocity)
        {
          Rigidbody attachedLimb = this.attachedLimb;
          velocity = this.attachedLimb.velocity;
          Vector3 vector3 = velocity.normalized * this.maxVelocity;
          attachedLimb.velocity = vector3;
        }
        if ((Object) this.secondaryAttachedLimb == (Object) null || (Object) this.secondaryAttachedTo == (Object) null)
          return;
        this.forceDirection = Vector3.Normalize(this.secondaryAttachedTo.position - this.secondaryAttachedLimb.position);
        this.secondaryAttachedLimb.AddForce(this.forceDirection * this.speedMultiplier * Mathf.Clamp(Vector3.Distance(this.secondaryAttachedTo.position, this.secondaryAttachedLimb.position), 0.2f, 2.5f), ForceMode.VelocityChange);
        velocity = this.secondaryAttachedLimb.velocity;
        if ((double) velocity.sqrMagnitude <= (double) this.maxVelocity)
          return;
        Rigidbody secondaryAttachedLimb = this.secondaryAttachedLimb;
        velocity = this.secondaryAttachedLimb.velocity;
        Vector3 vector3_1 = velocity.normalized * this.maxVelocity;
        secondaryAttachedLimb.velocity = vector3_1;
      }
    }
  }

  private void DetectBodyMovedDistanceThreshold()
  {
    this.bodyMovedThisFrame = false;
    if (this.isInShip)
    {
      if ((double) Vector3.Distance(this.previousBodyPosition, this.transform.localPosition) <= 1.0)
        return;
      this.previousBodyPosition = this.transform.localPosition;
      this.bodyMovedThisFrame = true;
    }
    else
    {
      if ((double) Vector3.Distance(this.previousBodyPosition, this.transform.position) <= 1.0)
        return;
      this.previousBodyPosition = this.transform.position;
      this.bodyMovedThisFrame = true;
    }
  }

  private void ResetBodyPositionIfTooFarFromAttachment()
  {
    for (int index = 0; index < this.bodyParts.Length; ++index)
    {
      if ((double) Vector3.Distance(this.bodyParts[index].position, this.attachedTo.position) > 4.0)
      {
        this.resetBodyPartsTimer = 0.0f;
        this.bodyParts[index].GetComponent<Collider>().enabled = false;
      }
    }
  }

  private void EnableCollisionOnBodyParts()
  {
    for (int index = 0; index < this.bodyParts.Length; ++index)
      this.bodyParts[index].GetComponent<Collider>().enabled = true;
  }

  public void MakeCorpseBloody()
  {
    for (int index = 0; index < this.bodyBloodDecals.Length; ++index)
      this.bodyBloodDecals[index].SetActive(true);
  }

  public void SetBodyPartsKinematic(bool setKinematic = true)
  {
    if (setKinematic)
    {
      this.bodySetToKinematic = true;
      for (int index = 0; index < this.bodyParts.Length; ++index)
      {
        this.bodyParts[index].velocity = Vector3.zero;
        this.bodyParts[index].isKinematic = true;
      }
    }
    else
    {
      for (int index = 0; index < this.bodyParts.Length; ++index)
      {
        this.bodyParts[index].velocity = Vector3.zero;
        if (!((Object) this.bodyParts[index] == (Object) this.attachedLimb) || !this.matchPositionExactly)
          this.bodyParts[index].isKinematic = false;
      }
    }
  }

  public void DeactivateBody(bool setActive)
  {
    this.gameObject.SetActive(setActive);
    this.SetBodyPartsKinematic();
    this.isInShip = false;
    this.deactivated = true;
  }

  public void ResetRagdollPosition()
  {
    if ((Object) this.attachedLimb != (Object) null && (Object) this.attachedTo != (Object) null)
      this.transform.position = this.attachedTo.position + Vector3.up * 2f;
    else
      this.transform.position = this.spawnPosition;
    for (int index = 0; index < this.bodyParts.Length; ++index)
    {
      this.bodyParts[index].velocity = Vector3.zero;
      this.bodyParts[index].GetComponent<Collider>().enabled = false;
    }
  }

  public void SetRagdollPositionSafely(Vector3 newPosition, bool disableSpecialEffects = false)
  {
    this.transform.position = newPosition + Vector3.up * 2.5f;
    if (disableSpecialEffects)
      this.StopFloatingBody();
    for (int index = 0; index < this.bodyParts.Length; ++index)
      this.bodyParts[index].velocity = Vector3.zero;
    this.timeSinceLastCollisionSFX = -1f;
  }

  public void AddForceToBodyPart(int bodyPartIndex, Vector3 force)
  {
    this.bodyParts[bodyPartIndex].AddForce(force, ForceMode.Impulse);
  }

  public void ChangeMesh(Mesh changeMesh, Material changeMaterial = null)
  {
    this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = changeMesh;
    if (!((Object) changeMaterial != (Object) null))
      return;
    this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMaterial = changeMaterial;
  }
}
