// Decompiled with JetBrains decompiler
// Type: SettingsOption
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#nullable disable
public class SettingsOption : MonoBehaviour
{
  public SettingsOptionType optionType;
  public TextMeshProUGUI textElement;
  public Image toggleImage;
  public Sprite enabledImage;
  public Sprite disabledImage;
  private bool askedForConfirmation;
  [Header("Key rebinding")]
  public InputActionReference rebindableAction;
  public int rebindableActionBindingIndex = -1;
  public bool gamepadOnlyRebinding;
  public bool requireButtonType;
  public GameObject waitingForInput;
  public TextMeshProUGUI currentlyUsedKeyText;

  public void CancelRebinds() => IngamePlayerSettings.Instance.CancelRebind();

  public void SetBindingToCurrentSetting()
  {
    if (this.optionType != SettingsOptionType.ChangeBinding)
      return;
    this.currentlyUsedKeyText.text = InputControlPath.ToHumanReadableString(this.rebindableAction.action.bindings[this.rebindableAction.action.GetBindingIndexForControl(this.rebindableAction.action.controls[0])].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
  }

  public void ResetBindingsToDefaultButton() => IngamePlayerSettings.Instance.ResetAllKeybinds();

  public void SetValueToMatchSettings()
  {
    switch (this.optionType)
    {
      case SettingsOptionType.LookSens:
        this.gameObject.GetComponentInChildren<Slider>().SetValueWithoutNotify((float) IngamePlayerSettings.Instance.settings.lookSensitivity);
        break;
      case SettingsOptionType.Gamma:
        this.gameObject.GetComponentInChildren<Slider>().SetValueWithoutNotify(IngamePlayerSettings.Instance.settings.gammaSetting / 0.05f);
        break;
      case SettingsOptionType.MicEnabled:
        this.ToggleEnabledImage(4);
        break;
      case SettingsOptionType.ChangeBinding:
        this.SetBindingToCurrentSetting();
        break;
      case SettingsOptionType.MasterVolume:
        this.gameObject.GetComponentInChildren<Slider>().SetValueWithoutNotify(IngamePlayerSettings.Instance.settings.masterVolume * 100f);
        break;
      case SettingsOptionType.FramerateCap:
        this.gameObject.GetComponentInChildren<TMP_Dropdown>().SetValueWithoutNotify(IngamePlayerSettings.Instance.settings.framerateCapIndex);
        break;
      case SettingsOptionType.FullscreenType:
        this.gameObject.GetComponentInChildren<TMP_Dropdown>().SetValueWithoutNotify((int) IngamePlayerSettings.Instance.settings.fullScreenType);
        break;
      case SettingsOptionType.InvertYAxis:
        this.ToggleEnabledImage(11);
        break;
      case SettingsOptionType.SpiderSafeMode:
        this.ToggleEnabledImage(12);
        break;
    }
  }

  public void SetMasterVolume()
  {
    AudioListener.volume = IngamePlayerSettings.Instance.settings.masterVolume;
  }

  public void StartRebindKey()
  {
    IngamePlayerSettings.Instance.RebindKey(this.rebindableAction, this, this.rebindableActionBindingIndex, this.gamepadOnlyRebinding);
  }

  public void OnEnable()
  {
    if (this.optionType != SettingsOptionType.MicDevice)
      return;
    IngamePlayerSettings.Instance.RefreshAndDisplayCurrentMicrophone();
  }

  public void OnDisable()
  {
    if (this.optionType != SettingsOptionType.ChangeBinding)
      return;
    if (IngamePlayerSettings.Instance.rebindingOperation != null)
      IngamePlayerSettings.Instance.CancelRebind();
    this.currentlyUsedKeyText.enabled = true;
    this.waitingForInput.SetActive(false);
  }

  public void SetSettingsOptionInt(int value)
  {
    IngamePlayerSettings.Instance.SetOption(this.optionType, value);
  }

  public void SetSettingsOptionFloat(float value)
  {
    IngamePlayerSettings.Instance.SetOption(this.optionType, (int) value);
  }

  public void ToggleEnabledImage(int optionType)
  {
    if ((Object) this.toggleImage == (Object) null)
      return;
    SettingsOptionType settingsOptionType = (SettingsOptionType) optionType;
    bool flag = false;
    switch (settingsOptionType)
    {
      case SettingsOptionType.MicEnabled:
        flag = IngamePlayerSettings.Instance.unsavedSettings.micEnabled;
        break;
      case SettingsOptionType.InvertYAxis:
        flag = IngamePlayerSettings.Instance.unsavedSettings.invertYAxis;
        break;
      case SettingsOptionType.SpiderSafeMode:
        flag = IngamePlayerSettings.Instance.unsavedSettings.spiderSafeMode;
        break;
    }
    if (flag)
    {
      this.toggleImage.sprite = this.enabledImage;
      if (!((Object) this.textElement != (Object) null))
        return;
      this.textElement.text = "ENABLED";
    }
    else
    {
      this.toggleImage.sprite = this.disabledImage;
      if (!((Object) this.textElement != (Object) null))
        return;
      this.textElement.text = "DISABLED";
    }
  }

  public void ConfirmSettings() => IngamePlayerSettings.Instance.SaveChangedSettings();

  public void ResetSettingsToDefault() => IngamePlayerSettings.Instance.ResetSettingsToDefault();

  public void CancelSettings() => IngamePlayerSettings.Instance.DiscardChangedSettings();
}
