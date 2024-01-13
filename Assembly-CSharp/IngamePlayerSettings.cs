// Decompiled with JetBrains decompiler
// Type: IngamePlayerSettings
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

#nullable disable
public class IngamePlayerSettings : MonoBehaviour
{
  public IngamePlayerSettings.Settings settings;
  public IngamePlayerSettings.Settings unsavedSettings;
  public AudioSource SettingsAudio;
  public Volume universalVolume;
  private DissonanceComms comms;
  public bool redoLaunchSettings;
  public bool changesNotApplied;
  public InputActionRebindingExtensions.RebindingOperation rebindingOperation;
  private SettingsOption currentRebindingKeyUI;
  public PlayerInput playerInput;
  public bool encounteredErrorDuringSave;

  public static IngamePlayerSettings Instance { get; private set; }

  private void Awake()
  {
    if ((UnityEngine.Object) IngamePlayerSettings.Instance == (UnityEngine.Object) null)
    {
      IngamePlayerSettings.Instance = this;
      UnityEngine.Object.DontDestroyOnLoad((UnityEngine.Object) this.gameObject);
      this.StartCoroutine(this.waitToLoadSettings());
    }
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
  }

  private IEnumerator waitToLoadSettings()
  {
    ES3.Init();
    yield return (object) new WaitForSeconds(0.5f);
    try
    {
      this.LoadSettingsFromPrefs();
      this.UpdateGameToMatchSettings();
    }
    catch (Exception ex)
    {
      this.DisplaySaveFileError(ex);
      yield break;
    }
    PreInitSceneScript objectOfType = UnityEngine.Object.FindObjectOfType<PreInitSceneScript>();
    objectOfType.SetLaunchPanelsEnabled();
    if (this.settings.playerHasFinishedSetup && (UnityEngine.Object) objectOfType != (UnityEngine.Object) null)
      objectOfType.SkipToFinalSetting();
  }

  private void DisplaySaveFileError(Exception e)
  {
    Debug.LogError((object) string.Format("Error while loading general save data file!: {0}, enabling error panel for player", (object) e));
    this.encounteredErrorDuringSave = true;
    PreInitSceneScript objectOfType = UnityEngine.Object.FindObjectOfType<PreInitSceneScript>();
    if (!((UnityEngine.Object) objectOfType != (UnityEngine.Object) null))
      return;
    objectOfType.EnableFileCorruptedScreen();
  }

  public void LoadSettingsFromPrefs()
  {
    string filePath = "LCGeneralSaveData";
    this.settings.playerHasFinishedSetup = ES3.Load<bool>("PlayerFinishedSetup", filePath, false);
    this.settings.startInOnlineMode = ES3.Load<bool>("StartInOnlineMode", filePath, false);
    this.settings.gammaSetting = ES3.Load<float>("Gamma", filePath, 0.0f);
    this.settings.masterVolume = ES3.Load<float>("MasterVolume", filePath, 1f);
    this.settings.lookSensitivity = ES3.Load<int>("LookSens", filePath, 10);
    this.settings.micEnabled = ES3.Load<bool>("MicEnabled", filePath, true);
    this.settings.pushToTalk = ES3.Load<bool>("PushToTalk", filePath, false);
    this.settings.micDevice = ES3.Load<string>("CurrentMic", filePath, "LCNoMic");
    this.settings.keyBindings = ES3.Load<string>("Bindings", filePath, string.Empty);
    this.settings.framerateCapIndex = ES3.Load<int>("FPSCap", filePath, 0);
    this.settings.fullScreenType = (FullScreenMode) ES3.Load<int>("ScreenMode", filePath, 1);
    this.settings.invertYAxis = ES3.Load<bool>("InvertYAxis", filePath, false);
    this.settings.spiderSafeMode = ES3.Load<bool>("SpiderSafeMode", filePath, false);
    if (!string.IsNullOrEmpty(this.settings.keyBindings))
      this.playerInput.actions.LoadBindingOverridesFromJson(this.settings.keyBindings);
    this.unsavedSettings.CopySettings(this.settings);
  }

  public void SaveSettingsToPrefs()
  {
    string filePath = "LCGeneralSaveData";
    try
    {
      ES3.Save<bool>("PlayerFinishedSetup", this.settings.playerHasFinishedSetup, filePath);
      ES3.Save<bool>("StartInOnlineMode", this.settings.startInOnlineMode, filePath);
      ES3.Save<float>("Gamma", this.settings.gammaSetting, filePath);
      ES3.Save<float>("MasterVolume", this.settings.masterVolume, filePath);
      ES3.Save<int>("LookSens", this.settings.lookSensitivity, filePath);
      ES3.Save<bool>("MicEnabled", this.settings.micEnabled, filePath);
      ES3.Save<bool>("PushToTalk", this.settings.pushToTalk, filePath);
      ES3.Save<string>("CurrentMic", this.settings.micDevice, filePath);
      ES3.Save<string>("Bindings", this.settings.keyBindings, filePath);
      ES3.Save<int>("FPSCap", this.settings.framerateCapIndex, filePath);
      ES3.Save<int>("ScreenMode", (int) this.settings.fullScreenType, filePath);
      ES3.Save<bool>("InvertYAxis", this.settings.invertYAxis, filePath);
      ES3.Save<bool>("SpiderSafeMode", this.settings.spiderSafeMode, filePath);
    }
    catch (Exception ex)
    {
      this.DisplaySaveFileError(ex);
    }
  }

  public void UpdateAllKeybindOptions()
  {
    foreach (SettingsOption settingsOption in UnityEngine.Object.FindObjectsOfType<SettingsOption>(true))
      settingsOption.SetBindingToCurrentSetting();
    KepRemapPanel objectOfType = UnityEngine.Object.FindObjectOfType<KepRemapPanel>();
    if (!((UnityEngine.Object) objectOfType != (UnityEngine.Object) null))
      return;
    Debug.Log((object) "Reseting keybind UI");
    objectOfType.ResetKeybindsUI();
  }

  public void UpdateGameToMatchSettings()
  {
    this.ChangeGamma(0, this.settings.gammaSetting);
    this.SetFramerateCap(this.settings.framerateCapIndex);
    this.SetFullscreenMode((int) this.settings.fullScreenType);
    AudioListener.volume = this.settings.masterVolume;
    this.UpdateMicPushToTalkButton();
    this.RefreshAndDisplayCurrentMicrophone();
    foreach (SettingsOption settingsOption in UnityEngine.Object.FindObjectsOfType<SettingsOption>(true))
      settingsOption.SetValueToMatchSettings();
    if (!((UnityEngine.Object) this.comms != (UnityEngine.Object) null) || !((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null))
      return;
    this.comms.IsMuted = !this.settings.micEnabled;
  }

  public void SetOption(SettingsOptionType optionType, int value)
  {
    if ((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null)
      this.SettingsAudio.PlayOneShot(GameNetworkManager.Instance.buttonTuneSFX);
    Debug.Log((object) string.Format("Set settings not applied!; {0}", (object) optionType));
    this.SetChangesNotAppliedTextVisible();
    switch (optionType)
    {
      case SettingsOptionType.LookSens:
        this.ChangeLookSens(value);
        break;
      case SettingsOptionType.Gamma:
        this.ChangeGamma(value);
        break;
      case SettingsOptionType.MicPushToTalk:
        this.SetMicPushToTalk();
        break;
      case SettingsOptionType.MicEnabled:
        this.SetMicrophoneEnabled();
        break;
      case SettingsOptionType.MicDevice:
        this.SwitchMicrophoneSetting();
        break;
      case SettingsOptionType.MasterVolume:
        this.ChangeMasterVolume(value);
        break;
      case SettingsOptionType.FramerateCap:
        this.SetFramerateCap(value);
        break;
      case SettingsOptionType.FullscreenType:
        this.SetFullscreenMode(value);
        break;
      case SettingsOptionType.InvertYAxis:
        this.SetInvertYAxis();
        break;
      case SettingsOptionType.SpiderSafeMode:
        this.SetSpiderSafeMode();
        break;
    }
  }

  private void SetSpiderSafeMode()
  {
    this.unsavedSettings.spiderSafeMode = !this.unsavedSettings.spiderSafeMode;
  }

  private void SetInvertYAxis()
  {
    this.unsavedSettings.invertYAxis = !this.unsavedSettings.invertYAxis;
  }

  private void SetFullscreenMode(int value)
  {
    Screen.fullScreenMode = (FullScreenMode) value;
    this.unsavedSettings.fullScreenType = (FullScreenMode) value;
  }

  private void SetFramerateCap(int value)
  {
    switch (value)
    {
      case 0:
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;
        break;
      case 1:
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 250;
        break;
      default:
        QualitySettings.vSyncCount = 0;
        if (value == 2)
        {
          Application.targetFrameRate = 144;
          break;
        }
        if (value == 3)
        {
          Application.targetFrameRate = 120;
          break;
        }
        if (value == 4)
        {
          Application.targetFrameRate = 60;
          break;
        }
        if (value == 5)
        {
          Application.targetFrameRate = 30;
          break;
        }
        break;
    }
    this.unsavedSettings.framerateCapIndex = value;
  }

  public void ChangeGamma(int setTo, float overrideWithFloat = -500f)
  {
    float w = Mathf.Clamp((float) setTo * 0.05f, -0.85f, 2f);
    if ((double) overrideWithFloat != -500.0)
      w = overrideWithFloat;
    LiftGammaGain component;
    if (this.universalVolume.sharedProfile.TryGet<LiftGammaGain>(out component))
      component.gamma.SetValue((VolumeParameter) new Vector4Parameter(new Vector4(0.0f, 0.0f, 0.0f, w), true));
    this.unsavedSettings.gammaSetting = w;
  }

  public void ChangeMasterVolume(int setTo)
  {
    this.unsavedSettings.masterVolume = (float) setTo / 100f;
    AudioListener.volume = (float) setTo / 100f;
  }

  public void ChangeLookSens(int setTo)
  {
    this.unsavedSettings.lookSensitivity = setTo;
    Debug.Log((object) string.Format("Set mouse sensitivity to new value: {0}", (object) setTo));
  }

  public void RefreshAndDisplayCurrentMicrophone(bool saveResult = true)
  {
    IngamePlayerSettings.Settings settings = !saveResult ? this.unsavedSettings : this.settings;
    settings.micDeviceIndex = 0;
    bool flag = false;
    string str = !saveResult ? this.unsavedSettings.micDevice : this.settings.micDevice;
    for (int index = 0; index < Microphone.devices.Length; ++index)
    {
      if (Microphone.devices[index] == str)
      {
        settings.micDeviceIndex = index;
        settings.micDevice = Microphone.devices[index];
        flag = true;
        break;
      }
    }
    if (!flag)
    {
      if (Microphone.devices.Length == 0)
      {
        this.SetSettingsOptionsText(SettingsOptionType.MicDevice, "No device found \n (click to refresh)");
        settings.micDevice = "LCNoMic";
        Debug.Log((object) "No recording devices found");
        return;
      }
      settings.micDevice = Microphone.devices[0];
      int num = saveResult ? 1 : 0;
    }
    this.SetSettingsOptionsText(SettingsOptionType.MicDevice, "Current input device: \n " + settings.micDevice);
    if (!saveResult || !((UnityEngine.Object) this.comms != (UnityEngine.Object) null))
      return;
    this.comms.MicrophoneName = settings.micDevice;
  }

  public void SetSettingsOptionsText(SettingsOptionType optionType, string setToText)
  {
    SettingsOption[] objectsOfType = UnityEngine.Object.FindObjectsOfType<SettingsOption>(true);
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].optionType == optionType && (UnityEngine.Object) objectsOfType[index].textElement != (UnityEngine.Object) null)
        objectsOfType[index].textElement.text = setToText;
    }
  }

  public void SwitchMicrophoneSetting()
  {
    if (Microphone.devices.Length == 0)
    {
      Debug.Log((object) "No mics found when trying to switch");
    }
    else
    {
      Debug.Log((object) "Switching microphone");
      this.unsavedSettings.micDeviceIndex = ++this.unsavedSettings.micDeviceIndex % Microphone.devices.Length;
      this.unsavedSettings.micDevice = Microphone.devices[this.unsavedSettings.micDeviceIndex];
      this.SetSettingsOptionsText(SettingsOptionType.MicDevice, "Current input device: \n " + this.unsavedSettings.micDevice);
      DisplayPlayerMicVolume objectOfType = UnityEngine.Object.FindObjectOfType<DisplayPlayerMicVolume>();
      if ((UnityEngine.Object) objectOfType != (UnityEngine.Object) null)
        objectOfType.SwitchMicrophone();
      if (!((UnityEngine.Object) this.comms != (UnityEngine.Object) null))
        return;
      this.comms.MicrophoneName = this.unsavedSettings.micDevice;
    }
  }

  public void SetMicrophoneEnabled()
  {
    this.unsavedSettings.micEnabled = !this.unsavedSettings.micEnabled;
    if (!((UnityEngine.Object) this.comms != (UnityEngine.Object) null) || !((UnityEngine.Object) StartOfRound.Instance != (UnityEngine.Object) null))
      return;
    this.comms.IsMuted = !this.settings.micEnabled;
  }

  public void SetMicPushToTalk()
  {
    this.unsavedSettings.pushToTalk = !this.unsavedSettings.pushToTalk;
    if (this.unsavedSettings.pushToTalk)
      this.SetSettingsOptionsText(SettingsOptionType.MicPushToTalk, "MODE: Push to talk");
    else
      this.SetSettingsOptionsText(SettingsOptionType.MicPushToTalk, "MODE: Voice activation");
  }

  public void UpdateMicPushToTalkButton()
  {
    if (this.settings.pushToTalk)
      this.SetSettingsOptionsText(SettingsOptionType.MicPushToTalk, "MODE: Push to talk");
    else
      this.SetSettingsOptionsText(SettingsOptionType.MicPushToTalk, "MODE: Voice activation");
  }

  public void SetPlayerFinishedLaunchOptions()
  {
    this.settings.playerHasFinishedSetup = true;
    this.unsavedSettings.playerHasFinishedSetup = true;
    ES3.Save<bool>("PlayerFinishedSetup", true, "LCGeneralSaveData");
  }

  public void SetLaunchInOnlineMode(bool enable)
  {
    this.settings.startInOnlineMode = enable;
    this.unsavedSettings.startInOnlineMode = enable;
    ES3.Save<bool>("StartInOnlineMode", enable, "LCGeneralSaveData");
  }

  public void RebindKey(
    InputActionReference rebindableAction,
    SettingsOption optionUI,
    int rebindIndex,
    bool gamepadRebinding = false)
  {
    if (this.rebindingOperation != null)
    {
      this.rebindingOperation.Dispose();
      if ((UnityEngine.Object) this.currentRebindingKeyUI != (UnityEngine.Object) null)
      {
        this.currentRebindingKeyUI.currentlyUsedKeyText.enabled = true;
        this.currentRebindingKeyUI.waitingForInput.SetActive(false);
      }
    }
    optionUI.currentlyUsedKeyText.enabled = false;
    optionUI.waitingForInput.SetActive(true);
    this.playerInput.DeactivateInput();
    this.currentRebindingKeyUI = optionUI;
    bool getBindingIndexManually = rebindIndex != -1;
    if (rebindIndex == -1)
      rebindIndex = 0;
    Debug.Log((object) string.Format("Rebinding starting.. rebindIndex: {0}", (object) rebindIndex));
    this.rebindingOperation = !gamepadRebinding ? rebindableAction.action.PerformInteractiveRebinding(rebindIndex).OnMatchWaitForAnother(0.1f).WithControlsHavingToMatchPath("<Keyboard>").WithControlsHavingToMatchPath("<Mouse>").WithControlsExcluding("<Mouse>/scroll/y").WithCancelingThrough(this.playerInput.actions.FindAction("OpenMenu", false).controls[0]).OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>) (operation => this.CompleteRebind(optionUI, getBindingIndexManually, rebindIndex))).Start() : rebindableAction.action.PerformInteractiveRebinding(rebindIndex).OnMatchWaitForAnother(0.1f).WithControlsHavingToMatchPath("<Gamepad>").WithCancelingThrough(this.playerInput.actions.FindAction("OpenMenu", false).controls[2]).OnComplete((Action<InputActionRebindingExtensions.RebindingOperation>) (operation => this.CompleteRebind(optionUI, getBindingIndexManually, rebindIndex))).Start();
    Debug.Log((object) "Rebinding starting.. B");
  }

  public void CompleteRebind(
    SettingsOption optionUI,
    bool getBindingIndexManually,
    int setBindingIndex = 0)
  {
    InputAction action = this.rebindingOperation.action;
    if (this.rebindingOperation != null)
      this.rebindingOperation.Dispose();
    this.playerInput.ActivateInput();
    int index;
    if (!getBindingIndexManually)
    {
      index = action.GetBindingIndexForControl(action.controls[0]);
      Debug.Log((object) string.Format("Setting binding index to default which is {0}", (object) index));
    }
    else
    {
      Debug.Log((object) string.Format("Setting binding index to manual which is {0}", (object) setBindingIndex));
      index = setBindingIndex;
    }
    optionUI.currentlyUsedKeyText.text = InputControlPath.ToHumanReadableString(action.bindings[index].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
    optionUI.currentlyUsedKeyText.enabled = true;
    optionUI.waitingForInput.SetActive(false);
    Debug.Log((object) "Rebinding finishing.. A");
    this.unsavedSettings.keyBindings = this.playerInput.actions.SaveBindingOverridesAsJson();
    this.SetChangesNotAppliedTextVisible();
    Debug.Log((object) "Rebinding finishing.. B");
  }

  public void CancelRebind(SettingsOption optionUI = null)
  {
    if (this.rebindingOperation != null)
      this.rebindingOperation.Dispose();
    try
    {
      this.playerInput.ActivateInput();
    }
    catch (Exception ex)
    {
      Debug.Log((object) string.Format("Unable to activate input!: {0}", (object) ex));
    }
    if ((UnityEngine.Object) optionUI == (UnityEngine.Object) null)
      return;
    optionUI.currentlyUsedKeyText.enabled = true;
    optionUI.waitingForInput.SetActive(false);
  }

  public void ResetSettingsToDefault()
  {
    this.SetChangesNotAppliedTextVisible(false);
    IngamePlayerSettings.Settings copyFrom = new IngamePlayerSettings.Settings(this.settings.playerHasFinishedSetup, this.settings.startInOnlineMode);
    this.settings.CopySettings(copyFrom);
    this.unsavedSettings.CopySettings(copyFrom);
    this.SaveSettingsToPrefs();
    this.UpdateGameToMatchSettings();
  }

  public void ResetAllKeybinds()
  {
    this.CancelRebind();
    this.playerInput.actions.RemoveAllBindingOverrides();
    this.unsavedSettings.keyBindings = string.Empty;
    this.SetChangesNotAppliedTextVisible();
    this.UpdateAllKeybindOptions();
  }

  public void SaveChangedSettings()
  {
    this.SetChangesNotAppliedTextVisible(false);
    Debug.Log((object) "Saving changed settings");
    this.settings.CopySettings(this.unsavedSettings);
    this.SaveSettingsToPrefs();
    this.UpdateGameToMatchSettings();
  }

  public void DisplayConfirmChangesScreen(bool visible)
  {
    MenuManager objectOfType1 = UnityEngine.Object.FindObjectOfType<MenuManager>();
    if ((UnityEngine.Object) objectOfType1 != (UnityEngine.Object) null)
    {
      objectOfType1.PleaseConfirmChangesSettingsPanel.SetActive(visible);
      objectOfType1.KeybindsPanel.SetActive(!visible);
      objectOfType1.PleaseConfirmChangesSettingsPanelBackButton.Select();
    }
    else
    {
      QuickMenuManager objectOfType2 = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
      if (!((UnityEngine.Object) objectOfType2 != (UnityEngine.Object) null))
        return;
      objectOfType2.PleaseConfirmChangesSettingsPanel.SetActive(visible);
      objectOfType2.KeybindsPanel.SetActive(!visible);
      objectOfType2.PleaseConfirmChangesSettingsPanelBackButton.Select();
    }
  }

  public void DiscardChangedSettings()
  {
    this.SetChangesNotAppliedTextVisible(false);
    Debug.Log((object) "Discarding changed settings");
    this.unsavedSettings.CopySettings(this.settings);
    if (!string.IsNullOrEmpty(this.settings.keyBindings))
      this.playerInput.actions.LoadBindingOverridesFromJson(this.settings.keyBindings);
    else
      this.playerInput.actions.RemoveAllBindingOverrides();
    this.UpdateGameToMatchSettings();
  }

  private void OnDestroy()
  {
    SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
  }

  private void OnDisable()
  {
    SceneManager.sceneLoaded -= new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
  }

  private void OnEnable()
  {
    SceneManager.sceneLoaded += new UnityAction<Scene, LoadSceneMode>(this.OnSceneLoaded);
  }

  private void OnSceneLoaded(Scene scene, LoadSceneMode loadType)
  {
    if (loadType != LoadSceneMode.Single)
      return;
    this.UpdateGameToMatchSettings();
    this.comms = UnityEngine.Object.FindObjectOfType<DissonanceComms>();
  }

  private void SetChangesNotAppliedTextVisible(bool visible = true)
  {
    this.changesNotApplied = visible;
    MenuManager objectOfType1 = UnityEngine.Object.FindObjectOfType<MenuManager>();
    if ((UnityEngine.Object) objectOfType1 != (UnityEngine.Object) null)
    {
      objectOfType1.changesNotAppliedText.enabled = visible;
      if (visible)
        objectOfType1.settingsBackButton.text = "DISCARD";
      else
        objectOfType1.settingsBackButton.text = "BACK";
    }
    else
    {
      QuickMenuManager objectOfType2 = UnityEngine.Object.FindObjectOfType<QuickMenuManager>();
      if (!((UnityEngine.Object) objectOfType2 != (UnityEngine.Object) null))
        return;
      objectOfType2.changesNotAppliedText.enabled = visible;
      if (visible)
        objectOfType2.settingsBackButton.text = "Discard changes";
      else
        objectOfType2.settingsBackButton.text = "Back";
    }
  }

  [Serializable]
  public class Settings
  {
    public bool playerHasFinishedSetup;
    public bool startInOnlineMode = true;
    public float gammaSetting;
    public int lookSensitivity = 10;
    public bool invertYAxis;
    public float masterVolume = 1f;
    public int framerateCapIndex;
    public FullScreenMode fullScreenType;
    [Header("MIC SETTINGS")]
    public bool micEnabled = true;
    public bool pushToTalk;
    public int micDeviceIndex;
    public string micDevice = string.Empty;
    [Header("BINDINGS")]
    public string keyBindings = string.Empty;
    [Header("ACCESSIBILITY")]
    public bool spiderSafeMode;

    public Settings(bool finishedSetup = true, bool onlineMode = true)
    {
      this.playerHasFinishedSetup = finishedSetup;
      this.startInOnlineMode = onlineMode;
    }

    public void CopySettings(IngamePlayerSettings.Settings copyFrom)
    {
      this.playerHasFinishedSetup = copyFrom.playerHasFinishedSetup;
      this.startInOnlineMode = copyFrom.startInOnlineMode;
      this.gammaSetting = copyFrom.gammaSetting;
      this.lookSensitivity = copyFrom.lookSensitivity;
      this.micEnabled = copyFrom.micEnabled;
      this.pushToTalk = copyFrom.pushToTalk;
      this.micDeviceIndex = copyFrom.micDeviceIndex;
      this.micDevice = copyFrom.micDevice;
      this.keyBindings = copyFrom.keyBindings;
      this.masterVolume = copyFrom.masterVolume;
      this.framerateCapIndex = copyFrom.framerateCapIndex;
      this.fullScreenType = copyFrom.fullScreenType;
      this.invertYAxis = copyFrom.invertYAxis;
      this.spiderSafeMode = copyFrom.spiderSafeMode;
    }
  }
}
