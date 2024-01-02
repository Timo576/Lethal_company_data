// Decompiled with JetBrains decompiler
// Type: GameNetcodeStuff.PlayerControllerB
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;

#nullable disable
namespace GameNetcodeStuff
{
  public class PlayerControllerB : NetworkBehaviour, IHittable, IShockableWithGun, IVisibleThreat
  {
    public bool isTestingPlayer;
    [Header("MODELS / ANIMATIONS")]
    public Transform[] bodyParts;
    public Transform thisPlayerBody;
    public SkinnedMeshRenderer thisPlayerModel;
    public SkinnedMeshRenderer thisPlayerModelLOD1;
    public SkinnedMeshRenderer thisPlayerModelLOD2;
    public SkinnedMeshRenderer thisPlayerModelArms;
    public Transform playerGlobalHead;
    public Transform playerModelArmsMetarig;
    public Transform localArmsRotationTarget;
    public Transform meshContainer;
    public Transform lowerSpine;
    public Camera gameplayCamera;
    public Transform cameraContainerTransform;
    public Transform playerEye;
    public float targetFOV = 66f;
    public Camera visorCamera;
    public CharacterController thisController;
    public Animator playerBodyAnimator;
    public MeshFilter playerBadgeMesh;
    public MeshRenderer playerBetaBadgeMesh;
    public int playerLevelNumber;
    public Transform localVisor;
    public Transform localVisorTargetPoint;
    private bool isSidling;
    private bool wasMovingForward;
    public MultiRotationConstraint cameraLookRig1;
    public MultiRotationConstraint cameraLookRig2;
    public Transform playerHudUIContainer;
    public Transform playerHudBaseRotation;
    public ChainIKConstraint rightArmNormalRig;
    public ChainIKConstraint rightArmProceduralRig;
    public Transform rightArmProceduralTarget;
    private Vector3 rightArmProceduralTargetBasePosition;
    public Transform leftHandItemTarget;
    public Light nightVision;
    public int currentSuitID;
    public bool performingEmote;
    public float emoteLayerWeight;
    public float timeSinceStartingEmote;
    public ParticleSystem beamUpParticle;
    public ParticleSystem beamOutParticle;
    public ParticleSystem beamOutBuildupParticle;
    public bool localArmsMatchCamera;
    public Transform localArmsTransform;
    public Collider playerCollider;
    public Collider[] bodyPartSpraypaintColliders;
    [Header("AUDIOS")]
    public AudioSource movementAudio;
    public AudioSource itemAudio;
    public AudioSource statusEffectAudio;
    public AudioSource waterBubblesAudio;
    public int currentFootstepSurfaceIndex;
    private int previousFootstepClip;
    [HideInInspector]
    public Dictionary<AudioSource, AudioReverbTrigger> audioCoroutines = new Dictionary<AudioSource, AudioReverbTrigger>();
    [HideInInspector]
    public Dictionary<AudioSource, IEnumerator> audioCoroutines2 = new Dictionary<AudioSource, IEnumerator>();
    [HideInInspector]
    public AudioReverbTrigger currentAudioTrigger;
    public AudioReverbTrigger currentAudioTriggerB;
    public float targetDryLevel;
    public float targetRoom;
    public float targetHighFreq;
    public float targetLowFreq;
    public float targetDecayTime;
    public ReverbPreset reverbPreset;
    public AudioListener activeAudioListener;
    public AudioReverbFilter activeAudioReverbFilter;
    public ParticleSystem bloodParticle;
    public bool playingQuickSpecialAnimation;
    private Coroutine quickSpecialAnimationCoroutine;
    [Header("INPUT / MOVEMENT")]
    public float movementSpeed = 0.5f;
    public PlayerActions playerActions;
    private bool isWalking;
    private bool movingForward;
    public Vector2 moveInputVector;
    public Vector3 velocityLastFrame;
    private float sprintMultiplier = 1f;
    public bool isSprinting;
    public float sprintTime = 5f;
    public Image sprintMeterUI;
    [HideInInspector]
    public float sprintMeter;
    [HideInInspector]
    public bool isExhausted;
    private float exhaustionEffectLerp;
    public float jumpForce = 5f;
    private bool isJumping;
    private bool isFallingFromJump;
    private Coroutine jumpCoroutine;
    public float fallValue;
    public bool isGroundedOnServer;
    public bool isPlayerSliding;
    private float playerSlidingTimer;
    public Vector3 playerGroundNormal;
    public float maxSlideFriction;
    private float slideFriction;
    public float fallValueUncapped;
    public bool takingFallDamage;
    public float minVelocityToTakeDamage;
    public bool isCrouching;
    private float timeSinceCrouching;
    private bool isFallingNoJump;
    public int isMovementHindered;
    private int movementHinderedPrev;
    public float hinderedMultiplier = 1f;
    public int sourcesCausingSinking;
    public bool isSinking;
    public bool isUnderwater;
    private float syncUnderwaterInterval;
    private bool isFaceUnderwaterOnServer;
    public Collider underwaterCollider;
    private bool wasUnderwaterLastFrame;
    public float sinkingValue;
    public float sinkingSpeedMultiplier;
    public int statusEffectAudioIndex;
    private float cameraUp;
    public float lookSensitivity = 0.4f;
    public bool disableLookInput;
    private float oldLookRot;
    private float targetLookRot;
    private float previousYRot;
    private float targetYRot;
    public Vector3 syncFullRotation;
    private Vector3 walkForce;
    public Vector3 externalForces;
    private Vector3 movementForcesLastFrame;
    public Rigidbody playerRigidbody;
    public float averageVelocity;
    public int velocityMovingAverageLength = 20;
    public int velocityAverageCount;
    public float getAverageVelocityInterval;
    public bool jetpackControls;
    public bool disablingJetpackControls;
    public Transform jetpackTurnCompass;
    private bool startedJetpackControls;
    private float previousFrameDeltaTime;
    private Collider[] nearByPlayers = new Collider[4];
    private bool teleportingThisFrame;
    public bool teleportedLastFrame;
    [Header("LOCATION")]
    public bool isInElevator;
    public bool isInHangarShipRoom;
    public bool isInsideFactory;
    [Space(5f)]
    public bool wasInElevatorLastFrame;
    public Vector3 previousElevatorPosition;
    [Header("CONTROL / NETWORKING")]
    public ulong playerClientId;
    public string playerUsername = "Player";
    public ulong playerSteamId;
    public ulong actualClientId;
    public bool isPlayerControlled;
    public bool justConnected = true;
    public bool disconnectedMidGame;
    [Space(5f)]
    private bool isCameraDisabled;
    public StartOfRound playersManager;
    public bool isHostPlayerObject;
    public Vector3 oldPlayerPosition;
    private int previousAnimationState;
    public Vector3 serverPlayerPosition;
    public bool snapToServerPosition;
    private float oldCameraUp;
    public float ladderCameraHorizontal;
    private float updatePlayerAnimationsInterval;
    private float updatePlayerLookInterval;
    private List<int> currentAnimationStateHash = new List<int>();
    private List<int> previousAnimationStateHash = new List<int>();
    private float currentAnimationSpeed;
    private float previousAnimationSpeed;
    private int previousAnimationServer;
    private int oldConnectedPlayersAmount;
    private int playerMask = 8;
    public RawImage playerScreen;
    public VoicePlayerState voicePlayerState;
    public AudioSource currentVoiceChatAudioSource;
    public PlayerVoiceIngameSettings currentVoiceChatIngameSettings;
    private float voiceChatUpdateInterval;
    public bool isTypingChat;
    [Header("DEATH")]
    public int health;
    public float healthRegenerateTimer;
    public bool criticallyInjured;
    public bool hasBeenCriticallyInjured;
    private float limpMultiplier = 0.2f;
    public CauseOfDeath causeOfDeath;
    public bool isPlayerDead;
    [HideInInspector]
    public bool setPositionOfDeadPlayer;
    [HideInInspector]
    public Vector3 placeOfDeath;
    public Transform spectateCameraPivot;
    public PlayerControllerB spectatedPlayerScript;
    public DeadBodyInfo deadBody;
    public GameObject[] bodyBloodDecals;
    private int currentBloodIndex;
    public List<GameObject> playerBloodPooledObjects = new List<GameObject>();
    public bool bleedingHeavily;
    private float bloodDropTimer;
    private bool alternatePlaceFootprints;
    public EnemyAI inAnimationWithEnemy;
    [Header("UI/MENU")]
    public bool inTerminalMenu;
    public QuickMenuManager quickMenuManager;
    public TextMeshProUGUI usernameBillboardText;
    public Transform usernameBillboard;
    public CanvasGroup usernameAlpha;
    public Canvas usernameCanvas;
    private Vector3 tempVelocity;
    [Header("ITEM INTERACTION")]
    public float grabDistance = 5f;
    public float throwPower = 17f;
    public bool isHoldingObject;
    private bool hasThrownObject;
    public bool twoHanded;
    public bool twoHandedAnimation;
    public float carryWeight = 1f;
    public bool isGrabbingObjectAnimation;
    public bool activatingItem;
    public float grabObjectAnimationTime;
    public Transform localItemHolder;
    public Transform serverItemHolder;
    public Transform propThrowPosition;
    public GrabbableObject currentlyHeldObject;
    private GrabbableObject currentlyGrabbingObject;
    public GrabbableObject currentlyHeldObjectServer;
    public GameObject heldObjectServerCopy;
    private Coroutine grabObjectCoroutine;
    private Ray interactRay;
    private int grabbableObjectsMask = 64;
    private int interactableObjectsMask = 832;
    private int walkableSurfacesNoPlayersMask = 268437761;
    private RaycastHit hit;
    private float upperBodyAnimationsWeight;
    public float doingUpperBodyEmote;
    private float handsOnWallWeight;
    public Light helmetLight;
    public Light[] allHelmetLights;
    private bool grabbedObjectValidated;
    private bool grabInvalidated;
    private bool throwingObject;
    [Space(5f)]
    public GrabbableObject[] ItemSlots;
    public int currentItemSlot;
    private MeshRenderer[] itemRenderers;
    private float timeSinceSwitchingSlots;
    [HideInInspector]
    public bool grabSetParentServer;
    [Header("TRIGGERS AND SPECIAL")]
    public Image cursorIcon;
    public TextMeshProUGUI cursorTip;
    public Sprite grabItemIcon;
    private bool hoveringOverItem;
    public InteractTrigger hoveringOverTrigger;
    public InteractTrigger previousHoveringOverTrigger;
    public InteractTrigger currentTriggerInAnimationWith;
    public bool isHoldingInteract;
    public bool inSpecialInteractAnimation;
    public bool disableSyncInAnimation;
    public float specialAnimationWeight;
    public bool isClimbingLadder;
    public bool enteringSpecialAnimation;
    public float climbSpeed = 4f;
    public Vector3 clampCameraRotation;
    public Transform lineOfSightCube;
    public bool voiceMuffledByEnemy;
    [Header("SPECIAL ITEMS")]
    public int shipTeleporterId;
    public EnemyAI redirectToEnemy;
    public MeshRenderer mapRadarDirectionIndicator;
    public Animator mapRadarDotAnimator;
    public bool equippedUsableItemQE;
    public bool IsInspectingItem;
    public bool isFirstFrameLateUpdate = true;
    public GrabbableObject pocketedFlashlight;
    public bool isFreeCamera;
    public bool isSpeedCheating;
    public bool inShockingMinigame;
    public Transform shockingTarget;
    public Transform turnCompass;
    public Transform smoothLookTurnCompass;
    public float smoothLookMultiplier = 25f;
    private bool smoothLookEnabledLastFrame;
    public Camera turnCompassCamera;
    [HideInInspector]
    public Vector3 targetScreenPos;
    [HideInInspector]
    public float shockMinigamePullPosition;
    [Space(5f)]
    public bool speakingToWalkieTalkie;
    public bool holdingWalkieTalkie;
    public float isInGameOverAnimation;
    [HideInInspector]
    public bool hasBegunSpectating;
    private Coroutine timeSpecialAnimationCoroutine;
    private float spectatedPlayerDeadTimer;
    public float insanityLevel;
    public float maxInsanityLevel = 50f;
    public float insanitySpeedMultiplier = 1f;
    public bool isPlayerAlone;
    public float timeSincePlayerMoving;
    public Scrollbar terminalScrollVertical;
    private bool updatePositionForNewlyJoinedClient;
    private float timeSinceTakingGravityDamage;
    [Space(5f)]
    public float drunkness;
    public float drunknessInertia = 1f;
    public float drunknessSpeed;
    public bool increasingDrunknessThisFrame;
    public float timeSinceMakingLoudNoise;

    ThreatType IVisibleThreat.type => ThreatType.Player;

    float IVisibleThreat.GetVisibility()
    {
      if (this.isPlayerDead)
        return 0.0f;
      float visibility = 1f;
      if (this.isCrouching)
        visibility -= 0.25f;
      if ((double) this.timeSincePlayerMoving > 0.5)
        visibility -= 0.16f;
      return visibility;
    }

    int IVisibleThreat.GetThreatLevel(Vector3 seenByPosition)
    {
      int threatLevel = 0;
      if (this.isHoldingObject && (UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null && this.currentlyHeldObjectServer.itemProperties.isDefensiveWeapon)
        threatLevel += 2;
      if ((double) this.timeSinceMakingLoudNoise < 0.800000011920929)
        ++threatLevel;
      float positionAngle = this.LineOfSightToPositionAngle(seenByPosition);
      Debug.Log((object) string.Format("angle: {0}", (object) positionAngle));
      if ((double) positionAngle == -361.0 || (double) positionAngle > 100.0)
      {
        --threatLevel;
        Debug.Log((object) "Subtracting threat level cause player is looking away");
      }
      else if ((double) positionAngle < 45.0)
      {
        ++threatLevel;
        Debug.Log((object) "Adding threat level cause player is looking at us");
      }
      if ((double) TimeOfDay.Instance.normalizedTimeOfDay < 0.20000000298023224)
        ++threatLevel;
      else if ((double) TimeOfDay.Instance.normalizedTimeOfDay > 0.800000011920929)
        --threatLevel;
      if (this.isInHangarShipRoom)
        ++threatLevel;
      else if ((double) Vector3.Distance(this.transform.position, StartOfRound.Instance.elevatorTransform.position) > 30.0)
        --threatLevel;
      int num = Physics.OverlapSphereNonAlloc(this.transform.position, 12f, this.nearByPlayers, StartOfRound.Instance.playersMask);
      for (int index = 0; index < num; ++index)
      {
        if ((double) Vector3.Distance(this.transform.position, this.nearByPlayers[index].transform.position) <= 6.0 || !Physics.Linecast(this.gameplayCamera.transform.position, this.nearByPlayers[index].transform.position + Vector3.up * 0.6f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
          ++threatLevel;
      }
      if (this.health >= 30)
        ++threatLevel;
      else if (this.criticallyInjured)
        threatLevel -= 2;
      if (StartOfRound.Instance.connectedPlayersAmount <= 0)
        ++threatLevel;
      return threatLevel;
    }

    Vector3 IVisibleThreat.GetThreatVelocity()
    {
      if (this.IsOwner)
        return Vector3.Normalize(this.thisController.velocity * 100f);
      return (double) this.timeSincePlayerMoving < 0.25 ? Vector3.Normalize((this.serverPlayerPosition - this.oldPlayerPosition) * 100f) : Vector3.zero;
    }

    int IVisibleThreat.GetInterestLevel()
    {
      int interestLevel = 0;
      if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null && this.currentlyHeldObjectServer.itemProperties.isScrap)
        ++interestLevel;
      if ((double) this.carryWeight > 1.2200000286102295)
        ++interestLevel;
      if ((double) this.carryWeight > 1.5)
        ++interestLevel;
      return interestLevel;
    }

    Transform IVisibleThreat.GetThreatLookTransform() => this.gameplayCamera.transform;

    Transform IVisibleThreat.GetThreatTransform() => this.transform;

    private void Awake()
    {
      this.isHostPlayerObject = (UnityEngine.Object) this.gameObject == (UnityEngine.Object) this.playersManager.allPlayerObjects[0];
      this.playerActions = new PlayerActions();
      this.previousAnimationState = 0;
      this.serverPlayerPosition = this.transform.position;
      this.gameplayCamera.enabled = false;
      this.visorCamera.enabled = false;
      this.thisPlayerModel.enabled = true;
      this.thisPlayerModel.shadowCastingMode = ShadowCastingMode.On;
      this.thisPlayerModelArms.enabled = false;
      this.gameplayCamera.enabled = false;
      this.previousAnimationStateHash = new List<int>((IEnumerable<int>) new int[this.playerBodyAnimator.layerCount]);
      this.currentAnimationStateHash = new List<int>((IEnumerable<int>) new int[this.playerBodyAnimator.layerCount]);
      if ((UnityEngine.Object) this.playerBodyAnimator.runtimeAnimatorController != (UnityEngine.Object) this.playersManager.otherClientsAnimatorController)
        this.playerBodyAnimator.runtimeAnimatorController = this.playersManager.otherClientsAnimatorController;
      this.isCameraDisabled = true;
      this.sprintMeter = 1f;
      this.ItemSlots = new GrabbableObject[4];
      this.rightArmProceduralTargetBasePosition = this.rightArmProceduralTarget.localPosition;
      this.playerUsername = string.Format("Player #{0}", (object) this.playerClientId);
      this.previousElevatorPosition = this.playersManager.elevatorTransform.position;
      if ((bool) (UnityEngine.Object) this.gameObject.GetComponent<Rigidbody>())
        this.gameObject.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
      this.gameObject.GetComponent<CharacterController>().enabled = false;
      this.syncFullRotation = this.transform.eulerAngles;
    }

    private void Start()
    {
      this.InstantiateBloodPooledObjects();
      this.StartCoroutine(this.PlayIntroTip());
      this.jetpackTurnCompass.SetParent((Transform) null);
      this.terminalScrollVertical.value += 500f;
    }

    private IEnumerator PlayIntroTip()
    {
      yield return (object) new WaitForSeconds(4f);
      QuickMenuManager quickMenu = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
      yield return (object) new WaitUntil((Func<bool>) (() => !quickMenu.isMenuOpen));
      HUDManager.Instance.DisplayTip("Welcome!", "Right-click to scan objects in the ship for info.", useSave: true, prefsKey: "LC_IntroTip1");
    }

    private void InstantiateBloodPooledObjects()
    {
      int num = 50;
      for (int index = 0; index < num; ++index)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.playersManager.playerBloodPrefab, this.playersManager.bloodObjectsContainer);
        gameObject.SetActive(false);
        this.playerBloodPooledObjects.Add(gameObject);
      }
    }

    public void ResetPlayerBloodObjects(bool resetBodyBlood = true)
    {
      if (this.playerBloodPooledObjects != null)
      {
        for (int index = 0; index < this.playerBloodPooledObjects.Count; ++index)
          this.playerBloodPooledObjects[index].SetActive(false);
      }
      if (!resetBodyBlood)
        return;
      for (int index = 0; index < this.bodyBloodDecals.Length; ++index)
        this.bodyBloodDecals[index].SetActive(false);
    }

    private void OnEnable()
    {
      InputActionAsset actions = IngamePlayerSettings.Instance.playerInput.actions;
      try
      {
        this.playerActions.Movement.Look.performed += new Action<InputAction.CallbackContext>(this.Look_performed);
        actions.FindAction("Jump", false).performed += new Action<InputAction.CallbackContext>(this.Jump_performed);
        actions.FindAction("Crouch", false).performed += new Action<InputAction.CallbackContext>(this.Crouch_performed);
        actions.FindAction("Interact", false).performed += new Action<InputAction.CallbackContext>(this.Interact_performed);
        actions.FindAction("ItemSecondaryUse", false).performed += new Action<InputAction.CallbackContext>(this.ItemSecondaryUse_performed);
        actions.FindAction("ItemTertiaryUse", false).performed += new Action<InputAction.CallbackContext>(this.ItemTertiaryUse_performed);
        actions.FindAction("ActivateItem", false).performed += new Action<InputAction.CallbackContext>(this.ActivateItem_performed);
        actions.FindAction("ActivateItem", false).canceled += new Action<InputAction.CallbackContext>(this.ActivateItem_canceled);
        actions.FindAction("Discard", false).performed += new Action<InputAction.CallbackContext>(this.Discard_performed);
        actions.FindAction("SwitchItem", false).performed += new Action<InputAction.CallbackContext>(this.ScrollMouse_performed);
        actions.FindAction("OpenMenu", false).performed += new Action<InputAction.CallbackContext>(this.OpenMenu_performed);
        actions.FindAction("InspectItem", false).performed += new Action<InputAction.CallbackContext>(this.InspectItem_performed);
        actions.FindAction("SpeedCheat", false).performed += new Action<InputAction.CallbackContext>(this.SpeedCheat_performed);
        actions.FindAction("Emote1", false).performed += new Action<InputAction.CallbackContext>(this.Emote1_performed);
        actions.FindAction("Emote2", false).performed += new Action<InputAction.CallbackContext>(this.Emote2_performed);
        this.playerActions.Movement.Enable();
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("Error while subscribing to input in PlayerController!: {0}", (object) ex));
      }
      this.playerActions.Movement.Enable();
    }

    private void OnDisable()
    {
      InputActionAsset actions = IngamePlayerSettings.Instance.playerInput.actions;
      try
      {
        this.playerActions.Movement.Look.performed -= new Action<InputAction.CallbackContext>(this.Look_performed);
        actions.FindAction("Jump", false).performed -= new Action<InputAction.CallbackContext>(this.Jump_performed);
        actions.FindAction("Crouch", false).performed -= new Action<InputAction.CallbackContext>(this.Crouch_performed);
        actions.FindAction("Interact", false).performed -= new Action<InputAction.CallbackContext>(this.Interact_performed);
        actions.FindAction("ItemSecondaryUse", false).performed -= new Action<InputAction.CallbackContext>(this.ItemSecondaryUse_performed);
        actions.FindAction("ItemTertiaryUse", false).performed -= new Action<InputAction.CallbackContext>(this.ItemTertiaryUse_performed);
        actions.FindAction("ActivateItem", false).performed -= new Action<InputAction.CallbackContext>(this.ActivateItem_performed);
        actions.FindAction("ActivateItem", false).canceled -= new Action<InputAction.CallbackContext>(this.ActivateItem_canceled);
        actions.FindAction("Discard", false).performed -= new Action<InputAction.CallbackContext>(this.Discard_performed);
        actions.FindAction("SwitchItem", false).performed -= new Action<InputAction.CallbackContext>(this.ScrollMouse_performed);
        actions.FindAction("OpenMenu", false).performed -= new Action<InputAction.CallbackContext>(this.OpenMenu_performed);
        actions.FindAction("InspectItem", false).performed -= new Action<InputAction.CallbackContext>(this.InspectItem_performed);
        actions.FindAction("SpeedCheat", false).performed -= new Action<InputAction.CallbackContext>(this.SpeedCheat_performed);
        actions.FindAction("Emote1", false).performed -= new Action<InputAction.CallbackContext>(this.Emote1_performed);
        actions.FindAction("Emote2", false).performed -= new Action<InputAction.CallbackContext>(this.Emote2_performed);
        this.playerActions.Movement.Enable();
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("Error while unsubscribing from input in PlayerController!: {0}", (object) ex));
      }
      this.playerActions.Movement.Disable();
    }

    public override void OnDestroy() => base.OnDestroy();

    private void SpeedCheat_performed(InputAction.CallbackContext context)
    {
      if ((!this.IsOwner || !this.isPlayerControlled && !this.isPlayerDead || this.inTerminalMenu || this.isTypingChat || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || !context.performed || (UnityEngine.Object) HUDManager.Instance == (UnityEngine.Object) null || (double) IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint", false).ReadValue<float>() <= 0.5)
        return;
      HUDManager.Instance.ToggleErrorConsole();
    }

    public bool AllowPlayerDeath()
    {
      return StartOfRound.Instance.allowLocalPlayerDeath && (!((UnityEngine.Object) this.playersManager.testRoom == (UnityEngine.Object) null) || (double) StartOfRound.Instance.timeSinceRoundStarted >= 2.0 && this.playersManager.shipDoorsEnabled);
    }

    public void DamagePlayer(
      int damageNumber,
      bool hasDamageSFX = true,
      bool callRPC = true,
      CauseOfDeath causeOfDeath = CauseOfDeath.Unknown,
      int deathAnimation = 0,
      bool fallDamage = false,
      Vector3 force = default (Vector3))
    {
      if (!this.IsOwner || this.isPlayerDead || !this.AllowPlayerDeath())
        return;
      this.health = this.health - damageNumber > 0 || this.criticallyInjured || damageNumber >= 50 ? Mathf.Clamp(this.health - damageNumber, 0, 100) : 5;
      Debug.Log((object) string.Format("player's health after taking {0} damage: {1}", (object) damageNumber, (object) this.health));
      HUDManager.Instance.UpdateHealthUI(this.health);
      if (this.health <= 0)
      {
        this.KillPlayer(force, causeOfDeath: causeOfDeath, deathAnimation: deathAnimation);
      }
      else
      {
        if (this.health < 10 && !this.criticallyInjured)
        {
          HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
          this.MakeCriticallyInjured(true);
        }
        else
        {
          if (damageNumber > 30)
            this.sprintMeter = Mathf.Clamp(this.sprintMeter + (float) damageNumber / 125f, 0.0f, 1f);
          if (callRPC)
          {
            if (this.IsServer)
              this.DamagePlayerClientRpc(damageNumber, this.health);
            else
              this.DamagePlayerServerRpc(damageNumber, this.health);
          }
        }
        if (fallDamage)
        {
          HUDManager.Instance.UIAudio.PlayOneShot(StartOfRound.Instance.fallDamageSFX, 1f);
          WalkieTalkie.TransmitOneShotAudio(this.movementAudio, StartOfRound.Instance.fallDamageSFX);
          this.BreakLegsSFXClientRpc();
        }
        else if (hasDamageSFX)
          HUDManager.Instance.UIAudio.PlayOneShot(StartOfRound.Instance.damageSFX, 1f);
      }
      this.takingFallDamage = false;
      if (!this.inSpecialInteractAnimation)
        this.playerBodyAnimator.SetTrigger("Damage");
      this.specialAnimationWeight = 1f;
      this.PlayQuickSpecialAnimation(0.7f);
    }

    [ServerRpc]
    public void BreakLegsSFXServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(800455552U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 800455552U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.BreakLegsSFXClientRpc();
    }

    [ClientRpc]
    public void BreakLegsSFXClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(3591743514U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 3591743514U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      this.movementAudio.PlayOneShot(StartOfRound.Instance.fallDamageSFX, 1f);
      WalkieTalkie.TransmitOneShotAudio(this.movementAudio, StartOfRound.Instance.fallDamageSFX);
    }

    public void MakeCriticallyInjured(bool enable)
    {
      if (enable)
      {
        this.criticallyInjured = true;
        this.playerBodyAnimator.SetBool("Limp", true);
        this.bleedingHeavily = true;
        if (this.IsServer)
          this.MakeCriticallyInjuredClientRpc();
        else
          this.MakeCriticallyInjuredServerRpc();
      }
      else
      {
        this.criticallyInjured = false;
        this.playerBodyAnimator.SetBool("Limp", false);
        this.bleedingHeavily = false;
        if (this.IsServer)
          this.HealClientRpc();
        else
          this.HealServerRpc();
      }
    }

    [ServerRpc]
    public void DamagePlayerServerRpc(int damageNumber, int newHealthAmount)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(1084949295U, serverRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, damageNumber);
        BytePacker.WriteValueBitPacked(bufferWriter, newHealthAmount);
        this.__endSendServerRpc(ref bufferWriter, 1084949295U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.DamagePlayerClientRpc(damageNumber, newHealthAmount);
    }

    [ClientRpc]
    public void DamagePlayerClientRpc(int damageNumber, int newHealthAmount)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(1822320450U, clientRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, damageNumber);
        BytePacker.WriteValueBitPacked(bufferWriter, newHealthAmount);
        this.__endSendClientRpc(ref bufferWriter, 1822320450U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      this.DamageOnOtherClients(damageNumber, newHealthAmount);
    }

    private void DamageOnOtherClients(int damageNumber, int newHealthAmount)
    {
      this.playersManager.gameStats.allPlayerStats[this.playerClientId].damageTaken += damageNumber;
      this.health = newHealthAmount;
      if (this.IsOwner)
        return;
      this.PlayQuickSpecialAnimation(0.7f);
    }

    public void PlayQuickSpecialAnimation(float animTime)
    {
      if (this.quickSpecialAnimationCoroutine != null)
        this.StopCoroutine(this.quickSpecialAnimationCoroutine);
      this.quickSpecialAnimationCoroutine = this.StartCoroutine(this.playQuickSpecialAnimation(animTime));
    }

    private IEnumerator playQuickSpecialAnimation(float animTime)
    {
      this.playingQuickSpecialAnimation = true;
      yield return (object) new WaitForSeconds(animTime);
      this.playingQuickSpecialAnimation = false;
    }

    [ServerRpc]
    public void StartSinkingServerRpc(float sinkingSpeed, int audioClipIndex)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(3986869491U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<float>(in sinkingSpeed, new FastBufferWriter.ForPrimitives());
        BytePacker.WriteValueBitPacked(bufferWriter, audioClipIndex);
        this.__endSendServerRpc(ref bufferWriter, 3986869491U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.StartSinkingClientRpc(sinkingSpeed, audioClipIndex);
    }

    [ClientRpc]
    public void StartSinkingClientRpc(float sinkingSpeed, int audioClipIndex)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(1090586009U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<float>(in sinkingSpeed, new FastBufferWriter.ForPrimitives());
        BytePacker.WriteValueBitPacked(bufferWriter, audioClipIndex);
        this.__endSendClientRpc(ref bufferWriter, 1090586009U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      this.sinkingSpeedMultiplier = sinkingSpeed;
      this.isSinking = true;
      this.statusEffectAudio.clip = StartOfRound.Instance.statusEffectClips[audioClipIndex];
      this.statusEffectAudio.Play();
    }

    [ServerRpc]
    public void StopSinkingServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(341877959U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 341877959U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.StopSinkingClientRpc();
    }

    [ClientRpc]
    public void StopSinkingClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(2005250174U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 2005250174U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      this.statusEffectAudio.Stop();
      this.isSinking = false;
      this.voiceMuffledByEnemy = false;
      if (this.IsOwner)
        return;
      if ((UnityEngine.Object) this.currentVoiceChatIngameSettings == (UnityEngine.Object) null)
        StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
      if (!((UnityEngine.Object) this.currentVoiceChatIngameSettings != (UnityEngine.Object) null))
        return;
      this.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
    }

    [ServerRpc]
    public void MakeCriticallyInjuredServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(4195705835U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 4195705835U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.MakeCriticallyInjuredClientRpc();
    }

    [ClientRpc]
    public void MakeCriticallyInjuredClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(3390857164U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 3390857164U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      this.bleedingHeavily = true;
      this.criticallyInjured = true;
      this.hasBeenCriticallyInjured = true;
    }

    [ServerRpc]
    public void HealServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(2585603452U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 2585603452U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.HealClientRpc();
    }

    [ClientRpc]
    public void HealClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(2196003333U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 2196003333U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      this.bleedingHeavily = false;
      this.criticallyInjured = false;
    }

    public void DropBlood(Vector3 direction = default (Vector3), bool leaveBlood = true, bool leaveFootprint = false)
    {
      bool flag = false;
      if (leaveBlood)
      {
        if ((double) this.bloodDropTimer >= 0.0 && !this.isPlayerDead)
          return;
        this.bloodDropTimer = 0.4f;
        if (direction == Vector3.zero)
          direction = Vector3.down;
        Transform transform = this.playerBloodPooledObjects[this.currentBloodIndex].transform;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        if (this.isInElevator)
          transform.SetParent(this.playersManager.elevatorTransform);
        else
          transform.SetParent(this.playersManager.bloodObjectsContainer);
        if (this.isPlayerDead)
        {
          if ((UnityEngine.Object) this.deadBody == (UnityEngine.Object) null || !this.deadBody.gameObject.activeSelf)
            return;
          this.interactRay = new Ray(this.deadBody.bodyParts[3].transform.position + Vector3.up * 0.5f, direction);
        }
        else
          this.interactRay = new Ray(this.transform.position + this.transform.up * 2f, direction);
        if (Physics.Raycast(this.interactRay, out this.hit, 6f, this.playersManager.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        {
          flag = true;
          transform.position = this.hit.point - direction.normalized * 0.45f;
          this.RandomizeBloodRotationAndScale(transform);
          transform.gameObject.SetActive(true);
        }
        this.currentBloodIndex = (this.currentBloodIndex + 1) % this.playerBloodPooledObjects.Count;
      }
      if (!leaveFootprint || this.isPlayerDead || this.playersManager.snowFootprintsPooledObjects == null || this.playersManager.snowFootprintsPooledObjects.Count <= 0)
        return;
      this.alternatePlaceFootprints = !this.alternatePlaceFootprints;
      if (this.alternatePlaceFootprints)
        return;
      Transform transform1 = this.playersManager.snowFootprintsPooledObjects[this.playersManager.currentFootprintIndex].transform;
      transform1.rotation = Quaternion.LookRotation(direction, Vector3.up);
      if (!flag)
      {
        this.interactRay = new Ray(this.transform.position + this.transform.up * 0.3f, direction);
        if (Physics.Raycast(this.interactRay, out this.hit, 6f, this.playersManager.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
          transform1.position = this.hit.point - direction.normalized * 0.45f;
      }
      else
        transform1.position = this.hit.point - direction.normalized * 0.45f;
      transform1.transform.eulerAngles = new Vector3(transform1.transform.eulerAngles.x, this.transform.eulerAngles.y, transform1.transform.eulerAngles.z);
      this.playersManager.snowFootprintsPooledObjects[this.playersManager.currentFootprintIndex].enabled = true;
      this.playersManager.currentFootprintIndex = (this.playersManager.currentFootprintIndex + 1) % this.playersManager.snowFootprintsPooledObjects.Count;
    }

    private void RandomizeBloodRotationAndScale(Transform blood)
    {
      Vector3 localEulerAngles = blood.localEulerAngles with
      {
        z = UnityEngine.Random.Range(-180f, 180f)
      };
      blood.localEulerAngles = localEulerAngles;
      blood.localScale = new Vector3(UnityEngine.Random.Range(0.15f, 0.7f), UnityEngine.Random.Range(0.15f, 0.7f), 0.55f);
    }

    private void Emote1_performed(InputAction.CallbackContext context)
    {
      this.PerformEmote(context, 1);
    }

    private void Emote2_performed(InputAction.CallbackContext context)
    {
      this.PerformEmote(context, 2);
    }

    public void PerformEmote(InputAction.CallbackContext context, int emoteID)
    {
      if (!context.performed || (!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || !this.CheckConditionsForEmote() || (double) this.timeSinceStartingEmote < 0.5)
        return;
      this.timeSinceStartingEmote = 0.0f;
      this.performingEmote = true;
      this.playerBodyAnimator.SetInteger("emoteNumber", emoteID);
      this.StartPerformingEmoteServerRpc();
    }

    [ServerRpc]
    public void StartPerformingEmoteServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(3803364611U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 3803364611U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.StartPerformingEmoteClientRpc();
    }

    [ClientRpc]
    public void StartPerformingEmoteClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(1955832627U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 1955832627U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      this.performingEmote = true;
    }

    [ServerRpc]
    public void StopPerformingEmoteServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(878005044U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 878005044U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.StopPerformingEmoteClientRpc();
    }

    [ClientRpc]
    public void StopPerformingEmoteClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(655708081U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 655708081U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      this.performingEmote = false;
    }

    public bool CheckConditionsForSinkingInQuicksand()
    {
      return this.thisController.isGrounded && !this.inSpecialInteractAnimation && !(bool) (UnityEngine.Object) this.inAnimationWithEnemy && !this.isClimbingLadder && (this.currentFootstepSurfaceIndex == 1 || this.currentFootstepSurfaceIndex == 4 || this.currentFootstepSurfaceIndex == 8);
    }

    private bool CheckConditionsForEmote()
    {
      return !this.inSpecialInteractAnimation && !this.isPlayerDead && !this.isJumping && !this.isWalking && !this.isCrouching && !this.isClimbingLadder && !this.isGrabbingObjectAnimation && !this.inTerminalMenu && !this.isTypingChat;
    }

    private void ActivateItem_performed(InputAction.CallbackContext context)
    {
      if (!context.performed)
        return;
      if (this.IsOwner && this.isPlayerDead && (!this.IsServer || this.isHostPlayerObject))
      {
        if (StartOfRound.Instance.overrideSpectateCamera || !((UnityEngine.Object) this.spectatedPlayerScript != (UnityEngine.Object) null) || this.spectatedPlayerScript.isPlayerDead)
          return;
        this.SpectateNextPlayer();
      }
      else
      {
        if (!this.CanUseItem() || (double) this.timeSinceSwitchingSlots < 0.075000002980232239)
          return;
        ShipBuildModeManager.Instance.CancelBuildMode();
        this.currentlyHeldObjectServer.gameObject.GetComponent<GrabbableObject>().UseItemOnClient();
        this.timeSinceSwitchingSlots = 0.0f;
      }
    }

    private void ActivateItem_canceled(InputAction.CallbackContext context)
    {
      if (!this.CanUseItem() || !this.currentlyHeldObjectServer.itemProperties.holdButtonUse)
        return;
      ShipBuildModeManager.Instance.CancelBuildMode();
      this.currentlyHeldObjectServer.gameObject.GetComponent<GrabbableObject>().UseItemOnClient(false);
    }

    private bool CanUseItem()
    {
      return (this.IsOwner && this.isPlayerControlled && (!this.IsServer || this.isHostPlayerObject) || this.isTestingPlayer) && this.isHoldingObject && !((UnityEngine.Object) this.currentlyHeldObjectServer == (UnityEngine.Object) null) && !this.quickMenuManager.isMenuOpen && !this.isPlayerDead && (this.currentlyHeldObjectServer.itemProperties.usableInSpecialAnimations || !this.isGrabbingObjectAnimation && !this.inTerminalMenu && !this.isTypingChat && (!this.inSpecialInteractAnimation || this.inShockingMinigame));
    }

    private int FirstEmptyItemSlot()
    {
      int num = -1;
      if ((UnityEngine.Object) this.ItemSlots[this.currentItemSlot] == (UnityEngine.Object) null)
      {
        num = this.currentItemSlot;
      }
      else
      {
        for (int index = 0; index < this.ItemSlots.Length; ++index)
        {
          if ((UnityEngine.Object) this.ItemSlots[index] == (UnityEngine.Object) null)
          {
            num = index;
            break;
          }
        }
      }
      return num;
    }

    private int NextItemSlot(bool forward)
    {
      if (forward)
        return (this.currentItemSlot + 1) % this.ItemSlots.Length;
      return this.currentItemSlot == 0 ? this.ItemSlots.Length - 1 : this.currentItemSlot - 1;
    }

    private void SwitchToItemSlot(int slot, GrabbableObject fillSlotWithItem = null)
    {
      this.currentItemSlot = slot;
      if (this.IsOwner)
      {
        for (int index = 0; index < HUDManager.Instance.itemSlotIconFrames.Length; ++index)
          HUDManager.Instance.itemSlotIconFrames[index].GetComponent<Animator>().SetBool("selectedSlot", false);
        HUDManager.Instance.itemSlotIconFrames[slot].GetComponent<Animator>().SetBool("selectedSlot", true);
      }
      if ((UnityEngine.Object) fillSlotWithItem != (UnityEngine.Object) null)
      {
        this.ItemSlots[slot] = fillSlotWithItem;
        if (this.IsOwner)
        {
          HUDManager.Instance.itemSlotIcons[slot].sprite = fillSlotWithItem.itemProperties.itemIcon;
          HUDManager.Instance.itemSlotIcons[this.currentItemSlot].enabled = true;
        }
      }
      if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
      {
        this.currentlyHeldObjectServer.playerHeldBy = this;
        if (this.IsOwner)
          this.SetSpecialGrabAnimationBool(false, this.currentlyHeldObjectServer);
        this.currentlyHeldObjectServer.PocketItem();
        if ((UnityEngine.Object) this.ItemSlots[slot] != (UnityEngine.Object) null && !string.IsNullOrEmpty(this.ItemSlots[slot].itemProperties.pocketAnim))
          this.playerBodyAnimator.SetTrigger(this.ItemSlots[slot].itemProperties.pocketAnim);
      }
      if ((UnityEngine.Object) this.ItemSlots[slot] != (UnityEngine.Object) null)
      {
        this.ItemSlots[slot].playerHeldBy = this;
        this.ItemSlots[slot].EquipItem();
        if (this.IsOwner)
          this.SetSpecialGrabAnimationBool(true, this.ItemSlots[slot]);
        if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
        {
          if (this.ItemSlots[slot].itemProperties.twoHandedAnimation || this.currentlyHeldObjectServer.itemProperties.twoHandedAnimation)
          {
            this.playerBodyAnimator.ResetTrigger("SwitchHoldAnimationTwoHanded");
            this.playerBodyAnimator.SetTrigger("SwitchHoldAnimationTwoHanded");
          }
          this.playerBodyAnimator.ResetTrigger("SwitchHoldAnimation");
          this.playerBodyAnimator.SetTrigger("SwitchHoldAnimation");
        }
        this.twoHandedAnimation = this.ItemSlots[slot].itemProperties.twoHandedAnimation;
        this.twoHanded = this.ItemSlots[slot].itemProperties.twoHanded;
        this.playerBodyAnimator.SetBool("GrabValidated", true);
        this.playerBodyAnimator.SetBool("cancelHolding", false);
        this.isHoldingObject = true;
        this.currentlyHeldObjectServer = this.ItemSlots[slot];
      }
      else
      {
        if (!this.IsOwner && (UnityEngine.Object) this.heldObjectServerCopy != (UnityEngine.Object) null)
          this.heldObjectServerCopy.SetActive(false);
        if (this.IsOwner)
          HUDManager.Instance.ClearControlTips();
        this.currentlyHeldObjectServer = (GrabbableObject) null;
        this.currentlyHeldObject = (GrabbableObject) null;
        this.isHoldingObject = false;
        this.twoHanded = false;
        this.playerBodyAnimator.SetBool("cancelHolding", true);
      }
      if (!this.IsOwner)
        return;
      if (this.twoHanded)
      {
        HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, 0.1f, 0.13f, 0.13f);
        HUDManager.Instance.holdingTwoHandedItem.enabled = true;
      }
      else
      {
        HUDManager.Instance.PingHUDElement(HUDManager.Instance.Inventory, 1.5f, endAlpha: 0.13f);
        HUDManager.Instance.holdingTwoHandedItem.enabled = false;
      }
    }

    private void ScrollMouse_performed(InputAction.CallbackContext context)
    {
      if (this.inTerminalMenu)
      {
        this.terminalScrollVertical.value += context.ReadValue<float>() / 3f;
      }
      else
      {
        if ((!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || (double) this.timeSinceSwitchingSlots < 0.30000001192092896 || this.isGrabbingObjectAnimation || this.quickMenuManager.isMenuOpen || this.inSpecialInteractAnimation || this.throwingObject || this.isTypingChat || this.twoHanded || this.activatingItem || this.jetpackControls || this.disablingJetpackControls)
          return;
        ShipBuildModeManager.Instance.CancelBuildMode();
        this.playerBodyAnimator.SetBool("GrabValidated", false);
        if ((double) context.ReadValue<float>() > 0.0)
        {
          this.SwitchToItemSlot(this.NextItemSlot(true));
          this.SwitchItemSlotsServerRpc(true);
        }
        else
        {
          this.SwitchToItemSlot(this.NextItemSlot(false));
          this.SwitchItemSlotsServerRpc(false);
        }
        if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
          this.currentlyHeldObjectServer.gameObject.GetComponent<AudioSource>().PlayOneShot(this.currentlyHeldObjectServer.itemProperties.grabSFX, 0.6f);
        this.timeSinceSwitchingSlots = 0.0f;
      }
    }

    [ServerRpc]
    private void SwitchItemSlotsServerRpc(bool forward)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(412259855U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<bool>(in forward, new FastBufferWriter.ForPrimitives());
        this.__endSendServerRpc(ref bufferWriter, 412259855U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.SwitchItemSlotsClientRpc(forward);
    }

    [ClientRpc]
    private void SwitchItemSlotsClientRpc(bool forward)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(141629807U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<bool>(in forward, new FastBufferWriter.ForPrimitives());
        this.__endSendClientRpc(ref bufferWriter, 141629807U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      this.SwitchToItemSlot(this.NextItemSlot(forward));
    }

    private bool InteractTriggerUseConditionsMet()
    {
      if ((double) this.sinkingValue > 0.73000001907348633)
        return false;
      if (this.isClimbingLadder)
      {
        if (this.hoveringOverTrigger.isLadder)
        {
          if (!this.hoveringOverTrigger.usingLadder)
            return false;
        }
        else if (this.hoveringOverTrigger.specialCharacterAnimation)
          return false;
      }
      else if (this.inSpecialInteractAnimation)
        return false;
      return !this.hoveringOverTrigger.isPlayingSpecialAnimation;
    }

    private void InspectItem_performed(InputAction.CallbackContext context)
    {
      if (ShipBuildModeManager.Instance.InBuildMode || !context.performed || (!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || this.isGrabbingObjectAnimation || this.isTypingChat || this.inSpecialInteractAnimation || this.throwingObject || this.activatingItem)
        return;
      ShipBuildModeManager.Instance.CancelBuildMode();
      if (!((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null))
        return;
      this.currentlyHeldObjectServer.InspectItem();
    }

    private void QEItemInteract_performed(InputAction.CallbackContext context)
    {
      if (!this.equippedUsableItemQE || (!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || !context.performed || this.isGrabbingObjectAnimation || this.isTypingChat || this.inTerminalMenu || this.inSpecialInteractAnimation || this.throwingObject || (double) this.timeSinceSwitchingSlots < 0.20000000298023224)
        return;
      float num = context.ReadValue<float>();
      if (!((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null))
        return;
      this.timeSinceSwitchingSlots = 0.0f;
      if ((double) num < 0.0)
        this.currentlyHeldObjectServer.ItemInteractLeftRightOnClient(false);
      else
        this.currentlyHeldObjectServer.ItemInteractLeftRightOnClient(true);
    }

    private void ItemSecondaryUse_performed(InputAction.CallbackContext context)
    {
      Debug.Log((object) "secondary use A");
      if (!this.equippedUsableItemQE)
        return;
      Debug.Log((object) "secondary use B");
      if ((!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer)
        return;
      Debug.Log((object) "secondary use C");
      if (!context.performed)
        return;
      Debug.Log((object) "secondary use D");
      if (this.isGrabbingObjectAnimation || this.isTypingChat || this.inTerminalMenu || this.inSpecialInteractAnimation || this.throwingObject)
        return;
      Debug.Log((object) "secondary use E");
      if ((double) this.timeSinceSwitchingSlots < 0.20000000298023224)
        return;
      Debug.Log((object) "secondary use F");
      if (!((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null))
        return;
      Debug.Log((object) "secondary use G");
      this.timeSinceSwitchingSlots = 0.0f;
      this.currentlyHeldObjectServer.ItemInteractLeftRightOnClient(false);
    }

    private void ItemTertiaryUse_performed(InputAction.CallbackContext context)
    {
      if (!this.equippedUsableItemQE || (!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || !context.performed || this.isGrabbingObjectAnimation || this.isTypingChat || this.inTerminalMenu || this.inSpecialInteractAnimation || this.throwingObject || (double) this.timeSinceSwitchingSlots < 0.20000000298023224 || !((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null))
        return;
      this.timeSinceSwitchingSlots = 0.0f;
      this.currentlyHeldObjectServer.ItemInteractLeftRightOnClient(true);
    }

    private void Interact_performed(InputAction.CallbackContext context)
    {
      if (this.IsOwner && this.isPlayerDead && (!this.IsServer || this.isHostPlayerObject))
      {
        if (StartOfRound.Instance.overrideSpectateCamera || !((UnityEngine.Object) this.spectatedPlayerScript != (UnityEngine.Object) null) || this.spectatedPlayerScript.isPlayerDead)
          return;
        this.SpectateNextPlayer();
      }
      else
      {
        if ((!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || !context.performed || (double) this.timeSinceSwitchingSlots < 0.20000000298023224)
          return;
        ShipBuildModeManager.Instance.CancelBuildMode();
        if (this.isGrabbingObjectAnimation || this.isTypingChat || this.inTerminalMenu || this.throwingObject || this.IsInspectingItem || (UnityEngine.Object) this.inAnimationWithEnemy != (UnityEngine.Object) null || this.jetpackControls || this.disablingJetpackControls || StartOfRound.Instance.suckingPlayersOutOfShip)
          return;
        if (!this.activatingItem)
          this.BeginGrabObject();
        if ((UnityEngine.Object) this.hoveringOverTrigger == (UnityEngine.Object) null || this.hoveringOverTrigger.holdInteraction || this.isHoldingObject && !this.hoveringOverTrigger.oneHandedItemAllowed || this.twoHanded && (!this.hoveringOverTrigger.twoHandedItemAllowed || this.hoveringOverTrigger.specialCharacterAnimation) || !this.InteractTriggerUseConditionsMet())
          return;
        this.hoveringOverTrigger.Interact(this.thisPlayerBody);
      }
    }

    private void BeginGrabObject()
    {
      this.interactRay = new Ray(this.gameplayCamera.transform.position, this.gameplayCamera.transform.forward);
      if (!Physics.Raycast(this.interactRay, out this.hit, this.grabDistance, this.interactableObjectsMask) || this.hit.collider.gameObject.layer == 8 || !(this.hit.collider.tag == "PhysicsProp") || this.twoHanded || (double) this.sinkingValue > 0.73000001907348633)
        return;
      this.currentlyGrabbingObject = this.hit.collider.transform.gameObject.GetComponent<GrabbableObject>();
      if (!GameNetworkManager.Instance.gameHasStarted && !this.currentlyGrabbingObject.itemProperties.canBeGrabbedBeforeGameStart && (UnityEngine.Object) StartOfRound.Instance.testRoom == (UnityEngine.Object) null)
        return;
      this.grabInvalidated = false;
      if ((UnityEngine.Object) this.currentlyGrabbingObject == (UnityEngine.Object) null || this.inSpecialInteractAnimation || this.currentlyGrabbingObject.isHeld || this.currentlyGrabbingObject.isPocketed)
        return;
      NetworkObject networkObject = this.currentlyGrabbingObject.NetworkObject;
      if ((UnityEngine.Object) networkObject == (UnityEngine.Object) null || !networkObject.IsSpawned)
        return;
      this.currentlyGrabbingObject.InteractItem();
      if (!this.currentlyGrabbingObject.grabbable || this.FirstEmptyItemSlot() == -1)
        return;
      this.playerBodyAnimator.SetBool("GrabInvalidated", false);
      this.playerBodyAnimator.SetBool("GrabValidated", false);
      this.playerBodyAnimator.SetBool("cancelHolding", false);
      this.playerBodyAnimator.ResetTrigger("Throw");
      this.SetSpecialGrabAnimationBool(true);
      this.isGrabbingObjectAnimation = true;
      this.cursorIcon.enabled = false;
      this.cursorTip.text = "";
      this.twoHanded = this.currentlyGrabbingObject.itemProperties.twoHanded;
      this.carryWeight += Mathf.Clamp(this.currentlyGrabbingObject.itemProperties.weight - 1f, 0.0f, 10f);
      this.grabObjectAnimationTime = (double) this.currentlyGrabbingObject.itemProperties.grabAnimationTime <= 0.0 ? 0.4f : this.currentlyGrabbingObject.itemProperties.grabAnimationTime;
      if (!this.isTestingPlayer)
        this.GrabObjectServerRpc((NetworkObjectReference) networkObject);
      if (this.grabObjectCoroutine != null)
        this.StopCoroutine(this.grabObjectCoroutine);
      this.grabObjectCoroutine = this.StartCoroutine(this.GrabObject());
    }

    private IEnumerator GrabObject()
    {
      this.grabbedObjectValidated = false;
      yield return (object) new WaitForSeconds(0.1f);
      this.currentlyGrabbingObject.parentObject = this.localItemHolder;
      if ((UnityEngine.Object) this.currentlyGrabbingObject.itemProperties.grabSFX != (UnityEngine.Object) null)
        this.itemAudio.PlayOneShot(this.currentlyGrabbingObject.itemProperties.grabSFX, 1f);
      if ((UnityEngine.Object) this.currentlyGrabbingObject.playerHeldBy != (UnityEngine.Object) null)
        Debug.Log((object) string.Format("playerHeldBy on currentlyGrabbingObject 1: {0}", (object) this.currentlyGrabbingObject.playerHeldBy));
      while (((UnityEngine.Object) this.currentlyGrabbingObject != (UnityEngine.Object) this.currentlyHeldObjectServer || !this.currentlyHeldObjectServer.wasOwnerLastFrame) && !this.grabInvalidated)
      {
        Debug.Log((object) string.Format("grabInvalidated: {0}", (object) this.grabInvalidated));
        yield return (object) null;
      }
      if (this.grabInvalidated)
      {
        this.grabInvalidated = false;
        Debug.Log((object) ("Grab was invalidated on object: " + this.currentlyGrabbingObject.name));
        if ((UnityEngine.Object) this.currentlyGrabbingObject.playerHeldBy != (UnityEngine.Object) null)
          Debug.Log((object) string.Format("playerHeldBy on currentlyGrabbingObject 2: {0}", (object) this.currentlyGrabbingObject.playerHeldBy));
        if ((UnityEngine.Object) this.currentlyGrabbingObject.parentObject == (UnityEngine.Object) this.localItemHolder)
        {
          if ((UnityEngine.Object) this.currentlyGrabbingObject.playerHeldBy != (UnityEngine.Object) null)
          {
            Debug.Log((object) string.Format("Grab invalidated; giving grabbed object to the client who got it first; {0}", (object) this.currentlyGrabbingObject.playerHeldBy));
            this.currentlyGrabbingObject.parentObject = this.currentlyGrabbingObject.playerHeldBy.serverItemHolder;
          }
          else
          {
            Debug.Log((object) "Grab invalidated; no other client has possession of it, so set its parent object to null.");
            this.currentlyGrabbingObject.parentObject = (Transform) null;
          }
        }
        this.twoHanded = false;
        this.SetSpecialGrabAnimationBool(false);
        if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
          this.playerBodyAnimator.SetBool("Grab", true);
        this.playerBodyAnimator.SetBool("GrabInvalidated", true);
        this.carryWeight = Mathf.Clamp(this.carryWeight - (this.currentlyGrabbingObject.itemProperties.weight - 1f), 0.0f, 10f);
        this.isGrabbingObjectAnimation = false;
        this.currentlyGrabbingObject = (GrabbableObject) null;
      }
      else
      {
        this.grabbedObjectValidated = true;
        this.currentlyHeldObjectServer.GrabItemOnClient();
        this.isHoldingObject = true;
        yield return (object) new WaitForSeconds(this.grabObjectAnimationTime - 0.2f);
        this.playerBodyAnimator.SetBool("GrabValidated", true);
        this.isGrabbingObjectAnimation = false;
      }
    }

    private void SetSpecialGrabAnimationBool(bool setTrue, GrabbableObject currentItem = null)
    {
      if ((UnityEngine.Object) currentItem == (UnityEngine.Object) null)
        currentItem = this.currentlyGrabbingObject;
      if (!this.IsOwner)
        return;
      this.playerBodyAnimator.SetBool("Grab", setTrue);
      if (string.IsNullOrEmpty(currentItem.itemProperties.grabAnim))
        return;
      try
      {
        this.playerBodyAnimator.SetBool(currentItem.itemProperties.grabAnim, setTrue);
      }
      catch (Exception ex)
      {
        Debug.LogError((object) ("An item tried to set an animator bool which does not exist: " + currentItem.itemProperties.grabAnim));
      }
    }

    [ServerRpc]
    private void GrabObjectServerRpc(NetworkObjectReference grabbedObject)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(1554282707U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<NetworkObjectReference>(in grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        this.__endSendServerRpc(ref bufferWriter, 1554282707U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      bool grabValidated = true;
      NetworkObject networkObject;
      if (grabbedObject.TryGet(out networkObject) && (bool) (UnityEngine.Object) networkObject.GetComponentInChildren<GrabbableObject>())
      {
        if (networkObject.GetComponentInChildren<GrabbableObject>().heldByPlayerOnServer)
        {
          grabValidated = false;
          Debug.Log((object) ("Invalidated grab on " + this.gameObject.name + " on client; another player was already grabbing the same object"));
        }
      }
      else
        grabValidated = false;
      if (grabValidated)
      {
        networkObject.GetComponentInChildren<GrabbableObject>().heldByPlayerOnServer = true;
        networkObject.ChangeOwnership(this.actualClientId);
        this.GrabObjectClientRpc(true, (NetworkObjectReference) networkObject);
      }
      else
        this.GrabObjectClientRpc(grabValidated, grabbedObject);
    }

    [ClientRpc]
    private void GrabObjectClientRpc(bool grabValidated, NetworkObjectReference grabbedObject)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(2552479808U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<bool>(in grabValidated, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<NetworkObjectReference>(in grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        this.__endSendClientRpc(ref bufferWriter, 2552479808U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      if (grabValidated)
      {
        NetworkObject networkObject;
        if (!grabbedObject.TryGet(out networkObject))
        {
          Debug.Log((object) string.Format("Error! Networkobject grabbed was not found on client: {0}", (object) networkObject.NetworkObjectId));
        }
        else
        {
          this.SwitchToItemSlot(this.FirstEmptyItemSlot(), networkObject.gameObject.GetComponentInChildren<GrabbableObject>());
          this.currentlyHeldObjectServer.EnablePhysics(false);
          this.currentlyHeldObjectServer.isHeld = true;
          this.currentlyHeldObjectServer.hasHitGround = false;
          this.currentlyHeldObjectServer.isInFactory = this.isInsideFactory;
          this.twoHanded = this.currentlyHeldObjectServer.itemProperties.twoHanded;
          this.twoHandedAnimation = this.currentlyHeldObjectServer.itemProperties.twoHandedAnimation;
          if (this.IsOwner)
            return;
          this.currentlyHeldObjectServer.parentObject = this.serverItemHolder;
          this.isHoldingObject = true;
          this.carryWeight += Mathf.Clamp(this.currentlyHeldObjectServer.itemProperties.weight - 1f, 0.0f, 10f);
          if ((UnityEngine.Object) this.currentlyHeldObjectServer.itemProperties.grabSFX != (UnityEngine.Object) null)
            this.itemAudio.PlayOneShot(this.currentlyHeldObjectServer.itemProperties.grabSFX, 1f);
          if ((UnityEngine.Object) this.currentlyHeldObjectServer.playerHeldBy != (UnityEngine.Object) null)
            Debug.Log((object) string.Format("playerHeldBy on grabbed object: {0}", (object) this.currentlyHeldObjectServer.playerHeldBy));
          else
            Debug.Log((object) "grabbed object playerHeldBy is null");
        }
      }
      else
      {
        Debug.Log((object) string.Format("Player #{0}: Was grabbing object {1} validated by server? : {2}", (object) this.playerClientId, (object) grabbedObject.NetworkObjectId, (object) grabValidated));
        if (!this.IsOwner)
          return;
        Debug.Log((object) "Local client got grab invalidated");
        this.grabInvalidated = true;
      }
    }

    private void Discard_performed(InputAction.CallbackContext context)
    {
      if (!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject || !context.performed)
        return;
      if (StartOfRound.Instance.localPlayerUsingController && ShipBuildModeManager.Instance.InBuildMode)
      {
        ShipBuildModeManager.Instance.StoreObjectLocalClient();
      }
      else
      {
        if ((double) this.timeSinceSwitchingSlots < 0.20000000298023224 || this.isGrabbingObjectAnimation || (double) this.timeSinceSwitchingSlots < 0.20000000298023224 || this.isTypingChat || this.inSpecialInteractAnimation || this.activatingItem)
          return;
        ShipBuildModeManager.Instance.CancelBuildMode();
        if (this.throwingObject || !this.isHoldingObject || !((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null))
          return;
        if ((UnityEngine.Object) UnityEngine.Object.FindObjectOfType<DepositItemsDesk>() != (UnityEngine.Object) null && (UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
        {
          DepositItemsDesk objectOfType = UnityEngine.Object.FindObjectOfType<DepositItemsDesk>();
          if (UnityEngine.Object.FindObjectOfType<DepositItemsDesk>().triggerCollider.bounds.Contains(this.currentlyHeldObjectServer.transform.position))
          {
            objectOfType.PlaceItemOnCounter(this);
            return;
          }
        }
        this.StartCoroutine(this.waitToEndOfFrameToDiscard());
      }
    }

    private IEnumerator waitToEndOfFrameToDiscard()
    {
      yield return (object) new WaitForEndOfFrame();
      this.DiscardHeldObject();
    }

    public void DespawnHeldObject()
    {
      if (!((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null))
        return;
      this.SetSpecialGrabAnimationBool(false, this.currentlyHeldObjectServer);
      this.playerBodyAnimator.SetBool("cancelHolding", true);
      this.playerBodyAnimator.SetTrigger("Throw");
      HUDManager.Instance.itemSlotIcons[this.currentItemSlot].enabled = false;
      HUDManager.Instance.holdingTwoHandedItem.enabled = false;
      this.throwingObject = true;
      this.DespawnHeldObjectOnClient();
      this.DespawnHeldObjectServerRpc();
    }

    private void DespawnHeldObjectOnClient()
    {
      this.ItemSlots[this.currentItemSlot] = (GrabbableObject) null;
      this.twoHanded = false;
      this.twoHandedAnimation = false;
      this.carryWeight -= Mathf.Clamp(this.currentlyHeldObjectServer.itemProperties.weight - 1f, 0.0f, 10f);
      this.isHoldingObject = false;
      this.hasThrownObject = true;
    }

    [ServerRpc]
    private void DespawnHeldObjectServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(1786952262U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 1786952262U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
        this.currentlyHeldObjectServer.gameObject.GetComponent<NetworkObject>().Despawn();
      this.DespawnHeldObjectClientRpc();
    }

    [ClientRpc]
    private void DespawnHeldObjectClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(2217326231U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 2217326231U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      if (!this.IsOwner)
        this.DespawnHeldObjectOnClient();
      else
        this.throwingObject = false;
    }

    public void DiscardHeldObject(
      bool placeObject = false,
      NetworkObject parentObjectTo = null,
      Vector3 placePosition = default (Vector3),
      bool matchRotationOfParent = true)
    {
      this.SetSpecialGrabAnimationBool(false, this.currentlyHeldObjectServer);
      this.playerBodyAnimator.SetBool("cancelHolding", true);
      this.playerBodyAnimator.SetTrigger("Throw");
      HUDManager.Instance.itemSlotIcons[this.currentItemSlot].enabled = false;
      HUDManager.Instance.holdingTwoHandedItem.enabled = false;
      if (placeObject)
      {
        if ((UnityEngine.Object) parentObjectTo == (UnityEngine.Object) null)
        {
          this.throwingObject = true;
          placePosition = !this.isInElevator ? StartOfRound.Instance.propsContainer.InverseTransformPoint(placePosition) : StartOfRound.Instance.elevatorTransform.InverseTransformPoint(placePosition);
          int y = (int) this.transform.localEulerAngles.y;
          this.SetObjectAsNoLongerHeld(this.isInElevator, this.isInHangarShipRoom, placePosition, this.currentlyHeldObjectServer, y);
          this.currentlyHeldObjectServer.DiscardItemOnClient();
          this.ThrowObjectServerRpc((NetworkObjectReference) this.currentlyHeldObjectServer.gameObject.GetComponent<NetworkObject>(), this.isInElevator, this.isInHangarShipRoom, placePosition, y);
        }
        else
        {
          this.PlaceGrabbableObject(parentObjectTo.transform, placePosition, matchRotationOfParent, this.currentlyHeldObjectServer);
          this.currentlyHeldObjectServer.DiscardItemOnClient();
          this.PlaceObjectServerRpc((NetworkObjectReference) this.currentlyHeldObjectServer.gameObject.GetComponent<NetworkObject>(), (NetworkObjectReference) parentObjectTo, placePosition, matchRotationOfParent);
        }
      }
      else
      {
        this.throwingObject = true;
        bool droppedInElevator = this.isInElevator;
        Vector3 targetFloorPosition;
        if (!this.isInElevator)
        {
          Vector3 vector3 = !this.currentlyHeldObjectServer.itemProperties.allowDroppingAheadOfPlayer ? this.currentlyHeldObjectServer.GetItemFloorPosition() : this.DropItemAheadOfPlayer();
          if (!this.playersManager.shipBounds.bounds.Contains(vector3))
          {
            targetFloorPosition = this.playersManager.propsContainer.InverseTransformPoint(vector3);
          }
          else
          {
            droppedInElevator = true;
            targetFloorPosition = this.playersManager.elevatorTransform.InverseTransformPoint(vector3);
          }
        }
        else
        {
          Vector3 itemFloorPosition = this.currentlyHeldObjectServer.GetItemFloorPosition();
          if (!this.playersManager.shipBounds.bounds.Contains(itemFloorPosition))
          {
            droppedInElevator = false;
            targetFloorPosition = this.playersManager.propsContainer.InverseTransformPoint(itemFloorPosition);
          }
          else
            targetFloorPosition = this.playersManager.elevatorTransform.InverseTransformPoint(itemFloorPosition);
        }
        int y = (int) this.transform.localEulerAngles.y;
        this.SetObjectAsNoLongerHeld(droppedInElevator, this.isInHangarShipRoom, targetFloorPosition, this.currentlyHeldObjectServer, y);
        this.currentlyHeldObjectServer.DiscardItemOnClient();
        this.ThrowObjectServerRpc((NetworkObjectReference) this.currentlyHeldObjectServer.NetworkObject, droppedInElevator, this.isInHangarShipRoom, targetFloorPosition, y);
      }
    }

    private Vector3 DropItemAheadOfPlayer()
    {
      Vector3 zero = Vector3.zero;
      Ray ray = new Ray(this.transform.position + Vector3.up * 0.4f, this.gameplayCamera.transform.forward);
      Vector3 startPosition = !Physics.Raycast(ray, out this.hit, 1.7f, 268438273, QueryTriggerInteraction.Ignore) ? ray.GetPoint(1.7f) : ray.GetPoint(Mathf.Clamp(this.hit.distance - 0.3f, 0.01f, 2f));
      Vector3 itemFloorPosition = this.currentlyHeldObjectServer.GetItemFloorPosition(startPosition);
      if (itemFloorPosition == startPosition)
        itemFloorPosition = this.currentlyHeldObjectServer.GetItemFloorPosition();
      return itemFloorPosition;
    }

    [ServerRpc]
    private void ThrowObjectServerRpc(
      NetworkObjectReference grabbedObject,
      bool droppedInElevator,
      bool droppedInShipRoom,
      Vector3 targetFloorPosition,
      int floorYRot)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(2376977494U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<NetworkObjectReference>(in grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        bufferWriter.WriteValueSafe<bool>(in droppedInElevator, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<bool>(in droppedInShipRoom, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe(in targetFloorPosition);
        BytePacker.WriteValueBitPacked(bufferWriter, floorYRot);
        this.__endSendServerRpc(ref bufferWriter, 2376977494U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      if (grabbedObject.TryGet(out NetworkObject _))
        this.ThrowObjectClientRpc(droppedInElevator, droppedInShipRoom, targetFloorPosition, grabbedObject, floorYRot);
      else
        Debug.LogError((object) "Object was not thrown because it does not exist on the server.");
    }

    [ClientRpc]
    private void ThrowObjectClientRpc(
      bool droppedInElevator,
      bool droppedInShipRoom,
      Vector3 targetFloorPosition,
      NetworkObjectReference grabbedObject,
      int floorYRot)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(3943098567U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<bool>(in droppedInElevator, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<bool>(in droppedInShipRoom, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe(in targetFloorPosition);
        bufferWriter.WriteValueSafe<NetworkObjectReference>(in grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        BytePacker.WriteValueBitPacked(bufferWriter, floorYRot);
        this.__endSendClientRpc(ref bufferWriter, 3943098567U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      NetworkObject networkObject;
      if (grabbedObject.TryGet(out networkObject))
      {
        GrabbableObject component = networkObject.GetComponent<GrabbableObject>();
        if (!this.IsOwner)
          this.SetObjectAsNoLongerHeld(droppedInElevator, droppedInShipRoom, targetFloorPosition, component);
        if (!component.itemProperties.syncDiscardFunction)
          component.playerHeldBy = (PlayerControllerB) null;
        if ((UnityEngine.Object) component == (UnityEngine.Object) this.currentlyHeldObjectServer)
        {
          this.currentlyHeldObjectServer = (GrabbableObject) null;
        }
        else
        {
          string str = "null";
          if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
            str = this.currentlyHeldObjectServer.gameObject.name;
          Debug.LogError((object) string.Format("ThrowObjectClientRpc called for an object which is not the same as currentlyHeldObjectServer which is {0}, on player #{1}.", (object) str, (object) this.playerClientId));
        }
      }
      else
        Debug.LogError((object) "The server did not have a reference to the held object (when attempting to THROW on client.)");
      if (!this.IsOwner)
        return;
      this.throwingObject = false;
    }

    public void SetObjectAsNoLongerHeld(
      bool droppedInElevator,
      bool droppedInShipRoom,
      Vector3 targetFloorPosition,
      GrabbableObject dropObject,
      int floorYRot = -1)
    {
      for (int index = 0; index < this.ItemSlots.Length; ++index)
      {
        if ((UnityEngine.Object) this.ItemSlots[index] == (UnityEngine.Object) dropObject)
          this.ItemSlots[index] = (GrabbableObject) null;
      }
      dropObject.heldByPlayerOnServer = false;
      dropObject.parentObject = (Transform) null;
      if (droppedInElevator)
        dropObject.transform.SetParent(this.playersManager.elevatorTransform, true);
      else
        dropObject.transform.SetParent(this.playersManager.propsContainer, true);
      this.SetItemInElevator(droppedInShipRoom, droppedInElevator, dropObject);
      dropObject.EnablePhysics(true);
      dropObject.EnableItemMeshes(true);
      dropObject.transform.localScale = dropObject.originalScale;
      dropObject.isHeld = false;
      dropObject.isPocketed = false;
      dropObject.fallTime = 0.0f;
      dropObject.startFallingPosition = dropObject.transform.parent.InverseTransformPoint(dropObject.transform.position);
      dropObject.targetFloorPosition = targetFloorPosition;
      dropObject.floorYRot = floorYRot;
      this.twoHanded = false;
      this.twoHandedAnimation = false;
      this.carryWeight -= Mathf.Clamp(dropObject.itemProperties.weight - 1f, 0.0f, 10f);
      this.isHoldingObject = false;
      this.hasThrownObject = true;
    }

    public void SetAllItemsInElevator(bool inShipRoom, bool inElevator)
    {
      for (int index = 0; index < this.ItemSlots.Length; ++index)
      {
        if ((UnityEngine.Object) this.ItemSlots[index] != (UnityEngine.Object) null)
          this.SetItemInElevator(inShipRoom, inElevator, this.ItemSlots[index]);
      }
    }

    public void SetItemInElevator(
      bool droppedInShipRoom,
      bool droppedInElevator,
      GrabbableObject gObject)
    {
      gObject.isInElevator = droppedInElevator;
      if (gObject.isInShipRoom == droppedInShipRoom)
        return;
      gObject.isInShipRoom = droppedInShipRoom;
      if (!gObject.scrapPersistedThroughRounds)
      {
        if (droppedInShipRoom)
        {
          RoundManager.Instance.scrapCollectedInLevel += gObject.scrapValue;
          StartOfRound.Instance.gameStats.allPlayerStats[this.playerClientId].profitable += gObject.scrapValue;
          RoundManager.Instance.CollectNewScrapForThisRound(gObject);
          gObject.OnBroughtToShip();
          if (gObject.itemProperties.isScrap && (double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, gObject.transform.position) < 12.0)
            HUDManager.Instance.DisplayTip("Got scrap!", "To sell, use the terminal to route the ship to the company building.", useSave: true, prefsKey: "LCTip_SellScrap");
        }
        else
        {
          if (gObject.scrapPersistedThroughRounds)
            return;
          RoundManager.Instance.scrapCollectedInLevel -= gObject.scrapValue;
          StartOfRound.Instance.gameStats.allPlayerStats[this.playerClientId].profitable -= gObject.scrapValue;
        }
        HUDManager.Instance.SetQuota(RoundManager.Instance.scrapCollectedInLevel);
      }
      if (droppedInShipRoom)
        ++StartOfRound.Instance.currentShipItemCount;
      else
        --StartOfRound.Instance.currentShipItemCount;
    }

    [ServerRpc]
    private void PlaceObjectServerRpc(
      NetworkObjectReference grabbedObject,
      NetworkObjectReference parentObject,
      Vector3 placePositionOffset = default (Vector3),
      bool matchRotationOfParent = true)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(3830452098U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<NetworkObjectReference>(in grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        bufferWriter.WriteValueSafe<NetworkObjectReference>(in parentObject, new FastBufferWriter.ForNetworkSerializable());
        bufferWriter.WriteValueSafe(in placePositionOffset);
        bufferWriter.WriteValueSafe<bool>(in matchRotationOfParent, new FastBufferWriter.ForPrimitives());
        this.__endSendServerRpc(ref bufferWriter, 3830452098U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      if (grabbedObject.TryGet(out NetworkObject _) && parentObject.TryGet(out NetworkObject _))
        this.PlaceObjectClientRpc(parentObject, placePositionOffset, matchRotationOfParent, grabbedObject);
      else if (!grabbedObject.TryGet(out NetworkObject _))
      {
        Debug.LogError((object) string.Format("Object placement not synced to clients, missing reference to a network object: placing object with id: {0}; player #{1}", (object) grabbedObject.NetworkObjectId, (object) this.playerClientId));
      }
      else
      {
        if (parentObject.TryGet(out NetworkObject _))
          return;
        Debug.LogError((object) string.Format("Object placement not synced to clients, missing reference to a network object: parent object with id: {0}; player #{1}", (object) grabbedObject.NetworkObjectId, (object) this.playerClientId));
      }
    }

    public void PlaceGrabbableObject(
      Transform parentObject,
      Vector3 positionOffset,
      bool matchRotationOfParent,
      GrabbableObject placeObject)
    {
      placeObject.EnablePhysics(true);
      placeObject.EnableItemMeshes(true);
      placeObject.isHeld = false;
      placeObject.isPocketed = false;
      placeObject.heldByPlayerOnServer = false;
      this.SetItemInElevator(this.isInHangarShipRoom, this.isInElevator, placeObject);
      placeObject.parentObject = (Transform) null;
      placeObject.transform.SetParent(parentObject, true);
      placeObject.startFallingPosition = placeObject.transform.localPosition;
      placeObject.transform.localScale = placeObject.originalScale;
      placeObject.transform.localPosition = positionOffset;
      placeObject.targetFloorPosition = positionOffset;
      if (!matchRotationOfParent)
      {
        placeObject.fallTime = 0.0f;
      }
      else
      {
        placeObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
        placeObject.fallTime = 1.1f;
      }
      placeObject.OnPlaceObject();
      for (int index = 0; index < this.ItemSlots.Length; ++index)
      {
        if ((UnityEngine.Object) this.ItemSlots[index] == (UnityEngine.Object) placeObject)
          this.ItemSlots[index] = (GrabbableObject) null;
      }
      this.twoHanded = false;
      this.twoHandedAnimation = false;
      this.carryWeight -= Mathf.Clamp(placeObject.itemProperties.weight - 1f, 0.0f, 10f);
      this.isHoldingObject = false;
      this.hasThrownObject = true;
    }

    [ClientRpc]
    private void PlaceObjectClientRpc(
      NetworkObjectReference parentObjectReference,
      Vector3 placePositionOffset,
      bool matchRotationOfParent,
      NetworkObjectReference grabbedObject)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(3771510012U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<NetworkObjectReference>(in parentObjectReference, new FastBufferWriter.ForNetworkSerializable());
        bufferWriter.WriteValueSafe(in placePositionOffset);
        bufferWriter.WriteValueSafe<bool>(in matchRotationOfParent, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<NetworkObjectReference>(in grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        this.__endSendClientRpc(ref bufferWriter, 3771510012U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      NetworkObject networkObject1;
      if (grabbedObject.TryGet(out networkObject1))
      {
        GrabbableObject component = networkObject1.GetComponent<GrabbableObject>();
        if (!this.IsOwner)
        {
          NetworkObject networkObject2;
          if (parentObjectReference.TryGet(out networkObject2))
          {
            this.PlaceGrabbableObject(networkObject2.transform, placePositionOffset, matchRotationOfParent, component);
          }
          else
          {
            this.PlaceGrabbableObject((Transform) null, placePositionOffset, matchRotationOfParent, component);
            Debug.LogError((object) string.Format("Reference to parent object when placing was missing. object: {0} placed by {1}", (object) component, (object) this.gameObject.name));
          }
        }
        if (!component.itemProperties.syncDiscardFunction)
          component.playerHeldBy = (PlayerControllerB) null;
        if ((UnityEngine.Object) this.currentlyHeldObjectServer == (UnityEngine.Object) component)
        {
          this.currentlyHeldObjectServer = (GrabbableObject) null;
        }
        else
        {
          string str = "null";
          if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
            str = this.currentlyHeldObjectServer.gameObject.name;
          Debug.LogError((object) string.Format("ThrowObjectClientRpc called for an object which is not the same as currentlyHeldObjectServer which is {0}, on player #{1}.", (object) str, (object) this.playerClientId));
        }
      }
      else
        Debug.LogError((object) "The server did not have a reference to the held object (when attempting to PLACE object on client.)");
      if (!this.IsOwner)
        return;
      this.throwingObject = false;
      HUDManager.Instance.itemSlotIcons[this.currentItemSlot].enabled = false;
    }

    private void SetFreeCamera_performed(InputAction.CallbackContext context)
    {
      if (!this.IsServer || !this.IsOwner || !context.performed || this.isTypingChat)
        return;
      ShipBuildModeManager.Instance.CancelBuildMode();
      int index;
      if (TimeOfDay.Instance.insideLighting && !StartOfRound.Instance.inShipPhase)
      {
        if ((UnityEngine.Object) RoundManager.Instance.dungeonGenerator == (UnityEngine.Object) null || (UnityEngine.Object) RoundManager.Instance.dungeonGenerator.Root == (UnityEngine.Object) null)
          return;
        StartOfRound.Instance.freeCinematicCamera.transform.position = RoundManager.Instance.dungeonGenerator.Root.transform.position;
        StartOfRound.Instance.freeCinematicCameraTurnCompass.transform.position = RoundManager.Instance.dungeonGenerator.Root.transform.position;
        index = 3;
      }
      else
      {
        if ((UnityEngine.Object) StartOfRound.Instance.elevatorTransform == (UnityEngine.Object) null)
          return;
        index = 1;
        StartOfRound.Instance.freeCinematicCamera.transform.position = StartOfRound.Instance.elevatorTransform.position;
        StartOfRound.Instance.freeCinematicCameraTurnCompass.transform.position = StartOfRound.Instance.elevatorTransform.position;
      }
      AudioReverbPresets objectOfType = UnityEngine.Object.FindObjectOfType<AudioReverbPresets>();
      if ((UnityEngine.Object) objectOfType != (UnityEngine.Object) null)
      {
        if (objectOfType.audioPresets[index].usePreset != -1)
        {
          Debug.LogError((object) "Audio preset AudioReverbTrigger is set to call another audio preset. This would cause a crash.");
          return;
        }
        objectOfType.audioPresets[index].ChangeAudioReverbForPlayer(this);
      }
      TimeOfDay.Instance.insideLighting = !TimeOfDay.Instance.insideLighting;
    }

    public void ChangeHelmetLight(int lightNumber, bool enable = true)
    {
      for (int index = 0; index < this.allHelmetLights.Length; ++index)
        this.allHelmetLights[index].enabled = false;
      this.allHelmetLights[lightNumber].enabled = enable;
      this.helmetLight = this.allHelmetLights[lightNumber];
    }

    private void OpenMenu_performed(InputAction.CallbackContext context)
    {
      Debug.Log((object) "PLAYER OPENED MENU");
      Debug.Log((object) string.Format("In terminal menu: {0}", (object) this.inTerminalMenu));
      if (!context.performed)
        return;
      if ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
      {
        if (!this.isTestingPlayer)
          return;
      }
      else if (!this.IsOwner || !this.isPlayerControlled && !this.isPlayerDead || this.IsServer && !this.isHostPlayerObject)
        return;
      if (this.inTerminalMenu || (bool) (UnityEngine.Object) UnityEngine.Object.FindObjectOfType<Terminal>() && (double) UnityEngine.Object.FindObjectOfType<Terminal>().timeSinceTerminalInUse < 0.25 || this.isTypingChat)
        return;
      if (!this.quickMenuManager.isMenuOpen)
        this.quickMenuManager.OpenQuickMenu();
      else if (IngamePlayerSettings.Instance.changesNotApplied)
        IngamePlayerSettings.Instance.DisplayConfirmChangesScreen(true);
      else
        this.quickMenuManager.CloseQuickMenu();
    }

    private void Jump_performed(InputAction.CallbackContext context)
    {
      if (this.quickMenuManager.isMenuOpen || (!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || this.inSpecialInteractAnimation || this.isTypingChat || this.isMovementHindered > 0 && !this.isUnderwater || this.isExhausted || !this.thisController.isGrounded && (this.isJumping || !this.IsPlayerNearGround()) || this.isJumping || this.isPlayerSliding && (double) this.playerSlidingTimer <= 2.5 || this.isCrouching)
        return;
      this.playerSlidingTimer = 0.0f;
      this.isJumping = true;
      this.sprintMeter = Mathf.Clamp(this.sprintMeter - 0.08f, 0.0f, 1f);
      this.movementAudio.PlayOneShot(StartOfRound.Instance.playerJumpSFX);
      if (this.jumpCoroutine != null)
        this.StopCoroutine(this.jumpCoroutine);
      this.jumpCoroutine = this.StartCoroutine(this.PlayerJump());
    }

    private IEnumerator PlayerJump()
    {
      PlayerControllerB playerControllerB = this;
      playerControllerB.playerBodyAnimator.SetBool("Jumping", true);
      yield return (object) new WaitForSeconds(0.15f);
      playerControllerB.fallValue = playerControllerB.jumpForce;
      playerControllerB.fallValueUncapped = playerControllerB.jumpForce;
      yield return (object) new WaitForSeconds(0.1f);
      playerControllerB.isJumping = false;
      playerControllerB.isFallingFromJump = true;
      // ISSUE: reference to a compiler-generated method
      yield return (object) new WaitUntil(new Func<bool>(playerControllerB.\u003CPlayerJump\u003Eb__374_0));
      playerControllerB.playerBodyAnimator.SetBool("Jumping", false);
      playerControllerB.isFallingFromJump = false;
      playerControllerB.PlayerHitGroundEffects();
      playerControllerB.jumpCoroutine = (Coroutine) null;
    }

    public void ResetFallGravity()
    {
      this.takingFallDamage = false;
      this.fallValue = 0.0f;
      this.fallValueUncapped = 0.0f;
    }

    private void PlayerLookInput()
    {
      if (this.quickMenuManager.isMenuOpen || StartOfRound.Instance.newGameIsLoading || this.disableLookInput)
        return;
      Vector2 vector2_1 = this.playerActions.Movement.Look.ReadValue<Vector2>() * 0.008f * (float) IngamePlayerSettings.Instance.settings.lookSensitivity;
      if (IngamePlayerSettings.Instance.settings.invertYAxis)
        vector2_1.y *= -1f;
      if (this.isFreeCamera)
      {
        StartOfRound.Instance.freeCinematicCameraTurnCompass.Rotate(new Vector3(0.0f, vector2_1.x, 0.0f));
        this.cameraUp -= vector2_1.y;
        this.cameraUp = Mathf.Clamp(this.cameraUp, -80f, 80f);
        StartOfRound.Instance.freeCinematicCameraTurnCompass.transform.localEulerAngles = new Vector3(this.cameraUp, StartOfRound.Instance.freeCinematicCameraTurnCompass.transform.localEulerAngles.y, 0.0f);
      }
      else if (this.IsInspectingItem)
      {
        Vector2 vector2_2 = vector2_1 * 0.01f;
        Vector3 localPosition = this.rightArmProceduralTarget.localPosition;
        localPosition.x = Mathf.Clamp(localPosition.x + vector2_2.x, this.rightArmProceduralTargetBasePosition.x - 0.1f, this.rightArmProceduralTargetBasePosition.x + 0.1f);
        localPosition.y = Mathf.Clamp(localPosition.y + vector2_2.y, this.rightArmProceduralTargetBasePosition.y - 0.3f, this.rightArmProceduralTargetBasePosition.y + 0.3f);
        this.rightArmProceduralTarget.localPosition = new Vector3(localPosition.x, localPosition.y, this.rightArmProceduralTarget.localPosition.z);
      }
      else
      {
        if (this.IsOwner && this.isPlayerDead && (!this.IsServer || this.isHostPlayerObject))
        {
          this.spectateCameraPivot.Rotate(new Vector3(0.0f, vector2_1.x, 0.0f));
          this.cameraUp -= vector2_1.y;
          this.cameraUp = Mathf.Clamp(this.cameraUp, -80f, 80f);
          this.spectateCameraPivot.transform.localEulerAngles = new Vector3(this.cameraUp, this.spectateCameraPivot.transform.localEulerAngles.y, 0.0f);
        }
        if ((!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer)
          return;
        StartOfRound.Instance.playerLookMagnitudeThisFrame = vector2_1.magnitude * Time.deltaTime;
        if (this.inSpecialInteractAnimation && this.isClimbingLadder)
        {
          this.LookWhileClimbingLadder(vector2_1);
          this.SyncFullRotWithClients();
        }
        else
        {
          if ((double) this.smoothLookMultiplier != 25.0)
            this.CalculateSmoothLookingInput(vector2_1);
          else
            this.CalculateNormalLookingInput(vector2_1);
          if (this.isTestingPlayer || this.IsServer && this.playersManager.connectedPlayersAmount < 1)
            return;
          if (this.jetpackControls)
          {
            this.SyncFullRotWithClients();
          }
          else
          {
            if ((double) this.updatePlayerLookInterval <= 0.10000000149011612 || Physics.OverlapSphere(this.transform.position, 35f, this.playerMask).Length == 0)
              return;
            this.updatePlayerLookInterval = 0.0f;
            if ((double) Mathf.Abs((float) ((double) this.oldCameraUp + (double) this.previousYRot - ((double) this.cameraUp + (double) this.thisPlayerBody.eulerAngles.y))) <= 3.0 || this.playersManager.newGameIsLoading)
              return;
            this.UpdatePlayerRotationServerRpc((short) this.cameraUp, (short) this.thisPlayerBody.eulerAngles.y);
            this.oldCameraUp = this.cameraUp;
            this.previousYRot = this.thisPlayerBody.eulerAngles.y;
          }
        }
      }
    }

    private void SyncFullRotWithClients()
    {
      if (!this.jetpackControls && !this.isClimbingLadder || (double) this.updatePlayerLookInterval <= 0.15000000596046448)
        return;
      this.updatePlayerLookInterval = 0.0f;
      this.UpdatePlayerRotationFullServerRpc(this.transform.eulerAngles);
      this.syncFullRotation = this.transform.eulerAngles;
    }

    private void CalculateSmoothLookingInput(Vector2 inputVector)
    {
      if (!this.smoothLookEnabledLastFrame)
      {
        this.smoothLookEnabledLastFrame = true;
        this.smoothLookTurnCompass.rotation = this.gameplayCamera.transform.rotation;
        this.smoothLookTurnCompass.SetParent((Transform) null);
      }
      this.smoothLookTurnCompass.Rotate(new Vector3(0.0f, inputVector.x, 0.0f), UnityEngine.Space.Self);
      this.cameraUp -= inputVector.y;
      this.cameraUp = Mathf.Clamp(this.cameraUp, -80f, 60f);
      this.smoothLookTurnCompass.localEulerAngles = new Vector3(this.cameraUp, this.smoothLookTurnCompass.localEulerAngles.y, this.smoothLookTurnCompass.localEulerAngles.z);
      this.smoothLookTurnCompass.eulerAngles = new Vector3(this.smoothLookTurnCompass.eulerAngles.x, this.smoothLookTurnCompass.eulerAngles.y, this.thisPlayerBody.transform.eulerAngles.z);
      this.thisPlayerBody.eulerAngles = new Vector3(this.thisPlayerBody.eulerAngles.x, Mathf.LerpAngle(this.thisPlayerBody.eulerAngles.y, this.smoothLookTurnCompass.eulerAngles.y, this.smoothLookMultiplier * Time.deltaTime), this.thisPlayerBody.eulerAngles.z);
      this.gameplayCamera.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(this.gameplayCamera.transform.localEulerAngles.x, this.cameraUp, this.smoothLookMultiplier * Time.deltaTime), this.gameplayCamera.transform.localEulerAngles.y, this.gameplayCamera.transform.localEulerAngles.z);
    }

    private void CalculateNormalLookingInput(Vector2 inputVector)
    {
      if (this.smoothLookEnabledLastFrame)
        this.smoothLookEnabledLastFrame = false;
      if (this.inShockingMinigame)
      {
        inputVector.x = Mathf.Clamp(inputVector.x, -15f, 15f);
        inputVector.y = Mathf.Clamp(inputVector.y, -15f, 15f);
        this.turnCompass.Rotate(new Vector3(0.0f, inputVector.x, 0.0f));
      }
      else if (this.jetpackControls)
        this.jetpackTurnCompass.Rotate(new Vector3(0.0f, inputVector.x, 0.0f), UnityEngine.Space.Self);
      else
        this.thisPlayerBody.Rotate(new Vector3(0.0f, inputVector.x, 0.0f), UnityEngine.Space.Self);
      this.cameraUp -= inputVector.y;
      this.cameraUp = Mathf.Clamp(this.cameraUp, -80f, 60f);
      this.gameplayCamera.transform.localEulerAngles = new Vector3(this.cameraUp, this.gameplayCamera.transform.localEulerAngles.y, this.gameplayCamera.transform.localEulerAngles.z);
      this.playerHudUIContainer.Rotate(new Vector3(inputVector.y / 4f, (float) (-(double) inputVector.x / 8.0), 0.0f) * Mathf.Clamp(Time.deltaTime * 15f, 0.02f, 4f));
    }

    private void Look_performed(InputAction.CallbackContext context)
    {
      if (this.quickMenuManager.isMenuOpen)
      {
        if ((double) context.ReadValue<Vector2>().magnitude <= 1.0 / 1000.0)
          return;
        Cursor.visible = true;
      }
      else
      {
        if ((!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer)
          return;
        StartOfRound.Instance.localPlayerUsingController = !InputControlPath.MatchesPrefix("<Mouse>", context.control);
      }
    }

    bool IShockableWithGun.CanBeShocked() => !this.isPlayerDead;

    float IShockableWithGun.GetDifficultyMultiplier() => 1.5f;

    NetworkObject IShockableWithGun.GetNetworkObject() => this.NetworkObject;

    Transform IShockableWithGun.GetShockableTransform() => this.transform;

    Vector3 IShockableWithGun.GetShockablePosition() => this.gameplayCamera.transform.position;

    void IShockableWithGun.ShockWithGun(PlayerControllerB shockedByPlayer)
    {
      ++this.isMovementHindered;
      this.hinderedMultiplier *= 3.5f;
    }

    void IShockableWithGun.StopShockingWithGun()
    {
      this.isMovementHindered = Mathf.Clamp(this.isMovementHindered - 1, 0, 1000);
      this.hinderedMultiplier /= 3.5f;
    }

    public void ForceTurnTowardsTarget()
    {
      if (!this.inSpecialInteractAnimation || !this.inShockingMinigame || !((UnityEngine.Object) this.shockingTarget != (UnityEngine.Object) null))
        return;
      this.targetScreenPos = this.turnCompassCamera.WorldToViewportPoint(this.shockingTarget.position);
      this.shockMinigamePullPosition = this.targetScreenPos.x - 0.5f;
      float num = Mathf.Clamp(Time.deltaTime, 0.0f, 0.1f);
      if ((double) this.targetScreenPos.x > 0.54000002145767212)
      {
        this.turnCompass.Rotate(Vector3.up * 2000f * num * Mathf.Abs(this.shockMinigamePullPosition));
        this.playerBodyAnimator.SetBool("PullingCameraRight", false);
        this.playerBodyAnimator.SetBool("PullingCameraLeft", true);
      }
      else if ((double) this.targetScreenPos.x < 0.46000000834465027)
      {
        this.turnCompass.Rotate(Vector3.up * -2000f * num * Mathf.Abs(this.shockMinigamePullPosition));
        this.playerBodyAnimator.SetBool("PullingCameraLeft", false);
        this.playerBodyAnimator.SetBool("PullingCameraRight", true);
      }
      else
      {
        this.playerBodyAnimator.SetBool("PullingCameraLeft", false);
        this.playerBodyAnimator.SetBool("PullingCameraRight", false);
      }
      this.targetScreenPos = this.gameplayCamera.WorldToViewportPoint(this.shockingTarget.position + Vector3.up * 0.35f);
      if ((double) this.targetScreenPos.y > 0.60000002384185791)
        this.cameraUp = Mathf.Clamp(Mathf.Lerp(this.cameraUp, this.cameraUp - 25f, 25f * num * Mathf.Abs(this.targetScreenPos.y - 0.5f)), -89f, 89f);
      else if ((double) this.targetScreenPos.y < 0.34999999403953552)
        this.cameraUp = Mathf.Clamp(Mathf.Lerp(this.cameraUp, this.cameraUp + 25f, 25f * num * Mathf.Abs(this.targetScreenPos.y - 0.5f)), -89f, 89f);
      this.gameplayCamera.transform.localEulerAngles = new Vector3(this.cameraUp, this.gameplayCamera.transform.localEulerAngles.y, this.gameplayCamera.transform.localEulerAngles.z);
      this.thisPlayerBody.rotation = Quaternion.Lerp(this.thisPlayerBody.rotation, Quaternion.Euler(Vector3.zero with
      {
        y = this.turnCompass.eulerAngles.y
      }), (float) ((double) Time.deltaTime * 20.0 * (1.0 - (double) Mathf.Abs(this.shockMinigamePullPosition))));
    }

    private void LookWhileClimbingLadder(Vector2 lookInput)
    {
      lookInput *= 2f;
      this.ladderCameraHorizontal += lookInput.x;
      this.ladderCameraHorizontal = Mathf.Clamp(this.ladderCameraHorizontal, -60f, 60f);
      this.cameraUp -= lookInput.y;
      this.cameraUp = Mathf.Clamp(this.cameraUp, -60f, 25f);
      this.gameplayCamera.transform.localEulerAngles = new Vector3(this.cameraUp, this.ladderCameraHorizontal, this.gameplayCamera.transform.localEulerAngles.z);
    }

    private void Crouch_performed(InputAction.CallbackContext context)
    {
      if (!context.performed || this.quickMenuManager.isMenuOpen || (!this.IsOwner || !this.isPlayerControlled || this.IsServer && !this.isHostPlayerObject) && !this.isTestingPlayer || this.inSpecialInteractAnimation || !this.thisController.isGrounded || this.isTypingChat || this.isJumping)
        return;
      this.Crouch(!this.isCrouching);
    }

    public void Crouch(bool crouch)
    {
      if (crouch)
      {
        if (this.sourcesCausingSinking > 0 && (double) this.sinkingValue > 0.60000002384185791)
          return;
        this.isCrouching = true;
        StartOfRound.Instance.timeAtMakingLastPersonalMovement = Time.realtimeSinceStartup;
        this.playerBodyAnimator.SetTrigger("startCrouching");
        this.playerBodyAnimator.SetBool("crouching", true);
      }
      else
      {
        if (Physics.Raycast(this.gameplayCamera.transform.position, Vector3.up, out this.hit, 0.8f, this.playersManager.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
          return;
        this.isCrouching = false;
        StartOfRound.Instance.timeAtMakingLastPersonalMovement = Time.realtimeSinceStartup;
        this.playerBodyAnimator.SetBool("crouching", false);
      }
    }

    [ServerRpc]
    private void UpdatePlayerRotationServerRpc(short newRot, short newYRot)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(588787670U, serverRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, newRot);
        BytePacker.WriteValueBitPacked(bufferWriter, newYRot);
        this.__endSendServerRpc(ref bufferWriter, 588787670U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      try
      {
        this.UpdatePlayerRotationClientRpc(newRot, newYRot);
      }
      catch (Exception ex)
      {
        Debug.Log((object) string.Format("Client rpc parameters were likely not correct, so an RPC was skipped: {0}", (object) ex));
      }
    }

    [ClientRpc]
    private void UpdatePlayerRotationClientRpc(short newRot, short newYRot)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(2188611472U, clientRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, newRot);
        BytePacker.WriteValueBitPacked(bufferWriter, newYRot);
        this.__endSendClientRpc(ref bufferWriter, 2188611472U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      ++this.playersManager.gameStats.allPlayerStats[this.playerClientId].turnAmount;
      if (this.IsOwner)
        return;
      this.targetYRot = (float) newYRot;
      this.targetLookRot = (float) newRot;
    }

    [ServerRpc]
    private void UpdatePlayerRotationFullServerRpc(Vector3 playerEulers)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(3789403418U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe(in playerEulers);
        this.__endSendServerRpc(ref bufferWriter, 3789403418U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      try
      {
        this.UpdatePlayerRotationFullClientRpc(playerEulers);
      }
      catch (Exception ex)
      {
        Debug.Log((object) string.Format("Client rpc parameters were likely not correct, so an RPC was skipped: {0}", (object) ex));
      }
    }

    [ClientRpc]
    private void UpdatePlayerRotationFullClientRpc(Vector3 playerEulers)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(2444895710U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe(in playerEulers);
        this.__endSendClientRpc(ref bufferWriter, 2444895710U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      this.syncFullRotation = playerEulers;
    }

    private void UpdatePlayerAnimationsToOtherClients(Vector2 moveInputVector)
    {
      this.updatePlayerAnimationsInterval += Time.deltaTime;
      if (!this.inSpecialInteractAnimation && (double) this.updatePlayerAnimationsInterval <= 0.14000000059604645)
        return;
      this.updatePlayerAnimationsInterval = 0.0f;
      this.currentAnimationSpeed = this.playerBodyAnimator.GetFloat("animationSpeed");
      for (int index = 0; index < this.playerBodyAnimator.layerCount; ++index)
      {
        this.currentAnimationStateHash[index] = this.playerBodyAnimator.GetCurrentAnimatorStateInfo(index).fullPathHash;
        if (this.previousAnimationStateHash[index] != this.currentAnimationStateHash[index])
        {
          this.previousAnimationStateHash[index] = this.currentAnimationStateHash[index];
          this.previousAnimationSpeed = this.currentAnimationSpeed;
          this.UpdatePlayerAnimationServerRpc(this.currentAnimationStateHash[index], this.currentAnimationSpeed);
          return;
        }
      }
      if ((double) this.previousAnimationSpeed == (double) this.currentAnimationSpeed)
        return;
      this.previousAnimationSpeed = this.currentAnimationSpeed;
      this.UpdatePlayerAnimationServerRpc(0, this.currentAnimationSpeed);
    }

    [ServerRpc]
    private void UpdatePlayerAnimationServerRpc(int animationState, float animationSpeed)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(3473255830U, serverRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, animationState);
        bufferWriter.WriteValueSafe<float>(in animationSpeed, new FastBufferWriter.ForPrimitives());
        this.__endSendServerRpc(ref bufferWriter, 3473255830U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      try
      {
        this.UpdatePlayerAnimationClientRpc(animationState, animationSpeed);
      }
      catch (Exception ex)
      {
        Debug.Log((object) string.Format("Client rpc parameters were likely not correct, so an RPC was skipped: {0}", (object) ex));
      }
    }

    [ClientRpc]
    private void UpdatePlayerAnimationClientRpc(int animationState, float animationSpeed)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(3386813972U, clientRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, animationState);
        bufferWriter.WriteValueSafe<float>(in animationSpeed, new FastBufferWriter.ForPrimitives());
        this.__endSendClientRpc(ref bufferWriter, 3386813972U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      if ((double) this.playerBodyAnimator.GetFloat(nameof (animationSpeed)) != (double) animationSpeed)
        this.playerBodyAnimator.SetFloat(nameof (animationSpeed), animationSpeed);
      if (animationState == 0 || this.playerBodyAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == animationState)
        return;
      for (int layerIndex = 0; layerIndex < this.playerBodyAnimator.layerCount; ++layerIndex)
      {
        if (this.playerBodyAnimator.HasState(layerIndex, animationState))
        {
          this.playerBodyAnimator.CrossFadeInFixedTime(animationState, 0.1f);
          break;
        }
      }
    }

    public void UpdateSpecialAnimationValue(
      bool specialAnimation,
      short yVal = 0,
      float timed = 0.0f,
      bool climbingLadder = false)
    {
      this.IsInSpecialAnimationServerRpc(specialAnimation, timed, climbingLadder);
      this.ResetZAndXRotation();
      if (!specialAnimation)
        return;
      this.UpdatePlayerRotationServerRpc((short) 0, yVal);
    }

    public void ResetZAndXRotation()
    {
      this.thisPlayerBody.localEulerAngles = this.thisPlayerBody.localEulerAngles with
      {
        x = 0.0f,
        z = 0.0f
      };
    }

    [ServerRpc]
    private void IsInSpecialAnimationServerRpc(
      bool specialAnimation,
      float timed = 0.0f,
      bool climbingLadder = false)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(2480354441U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<bool>(in specialAnimation, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<float>(in timed, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<bool>(in climbingLadder, new FastBufferWriter.ForPrimitives());
        this.__endSendServerRpc(ref bufferWriter, 2480354441U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      try
      {
        this.IsInSpecialAnimationClientRpc(specialAnimation, timed, climbingLadder);
      }
      catch (Exception ex)
      {
        Debug.Log((object) string.Format("Client rpc parameters were likely not correct, so an RPC was skipped: {0}", (object) ex));
      }
    }

    [ClientRpc]
    private void IsInSpecialAnimationClientRpc(
      bool specialAnimation,
      float timed = 0.0f,
      bool climbingLadder = false)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(2281795056U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<bool>(in specialAnimation, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<float>(in timed, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<bool>(in climbingLadder, new FastBufferWriter.ForPrimitives());
        this.__endSendClientRpc(ref bufferWriter, 2281795056U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      Debug.Log((object) "Setting animation on client");
      this.inSpecialInteractAnimation = specialAnimation;
      this.isClimbingLadder = climbingLadder;
      if (!specialAnimation && !climbingLadder)
        this.ResetZAndXRotation();
      if ((double) timed <= 0.0)
        return;
      if (this.timeSpecialAnimationCoroutine != null)
        this.StopCoroutine(this.timeSpecialAnimationCoroutine);
      this.timeSpecialAnimationCoroutine = this.StartCoroutine(this.timeSpecialAnimation(timed));
    }

    private IEnumerator timeSpecialAnimation(float time)
    {
      yield return (object) new WaitForSeconds(time);
      this.inSpecialInteractAnimation = false;
      this.timeSpecialAnimationCoroutine = (Coroutine) null;
    }

    public void GetCurrentMaterialStandingOn()
    {
      this.interactRay = new Ray(this.thisPlayerBody.position + Vector3.up, -Vector3.up);
      if (!Physics.Raycast(this.interactRay, out this.hit, 6f, StartOfRound.Instance.walkableSurfacesMask, QueryTriggerInteraction.Ignore) || this.hit.collider.CompareTag(StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].surfaceTag))
        return;
      for (int index = 0; index < StartOfRound.Instance.footstepSurfaces.Length; ++index)
      {
        if (this.hit.collider.CompareTag(StartOfRound.Instance.footstepSurfaces[index].surfaceTag))
        {
          this.currentFootstepSurfaceIndex = index;
          break;
        }
      }
    }

    public void PlayFootstepSound()
    {
      this.GetCurrentMaterialStandingOn();
      int index = UnityEngine.Random.Range(0, StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].clips.Length);
      if (index == this.previousFootstepClip)
        index = (index + 1) % StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].clips.Length;
      this.movementAudio.pitch = UnityEngine.Random.Range(0.93f, 1.07f);
      bool flag = !this.IsOwner ? this.playerBodyAnimator.GetCurrentAnimatorStateInfo(0).IsTag("Sprinting") : this.isSprinting;
      float num = 0.9f;
      if (!flag)
        num = 0.6f;
      this.movementAudio.PlayOneShot(StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].clips[index], num);
      this.previousFootstepClip = index;
      WalkieTalkie.TransmitOneShotAudio(this.movementAudio, StartOfRound.Instance.footstepSurfaces[this.currentFootstepSurfaceIndex].clips[index], num);
    }

    public void PlayFootstepServer()
    {
      if (this.isClimbingLadder || this.inSpecialInteractAnimation || this.IsOwner || !this.isPlayerControlled)
        return;
      bool noiseIsInsideClosedShip = this.isInHangarShipRoom && this.playersManager.hangarDoorsClosed;
      if (this.isSprinting)
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, 22f, 0.6f, noiseIsInsideClosedShip: noiseIsInsideClosedShip, noiseID: 7);
      else
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, 17f, 0.4f, noiseIsInsideClosedShip: noiseIsInsideClosedShip, noiseID: 7);
      this.PlayFootstepSound();
    }

    public void PlayFootstepLocal()
    {
      if (this.isClimbingLadder || this.inSpecialInteractAnimation || !this.isTestingPlayer && (!this.IsOwner || !this.isPlayerControlled))
        return;
      bool noiseIsInsideClosedShip = this.isInHangarShipRoom && this.playersManager.hangarDoorsClosed;
      if (this.isSprinting)
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, 22f, 0.6f, noiseIsInsideClosedShip: noiseIsInsideClosedShip, noiseID: 6);
      else
        RoundManager.Instance.PlayAudibleNoise(this.transform.position, 17f, 0.4f, noiseIsInsideClosedShip: noiseIsInsideClosedShip, noiseID: 6);
      this.PlayFootstepSound();
    }

    [ServerRpc]
    private void UpdatePlayerPositionServerRpc(
      Vector3 newPos,
      bool inElevator,
      bool exhausted,
      bool isPlayerGrounded)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(2581007949U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe(in newPos);
        bufferWriter.WriteValueSafe<bool>(in inElevator, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<bool>(in exhausted, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<bool>(in isPlayerGrounded, new FastBufferWriter.ForPrimitives());
        this.__endSendServerRpc(ref bufferWriter, 2581007949U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      try
      {
        this.UpdatePlayerPositionClientRpc(newPos, inElevator, exhausted, isPlayerGrounded);
      }
      catch (Exception ex)
      {
        Debug.Log((object) string.Format("Caught an error when sending player position RPC; likely a player disconnected to cause this. Error: {0}", (object) ex));
      }
    }

    [ClientRpc]
    private void UpdatePlayerPositionClientRpc(
      Vector3 newPos,
      bool inElevator,
      bool exhausted,
      bool isPlayerGrounded)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(153310197U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe(in newPos);
        bufferWriter.WriteValueSafe<bool>(in inElevator, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<bool>(in exhausted, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe<bool>(in isPlayerGrounded, new FastBufferWriter.ForPrimitives());
        this.__endSendClientRpc(ref bufferWriter, 153310197U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      ++this.playersManager.gameStats.allPlayerStats[this.playerClientId].stepsTaken;
      ++this.playersManager.gameStats.allStepsTaken;
      bool leaveFootprint = this.currentFootstepSurfaceIndex == 8 && ((!this.IsOwner ? 0 : (this.thisController.isGrounded ? 1 : 0)) | (isPlayerGrounded ? 1 : 0)) != 0;
      if (this.bleedingHeavily | leaveFootprint)
        this.DropBlood(Vector3.down, this.bleedingHeavily, leaveFootprint);
      if (this.IsOwner)
        return;
      if (!inElevator)
        this.isInHangarShipRoom = false;
      this.isExhausted = exhausted;
      this.isInElevator = inElevator;
      this.oldPlayerPosition = this.serverPlayerPosition;
      if (!this.disableSyncInAnimation)
        this.serverPlayerPosition = newPos;
      if (this.isInElevator)
      {
        if (!this.wasInElevatorLastFrame)
        {
          this.wasInElevatorLastFrame = true;
          this.transform.SetParent(this.playersManager.elevatorTransform);
        }
      }
      else if (this.wasInElevatorLastFrame)
      {
        this.wasInElevatorLastFrame = false;
        this.transform.SetParent(this.playersManager.playersContainer);
        this.transform.eulerAngles = new Vector3(0.0f, this.transform.eulerAngles.y, 0.0f);
      }
      this.timeSincePlayerMoving = 0.0f;
    }

    [ServerRpc]
    public void LandFromJumpServerRpc(bool fallHard)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(3332990272U, serverRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<bool>(in fallHard, new FastBufferWriter.ForPrimitives());
        this.__endSendServerRpc(ref bufferWriter, 3332990272U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.LandFromJumpClientRpc(fallHard);
    }

    [ClientRpc]
    public void LandFromJumpClientRpc(bool fallHard)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(983565270U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe<bool>(in fallHard, new FastBufferWriter.ForPrimitives());
        this.__endSendClientRpc(ref bufferWriter, 983565270U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      if (fallHard)
        this.movementAudio.PlayOneShot(StartOfRound.Instance.playerHitGroundHard, 1f);
      else
        this.movementAudio.PlayOneShot(StartOfRound.Instance.playerHitGroundSoft, 0.7f);
    }

    public void LimpAnimationSpeed()
    {
      if (!this.IsOwner)
        return;
      this.limpMultiplier = 0.75f;
    }

    public void SpawnPlayerAnimation()
    {
      this.UpdateSpecialAnimationValue(true);
      this.inSpecialInteractAnimation = true;
      this.playerBodyAnimator.ResetTrigger("SpawnPlayer");
      this.playerBodyAnimator.SetTrigger("SpawnPlayer");
      this.StartCoroutine(this.spawnPlayerAnimTimer());
    }

    private IEnumerator spawnPlayerAnimTimer()
    {
      yield return (object) new WaitForSeconds(3f);
      this.inSpecialInteractAnimation = false;
      this.UpdateSpecialAnimationValue(false);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendNewPlayerValuesServerRpc(ulong newPlayerSteamId)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(2504133785U, serverRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, newPlayerSteamId);
        this.__endSendServerRpc(ref bufferWriter, 2504133785U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      if (!GameNetworkManager.Instance.disableSteam && GameNetworkManager.Instance.currentLobby.HasValue)
      {
        if (!GameNetworkManager.Instance.steamIdsInLobby.Contains((SteamId) newPlayerSteamId))
        {
          NetworkManager.Singleton.DisconnectClient(this.actualClientId);
          return;
        }
        if (StartOfRound.Instance.KickedClientIds.Contains(newPlayerSteamId))
        {
          NetworkManager.Singleton.DisconnectClient(this.actualClientId);
          return;
        }
      }
      List<ulong> ulongList = new List<ulong>();
      for (int index = 0; index < 4; ++index)
      {
        if (index == (int) this.playerClientId)
          ulongList.Add(newPlayerSteamId);
        else
          ulongList.Add(this.playersManager.allPlayerScripts[index].playerSteamId);
      }
      this.SendNewPlayerValuesClientRpc(ulongList.ToArray());
    }

    [ClientRpc]
    private void SendNewPlayerValuesClientRpc(ulong[] playerSteamIds)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(956616685U, clientRpcParams, RpcDelivery.Reliable);
        bool flag = playerSteamIds != null;
        bufferWriter.WriteValueSafe<bool>(in flag, new FastBufferWriter.ForPrimitives());
        if (flag)
          bufferWriter.WriteValueSafe<ulong>(playerSteamIds, new FastBufferWriter.ForPrimitives());
        this.__endSendClientRpc(ref bufferWriter, 956616685U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      for (int index = 0; index < playerSteamIds.Length; ++index)
      {
        if (this.playersManager.allPlayerScripts[index].isPlayerControlled || this.playersManager.allPlayerScripts[index].isPlayerDead)
        {
          string str = Regex.Replace(this.NoPunctuation(new Friend((SteamId) playerSteamIds[index]).Name), "[^\\w\\._]", "");
          if (str == string.Empty || str.Length == 0)
            str = "Nameless";
          else if (str.Length <= 2)
            str += "0";
          this.playersManager.allPlayerScripts[index].playerSteamId = playerSteamIds[index];
          this.playersManager.allPlayerScripts[index].playerUsername = str;
          this.playersManager.allPlayerScripts[index].usernameBillboardText.text = str;
          string playerName = str;
          int duplicateNamesInLobby = this.GetNumberOfDuplicateNamesInLobby();
          if (duplicateNamesInLobby > 0)
            playerName = string.Format("{0}{1}", (object) str, (object) duplicateNamesInLobby);
          this.quickMenuManager.AddUserToPlayerList(playerSteamIds[index], playerName, index);
          StartOfRound.Instance.mapScreen.radarTargets[index].name = playerName;
        }
      }
      StartOfRound.Instance.StartTrackingAllPlayerVoices();
      if (!((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null) || !((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null))
        return;
      GameNetworkManager.Instance.localPlayerController.updatePositionForNewlyJoinedClient = true;
    }

    private int GetNumberOfDuplicateNamesInLobby()
    {
      int duplicateNamesInLobby = 0;
      for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
      {
        if ((StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled || this.playersManager.allPlayerScripts[index].isPlayerDead) && !((UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[index] == (UnityEngine.Object) this) && StartOfRound.Instance.allPlayerScripts[index].playerUsername == this.playerUsername)
          ++duplicateNamesInLobby;
      }
      for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
      {
        if ((StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled || this.playersManager.allPlayerScripts[index].isPlayerDead) && !((UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[index] == (UnityEngine.Object) this) && StartOfRound.Instance.allPlayerScripts[index].playerUsername == string.Format("{0}{1}", (object) StartOfRound.Instance.allPlayerScripts[index].playerUsername, (object) duplicateNamesInLobby))
          ++duplicateNamesInLobby;
      }
      return duplicateNamesInLobby;
    }

    private string NoPunctuation(string input)
    {
      return new string(input.Where<char>((Func<char, bool>) (c => char.IsLetter(c))).ToArray<char>());
    }

    public void ConnectClientToPlayerObject()
    {
      if (!this.isTestingPlayer)
      {
        this.actualClientId = NetworkManager.Singleton.LocalClientId;
        this.playersManager.thisClientPlayerId = (int) this.playerClientId;
      }
      if ((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null)
        GameNetworkManager.Instance.localPlayerController = this;
      this.playersManager.localPlayerController = this;
      for (int index = 0; index < this.playersManager.allPlayerObjects.Length; ++index)
      {
        PlayerControllerB component = this.playersManager.allPlayerObjects[index].GetComponent<PlayerControllerB>();
        if (!component.isPlayerControlled && !component.isTestingPlayer)
          component.TeleportPlayer(this.playersManager.notSpawnedPosition.position);
        if ((UnityEngine.Object) component != (UnityEngine.Object) this.playersManager.localPlayerController)
          this.playersManager.OtherClients.Add(component);
      }
      this.playersManager.localClientHasControl = true;
      if ((UnityEngine.Object) this.playerBodyAnimator.runtimeAnimatorController != (UnityEngine.Object) this.playersManager.localClientAnimatorController)
        this.playerBodyAnimator.runtimeAnimatorController = this.playersManager.localClientAnimatorController;
      if (this.isTestingPlayer)
        return;
      if (!GameNetworkManager.Instance.disableSteam)
      {
        this.playerUsername = GameNetworkManager.Instance.username;
        this.SendNewPlayerValuesServerRpc((ulong) SteamClient.SteamId);
      }
      else if (this.IsServer)
        UnityEngine.Object.FindObjectOfType<QuickMenuManager>().AddUserToPlayerList(0UL, "Player #0", 0);
      HUDManager.Instance.AddTextToChatOnServer(this.playerUsername + " joined the ship.");
      this.usernameAlpha.alpha = 0.0f;
      this.usernameBillboardText.enabled = false;
    }

    private void ChangeAudioListenerToObject(GameObject addToObject)
    {
      this.activeAudioListener.transform.SetParent(addToObject.transform);
      this.activeAudioListener.transform.localEulerAngles = Vector3.zero;
      this.activeAudioListener.transform.localPosition = Vector3.zero;
      StartOfRound.Instance.audioListener = this.activeAudioListener;
    }

    private void PlayerHitGroundEffects()
    {
      this.GetCurrentMaterialStandingOn();
      if ((double) this.fallValue < -9.0)
      {
        if ((double) this.fallValue < -16.0)
        {
          this.movementAudio.PlayOneShot(StartOfRound.Instance.playerHitGroundHard, 1f);
          WalkieTalkie.TransmitOneShotAudio(this.movementAudio, StartOfRound.Instance.playerHitGroundHard);
        }
        else if ((double) this.fallValue < -2.0)
          this.movementAudio.PlayOneShot(StartOfRound.Instance.playerHitGroundSoft, 1f);
        this.LandFromJumpServerRpc((double) this.fallValue < -16.0);
      }
      if (this.takingFallDamage && !this.jetpackControls && !this.disablingJetpackControls && !this.isSpeedCheating)
      {
        if ((double) this.fallValueUncapped < -50.0)
          this.DamagePlayer(100, causeOfDeath: CauseOfDeath.Gravity);
        else
          this.DamagePlayer(40, causeOfDeath: CauseOfDeath.Gravity);
      }
      if ((double) this.fallValue >= -16.0)
        return;
      RoundManager.Instance.PlayAudibleNoise(this.transform.position, 7f);
    }

    private void CalculateGroundNormal()
    {
      if (Physics.Raycast(this.transform.position + Vector3.up * 0.2f, -Vector3.up, out this.hit, 6f, 268438273, QueryTriggerInteraction.Ignore))
        this.playerGroundNormal = this.hit.normal;
      else
        this.playerGroundNormal = Vector3.up;
    }

    private bool IsPlayerNearGround()
    {
      this.interactRay = new Ray(this.transform.position, Vector3.down);
      return Physics.Raycast(this.interactRay, 0.15f, StartOfRound.Instance.allPlayersCollideWithMask, QueryTriggerInteraction.Ignore);
    }

    [ServerRpc]
    public void DisableJetpackModeServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(3237016509U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 3237016509U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.DisableJetpackModeClientRpc();
    }

    [ClientRpc]
    public void DisableJetpackModeClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(1367193869U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 1367193869U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      this.DisableJetpackControlsLocally();
    }

    public void DisableJetpackControlsLocally()
    {
      this.jetpackControls = false;
      this.thisController.radius = 0.4f;
      this.jetpackTurnCompass.rotation = this.transform.rotation;
      this.startedJetpackControls = false;
      this.disablingJetpackControls = false;
    }

    private void Update()
    {
      if (this.IsOwner && this.isPlayerControlled && (!this.IsServer || this.isHostPlayerObject) || this.isTestingPlayer)
      {
        if (this.isCameraDisabled)
        {
          this.isCameraDisabled = false;
          Debug.Log((object) ("Taking control of player " + this.gameObject.name + " and enabling camera!"));
          StartOfRound.Instance.SwitchCamera(this.gameplayCamera);
          this.thisPlayerModel.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
          this.mapRadarDirectionIndicator.enabled = true;
          this.thisPlayerModelArms.enabled = true;
          this.playerScreen.enabled = true;
          Cursor.lockState = CursorLockMode.Locked;
          Cursor.visible = false;
          this.gameObject.GetComponent<CharacterController>().enabled = true;
          this.activeAudioReverbFilter = this.activeAudioListener.GetComponent<AudioReverbFilter>();
          this.activeAudioReverbFilter.enabled = true;
          this.ChangeAudioListenerToObject(this.gameplayCamera.gameObject);
          if ((UnityEngine.Object) this.playerBodyAnimator.runtimeAnimatorController != (UnityEngine.Object) this.playersManager.localClientAnimatorController)
            this.playerBodyAnimator.runtimeAnimatorController = this.playersManager.localClientAnimatorController;
          if (this.justConnected)
          {
            this.justConnected = false;
            this.ConnectClientToPlayerObject();
          }
          this.SpawnPlayerAnimation();
          Debug.Log((object) ("!!!! ENABLING CAMERA FOR PLAYER: " + this.gameObject.name));
          Debug.Log((object) string.Format("!!!! connectedPlayersAmount: {0}", (object) this.playersManager.connectedPlayersAmount));
        }
        this.hasBegunSpectating = false;
        this.playerHudUIContainer.rotation = Quaternion.Lerp(this.playerHudUIContainer.rotation, this.playerHudBaseRotation.rotation, 24f * Time.deltaTime);
        this.SetNightVisionEnabled(false);
        if (!this.inSpecialInteractAnimation || this.inShockingMinigame)
        {
          if (!this.thisController.isGrounded)
          {
            if (this.jetpackControls && !this.disablingJetpackControls)
            {
              this.fallValue = Mathf.MoveTowards(this.fallValue, -8f, 7f * Time.deltaTime);
              this.fallValueUncapped = -8f;
            }
            else
            {
              this.fallValue = Mathf.Clamp(this.fallValue - 38f * Time.deltaTime, -150f, this.jumpForce);
              this.fallValueUncapped -= 38f * Time.deltaTime;
            }
            if (!this.isJumping && !this.isFallingFromJump)
            {
              if (!this.isFallingNoJump)
              {
                this.isFallingNoJump = true;
                this.fallValue = -7f;
              }
              else if ((double) this.fallValue < -20.0)
              {
                this.isCrouching = false;
                this.playerBodyAnimator.SetBool("crouching", false);
                this.playerBodyAnimator.SetBool("FallNoJump", true);
              }
            }
            if ((double) this.fallValueUncapped < -40.0)
              this.takingFallDamage = true;
          }
          else
          {
            this.movementHinderedPrev = this.isMovementHindered;
            if (!this.isJumping)
            {
              if (this.isFallingNoJump && !this.jetpackControls)
              {
                this.isFallingNoJump = false;
                if (!this.isCrouching && (double) this.fallValue < -9.0)
                  this.playerBodyAnimator.SetTrigger("ShortFallLanding");
                this.PlayerHitGroundEffects();
              }
              this.fallValue = -7f;
              this.fallValueUncapped = -7f;
            }
            this.playerBodyAnimator.SetBool("FallNoJump", false);
          }
        }
        this.ForceTurnTowardsTarget();
        if (this.inTerminalMenu)
          this.targetFOV = 60f;
        else if (this.IsInspectingItem)
        {
          this.rightArmProceduralRig.weight = Mathf.Lerp(this.rightArmProceduralRig.weight, 1f, 25f * Time.deltaTime);
          this.targetFOV = 46f;
        }
        else
        {
          this.rightArmProceduralRig.weight = Mathf.Lerp(this.rightArmProceduralRig.weight, 0.0f, 25f * Time.deltaTime);
          this.targetFOV = !this.isSprinting ? 66f : 68f;
        }
        this.gameplayCamera.fieldOfView = Mathf.Lerp(this.gameplayCamera.fieldOfView, this.targetFOV, 6f * Time.deltaTime);
        this.moveInputVector = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Move", false).ReadValue<Vector2>();
        float num1 = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Sprint", false).ReadValue<float>();
        if (this.quickMenuManager.isMenuOpen || this.isTypingChat || this.inSpecialInteractAnimation && !this.isClimbingLadder && !this.inShockingMinigame)
          this.moveInputVector = Vector2.zero;
        this.SetFaceUnderwaterFilters();
        if (this.isWalking)
        {
          if (this.isFreeCamera || (double) this.moveInputVector.sqrMagnitude <= 0.18999999761581421 || this.inSpecialInteractAnimation && !this.isClimbingLadder && !this.inShockingMinigame)
          {
            this.isWalking = false;
            this.isSprinting = false;
            this.playerBodyAnimator.SetBool("Walking", false);
            this.playerBodyAnimator.SetBool("Sprinting", false);
            this.playerBodyAnimator.SetBool("Sideways", false);
          }
          else if ((double) num1 > 0.30000001192092896 && this.movementHinderedPrev <= 0 && !this.criticallyInjured && (double) this.sprintMeter > 0.10000000149011612)
          {
            if (!this.isSprinting && (double) this.sprintMeter < 0.30000001192092896)
            {
              if (!this.isExhausted)
                this.isExhausted = true;
            }
            else
            {
              if (this.isCrouching)
                this.Crouch(false);
              this.isSprinting = true;
              this.playerBodyAnimator.SetBool("Sprinting", true);
            }
          }
          else
          {
            this.isSprinting = false;
            if ((double) this.sprintMeter < 0.10000000149011612)
              this.isExhausted = true;
            this.playerBodyAnimator.SetBool("Sprinting", false);
          }
          this.sprintMultiplier = !this.isSprinting ? Mathf.Lerp(this.sprintMultiplier, 1f, 10f * Time.deltaTime) : Mathf.Lerp(this.sprintMultiplier, 2.25f, Time.deltaTime * 1f);
          if ((double) this.moveInputVector.y < 0.20000000298023224 && (double) this.moveInputVector.y > -0.20000000298023224 && !this.inSpecialInteractAnimation)
          {
            this.playerBodyAnimator.SetBool("Sideways", true);
            this.isSidling = true;
          }
          else
          {
            this.playerBodyAnimator.SetBool("Sideways", false);
            this.isSidling = false;
          }
          if (this.enteringSpecialAnimation)
            this.playerBodyAnimator.SetFloat("animationSpeed", 1f);
          else if ((double) this.moveInputVector.y < 0.5 && (double) this.moveInputVector.x < 0.5)
          {
            this.playerBodyAnimator.SetFloat("animationSpeed", -1f);
            this.movingForward = false;
          }
          else
          {
            this.playerBodyAnimator.SetFloat("animationSpeed", 1f);
            this.movingForward = true;
          }
        }
        else
        {
          if (this.enteringSpecialAnimation)
            this.playerBodyAnimator.SetFloat("animationSpeed", 1f);
          else if (this.isClimbingLadder)
            this.playerBodyAnimator.SetFloat("animationSpeed", 0.0f);
          if (!this.isFreeCamera && (double) this.moveInputVector.sqrMagnitude >= 0.18999999761581421 && (!this.inSpecialInteractAnimation || this.isClimbingLadder || this.inShockingMinigame))
          {
            this.isWalking = true;
            this.playerBodyAnimator.SetBool("Walking", true);
          }
        }
        if (this.performingEmote && !this.CheckConditionsForEmote())
        {
          this.performingEmote = false;
          this.StopPerformingEmoteServerRpc();
          this.timeSinceStartingEmote = 0.0f;
        }
        this.timeSinceStartingEmote += Time.deltaTime;
        this.playerBodyAnimator.SetBool("hinderedMovement", this.isMovementHindered > 0);
        if (this.sourcesCausingSinking == 0)
        {
          if (this.isSinking)
          {
            this.isSinking = false;
            this.StopSinkingServerRpc();
          }
        }
        else
        {
          if (this.isSinking)
          {
            this.GetCurrentMaterialStandingOn();
            if (!this.CheckConditionsForSinkingInQuicksand())
            {
              this.isSinking = false;
              this.StopSinkingServerRpc();
            }
          }
          else if (!this.isSinking && this.CheckConditionsForSinkingInQuicksand())
          {
            this.isSinking = true;
            this.StartSinkingServerRpc(this.sinkingSpeedMultiplier, this.statusEffectAudioIndex);
          }
          if ((double) this.sinkingValue >= 1.0)
            this.KillPlayer(Vector3.zero, false, CauseOfDeath.Suffocation);
          else if ((double) this.sinkingValue > 0.5)
            this.Crouch(false);
        }
        if (this.isCrouching)
        {
          this.timeSinceCrouching = 0.0f;
          this.thisController.center = Vector3.Lerp(this.thisController.center, new Vector3(this.thisController.center.x, 0.72f, this.thisController.center.z), 8f * Time.deltaTime);
          this.thisController.height = Mathf.Lerp(this.thisController.height, 1.5f, 8f * Time.deltaTime);
        }
        else
        {
          this.timeSinceCrouching += Time.deltaTime;
          this.thisController.center = Vector3.Lerp(this.thisController.center, new Vector3(this.thisController.center.x, 1.28f, this.thisController.center.z), 8f * Time.deltaTime);
          this.thisController.height = Mathf.Lerp(this.thisController.height, 2.5f, 8f * Time.deltaTime);
        }
        if (this.isFreeCamera)
        {
          float num2 = this.movementSpeed / 1.75f;
          if ((double) num1 > 0.5)
            num2 *= 5f;
          this.playersManager.freeCinematicCameraTurnCompass.transform.position += (this.playersManager.freeCinematicCameraTurnCompass.transform.right * this.moveInputVector.x + this.playersManager.freeCinematicCameraTurnCompass.transform.forward * this.moveInputVector.y) * num2 * Time.deltaTime;
          StartOfRound.Instance.freeCinematicCamera.transform.position = Vector3.Lerp(StartOfRound.Instance.freeCinematicCamera.transform.position, StartOfRound.Instance.freeCinematicCameraTurnCompass.transform.position, 3f * Time.deltaTime);
          StartOfRound.Instance.freeCinematicCamera.transform.rotation = Quaternion.Slerp(StartOfRound.Instance.freeCinematicCamera.transform.rotation, StartOfRound.Instance.freeCinematicCameraTurnCompass.rotation, 3f * Time.deltaTime);
        }
        if (this.jetpackControls)
        {
          if (this.disablingJetpackControls && this.thisController.isGrounded)
          {
            this.DisableJetpackControlsLocally();
            this.DisableJetpackModeServerRpc();
          }
          else if (!this.thisController.isGrounded)
          {
            if (!this.startedJetpackControls)
            {
              this.startedJetpackControls = true;
              this.jetpackTurnCompass.rotation = this.transform.rotation;
            }
            this.thisController.radius = Mathf.Lerp(this.thisController.radius, 1.25f, 10f * Time.deltaTime);
            this.jetpackTurnCompass.Rotate(new Vector3(0.0f, 0.0f, -this.moveInputVector.x) * (180f * Time.deltaTime), UnityEngine.Space.Self);
            this.jetpackTurnCompass.Rotate(new Vector3(this.moveInputVector.y, 0.0f, 0.0f) * (180f * Time.deltaTime), UnityEngine.Space.Self);
            this.transform.rotation = Quaternion.Slerp(this.transform.rotation, this.jetpackTurnCompass.rotation, 8f * Time.deltaTime);
          }
        }
        else if (!this.isClimbingLadder)
        {
          Vector3 localEulerAngles = this.transform.localEulerAngles;
          localEulerAngles.x = Mathf.LerpAngle(localEulerAngles.x, 0.0f, 15f * Time.deltaTime);
          localEulerAngles.z = Mathf.LerpAngle(localEulerAngles.z, 0.0f, 15f * Time.deltaTime);
          this.transform.localEulerAngles = localEulerAngles;
        }
        if (!this.inSpecialInteractAnimation || this.inShockingMinigame || StartOfRound.Instance.suckingPlayersOutOfShip)
        {
          if (this.isFreeCamera)
            this.moveInputVector = Vector2.zero;
          this.CalculateGroundNormal();
          float num3 = this.movementSpeed / this.carryWeight;
          if ((double) this.sinkingValue > 0.73000001907348633)
          {
            num3 = 0.0f;
          }
          else
          {
            if (this.isCrouching)
              num3 /= 1.5f;
            else if (this.criticallyInjured && !this.isCrouching)
              num3 *= this.limpMultiplier;
            if (this.isSpeedCheating)
              num3 *= 15f;
            if (this.movementHinderedPrev > 0)
              num3 /= 2f * this.hinderedMultiplier;
            if ((double) this.drunkness > 0.0)
              num3 *= (float) ((double) StartOfRound.Instance.drunknessSpeedEffect.Evaluate(this.drunkness) / 5.0 + 1.0);
            if (!this.isCrouching && (double) this.timeSinceCrouching < 1.0)
              num3 *= Mathf.Min(this.timeSinceCrouching + 0.4f, 1f);
          }
          if (this.isTypingChat || this.jetpackControls && !this.thisController.isGrounded || StartOfRound.Instance.suckingPlayersOutOfShip)
            this.moveInputVector = Vector2.zero;
          Vector3 vector3_1 = new Vector3(0.0f, 0.0f, 0.0f);
          int num4 = Physics.OverlapSphereNonAlloc(this.transform.position, 0.65f, this.nearByPlayers, StartOfRound.Instance.playersMask);
          for (int index = 0; index < num4; ++index)
            vector3_1 += Vector3.Normalize((this.transform.position - this.nearByPlayers[index].transform.position) * 100f) * 1.2f;
          int num5 = Physics.OverlapSphereNonAlloc(this.transform.position, 1.25f, this.nearByPlayers, 524288);
          for (int index = 0; index < num5; ++index)
          {
            EnemyAICollisionDetect component = this.nearByPlayers[index].gameObject.GetComponent<EnemyAICollisionDetect>();
            if ((UnityEngine.Object) component != (UnityEngine.Object) null && !component.mainScript.isEnemyDead)
              vector3_1 += Vector3.Normalize((this.transform.position - this.nearByPlayers[index].transform.position) * 100f) * 0.16f;
          }
          this.walkForce = Vector3.MoveTowards(this.walkForce, this.transform.right * this.moveInputVector.x + this.transform.forward * this.moveInputVector.y, (this.isFallingFromJump || this.isFallingNoJump ? 1.33f : ((double) this.drunkness <= 0.30000001192092896 ? (this.isCrouching || (double) this.timeSinceCrouching >= 0.20000000298023224 ? (!this.isSprinting ? 10f / this.carryWeight : (float) (5.0 / ((double) this.carryWeight * 1.5))) : 15f) : Mathf.Clamp(Mathf.Abs(this.drunkness - 2.25f), 0.3f, 2.5f))) * Time.deltaTime);
          Vector3 vector3_2 = this.walkForce * num3 * this.sprintMultiplier + new Vector3(0.0f, this.fallValue, 0.0f) + vector3_1 + this.externalForces;
          this.externalForces = Vector3.zero;
          if (this.isPlayerSliding && this.thisController.isGrounded)
          {
            this.playerSlidingTimer += Time.deltaTime;
            if ((double) this.slideFriction > (double) this.maxSlideFriction)
              this.slideFriction -= 35f * Time.deltaTime;
            vector3_2 = new Vector3(vector3_2.x + (float) ((1.0 - (double) this.playerGroundNormal.y) * (double) this.playerGroundNormal.x * (1.0 - (double) this.slideFriction)), vector3_2.y, vector3_2.z + (float) ((1.0 - (double) this.playerGroundNormal.y) * (double) this.playerGroundNormal.z * (1.0 - (double) this.slideFriction)));
          }
          else
          {
            this.playerSlidingTimer = 0.0f;
            this.slideFriction = 0.0f;
          }
          float magnitude1 = this.thisController.velocity.magnitude;
          int num6 = (int) this.thisController.Move(vector3_2 * Time.deltaTime);
          if (!this.teleportingThisFrame && this.teleportedLastFrame)
            this.teleportedLastFrame = false;
          if (this.jetpackControls || this.disablingJetpackControls)
          {
            if (!this.teleportingThisFrame && !this.inSpecialInteractAnimation && !this.enteringSpecialAnimation && !this.isClimbingLadder && (double) StartOfRound.Instance.timeSinceRoundStarted > 1.0)
            {
              float magnitude2 = this.thisController.velocity.magnitude;
              if ((double) this.getAverageVelocityInterval <= 0.0)
              {
                this.getAverageVelocityInterval = 0.04f;
                ++this.velocityAverageCount;
                if (this.velocityAverageCount > this.velocityMovingAverageLength)
                {
                  this.averageVelocity += (magnitude2 - this.averageVelocity) / (float) (this.velocityMovingAverageLength + 1);
                }
                else
                {
                  this.averageVelocity += magnitude2;
                  if (this.velocityAverageCount == this.velocityMovingAverageLength)
                    this.averageVelocity /= (float) this.velocityAverageCount;
                }
              }
              else
                this.getAverageVelocityInterval -= Time.deltaTime;
              float num7 = this.averageVelocity - (float) (((double) magnitude2 + (double) magnitude1) / 2.0);
              this.minVelocityToTakeDamage = 15f;
              if (this.jetpackControls)
              {
                float num8 = Vector3.Angle(Vector3.up, this.transform.up);
                if ((double) num8 > 65.0)
                  this.minVelocityToTakeDamage = 10f;
                else if ((double) num8 > 47.0)
                  this.minVelocityToTakeDamage = 12.5f;
              }
              if ((double) this.timeSinceTakingGravityDamage > 1.0 && (double) num7 > (double) this.minVelocityToTakeDamage)
              {
                if (Physics.CheckSphere(this.gameplayCamera.transform.position, 3f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
                {
                  Physics.OverlapSphere(this.gameplayCamera.transform.position, 3f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore);
                  this.averageVelocity = 0.0f;
                  int num9 = (int) (((double) num7 - (double) this.minVelocityToTakeDamage) / 6.0);
                  if (this.jetpackControls && (!this.disablingJetpackControls || (double) Vector3.Angle(Vector3.up, this.transform.up) > 50.0))
                    num9 += 40;
                  this.DamagePlayer(Mathf.Clamp(num9, 20, 100), causeOfDeath: CauseOfDeath.Gravity, fallDamage: true, force: Vector3.ClampMagnitude(this.velocityLastFrame, 50f));
                  this.timeSinceTakingGravityDamage = 0.0f;
                }
              }
              else
                this.timeSinceTakingGravityDamage += Time.deltaTime;
              this.velocityLastFrame = this.thisController.velocity;
              this.previousFrameDeltaTime = Time.deltaTime;
            }
            else
              this.teleportingThisFrame = false;
          }
          this.isPlayerSliding = (double) Vector3.Angle(Vector3.up, this.playerGroundNormal) >= (double) this.thisController.slopeLimit;
        }
        else if (this.isClimbingLadder)
        {
          Vector3 direction = this.thisPlayerBody.up;
          Vector3 origin = this.gameplayCamera.transform.position + this.thisPlayerBody.up * 0.07f;
          if ((double) this.moveInputVector.y < 0.0)
          {
            direction = -this.thisPlayerBody.up;
            origin = this.transform.position;
          }
          if (!Physics.Raycast(origin, direction, 0.15f, StartOfRound.Instance.allPlayersCollideWithMask, QueryTriggerInteraction.Ignore))
            this.thisPlayerBody.transform.position += this.thisPlayerBody.up * (this.moveInputVector.y * this.climbSpeed * Time.deltaTime);
        }
        this.playerEye.position = this.gameplayCamera.transform.position;
        this.playerEye.rotation = this.gameplayCamera.transform.rotation;
        if ((UnityEngine.Object) NetworkManager.Singleton != (UnityEngine.Object) null && !this.IsServer || !this.isTestingPlayer && this.playersManager.connectedPlayersAmount > 0 || this.oldConnectedPlayersAmount >= 1)
        {
          this.updatePlayerLookInterval += Time.deltaTime;
          this.UpdatePlayerAnimationsToOtherClients(this.moveInputVector);
        }
        this.ClickHoldInteraction();
      }
      else
      {
        if (!this.isCameraDisabled)
        {
          this.isCameraDisabled = true;
          this.gameplayCamera.enabled = false;
          this.visorCamera.enabled = false;
          this.thisPlayerModel.shadowCastingMode = ShadowCastingMode.On;
          this.thisPlayerModelArms.enabled = false;
          this.mapRadarDirectionIndicator.enabled = false;
          this.gameObject.GetComponent<CharacterController>().enabled = false;
          if ((UnityEngine.Object) this.playerBodyAnimator.runtimeAnimatorController != (UnityEngine.Object) this.playersManager.otherClientsAnimatorController)
            this.playerBodyAnimator.runtimeAnimatorController = this.playersManager.otherClientsAnimatorController;
          if (!this.isPlayerDead)
          {
            for (int index = 0; index < this.playersManager.allPlayerObjects.Length && !this.playersManager.allPlayerObjects[index].GetComponent<PlayerControllerB>().gameplayCamera.enabled; ++index)
            {
              if (index == 4)
              {
                Debug.LogWarning((object) "!!! No cameras are enabled !!!");
                this.playerScreen.enabled = false;
              }
            }
          }
          if ((bool) (UnityEngine.Object) this.gameObject.GetComponent<Rigidbody>())
            this.gameObject.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
          Debug.Log((object) ("!!!! DISABLING CAMERA FOR PLAYER: " + this.gameObject.name));
          Debug.Log((object) string.Format("!!!! connectedPlayersAmount: {0}", (object) this.playersManager.connectedPlayersAmount));
        }
        this.SetNightVisionEnabled(true);
        if (!this.isTestingPlayer && !this.isPlayerDead && this.isPlayerControlled)
        {
          if (!this.disableSyncInAnimation)
          {
            if (this.snapToServerPosition)
            {
              this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.serverPlayerPosition, 16f * Time.deltaTime);
            }
            else
            {
              float num = 8f;
              if (this.jetpackControls)
                num = 15f;
              this.transform.localPosition = Vector3.MoveTowards(this.transform.localPosition, this.serverPlayerPosition, Mathf.Clamp(num * Vector3.Distance(this.transform.localPosition, this.serverPlayerPosition), 0.9f, 300f) * Time.deltaTime);
            }
          }
          this.gameplayCamera.transform.localEulerAngles = new Vector3(Mathf.LerpAngle(this.gameplayCamera.transform.localEulerAngles.x, this.targetLookRot, 14f * Time.deltaTime), this.gameplayCamera.transform.localEulerAngles.y, this.gameplayCamera.transform.localEulerAngles.z);
          if (this.jetpackControls || this.isClimbingLadder)
          {
            if (this.disableSyncInAnimation)
            {
              RoundManager.Instance.tempTransform.rotation = Quaternion.Euler(this.syncFullRotation);
              this.transform.rotation = Quaternion.Lerp(Quaternion.Euler(this.transform.eulerAngles), Quaternion.Euler(this.syncFullRotation), 8f * Time.deltaTime);
            }
          }
          else
          {
            if (!this.disableSyncInAnimation)
              this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, Mathf.LerpAngle(this.transform.eulerAngles.y, this.targetYRot, 14f * Time.deltaTime), this.transform.eulerAngles.z);
            if (!this.inSpecialInteractAnimation && !this.disableSyncInAnimation)
            {
              Vector3 localEulerAngles = this.transform.localEulerAngles;
              localEulerAngles.x = Mathf.LerpAngle(localEulerAngles.x, 0.0f, 25f * Time.deltaTime);
              localEulerAngles.z = Mathf.LerpAngle(localEulerAngles.z, 0.0f, 25f * Time.deltaTime);
              this.transform.localEulerAngles = localEulerAngles;
            }
          }
          this.playerEye.position = this.gameplayCamera.transform.position;
          this.playerEye.localEulerAngles = new Vector3(this.targetLookRot, 0.0f, 0.0f);
          this.playerEye.eulerAngles = new Vector3(this.playerEye.eulerAngles.x, this.targetYRot, this.playerEye.eulerAngles.z);
        }
        else if ((this.isPlayerDead || !this.isPlayerControlled) && this.setPositionOfDeadPlayer)
          this.transform.position = this.playersManager.notSpawnedPosition.position;
        if ((double) this.isInGameOverAnimation > 0.0 && (UnityEngine.Object) this.deadBody != (UnityEngine.Object) null && this.deadBody.gameObject.activeSelf)
        {
          Debug.Log((object) "Waiting time before spectating");
          this.isInGameOverAnimation -= Time.deltaTime;
        }
        else if (!this.hasBegunSpectating)
        {
          if ((UnityEngine.Object) this.deadBody != (UnityEngine.Object) null)
            Debug.Log((object) this.deadBody.gameObject.activeSelf);
          Debug.Log((object) "Started spectating");
          this.isInGameOverAnimation = 0.0f;
          this.hasBegunSpectating = true;
        }
      }
      this.timeSincePlayerMoving += Time.deltaTime;
      this.timeSinceMakingLoudNoise += Time.deltaTime;
      if (!this.inSpecialInteractAnimation)
      {
        this.specialAnimationWeight = !this.playingQuickSpecialAnimation ? Mathf.Lerp(this.specialAnimationWeight, 0.0f, Time.deltaTime * 12f) : 1f;
        if (!this.localArmsMatchCamera)
        {
          this.localArmsTransform.position = this.playerModelArmsMetarig.position + this.playerModelArmsMetarig.forward * -0.445f;
          this.playerModelArmsMetarig.rotation = Quaternion.Lerp(this.playerModelArmsMetarig.rotation, this.localArmsRotationTarget.rotation, 15f * Time.deltaTime);
        }
      }
      else
      {
        if (!this.isClimbingLadder && !this.inShockingMinigame)
        {
          this.cameraUp = Mathf.Lerp(this.cameraUp, 0.0f, 5f * Time.deltaTime);
          this.gameplayCamera.transform.localEulerAngles = new Vector3(this.cameraUp, this.gameplayCamera.transform.localEulerAngles.y, this.gameplayCamera.transform.localEulerAngles.z);
        }
        this.specialAnimationWeight = Mathf.Lerp(this.specialAnimationWeight, 1f, Time.deltaTime * 20f);
        this.playerModelArmsMetarig.localEulerAngles = new Vector3(-90f, 0.0f, 0.0f);
      }
      this.interactRay = new Ray(this.transform.position + Vector3.up * 2.3f, this.transform.forward);
      if ((double) this.doingUpperBodyEmote > 0.0 || !this.twoHanded && Physics.Raycast(this.interactRay, out this.hit, 0.53f, this.walkableSurfacesNoPlayersMask, QueryTriggerInteraction.Ignore))
      {
        this.doingUpperBodyEmote -= Time.deltaTime;
        this.handsOnWallWeight = Mathf.Lerp(this.handsOnWallWeight, 1f, 15f * Time.deltaTime);
      }
      else
        this.handsOnWallWeight = Mathf.Lerp(this.handsOnWallWeight, 0.0f, 15f * Time.deltaTime);
      this.playerBodyAnimator.SetLayerWeight(this.playerBodyAnimator.GetLayerIndex("UpperBodyEmotes"), this.handsOnWallWeight);
      this.emoteLayerWeight = !this.performingEmote ? Mathf.Lerp(this.emoteLayerWeight, 0.0f, 10f * Time.deltaTime) : Mathf.Lerp(this.emoteLayerWeight, 1f, 10f * Time.deltaTime);
      this.playerBodyAnimator.SetLayerWeight(this.playerBodyAnimator.GetLayerIndex("EmotesNoArms"), this.emoteLayerWeight);
      this.meshContainer.position = Vector3.Lerp(this.transform.position, this.transform.position - Vector3.up * 2.8f, StartOfRound.Instance.playerSinkingCurve.Evaluate(this.sinkingValue));
      this.sinkingValue = !this.isSinking || this.inSpecialInteractAnimation || !((UnityEngine.Object) this.inAnimationWithEnemy == (UnityEngine.Object) null) ? Mathf.Clamp(this.sinkingValue - Time.deltaTime * 0.75f, 0.0f, 1f) : Mathf.Clamp(this.sinkingValue + Time.deltaTime * this.sinkingSpeedMultiplier, 0.0f, 1f);
      if ((double) this.sinkingValue > 0.73000001907348633 || this.isUnderwater)
      {
        if (!this.wasUnderwaterLastFrame)
        {
          this.wasUnderwaterLastFrame = true;
          if (!this.IsOwner)
            this.waterBubblesAudio.Play();
        }
        this.voiceMuffledByEnemy = true;
        if (!this.IsOwner)
        {
          this.statusEffectAudio.volume = Mathf.Lerp(this.statusEffectAudio.volume, 0.0f, 4f * Time.deltaTime);
          if ((UnityEngine.Object) this.currentVoiceChatIngameSettings != (UnityEngine.Object) null)
          {
            OccludeAudio component = this.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>();
            component.overridingLowPass = true;
            component.lowPassOverride = 600f;
            this.waterBubblesAudio.volume = Mathf.Clamp(this.currentVoiceChatIngameSettings._playerState.Amplitude * 120f, 0.0f, 1f);
          }
          else
            StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
        }
        else if ((double) this.sinkingValue > 0.73000001907348633)
          HUDManager.Instance.sinkingCoveredFace = true;
      }
      else if (this.IsOwner)
        HUDManager.Instance.sinkingCoveredFace = false;
      else if (this.wasUnderwaterLastFrame)
      {
        this.waterBubblesAudio.Stop();
        if ((UnityEngine.Object) this.currentVoiceChatIngameSettings != (UnityEngine.Object) null)
        {
          this.wasUnderwaterLastFrame = false;
          this.currentVoiceChatIngameSettings.voiceAudio.GetComponent<OccludeAudio>().overridingLowPass = false;
          this.voiceMuffledByEnemy = false;
        }
        else
        {
          StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
          StartOfRound.Instance.UpdatePlayerVoiceEffects();
        }
      }
      else
        this.statusEffectAudio.volume = Mathf.Lerp(this.statusEffectAudio.volume, 1f, 4f * Time.deltaTime);
      if ((UnityEngine.Object) this.activeAudioReverbFilter == (UnityEngine.Object) null)
      {
        this.activeAudioReverbFilter = this.activeAudioListener.GetComponent<AudioReverbFilter>();
        this.activeAudioReverbFilter.enabled = true;
      }
      if ((UnityEngine.Object) this.reverbPreset != (UnityEngine.Object) null && (UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null && (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null && ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) this && (!this.isPlayerDead || StartOfRound.Instance.overrideSpectateCamera) || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript == (UnityEngine.Object) this && !StartOfRound.Instance.overrideSpectateCamera))
      {
        this.activeAudioReverbFilter.dryLevel = Mathf.Lerp(this.activeAudioReverbFilter.dryLevel, this.reverbPreset.dryLevel, 15f * Time.deltaTime);
        this.activeAudioReverbFilter.roomLF = Mathf.Lerp(this.activeAudioReverbFilter.roomLF, this.reverbPreset.lowFreq, 15f * Time.deltaTime);
        this.activeAudioReverbFilter.roomLF = Mathf.Lerp(this.activeAudioReverbFilter.roomHF, this.reverbPreset.highFreq, 15f * Time.deltaTime);
        this.activeAudioReverbFilter.decayTime = Mathf.Lerp(this.activeAudioReverbFilter.decayTime, this.reverbPreset.decayTime, 15f * Time.deltaTime);
        this.activeAudioReverbFilter.room = Mathf.Lerp(this.activeAudioReverbFilter.room, this.reverbPreset.room, 15f * Time.deltaTime);
      }
      if (this.isHoldingObject || this.isGrabbingObjectAnimation || this.inShockingMinigame)
      {
        this.upperBodyAnimationsWeight = Mathf.Lerp(this.upperBodyAnimationsWeight, 1f, 25f * Time.deltaTime);
        this.playerBodyAnimator.SetLayerWeight(this.playerBodyAnimator.GetLayerIndex("HoldingItemsRightHand"), this.upperBodyAnimationsWeight);
        if (this.twoHandedAnimation || this.inShockingMinigame)
          this.playerBodyAnimator.SetLayerWeight(this.playerBodyAnimator.GetLayerIndex("HoldingItemsBothHands"), this.upperBodyAnimationsWeight);
        else
          this.playerBodyAnimator.SetLayerWeight(this.playerBodyAnimator.GetLayerIndex("HoldingItemsBothHands"), Mathf.Abs(this.upperBodyAnimationsWeight - 1f));
      }
      else
      {
        this.upperBodyAnimationsWeight = Mathf.Lerp(this.upperBodyAnimationsWeight, 0.0f, 25f * Time.deltaTime);
        this.playerBodyAnimator.SetLayerWeight(this.playerBodyAnimator.GetLayerIndex("HoldingItemsRightHand"), this.upperBodyAnimationsWeight);
        this.playerBodyAnimator.SetLayerWeight(this.playerBodyAnimator.GetLayerIndex("HoldingItemsBothHands"), this.upperBodyAnimationsWeight);
      }
      this.playerBodyAnimator.SetLayerWeight(this.playerBodyAnimator.GetLayerIndex("SpecialAnimations"), this.specialAnimationWeight);
      if (this.inSpecialInteractAnimation && !this.inShockingMinigame)
      {
        this.cameraLookRig1.weight = Mathf.Lerp(this.cameraLookRig1.weight, 0.0f, Time.deltaTime * 25f);
        this.cameraLookRig2.weight = Mathf.Lerp(this.cameraLookRig1.weight, 0.0f, Time.deltaTime * 25f);
      }
      else
      {
        this.cameraLookRig1.weight = 0.45f;
        this.cameraLookRig2.weight = 1f;
      }
      this.exhaustionEffectLerp = !this.isExhausted ? Mathf.Lerp(this.exhaustionEffectLerp, 0.0f, 10f * Time.deltaTime) : Mathf.Lerp(this.exhaustionEffectLerp, 1f, 10f * Time.deltaTime);
      this.playerBodyAnimator.SetFloat("tiredAmount", this.exhaustionEffectLerp);
      if (this.isPlayerDead)
      {
        this.drunkness = 0.0f;
        this.drunknessInertia = 0.0f;
      }
      else
      {
        this.drunkness = Mathf.Clamp(this.drunkness + Time.deltaTime / 12f * this.drunknessSpeed * this.drunknessInertia, 0.0f, 1f);
        if (!this.increasingDrunknessThisFrame)
          this.drunknessInertia = (double) this.drunkness <= 0.0 ? 0.0f : Mathf.Clamp(this.drunknessInertia - Time.deltaTime / 3f * this.drunknessSpeed / Mathf.Clamp(Mathf.Abs(this.drunknessInertia), 0.2f, 1f), -2.5f, 2.5f);
        else
          this.increasingDrunknessThisFrame = false;
        float num = StartOfRound.Instance.drunknessSideEffect.Evaluate(this.drunkness);
        SoundManager.Instance.playerVoicePitchTargets[this.playerClientId] = (double) num <= 0.15000000596046448 ? 1f : 1f + num;
      }
      this.smoothLookMultiplier = 25f * Mathf.Clamp(Mathf.Abs(this.drunkness - 1.5f), 0.15f, 1f);
      if (this.bleedingHeavily && (double) this.bloodDropTimer >= 0.0)
        this.bloodDropTimer -= Time.deltaTime;
      this.lineOfSightCube.localScale = !Physics.Raycast(this.lineOfSightCube.position, this.lineOfSightCube.forward, out this.hit, 10f, this.playersManager.collidersAndRoomMask, QueryTriggerInteraction.Ignore) ? new Vector3(1.5f, 1.5f, 10f) : new Vector3(1.5f, 1.5f, this.hit.distance);
      this.SetPlayerSanityLevel();
    }

    private void SetFaceUnderwaterFilters()
    {
      if (this.isPlayerDead)
        return;
      if (this.isUnderwater && (UnityEngine.Object) this.underwaterCollider != (UnityEngine.Object) null && this.underwaterCollider.bounds.Contains(this.gameplayCamera.transform.position))
      {
        HUDManager.Instance.setUnderwaterFilter = true;
        this.statusEffectAudio.volume = Mathf.Lerp(this.statusEffectAudio.volume, 0.0f, 4f * Time.deltaTime);
        StartOfRound.Instance.drowningTimer -= Time.deltaTime / 10f;
        if ((double) StartOfRound.Instance.drowningTimer < 0.0)
        {
          StartOfRound.Instance.drowningTimer = 1f;
          this.KillPlayer(Vector3.zero, causeOfDeath: CauseOfDeath.Drowning);
        }
        else if ((double) StartOfRound.Instance.drowningTimer <= 0.30000001192092896)
        {
          if (!StartOfRound.Instance.playedDrowningSFX)
          {
            StartOfRound.Instance.playedDrowningSFX = true;
            HUDManager.Instance.UIAudio.PlayOneShot(StartOfRound.Instance.HUDSystemAlertSFX);
          }
          HUDManager.Instance.DisplayStatusEffect("Oxygen critically low!");
        }
      }
      else
      {
        this.statusEffectAudio.volume = Mathf.Lerp(this.statusEffectAudio.volume, 1f, 4f * Time.deltaTime);
        StartOfRound.Instance.playedDrowningSFX = false;
        StartOfRound.Instance.drowningTimer = Mathf.Clamp(StartOfRound.Instance.drowningTimer + Time.deltaTime, 0.1f, 1f);
        HUDManager.Instance.setUnderwaterFilter = false;
      }
      if ((double) this.syncUnderwaterInterval <= 0.0)
      {
        if (HUDManager.Instance.setUnderwaterFilter)
        {
          if (this.isFaceUnderwaterOnServer)
            return;
          this.isFaceUnderwaterOnServer = true;
          this.SetFaceUnderwaterServerRpc();
        }
        else
        {
          if (!this.isFaceUnderwaterOnServer)
            return;
          this.isFaceUnderwaterOnServer = false;
          this.SetFaceOutOfWaterServerRpc();
        }
      }
      else
        this.syncUnderwaterInterval = 0.5f;
    }

    [ServerRpc]
    private void SetFaceUnderwaterServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(1048203095U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 1048203095U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.SetFaceUnderwaterClientRpc();
    }

    [ClientRpc]
    private void SetFaceUnderwaterClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(1284827260U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 1284827260U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      this.isUnderwater = true;
    }

    [ServerRpc]
    private void SetFaceOutOfWaterServerRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(3262284737U, serverRpcParams, RpcDelivery.Reliable);
        this.__endSendServerRpc(ref bufferWriter, 3262284737U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.SetFaceOutOfWaterClientRpc();
    }

    [ClientRpc]
    private void SetFaceOutOfWaterClientRpc()
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(4067397557U, clientRpcParams, RpcDelivery.Reliable);
        this.__endSendClientRpc(ref bufferWriter, 4067397557U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      this.isUnderwater = false;
    }

    public void IncreaseFearLevelOverTime(float amountMultiplier = 1f, float cap = 1f)
    {
      this.playersManager.fearLevelIncreasing = true;
      if ((double) this.playersManager.fearLevel > (double) cap)
        return;
      this.playersManager.fearLevel += Time.deltaTime * amountMultiplier;
    }

    public void JumpToFearLevel(float targetFearLevel, bool onlyGoUp = true)
    {
      if (onlyGoUp && (double) targetFearLevel - (double) this.playersManager.fearLevel < 0.05000000074505806)
        return;
      this.playersManager.fearLevel = targetFearLevel;
      this.playersManager.fearLevelIncreasing = true;
    }

    private void SetPlayerSanityLevel()
    {
      if (StartOfRound.Instance.inShipPhase || !TimeOfDay.Instance.currentDayTimeStarted)
      {
        this.insanityLevel = 0.0f;
      }
      else
      {
        if (!this.NearOtherPlayers(this, 17f) && !this.PlayerIsHearingOthersThroughWalkieTalkie(this))
        {
          this.insanitySpeedMultiplier = !this.isInsideFactory ? (!this.isInHangarShipRoom ? (StartOfRound.Instance.connectedPlayersAmount != 0 ? (TimeOfDay.Instance.dayMode < DayMode.Sundown ? 0.3f : 0.5f) : -2f) : 0.2f) : 0.8f;
          this.isPlayerAlone = true;
        }
        else
        {
          this.insanitySpeedMultiplier = -3f;
          this.isPlayerAlone = false;
        }
        if ((double) this.insanitySpeedMultiplier < 0.0)
          this.insanityLevel = Mathf.MoveTowards(this.insanityLevel, 0.0f, Time.deltaTime * -this.insanitySpeedMultiplier);
        else if ((double) this.insanityLevel > (double) this.maxInsanityLevel)
        {
          this.insanityLevel = Mathf.MoveTowards(this.insanityLevel, this.maxInsanityLevel, Time.deltaTime * 2f);
        }
        else
        {
          if (StartOfRound.Instance.connectedPlayersAmount == 0)
            this.insanitySpeedMultiplier /= 1.6f;
          this.insanityLevel = Mathf.MoveTowards(this.insanityLevel, this.maxInsanityLevel, Time.deltaTime * this.insanitySpeedMultiplier);
        }
      }
    }

    private void SetNightVisionEnabled(bool isNotLocalClient)
    {
      this.nightVision.enabled = false;
      if ((UnityEngine.Object) GameNetworkManager.Instance == (UnityEngine.Object) null || (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || isNotLocalClient && !((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController.spectatedPlayerScript == (UnityEngine.Object) this) || !this.isInsideFactory)
        return;
      this.nightVision.enabled = true;
    }

    public void ClickHoldInteraction()
    {
      bool flag = IngamePlayerSettings.Instance.playerInput.actions.FindAction("Interact", false).IsPressed();
      this.isHoldingInteract = flag;
      if (!flag)
        this.StopHoldInteractionOnTrigger();
      else if ((UnityEngine.Object) this.hoveringOverTrigger == (UnityEngine.Object) null || !this.hoveringOverTrigger.interactable)
        this.StopHoldInteractionOnTrigger();
      else if ((UnityEngine.Object) this.hoveringOverTrigger == (UnityEngine.Object) null || !this.hoveringOverTrigger.gameObject.activeInHierarchy || !this.hoveringOverTrigger.holdInteraction || (double) this.hoveringOverTrigger.currentCooldownValue > 0.0 || this.isHoldingObject && !this.hoveringOverTrigger.oneHandedItemAllowed || this.twoHanded && !this.hoveringOverTrigger.twoHandedItemAllowed)
        this.StopHoldInteractionOnTrigger();
      else if (this.isGrabbingObjectAnimation || this.isTypingChat || this.inSpecialInteractAnimation || this.throwingObject)
        this.StopHoldInteractionOnTrigger();
      else if (!HUDManager.Instance.HoldInteractionFill(this.hoveringOverTrigger.timeToHold, this.hoveringOverTrigger.timeToHoldSpeedMultiplier))
        this.hoveringOverTrigger.HoldInteractNotFilled();
      else
        this.hoveringOverTrigger.Interact(this.thisPlayerBody);
    }

    private void StopHoldInteractionOnTrigger()
    {
      HUDManager.Instance.holdFillAmount = 0.0f;
      if ((UnityEngine.Object) this.previousHoveringOverTrigger != (UnityEngine.Object) null)
        this.previousHoveringOverTrigger.StopInteraction();
      if (!((UnityEngine.Object) this.hoveringOverTrigger != (UnityEngine.Object) null))
        return;
      this.hoveringOverTrigger.StopInteraction();
    }

    public void CancelSpecialTriggerAnimations()
    {
      Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
      if (objectOfType.terminalInUse)
      {
        objectOfType.QuitTerminal();
      }
      else
      {
        if (!((UnityEngine.Object) this.currentTriggerInAnimationWith != (UnityEngine.Object) null))
          return;
        this.currentTriggerInAnimationWith.StopSpecialAnimation();
      }
    }

    public void TeleportPlayer(
      Vector3 pos,
      bool withRotation = false,
      float rot = 0.0f,
      bool allowInteractTrigger = false,
      bool enableController = true)
    {
      if (this.IsOwner && !allowInteractTrigger)
        this.CancelSpecialTriggerAnimations();
      else if (!allowInteractTrigger && (UnityEngine.Object) this.currentTriggerInAnimationWith != (UnityEngine.Object) null)
      {
        this.currentTriggerInAnimationWith.onCancelAnimation.Invoke(this);
        this.currentTriggerInAnimationWith.SetInteractTriggerNotInAnimation();
      }
      if ((bool) (UnityEngine.Object) this.inAnimationWithEnemy)
        this.inAnimationWithEnemy.CancelSpecialAnimationWithPlayer();
      StartOfRound.Instance.playerTeleportedEvent.Invoke(this);
      if (withRotation)
      {
        this.targetYRot = rot;
        this.transform.eulerAngles = new Vector3(0.0f, this.targetYRot, 0.0f);
      }
      this.serverPlayerPosition = pos;
      this.thisController.enabled = false;
      this.transform.position = pos;
      if (enableController)
        this.thisController.enabled = true;
      this.teleportingThisFrame = true;
      this.teleportedLastFrame = true;
      this.timeSinceTakingGravityDamage = 1f;
      this.averageVelocity = 0.0f;
      if (!this.isUnderwater && !this.isSinking)
        return;
      QuicksandTrigger[] objectsByType = UnityEngine.Object.FindObjectsByType<QuicksandTrigger>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
      for (int index = 0; index < objectsByType.Length; ++index)
      {
        if (objectsByType[index].sinkingLocalPlayer)
        {
          objectsByType[index].OnExit(this.gameObject.GetComponent<Collider>());
          break;
        }
      }
    }

    public void KillPlayer(
      Vector3 bodyVelocity,
      bool spawnBody = true,
      CauseOfDeath causeOfDeath = CauseOfDeath.Unknown,
      int deathAnimation = 0)
    {
      if (!this.IsOwner || this.isPlayerDead || !this.AllowPlayerDeath())
        return;
      this.isPlayerDead = true;
      this.isPlayerControlled = false;
      this.thisPlayerModelArms.enabled = false;
      this.localVisor.position = this.playersManager.notSpawnedPosition.position;
      this.DisablePlayerModel(this.gameObject);
      this.isInsideFactory = false;
      this.IsInspectingItem = false;
      this.inTerminalMenu = false;
      this.twoHanded = false;
      this.carryWeight = 1f;
      this.fallValue = 0.0f;
      this.fallValueUncapped = 0.0f;
      this.takingFallDamage = false;
      this.isSinking = false;
      this.isUnderwater = false;
      StartOfRound.Instance.drowningTimer = 1f;
      HUDManager.Instance.setUnderwaterFilter = false;
      this.wasUnderwaterLastFrame = false;
      this.sourcesCausingSinking = 0;
      this.sinkingValue = 0.0f;
      this.hinderedMultiplier = 1f;
      this.isMovementHindered = 0;
      this.inAnimationWithEnemy = (EnemyAI) null;
      UnityEngine.Object.FindObjectOfType<Terminal>().terminalInUse = false;
      this.ChangeAudioListenerToObject(this.playersManager.spectateCamera.gameObject);
      SoundManager.Instance.SetDiageticMixerSnapshot();
      HUDManager.Instance.SetNearDepthOfFieldEnabled(true);
      HUDManager.Instance.HUDAnimator.SetBool("biohazardDamage", false);
      Debug.Log((object) ("Running kill player function for LOCAL client, player object: " + this.gameObject.name));
      HUDManager.Instance.gameOverAnimator.SetTrigger("gameOver");
      HUDManager.Instance.HideHUD(true);
      this.StopHoldInteractionOnTrigger();
      this.KillPlayerServerRpc((int) this.playerClientId, spawnBody, bodyVelocity, (int) causeOfDeath, deathAnimation);
      if (spawnBody)
        this.SpawnDeadBody((int) this.playerClientId, bodyVelocity, (int) causeOfDeath, this, deathAnimation);
      StartOfRound.Instance.SwitchCamera(StartOfRound.Instance.spectateCamera);
      this.isInGameOverAnimation = 1.5f;
      this.cursorTip.text = "";
      this.cursorIcon.enabled = false;
      this.DropAllHeldItems(spawnBody);
      this.DisableJetpackControlsLocally();
    }

    [ServerRpc]
    private void KillPlayerServerRpc(
      int playerId,
      bool spawnBody,
      Vector3 bodyVelocity,
      int causeOfDeath,
      int deathAnimation)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(1346025125U, serverRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, playerId);
        bufferWriter.WriteValueSafe<bool>(in spawnBody, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe(in bodyVelocity);
        BytePacker.WriteValueBitPacked(bufferWriter, causeOfDeath);
        BytePacker.WriteValueBitPacked(bufferWriter, deathAnimation);
        this.__endSendServerRpc(ref bufferWriter, 1346025125U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      --this.playersManager.livingPlayers;
      if (this.playersManager.livingPlayers == 0)
      {
        this.playersManager.allPlayersDead = true;
        this.playersManager.ShipLeaveAutomatically();
      }
      if (!spawnBody)
      {
        PlayerControllerB component = this.playersManager.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
        for (int index = 0; index < component.ItemSlots.Length; ++index)
        {
          GrabbableObject itemSlot = component.ItemSlots[index];
          if ((UnityEngine.Object) itemSlot != (UnityEngine.Object) null)
            itemSlot.gameObject.GetComponent<NetworkObject>().Despawn();
        }
      }
      else
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(StartOfRound.Instance.ragdollGrabbableObjectPrefab, this.playersManager.propsContainer);
        gameObject.GetComponent<NetworkObject>().Spawn();
        gameObject.GetComponent<RagdollGrabbableObject>().bodyID.Value = playerId;
      }
      this.KillPlayerClientRpc(playerId, spawnBody, bodyVelocity, causeOfDeath, deathAnimation);
    }

    [ClientRpc]
    private void KillPlayerClientRpc(
      int playerId,
      bool spawnBody,
      Vector3 bodyVelocity,
      int causeOfDeath,
      int deathAnimation)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(168339603U, clientRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, playerId);
        bufferWriter.WriteValueSafe<bool>(in spawnBody, new FastBufferWriter.ForPrimitives());
        bufferWriter.WriteValueSafe(in bodyVelocity);
        BytePacker.WriteValueBitPacked(bufferWriter, causeOfDeath);
        BytePacker.WriteValueBitPacked(bufferWriter, deathAnimation);
        this.__endSendClientRpc(ref bufferWriter, 168339603U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
        return;
      ++StartOfRound.Instance.gameStats.deaths;
      Debug.Log((object) ("A player died. player object: " + this.gameObject.name));
      if (!this.IsServer)
      {
        Debug.Log((object) "Setting living players minus one.");
        --this.playersManager.livingPlayers;
        Debug.Log((object) this.playersManager.livingPlayers);
        if (this.playersManager.livingPlayers == 0)
        {
          this.playersManager.allPlayersDead = true;
          this.playersManager.ShipLeaveAutomatically();
        }
      }
      PlayerControllerB component = this.playersManager.allPlayerObjects[playerId].GetComponent<PlayerControllerB>();
      component.bleedingHeavily = false;
      this.statusEffectAudio.Stop();
      if (!this.IsOwner & spawnBody)
      {
        this.SpawnDeadBody(playerId, bodyVelocity, causeOfDeath, component, deathAnimation);
        this.DropAllHeldItems(spawnBody);
      }
      this.placeOfDeath = component.transform.position;
      this.DisablePlayerModel(this.playersManager.allPlayerObjects[playerId]);
      component.setPositionOfDeadPlayer = true;
      component.isPlayerDead = true;
      component.isPlayerControlled = false;
      component.snapToServerPosition = false;
      component.isUnderwater = false;
      component.isHoldingObject = false;
      component.currentlyHeldObjectServer = (GrabbableObject) null;
      SoundManager.Instance.playerVoicePitchTargets[playerId] = 1f;
      SoundManager.Instance.playerVoicePitchLerpSpeed[playerId] = 3f;
      component.causeOfDeath = (CauseOfDeath) causeOfDeath;
      if (!this.IsOwner && GameNetworkManager.Instance.localPlayerController.isPlayerDead)
        HUDManager.Instance.UpdateBoxesSpectateUI();
      StartOfRound.Instance.UpdatePlayerVoiceEffects();
    }

    public void SpawnDeadBody(
      int playerId,
      Vector3 bodyVelocity,
      int causeOfDeath,
      PlayerControllerB deadPlayerController,
      int deathAnimation = 0,
      Transform overridePosition = null)
    {
      float num = 1.32f;
      Transform parent = (Transform) null;
      if (this.isInElevator)
        parent = this.playersManager.elevatorTransform;
      GameObject gameObject = !((UnityEngine.Object) overridePosition != (UnityEngine.Object) null) ? UnityEngine.Object.Instantiate<GameObject>(this.playersManager.playerRagdolls[deathAnimation], deadPlayerController.thisPlayerBody.position + Vector3.up * num, deadPlayerController.thisPlayerBody.rotation, parent) : UnityEngine.Object.Instantiate<GameObject>(this.playersManager.playerRagdolls[deathAnimation], overridePosition.position + Vector3.up * num, overridePosition.rotation, parent);
      DeadBodyInfo component = gameObject.GetComponent<DeadBodyInfo>();
      if ((bool) (UnityEngine.Object) overridePosition)
        component.overrideSpawnPosition = true;
      component.parentedToShip = this.isInElevator;
      component.playerObjectId = playerId;
      this.deadBody = component;
      foreach (Rigidbody componentsInChild in gameObject.GetComponentsInChildren<Rigidbody>())
        componentsInChild.velocity = bodyVelocity;
      for (int index = 0; index < this.bodyBloodDecals.Length; ++index)
        this.deadBody.bodyBloodDecals[index].SetActive(this.bodyBloodDecals[index].activeSelf);
      ScanNodeProperties componentInChildren = component.gameObject.GetComponentInChildren<ScanNodeProperties>();
      componentInChildren.headerText = "Body of " + deadPlayerController.playerUsername;
      CauseOfDeath causeOfDeath1 = (CauseOfDeath) causeOfDeath;
      componentInChildren.subText = "Cause of death: " + causeOfDeath1.ToString();
      this.deadBody.causeOfDeath = causeOfDeath1;
      if (causeOfDeath1 == CauseOfDeath.Bludgeoning || causeOfDeath1 == CauseOfDeath.Mauling || causeOfDeath1 == CauseOfDeath.Gunshots)
        this.deadBody.MakeCorpseBloody();
      if (causeOfDeath1 != CauseOfDeath.Gravity)
        return;
      this.deadBody.bodyAudio.PlayOneShot(StartOfRound.Instance.playerFallDeath);
      WalkieTalkie.TransmitOneShotAudio(this.deadBody.bodyAudio, StartOfRound.Instance.playerFallDeath);
    }

    public void DestroyItemInSlotAndSync(int itemSlot)
    {
      if (!this.IsOwner)
        return;
      if (itemSlot >= this.ItemSlots.Length || (UnityEngine.Object) this.ItemSlots[itemSlot] == (UnityEngine.Object) null)
        Debug.LogError((object) string.Format("Destroy item in slot called for a slot (slot {0}) which is empty or incorrect", (object) itemSlot));
      this.timeSinceSwitchingSlots = 0.0f;
      this.DestroyItemInSlot(itemSlot);
      this.DestroyItemInSlotServerRpc(itemSlot);
    }

    [ServerRpc]
    public void DestroyItemInSlotServerRpc(int itemSlot)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
        {
          if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
            return;
          Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
          return;
        }
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(1388366573U, serverRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, itemSlot);
        this.__endSendServerRpc(ref bufferWriter, 1388366573U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.DestroyItemInSlotClientRpc(itemSlot);
    }

    [ClientRpc]
    public void DestroyItemInSlotClientRpc(int itemSlot)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(899109231U, clientRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, itemSlot);
        this.__endSendClientRpc(ref bufferWriter, 899109231U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
        return;
      this.DestroyItemInSlot(itemSlot);
    }

    public void DestroyItemInSlot(int itemSlot)
    {
      if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null || (UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null || NetworkManager.Singleton.ShutdownInProgress)
        return;
      Debug.Log((object) string.Format("Destroying item in slot {0}; {1}; is currentlyheldobjectserver null: {2}", (object) itemSlot, (object) this.currentItemSlot, (object) ((UnityEngine.Object) this.currentlyHeldObjectServer == (UnityEngine.Object) null)));
      if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
        Debug.Log((object) ("currentlyHeldObjectServer: " + this.currentlyHeldObjectServer.itemProperties.itemName));
      GrabbableObject itemSlot1 = this.ItemSlots[itemSlot];
      if (this.isHoldingObject)
      {
        if (this.currentItemSlot == itemSlot)
        {
          this.carryWeight -= Mathf.Clamp(this.currentlyHeldObjectServer.itemProperties.weight - 1f, 0.0f, 10f);
          this.isHoldingObject = false;
          this.twoHanded = false;
          if (this.IsOwner)
          {
            this.playerBodyAnimator.SetBool("cancelHolding", true);
            this.playerBodyAnimator.SetTrigger("Throw");
            HUDManager.Instance.holdingTwoHandedItem.enabled = false;
            HUDManager.Instance.ClearControlTips();
            this.activatingItem = false;
          }
        }
        HUDManager.Instance.itemSlotIcons[itemSlot].enabled = false;
        if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null && (UnityEngine.Object) this.currentlyHeldObjectServer == (UnityEngine.Object) this.ItemSlots[itemSlot])
        {
          if (this.IsOwner)
          {
            this.SetSpecialGrabAnimationBool(false, this.currentlyHeldObjectServer);
            this.currentlyHeldObjectServer.DiscardItemOnClient();
          }
          this.currentlyHeldObjectServer = (GrabbableObject) null;
        }
      }
      this.ItemSlots[itemSlot] = (GrabbableObject) null;
      if (!this.IsServer)
        return;
      itemSlot1.NetworkObject.Despawn();
    }

    public void DropAllHeldItems(bool itemsFall = true, bool disconnecting = false)
    {
      for (int index = 0; index < this.ItemSlots.Length; ++index)
      {
        GrabbableObject itemSlot = this.ItemSlots[index];
        if ((UnityEngine.Object) itemSlot != (UnityEngine.Object) null)
        {
          if (itemsFall)
          {
            itemSlot.parentObject = (Transform) null;
            itemSlot.heldByPlayerOnServer = false;
            if (this.isInElevator)
              itemSlot.transform.SetParent(this.playersManager.elevatorTransform, true);
            else
              itemSlot.transform.SetParent(this.playersManager.propsContainer, true);
            this.SetItemInElevator(this.isInHangarShipRoom, this.isInElevator, itemSlot);
            itemSlot.EnablePhysics(true);
            itemSlot.EnableItemMeshes(true);
            itemSlot.transform.localScale = itemSlot.originalScale;
            itemSlot.isHeld = false;
            itemSlot.isPocketed = false;
            itemSlot.startFallingPosition = itemSlot.transform.parent.InverseTransformPoint(itemSlot.transform.position);
            itemSlot.FallToGround(true);
            itemSlot.fallTime = UnityEngine.Random.Range(-0.3f, 0.05f);
            if (this.IsOwner)
              itemSlot.DiscardItemOnClient();
            else if (!itemSlot.itemProperties.syncDiscardFunction)
              itemSlot.playerHeldBy = (PlayerControllerB) null;
          }
          if (this.IsOwner && !disconnecting)
          {
            HUDManager.Instance.holdingTwoHandedItem.enabled = false;
            HUDManager.Instance.itemSlotIcons[index].enabled = false;
            HUDManager.Instance.ClearControlTips();
            this.activatingItem = false;
          }
          this.ItemSlots[index] = (GrabbableObject) null;
        }
      }
      if (this.isHoldingObject)
      {
        this.isHoldingObject = false;
        if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null)
          this.SetSpecialGrabAnimationBool(false, this.currentlyHeldObjectServer);
        this.playerBodyAnimator.SetBool("cancelHolding", true);
        this.playerBodyAnimator.SetTrigger("Throw");
      }
      this.activatingItem = false;
      this.twoHanded = false;
      this.carryWeight = 1f;
      this.currentlyHeldObjectServer = (GrabbableObject) null;
    }

    private bool NearOtherPlayers(PlayerControllerB playerScript = null, float checkRadius = 10f)
    {
      if ((UnityEngine.Object) playerScript == (UnityEngine.Object) null)
        playerScript = this;
      this.gameObject.layer = 0;
      int num = Physics.CheckSphere(playerScript.transform.position, checkRadius, 8, QueryTriggerInteraction.Ignore) ? 1 : 0;
      this.gameObject.layer = 3;
      return num != 0;
    }

    private bool PlayerIsHearingOthersThroughWalkieTalkie(PlayerControllerB playerScript = null)
    {
      if ((UnityEngine.Object) playerScript == (UnityEngine.Object) null)
        playerScript = this;
      if (!playerScript.holdingWalkieTalkie)
        return false;
      for (int index = 0; index < WalkieTalkie.allWalkieTalkies.Count; ++index)
      {
        if (WalkieTalkie.allWalkieTalkies[index].clientIsHoldingAndSpeakingIntoThis && (UnityEngine.Object) WalkieTalkie.allWalkieTalkies[index] != (UnityEngine.Object) (playerScript.currentlyHeldObjectServer as WalkieTalkie))
          return true;
      }
      return false;
    }

    public void DisablePlayerModel(GameObject playerObject, bool enable = false, bool disableLocalArms = false)
    {
      foreach (Renderer componentsInChild in playerObject.GetComponentsInChildren<SkinnedMeshRenderer>())
        componentsInChild.enabled = enable;
      if (!disableLocalArms)
        return;
      this.thisPlayerModelArms.enabled = false;
    }

    public void SyncBodyPositionWithClients()
    {
      if (!((UnityEngine.Object) this.deadBody != (UnityEngine.Object) null))
        return;
      this.SyncBodyPositionClientRpc(this.deadBody.transform.position);
    }

    [ClientRpc]
    public void SyncBodyPositionClientRpc(Vector3 newBodyPosition)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(301044013U, clientRpcParams, RpcDelivery.Reliable);
        bufferWriter.WriteValueSafe(in newBodyPosition);
        this.__endSendClientRpc(ref bufferWriter, 301044013U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || (double) Vector3.Distance(this.deadBody.transform.position, newBodyPosition) < 1.5)
        return;
      this.StartCoroutine(this.WaitUntilPlayerHasLeftBodyToTeleport(newBodyPosition));
    }

    private IEnumerator WaitUntilPlayerHasLeftBodyToTeleport(Vector3 newBodyPosition)
    {
      PlayerControllerB playerControllerB = this;
      // ISSUE: reference to a compiler-generated method
      yield return (object) new WaitUntil(new Func<bool>(playerControllerB.\u003CWaitUntilPlayerHasLeftBodyToTeleport\u003Eb__455_0));
      if (!((UnityEngine.Object) playerControllerB.deadBody == (UnityEngine.Object) null))
        playerControllerB.deadBody.SetRagdollPositionSafely(newBodyPosition);
    }

    private void LateUpdate()
    {
      if (this.isFirstFrameLateUpdate)
      {
        this.isFirstFrameLateUpdate = false;
        this.previousElevatorPosition = this.playersManager.elevatorTransform.position;
      }
      else if (this.IsOwner && this.isPlayerControlled && (!this.IsServer || this.isHostPlayerObject))
      {
        if (this.isInElevator)
        {
          if (!this.wasInElevatorLastFrame)
          {
            this.wasInElevatorLastFrame = true;
            this.transform.SetParent(this.playersManager.elevatorTransform);
          }
        }
        else if (this.wasInElevatorLastFrame)
        {
          this.wasInElevatorLastFrame = false;
          this.transform.SetParent(this.playersManager.playersContainer);
        }
      }
      this.previousElevatorPosition = this.playersManager.elevatorTransform.position;
      if (!this.isTestingPlayer)
      {
        if ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
          return;
        if (!this.IsOwner && (double) this.usernameAlpha.alpha >= 0.0 && (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null)
        {
          this.usernameAlpha.alpha -= Time.deltaTime;
          this.usernameBillboard.LookAt(GameNetworkManager.Instance.localPlayerController.localVisorTargetPoint);
        }
        else if (this.usernameCanvas.gameObject.activeSelf)
          this.usernameCanvas.gameObject.SetActive(false);
      }
      if (this.IsOwner && (!this.IsServer || this.isHostPlayerObject))
      {
        this.PlayerLookInput();
        if (this.isPlayerControlled && !this.isPlayerDead)
        {
          if ((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null)
          {
            if ((double) (this.oldPlayerPosition - this.transform.localPosition).sqrMagnitude > (!this.inSpecialInteractAnimation ? (!this.NearOtherPlayers(this) ? 0.23999999463558197 : 0.10000000149011612) : 0.059999998658895493) || this.updatePositionForNewlyJoinedClient)
            {
              this.updatePositionForNewlyJoinedClient = false;
              if (!this.playersManager.newGameIsLoading)
              {
                this.UpdatePlayerPositionServerRpc(this.thisPlayerBody.localPosition, this.isInElevator, this.isExhausted, this.thisController.isGrounded);
                this.oldPlayerPosition = this.transform.localPosition;
              }
            }
            if ((UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null && this.isHoldingObject && this.grabbedObjectValidated)
            {
              this.currentlyHeldObjectServer.transform.localPosition = this.currentlyHeldObjectServer.itemProperties.positionOffset;
              this.currentlyHeldObjectServer.transform.localEulerAngles = this.currentlyHeldObjectServer.itemProperties.rotationOffset;
            }
          }
          this.localVisor.position = this.localVisorTargetPoint.position;
          this.localVisor.rotation = Quaternion.Lerp(this.localVisor.rotation, this.localVisorTargetPoint.rotation, 53f * Mathf.Clamp(Time.deltaTime, 0.0167f, 20f));
          float num1 = 1f;
          if ((double) this.drunkness > 0.019999999552965164)
            num1 *= Mathf.Abs(StartOfRound.Instance.drunknessSpeedEffect.Evaluate(this.drunkness) - 1.25f);
          if (this.isSprinting)
            this.sprintMeter = Mathf.Clamp(this.sprintMeter - Time.deltaTime / this.sprintTime * this.carryWeight * num1, 0.0f, 1f);
          else if (this.isMovementHindered > 0)
          {
            if (this.isWalking)
              this.sprintMeter = Mathf.Clamp(this.sprintMeter - (float) ((double) Time.deltaTime / (double) this.sprintTime * (double) num1 * 0.5), 0.0f, 1f);
          }
          else
          {
            this.sprintMeter = this.isWalking ? Mathf.Clamp(this.sprintMeter + Time.deltaTime / (this.sprintTime + 9f) * num1, 0.0f, 1f) : Mathf.Clamp(this.sprintMeter + Time.deltaTime / (this.sprintTime + 4f) * num1, 0.0f, 1f);
            if (this.isExhausted && (double) this.sprintMeter > 0.20000000298023224)
              this.isExhausted = false;
          }
          this.sprintMeterUI.fillAmount = this.sprintMeter;
          float num2;
          if (this.isHoldingObject && (UnityEngine.Object) this.currentlyHeldObjectServer != (UnityEngine.Object) null && this.currentlyHeldObjectServer.itemProperties.requiresBattery)
          {
            HUDManager.Instance.batteryMeter.fillAmount = this.currentlyHeldObjectServer.insertedBattery.charge / 1.3f;
            HUDManager.Instance.batteryMeter.gameObject.SetActive(true);
            HUDManager.Instance.batteryIcon.enabled = true;
            num2 = this.currentlyHeldObjectServer.insertedBattery.charge / 1.3f;
          }
          else if (this.helmetLight.enabled)
          {
            HUDManager.Instance.batteryMeter.fillAmount = this.pocketedFlashlight.insertedBattery.charge / 1.3f;
            HUDManager.Instance.batteryMeter.gameObject.SetActive(true);
            HUDManager.Instance.batteryIcon.enabled = true;
            num2 = this.pocketedFlashlight.insertedBattery.charge / 1.3f;
          }
          else
          {
            HUDManager.Instance.batteryMeter.gameObject.SetActive(false);
            HUDManager.Instance.batteryIcon.enabled = false;
            num2 = 1f;
          }
          HUDManager.Instance.batteryBlinkUI.SetBool("blink", (double) num2 < 0.20000000298023224 && (double) num2 > 0.0);
          this.timeSinceSwitchingSlots += Time.deltaTime;
          if ((double) this.limpMultiplier > 0.0)
            this.limpMultiplier -= Time.deltaTime / 2f;
          if (this.health < 20)
          {
            if ((double) this.healthRegenerateTimer <= 0.0)
            {
              this.healthRegenerateTimer = 1f;
              ++this.health;
              if (this.health >= 20)
                this.MakeCriticallyInjured(false);
              HUDManager.Instance.UpdateHealthUI(this.health, false);
            }
            else
              this.healthRegenerateTimer -= Time.deltaTime;
          }
          this.SetHoverTipAndCurrentInteractTrigger();
        }
      }
      if (!this.inSpecialInteractAnimation && this.localArmsMatchCamera)
      {
        this.localArmsTransform.position = this.cameraContainerTransform.transform.position + this.gameplayCamera.transform.up * -0.5f;
        this.playerModelArmsMetarig.rotation = this.localArmsRotationTarget.rotation;
      }
      if (this.playersManager.overrideSpectateCamera || !this.IsOwner || !this.isPlayerDead || this.IsServer && !this.isHostPlayerObject)
        return;
      if ((double) this.isInGameOverAnimation > 0.0 && (UnityEngine.Object) this.deadBody != (UnityEngine.Object) null)
      {
        this.spectateCameraPivot.position = this.deadBody.bodyParts[0].position;
        this.RaycastSpectateCameraAroundPivot();
      }
      else if ((UnityEngine.Object) this.spectatedPlayerScript != (UnityEngine.Object) null)
      {
        if (this.spectatedPlayerScript.isPlayerDead)
        {
          if (StartOfRound.Instance.allPlayersDead)
            StartOfRound.Instance.SetSpectateCameraToGameOverMode(true);
          if ((double) this.spectatedPlayerDeadTimer >= 1.5)
          {
            this.spectatedPlayerDeadTimer = 0.0f;
            this.SpectateNextPlayer();
          }
          else
          {
            this.spectatedPlayerDeadTimer += Time.deltaTime;
            if (!((UnityEngine.Object) this.spectatedPlayerScript.deadBody != (UnityEngine.Object) null))
              return;
            this.spectateCameraPivot.position = this.spectatedPlayerScript.deadBody.bodyParts[0].position;
            this.RaycastSpectateCameraAroundPivot();
            return;
          }
        }
        this.spectateCameraPivot.position = this.spectatedPlayerScript.lowerSpine.position + Vector3.up * 0.7f;
        this.RaycastSpectateCameraAroundPivot();
      }
      else if (StartOfRound.Instance.allPlayersDead)
      {
        StartOfRound.Instance.SetSpectateCameraToGameOverMode(true);
        this.SetSpectatedPlayerEffects(true);
      }
      else
        this.SpectateNextPlayer();
    }

    private void RaycastSpectateCameraAroundPivot()
    {
      this.interactRay = new Ray(this.spectateCameraPivot.position, -this.spectateCameraPivot.forward);
      if (Physics.Raycast(this.interactRay, out this.hit, 1.4f, this.walkableSurfacesNoPlayersMask, QueryTriggerInteraction.Ignore))
        this.playersManager.spectateCamera.transform.position = this.interactRay.GetPoint(this.hit.distance - 0.25f);
      else
        this.playersManager.spectateCamera.transform.position = this.interactRay.GetPoint(1.3f);
      this.playersManager.spectateCamera.transform.LookAt(this.spectateCameraPivot);
    }

    private void SetHoverTipAndCurrentInteractTrigger()
    {
      if (!this.isGrabbingObjectAnimation)
      {
        this.interactRay = new Ray(this.gameplayCamera.transform.position, this.gameplayCamera.transform.forward);
        if (Physics.Raycast(this.interactRay, out this.hit, this.grabDistance, this.interactableObjectsMask) && this.hit.collider.gameObject.layer != 8)
        {
          switch (this.hit.collider.tag)
          {
            case "PhysicsProp":
              if (this.FirstEmptyItemSlot() == -1)
              {
                this.cursorTip.text = "Inventory full!";
              }
              else
              {
                GrabbableObject component = this.hit.collider.gameObject.GetComponent<GrabbableObject>();
                if (!GameNetworkManager.Instance.gameHasStarted && !component.itemProperties.canBeGrabbedBeforeGameStart && (UnityEngine.Object) StartOfRound.Instance.testRoom == (UnityEngine.Object) null)
                {
                  this.cursorTip.text = "(Cannot hold until ship has landed)";
                  break;
                }
                if ((UnityEngine.Object) component != (UnityEngine.Object) null && !string.IsNullOrEmpty(component.customGrabTooltip))
                  this.cursorTip.text = component.customGrabTooltip;
                else
                  this.cursorTip.text = "Grab : [E]";
              }
              this.cursorIcon.enabled = true;
              this.cursorIcon.sprite = this.grabItemIcon;
              break;
            case "InteractTrigger":
              InteractTrigger component1 = this.hit.transform.gameObject.GetComponent<InteractTrigger>();
              if ((UnityEngine.Object) component1 != (UnityEngine.Object) this.previousHoveringOverTrigger && (UnityEngine.Object) this.previousHoveringOverTrigger != (UnityEngine.Object) null)
                this.previousHoveringOverTrigger.isBeingHeldByPlayer = false;
              if (!((UnityEngine.Object) component1 == (UnityEngine.Object) null))
              {
                this.hoveringOverTrigger = component1;
                if (!component1.interactable)
                {
                  this.cursorIcon.sprite = component1.disabledHoverIcon;
                  this.cursorIcon.enabled = (UnityEngine.Object) component1.disabledHoverIcon != (UnityEngine.Object) null;
                  this.cursorTip.text = component1.disabledHoverTip;
                  break;
                }
                if (component1.isPlayingSpecialAnimation)
                {
                  this.cursorIcon.enabled = false;
                  this.cursorTip.text = "";
                  break;
                }
                if (this.isHoldingInteract)
                {
                  if (this.twoHanded)
                  {
                    this.cursorTip.text = "[Hands full]";
                    break;
                  }
                  if (!string.IsNullOrEmpty(component1.holdTip))
                  {
                    this.cursorTip.text = component1.holdTip;
                    break;
                  }
                  break;
                }
                this.cursorIcon.enabled = true;
                this.cursorIcon.sprite = component1.hoverIcon;
                this.cursorTip.text = component1.hoverTip;
                break;
              }
              break;
          }
        }
        else
        {
          this.cursorIcon.enabled = false;
          this.cursorTip.text = "";
          if ((UnityEngine.Object) this.hoveringOverTrigger != (UnityEngine.Object) null)
            this.previousHoveringOverTrigger = this.hoveringOverTrigger;
          this.hoveringOverTrigger = (InteractTrigger) null;
        }
        if (!this.isFreeCamera && Physics.Raycast(this.interactRay, out this.hit, 5f, this.playerMask))
        {
          PlayerControllerB component2 = this.hit.collider.gameObject.GetComponent<PlayerControllerB>();
          if ((UnityEngine.Object) component2 != (UnityEngine.Object) null)
            component2.ShowNameBillboard();
        }
      }
      if (StartOfRound.Instance.localPlayerUsingController)
      {
        StringBuilder stringBuilder = new StringBuilder(this.cursorTip.text);
        stringBuilder.Replace("[E]", "[X]");
        stringBuilder.Replace("[LMB]", "[X]");
        stringBuilder.Replace("[RMB]", "[R-Trigger]");
        stringBuilder.Replace("[F]", "[R-Shoulder]");
        stringBuilder.Replace("[Z]", "[L-Shoulder]");
        this.cursorTip.text = stringBuilder.ToString();
      }
      else
        this.cursorTip.text = this.cursorTip.text.Replace("[LMB]", "[E]");
    }

    public void ShowNameBillboard()
    {
      this.usernameAlpha.alpha = 1f;
      this.usernameCanvas.gameObject.SetActive(true);
    }

    public bool IsPlayerServer() => this.IsServer;

    private void SpectateNextPlayer()
    {
      int index1 = 0;
      if ((UnityEngine.Object) this.spectatedPlayerScript != (UnityEngine.Object) null)
        index1 = (int) this.spectatedPlayerScript.playerClientId;
      for (int index2 = 0; index2 < 4; ++index2)
      {
        index1 = (index1 + 1) % 4;
        if (!this.playersManager.allPlayerScripts[index1].isPlayerDead && this.playersManager.allPlayerScripts[index1].isPlayerControlled && (UnityEngine.Object) this.playersManager.allPlayerScripts[index1] != (UnityEngine.Object) this)
        {
          this.spectatedPlayerScript = this.playersManager.allPlayerScripts[index1];
          this.SetSpectatedPlayerEffects();
          return;
        }
      }
      if ((UnityEngine.Object) this.deadBody != (UnityEngine.Object) null && this.deadBody.gameObject.activeSelf)
      {
        this.spectateCameraPivot.position = this.deadBody.bodyParts[0].position;
        this.RaycastSpectateCameraAroundPivot();
      }
      StartOfRound.Instance.SetPlayerSafeInShip();
    }

    public void SetSpectatedPlayerEffects(bool allPlayersDead = false)
    {
      try
      {
        if ((UnityEngine.Object) this.spectatedPlayerScript != (UnityEngine.Object) null)
          HUDManager.Instance.SetSpectatingTextToPlayer(this.spectatedPlayerScript);
        else
          HUDManager.Instance.spectatingPlayerText.text = "";
        TimeOfDay objectOfType1 = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
        if (allPlayersDead)
        {
          for (int index = 0; index < objectOfType1.effects.Length; ++index)
            objectOfType1.effects[index].effectEnabled = false;
          if ((UnityEngine.Object) objectOfType1.sunDirect != (UnityEngine.Object) null)
          {
            objectOfType1.sunDirect.enabled = true;
            objectOfType1.sunIndirect.GetComponent<HDAdditionalLightData>().lightDimmer = 1f;
          }
          AudioReverbPresets objectOfType2 = UnityEngine.Object.FindObjectOfType<AudioReverbPresets>();
          if (!((UnityEngine.Object) objectOfType2 != (UnityEngine.Object) null) || objectOfType2.audioPresets.Length <= 3)
            return;
          GameNetworkManager.Instance.localPlayerController.reverbPreset = objectOfType2.audioPresets[3].reverbPreset;
        }
        else
        {
          AudioReverbTrigger currentAudioTrigger = this.spectatedPlayerScript.currentAudioTrigger;
          if ((UnityEngine.Object) currentAudioTrigger == (UnityEngine.Object) null)
          {
            TimeOfDay.Instance.SetInsideLightingDimness(true, this.spectatedPlayerScript.isInsideFactory || this.spectatedPlayerScript.isInHangarShipRoom);
          }
          else
          {
            if ((UnityEngine.Object) currentAudioTrigger.localFog != (UnityEngine.Object) null)
              currentAudioTrigger.localFog.parameters.meanFreePath = !currentAudioTrigger.toggleLocalFog ? 200f : currentAudioTrigger.fogEnabledAmount;
            TimeOfDay.Instance.SetInsideLightingDimness(true, currentAudioTrigger.setInsideAtmosphere && currentAudioTrigger.insideLighting);
            if (currentAudioTrigger.disableAllWeather || this.spectatedPlayerScript.isInsideFactory)
            {
              TimeOfDay.Instance.DisableAllWeather();
            }
            else
            {
              if (currentAudioTrigger.enableCurrentLevelWeather && TimeOfDay.Instance.currentLevelWeather != LevelWeatherType.None)
                TimeOfDay.Instance.effects[(int) TimeOfDay.Instance.currentLevelWeather].effectEnabled = true;
              if (currentAudioTrigger.weatherEffect != -1)
                TimeOfDay.Instance.effects[currentAudioTrigger.weatherEffect].effectEnabled = currentAudioTrigger.effectEnabled;
            }
            StartOfRound.Instance.UpdatePlayerVoiceEffects();
          }
        }
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("Error caught in SpectatedPlayerEffects: {0}", (object) ex));
      }
    }

    public void AddBloodToBody()
    {
      for (int index = 0; index < this.bodyBloodDecals.Length; ++index)
      {
        if (!this.bodyBloodDecals[index].activeSelf)
        {
          this.bodyBloodDecals[index].SetActive(true);
          break;
        }
      }
    }

    public void RemoveBloodFromBody()
    {
      for (int index = 0; index < this.bodyBloodDecals.Length; ++index)
        this.bodyBloodDecals[index].SetActive(false);
    }

    void IHittable.Hit(
      int force,
      Vector3 hitDirection,
      PlayerControllerB playerWhoHit,
      bool playHitSFX = false)
    {
      if (!this.AllowPlayerDeath())
        return;
      foreach (CentipedeAI centipedeAi in UnityEngine.Object.FindObjectsByType<CentipedeAI>(FindObjectsSortMode.None))
      {
        if ((UnityEngine.Object) centipedeAi.clingingToPlayer == (UnityEngine.Object) this)
          return;
      }
      if ((bool) (UnityEngine.Object) this.inAnimationWithEnemy)
        return;
      if (force <= 2)
        this.DamagePlayerFromOtherClientServerRpc(10, hitDirection, (int) playerWhoHit.playerClientId);
      else if (force <= 4)
        this.DamagePlayerFromOtherClientServerRpc(30, hitDirection, (int) playerWhoHit.playerClientId);
      else
        this.DamagePlayerFromOtherClientServerRpc(100, hitDirection, (int) playerWhoHit.playerClientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void DamagePlayerFromOtherClientServerRpc(
      int damageAmount,
      Vector3 hitDirection,
      int playerWhoHit)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
      {
        ServerRpcParams serverRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendServerRpc(638895557U, serverRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, damageAmount);
        bufferWriter.WriteValueSafe(in hitDirection);
        BytePacker.WriteValueBitPacked(bufferWriter, playerWhoHit);
        this.__endSendServerRpc(ref bufferWriter, 638895557U, serverRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
        return;
      this.DamagePlayerFromOtherClientClientRpc(damageAmount, hitDirection, playerWhoHit, this.health - damageAmount);
    }

    [ClientRpc]
    public void DamagePlayerFromOtherClientClientRpc(
      int damageAmount,
      Vector3 hitDirection,
      int playerWhoHit,
      int newHealthAmount)
    {
      NetworkManager networkManager = this.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
      {
        ClientRpcParams clientRpcParams;
        FastBufferWriter bufferWriter = this.__beginSendClientRpc(2557046125U, clientRpcParams, RpcDelivery.Reliable);
        BytePacker.WriteValueBitPacked(bufferWriter, damageAmount);
        bufferWriter.WriteValueSafe(in hitDirection);
        BytePacker.WriteValueBitPacked(bufferWriter, playerWhoHit);
        BytePacker.WriteValueBitPacked(bufferWriter, newHealthAmount);
        this.__endSendClientRpc(ref bufferWriter, 2557046125U, clientRpcParams, RpcDelivery.Reliable);
      }
      if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || !this.AllowPlayerDeath())
        return;
      this.DamageOnOtherClients(damageAmount, newHealthAmount);
      if (this.IsOwner && this.isPlayerControlled)
      {
        foreach (CentipedeAI centipedeAi in UnityEngine.Object.FindObjectsByType<CentipedeAI>(FindObjectsSortMode.None))
        {
          if ((UnityEngine.Object) centipedeAi.clingingToPlayer == (UnityEngine.Object) this)
            return;
        }
        this.DamagePlayer(damageAmount, callRPC: false, causeOfDeath: CauseOfDeath.Bludgeoning);
      }
      this.movementAudio.PlayOneShot(StartOfRound.Instance.hitPlayerSFX);
      if (this.health >= 6)
        return;
      this.DropBlood(hitDirection);
      this.bodyBloodDecals[0].SetActive(true);
      this.playersManager.allPlayerScripts[playerWhoHit].AddBloodToBody();
      this.playersManager.allPlayerScripts[playerWhoHit].movementAudio.PlayOneShot(StartOfRound.Instance.bloodGoreSFX);
      WalkieTalkie.TransmitOneShotAudio(this.playersManager.allPlayerScripts[playerWhoHit].movementAudio, StartOfRound.Instance.bloodGoreSFX);
    }

    public bool HasLineOfSightToPosition(
      Vector3 pos,
      float width = 45f,
      int range = 60,
      float proximityAwareness = -1f)
    {
      float num = Vector3.Distance(this.transform.position, pos);
      return (double) num < (double) range && ((double) Vector3.Angle(this.playerEye.transform.forward, pos - this.gameplayCamera.transform.position) < (double) width || (double) num < (double) proximityAwareness) && !Physics.Linecast(this.playerEye.transform.position, pos, StartOfRound.Instance.collidersRoomDefaultAndFoliage, QueryTriggerInteraction.Ignore);
    }

    public float LineOfSightToPositionAngle(Vector3 pos, int range = 60, float proximityAwareness = -1f)
    {
      return (double) Vector3.Distance(this.transform.position, pos) < (double) range && !Physics.Linecast(this.playerEye.transform.position, pos, StartOfRound.Instance.collidersRoomDefaultAndFoliage, QueryTriggerInteraction.Ignore) ? Vector3.Angle(this.playerEye.transform.forward, pos - this.gameplayCamera.transform.position) : -361f;
    }

    protected override void __initializeVariables() => base.__initializeVariables();

    [RuntimeInitializeOnLoadMethod]
    internal static void InitializeRPCS_PlayerControllerB()
    {
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(800455552U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_800455552)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3591743514U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3591743514)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1084949295U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1084949295)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1822320450U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1822320450)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3986869491U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3986869491)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1090586009U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1090586009)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(341877959U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_341877959)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2005250174U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2005250174)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(4195705835U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_4195705835)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3390857164U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3390857164)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2585603452U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2585603452)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2196003333U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2196003333)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3803364611U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3803364611)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1955832627U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1955832627)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(878005044U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_878005044)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(655708081U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_655708081)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(412259855U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_412259855)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(141629807U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_141629807)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1554282707U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1554282707)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2552479808U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2552479808)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1786952262U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1786952262)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2217326231U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2217326231)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2376977494U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2376977494)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3943098567U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3943098567)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3830452098U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3830452098)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3771510012U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3771510012)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(588787670U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_588787670)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2188611472U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2188611472)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3789403418U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3789403418)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2444895710U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2444895710)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3473255830U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3473255830)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3386813972U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3386813972)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2480354441U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2480354441)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2281795056U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2281795056)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2581007949U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2581007949)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(153310197U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_153310197)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3332990272U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3332990272)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(983565270U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_983565270)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2504133785U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2504133785)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(956616685U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_956616685)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3237016509U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3237016509)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1367193869U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1367193869)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1048203095U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1048203095)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1284827260U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1284827260)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(3262284737U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_3262284737)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(4067397557U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_4067397557)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1346025125U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1346025125)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(168339603U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_168339603)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(1388366573U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_1388366573)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(899109231U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_899109231)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(301044013U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_301044013)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(638895557U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_638895557)));
      // ISSUE: explicit non-virtual call
      __nonvirtual (NetworkManager.__rpc_func_table.Add(2557046125U, new NetworkManager.RpcReceiveHandler(PlayerControllerB.__rpc_handler_2557046125)));
    }

    private static void __rpc_handler_800455552(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).BreakLegsSFXServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_3591743514(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).BreakLegsSFXClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_1084949295(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        int damageNumber;
        ByteUnpacker.ReadValueBitPacked(reader, out damageNumber);
        int newHealthAmount;
        ByteUnpacker.ReadValueBitPacked(reader, out newHealthAmount);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).DamagePlayerServerRpc(damageNumber, newHealthAmount);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_1822320450(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      int damageNumber;
      ByteUnpacker.ReadValueBitPacked(reader, out damageNumber);
      int newHealthAmount;
      ByteUnpacker.ReadValueBitPacked(reader, out newHealthAmount);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).DamagePlayerClientRpc(damageNumber, newHealthAmount);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_3986869491(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        float sinkingSpeed;
        reader.ReadValueSafe<float>(out sinkingSpeed, new FastBufferWriter.ForPrimitives());
        int audioClipIndex;
        ByteUnpacker.ReadValueBitPacked(reader, out audioClipIndex);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).StartSinkingServerRpc(sinkingSpeed, audioClipIndex);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_1090586009(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      float sinkingSpeed;
      reader.ReadValueSafe<float>(out sinkingSpeed, new FastBufferWriter.ForPrimitives());
      int audioClipIndex;
      ByteUnpacker.ReadValueBitPacked(reader, out audioClipIndex);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).StartSinkingClientRpc(sinkingSpeed, audioClipIndex);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_341877959(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).StopSinkingServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_2005250174(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).StopSinkingClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_4195705835(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).MakeCriticallyInjuredServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_3390857164(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).MakeCriticallyInjuredClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_2585603452(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).HealServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_2196003333(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).HealClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_3803364611(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).StartPerformingEmoteServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_1955832627(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).StartPerformingEmoteClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_878005044(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).StopPerformingEmoteServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_655708081(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).StopPerformingEmoteClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_412259855(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        bool forward;
        reader.ReadValueSafe<bool>(out forward, new FastBufferWriter.ForPrimitives());
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).SwitchItemSlotsServerRpc(forward);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_141629807(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      bool forward;
      reader.ReadValueSafe<bool>(out forward, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).SwitchItemSlotsClientRpc(forward);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_1554282707(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        NetworkObjectReference grabbedObject;
        reader.ReadValueSafe<NetworkObjectReference>(out grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).GrabObjectServerRpc(grabbedObject);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_2552479808(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      bool grabValidated;
      reader.ReadValueSafe<bool>(out grabValidated, new FastBufferWriter.ForPrimitives());
      NetworkObjectReference grabbedObject;
      reader.ReadValueSafe<NetworkObjectReference>(out grabbedObject, new FastBufferWriter.ForNetworkSerializable());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).GrabObjectClientRpc(grabValidated, grabbedObject);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_1786952262(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).DespawnHeldObjectServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_2217326231(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).DespawnHeldObjectClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_2376977494(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        NetworkObjectReference grabbedObject;
        reader.ReadValueSafe<NetworkObjectReference>(out grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        bool droppedInElevator;
        reader.ReadValueSafe<bool>(out droppedInElevator, new FastBufferWriter.ForPrimitives());
        bool droppedInShipRoom;
        reader.ReadValueSafe<bool>(out droppedInShipRoom, new FastBufferWriter.ForPrimitives());
        Vector3 targetFloorPosition;
        reader.ReadValueSafe(out targetFloorPosition);
        int floorYRot;
        ByteUnpacker.ReadValueBitPacked(reader, out floorYRot);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).ThrowObjectServerRpc(grabbedObject, droppedInElevator, droppedInShipRoom, targetFloorPosition, floorYRot);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_3943098567(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      bool droppedInElevator;
      reader.ReadValueSafe<bool>(out droppedInElevator, new FastBufferWriter.ForPrimitives());
      bool droppedInShipRoom;
      reader.ReadValueSafe<bool>(out droppedInShipRoom, new FastBufferWriter.ForPrimitives());
      Vector3 targetFloorPosition;
      reader.ReadValueSafe(out targetFloorPosition);
      NetworkObjectReference grabbedObject;
      reader.ReadValueSafe<NetworkObjectReference>(out grabbedObject, new FastBufferWriter.ForNetworkSerializable());
      int floorYRot;
      ByteUnpacker.ReadValueBitPacked(reader, out floorYRot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).ThrowObjectClientRpc(droppedInElevator, droppedInShipRoom, targetFloorPosition, grabbedObject, floorYRot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_3830452098(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        NetworkObjectReference grabbedObject;
        reader.ReadValueSafe<NetworkObjectReference>(out grabbedObject, new FastBufferWriter.ForNetworkSerializable());
        NetworkObjectReference parentObject;
        reader.ReadValueSafe<NetworkObjectReference>(out parentObject, new FastBufferWriter.ForNetworkSerializable());
        Vector3 placePositionOffset;
        reader.ReadValueSafe(out placePositionOffset);
        bool matchRotationOfParent;
        reader.ReadValueSafe<bool>(out matchRotationOfParent, new FastBufferWriter.ForPrimitives());
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).PlaceObjectServerRpc(grabbedObject, parentObject, placePositionOffset, matchRotationOfParent);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_3771510012(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      NetworkObjectReference parentObjectReference;
      reader.ReadValueSafe<NetworkObjectReference>(out parentObjectReference, new FastBufferWriter.ForNetworkSerializable());
      Vector3 placePositionOffset;
      reader.ReadValueSafe(out placePositionOffset);
      bool matchRotationOfParent;
      reader.ReadValueSafe<bool>(out matchRotationOfParent, new FastBufferWriter.ForPrimitives());
      NetworkObjectReference grabbedObject;
      reader.ReadValueSafe<NetworkObjectReference>(out grabbedObject, new FastBufferWriter.ForNetworkSerializable());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).PlaceObjectClientRpc(parentObjectReference, placePositionOffset, matchRotationOfParent, grabbedObject);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_588787670(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        short newRot;
        ByteUnpacker.ReadValueBitPacked(reader, out newRot);
        short newYRot;
        ByteUnpacker.ReadValueBitPacked(reader, out newYRot);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).UpdatePlayerRotationServerRpc(newRot, newYRot);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_2188611472(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      short newRot;
      ByteUnpacker.ReadValueBitPacked(reader, out newRot);
      short newYRot;
      ByteUnpacker.ReadValueBitPacked(reader, out newYRot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).UpdatePlayerRotationClientRpc(newRot, newYRot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_3789403418(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        Vector3 playerEulers;
        reader.ReadValueSafe(out playerEulers);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).UpdatePlayerRotationFullServerRpc(playerEulers);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_2444895710(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      Vector3 playerEulers;
      reader.ReadValueSafe(out playerEulers);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).UpdatePlayerRotationFullClientRpc(playerEulers);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_3473255830(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        int animationState;
        ByteUnpacker.ReadValueBitPacked(reader, out animationState);
        float animationSpeed;
        reader.ReadValueSafe<float>(out animationSpeed, new FastBufferWriter.ForPrimitives());
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).UpdatePlayerAnimationServerRpc(animationState, animationSpeed);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_3386813972(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      int animationState;
      ByteUnpacker.ReadValueBitPacked(reader, out animationState);
      float animationSpeed;
      reader.ReadValueSafe<float>(out animationSpeed, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).UpdatePlayerAnimationClientRpc(animationState, animationSpeed);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_2480354441(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        bool specialAnimation;
        reader.ReadValueSafe<bool>(out specialAnimation, new FastBufferWriter.ForPrimitives());
        float timed;
        reader.ReadValueSafe<float>(out timed, new FastBufferWriter.ForPrimitives());
        bool climbingLadder;
        reader.ReadValueSafe<bool>(out climbingLadder, new FastBufferWriter.ForPrimitives());
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).IsInSpecialAnimationServerRpc(specialAnimation, timed, climbingLadder);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_2281795056(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      bool specialAnimation;
      reader.ReadValueSafe<bool>(out specialAnimation, new FastBufferWriter.ForPrimitives());
      float timed;
      reader.ReadValueSafe<float>(out timed, new FastBufferWriter.ForPrimitives());
      bool climbingLadder;
      reader.ReadValueSafe<bool>(out climbingLadder, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).IsInSpecialAnimationClientRpc(specialAnimation, timed, climbingLadder);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_2581007949(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        Vector3 newPos;
        reader.ReadValueSafe(out newPos);
        bool inElevator;
        reader.ReadValueSafe<bool>(out inElevator, new FastBufferWriter.ForPrimitives());
        bool exhausted;
        reader.ReadValueSafe<bool>(out exhausted, new FastBufferWriter.ForPrimitives());
        bool isPlayerGrounded;
        reader.ReadValueSafe<bool>(out isPlayerGrounded, new FastBufferWriter.ForPrimitives());
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).UpdatePlayerPositionServerRpc(newPos, inElevator, exhausted, isPlayerGrounded);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_153310197(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      Vector3 newPos;
      reader.ReadValueSafe(out newPos);
      bool inElevator;
      reader.ReadValueSafe<bool>(out inElevator, new FastBufferWriter.ForPrimitives());
      bool exhausted;
      reader.ReadValueSafe<bool>(out exhausted, new FastBufferWriter.ForPrimitives());
      bool isPlayerGrounded;
      reader.ReadValueSafe<bool>(out isPlayerGrounded, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).UpdatePlayerPositionClientRpc(newPos, inElevator, exhausted, isPlayerGrounded);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_3332990272(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        bool fallHard;
        reader.ReadValueSafe<bool>(out fallHard, new FastBufferWriter.ForPrimitives());
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).LandFromJumpServerRpc(fallHard);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_983565270(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      bool fallHard;
      reader.ReadValueSafe<bool>(out fallHard, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).LandFromJumpClientRpc(fallHard);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_2504133785(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      ulong newPlayerSteamId;
      ByteUnpacker.ReadValueBitPacked(reader, out newPlayerSteamId);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((PlayerControllerB) target).SendNewPlayerValuesServerRpc(newPlayerSteamId);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_956616685(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      bool flag;
      reader.ReadValueSafe<bool>(out flag, new FastBufferWriter.ForPrimitives());
      ulong[] playerSteamIds = (ulong[]) null;
      if (flag)
        reader.ReadValueSafe<ulong>(out playerSteamIds, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).SendNewPlayerValuesClientRpc(playerSteamIds);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_3237016509(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).DisableJetpackModeServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_1367193869(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).DisableJetpackModeClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_1048203095(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).SetFaceUnderwaterServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_1284827260(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).SetFaceUnderwaterClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_3262284737(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).SetFaceOutOfWaterServerRpc();
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_4067397557(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).SetFaceOutOfWaterClientRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_1346025125(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        int playerId;
        ByteUnpacker.ReadValueBitPacked(reader, out playerId);
        bool spawnBody;
        reader.ReadValueSafe<bool>(out spawnBody, new FastBufferWriter.ForPrimitives());
        Vector3 bodyVelocity;
        reader.ReadValueSafe(out bodyVelocity);
        int causeOfDeath;
        ByteUnpacker.ReadValueBitPacked(reader, out causeOfDeath);
        int deathAnimation;
        ByteUnpacker.ReadValueBitPacked(reader, out deathAnimation);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).KillPlayerServerRpc(playerId, spawnBody, bodyVelocity, causeOfDeath, deathAnimation);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_168339603(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      int playerId;
      ByteUnpacker.ReadValueBitPacked(reader, out playerId);
      bool spawnBody;
      reader.ReadValueSafe<bool>(out spawnBody, new FastBufferWriter.ForPrimitives());
      Vector3 bodyVelocity;
      reader.ReadValueSafe(out bodyVelocity);
      int causeOfDeath;
      ByteUnpacker.ReadValueBitPacked(reader, out causeOfDeath);
      int deathAnimation;
      ByteUnpacker.ReadValueBitPacked(reader, out deathAnimation);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).KillPlayerClientRpc(playerId, spawnBody, bodyVelocity, causeOfDeath, deathAnimation);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_1388366573(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
      {
        if (networkManager.LogLevel > Unity.Netcode.LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
      }
      else
      {
        int itemSlot;
        ByteUnpacker.ReadValueBitPacked(reader, out itemSlot);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
        ((PlayerControllerB) target).DestroyItemInSlotServerRpc(itemSlot);
        target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
      }
    }

    private static void __rpc_handler_899109231(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      int itemSlot;
      ByteUnpacker.ReadValueBitPacked(reader, out itemSlot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).DestroyItemInSlotClientRpc(itemSlot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_301044013(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      Vector3 newBodyPosition;
      reader.ReadValueSafe(out newBodyPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).SyncBodyPositionClientRpc(newBodyPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_638895557(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      int damageAmount;
      ByteUnpacker.ReadValueBitPacked(reader, out damageAmount);
      Vector3 hitDirection;
      reader.ReadValueSafe(out hitDirection);
      int playerWhoHit;
      ByteUnpacker.ReadValueBitPacked(reader, out playerWhoHit);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((PlayerControllerB) target).DamagePlayerFromOtherClientServerRpc(damageAmount, hitDirection, playerWhoHit);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    private static void __rpc_handler_2557046125(
      NetworkBehaviour target,
      FastBufferReader reader,
      __RpcParams rpcParams)
    {
      NetworkManager networkManager = target.NetworkManager;
      if (networkManager == null || !networkManager.IsListening)
        return;
      int damageAmount;
      ByteUnpacker.ReadValueBitPacked(reader, out damageAmount);
      Vector3 hitDirection;
      reader.ReadValueSafe(out hitDirection);
      int playerWhoHit;
      ByteUnpacker.ReadValueBitPacked(reader, out playerWhoHit);
      int newHealthAmount;
      ByteUnpacker.ReadValueBitPacked(reader, out newHealthAmount);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
      ((PlayerControllerB) target).DamagePlayerFromOtherClientClientRpc(damageAmount, hitDirection, playerWhoHit, newHealthAmount);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }

    protected internal override string __getTypeName() => nameof (PlayerControllerB);
  }
}
