// Decompiled with JetBrains decompiler
// Type: PreInitSceneScript
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#nullable disable
public class PreInitSceneScript : MonoBehaviour
{
  public AudioSource mainAudio;
  public AudioClip hoverSFX;
  public AudioClip selectSFX;
  private bool choseLaunchOption;
  [Header("Other initial launch settings")]
  public Slider gammaSlider;
  public GameObject continueButton;
  public Animator blackTransition;
  public GameObject OnlineModeButton;
  public GameObject[] LaunchSettingsPanels;
  public int currentLaunchSettingPanel;
  public TextMeshProUGUI headerText;
  private bool clickedDeleteFiles;
  public GameObject FileCorruptedPanel;
  public GameObject FileCorruptedDialoguePanel;
  public GameObject FileCorruptedRestartButton;
  public GameObject restartingGameText;
  public GameObject launchSettingsPanelsContainer;

  private void Awake() => DissonanceComms.TestDependencies();

  private void Start()
  {
    this.gammaSlider.value = IngamePlayerSettings.Instance.settings.gammaSetting / 0.05f;
  }

  public void PressContinueButton()
  {
    if (this.currentLaunchSettingPanel >= this.LaunchSettingsPanels.Length)
      return;
    this.LaunchSettingsPanels[this.currentLaunchSettingPanel].SetActive(false);
    ++this.currentLaunchSettingPanel;
    this.LaunchSettingsPanels[this.currentLaunchSettingPanel].SetActive(true);
    this.blackTransition.SetTrigger("Transition");
    if (this.currentLaunchSettingPanel < this.LaunchSettingsPanels.Length - 1)
      return;
    this.continueButton.SetActive(false);
    this.headerText.text = "LAUNCH MODE";
  }

  public void HoverButton() => this.mainAudio.PlayOneShot(this.hoverSFX);

  public void ChooseLaunchOption(bool online)
  {
    if (this.choseLaunchOption)
      return;
    this.choseLaunchOption = true;
    this.mainAudio.PlayOneShot(this.selectSFX);
    IngamePlayerSettings.Instance.SetPlayerFinishedLaunchOptions();
    IngamePlayerSettings.Instance.SaveChangedSettings();
    if (IngamePlayerSettings.Instance.encounteredErrorDuringSave)
      return;
    this.StartCoroutine(this.loadSceneDelayed(online));
  }

  private IEnumerator loadSceneDelayed(bool online)
  {
    yield return (object) new WaitForSeconds(0.2f);
    if (online)
      SceneManager.LoadScene("InitScene");
    else
      SceneManager.LoadScene("InitSceneLANMode");
  }

  public void SetLaunchPanelsEnabled() => this.launchSettingsPanelsContainer.SetActive(true);

  public void SkipToFinalSetting()
  {
    this.LaunchSettingsPanels[this.currentLaunchSettingPanel].SetActive(false);
    this.currentLaunchSettingPanel = this.LaunchSettingsPanels.Length - 1;
    this.LaunchSettingsPanels[this.currentLaunchSettingPanel].SetActive(true);
    this.continueButton.SetActive(false);
    this.headerText.text = "LAUNCH MODE";
    EventSystem.current.SetSelectedGameObject(this.OnlineModeButton);
  }

  public void EnableFileCorruptedScreen()
  {
    this.LaunchSettingsPanels[this.currentLaunchSettingPanel].SetActive(false);
    this.FileCorruptedPanel.SetActive(true);
    EventSystem.current.SetSelectedGameObject(this.FileCorruptedRestartButton);
  }

  public void EraseFileAndRestart() => this.StartCoroutine(this.restartGameDueToCorruptedFile());

  private IEnumerator restartGameDueToCorruptedFile()
  {
    if (ES3.FileExists("LCGeneralSaveData"))
      ES3.DeleteFile("LCGeneralSaveData");
    if (ES3.FileExists("LCSaveFile1"))
      ES3.DeleteFile("LCSaveFile1");
    if (ES3.FileExists("LCSaveFile2"))
      ES3.DeleteFile("LCSaveFile2");
    if (ES3.FileExists("LCSaveFile3"))
      ES3.DeleteFile("LCSaveFile3");
    this.FileCorruptedDialoguePanel.SetActive(false);
    this.restartingGameText.SetActive(true);
    yield return (object) new WaitForSeconds(2f);
    Application.Quit();
  }
}
