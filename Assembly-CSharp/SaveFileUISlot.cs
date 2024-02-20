// Decompiled with JetBrains decompiler
// Type: SaveFileUISlot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class SaveFileUISlot : MonoBehaviour
{
  public Button fileButton;
  public Animator buttonAnimator;
  public TextMeshProUGUI fileStatsText;
  public int fileNum;
  private string fileString;
  public TextMeshProUGUI fileNotCompatibleAlert;
  public TextMeshProUGUI specialTipText;
  public TextMeshProUGUI fileNameText;

  private void Awake()
  {
    switch (this.fileNum)
    {
      case -1:
        this.fileString = "LCChallengeFile";
        break;
      case 0:
        this.fileString = "LCSaveFile1";
        break;
      case 1:
        this.fileString = "LCSaveFile2";
        break;
      case 2:
        this.fileString = "LCSaveFile3";
        break;
      default:
        this.fileString = "LCSaveFile1";
        break;
    }
  }

  private void SetChallengeFileSettings()
  {
    if (Object.FindObjectOfType<MenuManager>().hasChallengeBeenCompleted)
    {
      int num = ES3.Load<int>("ProfitEarned", this.fileString, 0);
      Debug.Log((object) ES3.Load<int>("ProfitEarned", this.fileString, 0));
      Debug.Log((object) string.Format("scrapEarnedInFile: {0}", (object) num));
      this.fileStatsText.enabled = true;
      this.fileStatsText.text = string.Format("${0} Collected", (object) num);
      if (!(GameNetworkManager.Instance.currentSaveFileName == "LCChallengeFile"))
        return;
      GameNetworkManager.Instance.currentSaveFileName = "LCSaveFile1";
      GameNetworkManager.Instance.saveFileNum = 0;
      this.SetButtonColorForAllFileSlots();
    }
    else
      this.fileStatsText.enabled = false;
  }

  private void OnEnable()
  {
    if (this.fileNum == -1)
      this.fileNameText.text = GameNetworkManager.Instance.GetNameForWeekNumber();
    if (ES3.FileExists(this.fileString))
    {
      if (this.fileNum == -1)
        this.SetChallengeFileSettings();
      else
        this.fileStatsText.text = string.Format("${0}\nDays: {1}", (object) ES3.Load<int>("GroupCredits", this.fileString, 0), (object) ES3.Load<int>("Stats_DaysSpent", this.fileString, 0));
    }
    else
      this.fileStatsText.text = "";
    if (this.fileNum == -1 || Object.FindObjectOfType<MenuManager>().filesCompatible[this.fileNum])
      return;
    this.fileNotCompatibleAlert.enabled = true;
  }

  public void SetButtonColor()
  {
    this.buttonAnimator.SetBool("isPressed", GameNetworkManager.Instance.currentSaveFileName == this.fileString);
    if (!((Object) this.specialTipText != (Object) null) || !(GameNetworkManager.Instance.currentSaveFileName != this.fileString))
      return;
    this.specialTipText.enabled = false;
  }

  public void SetFileToThis()
  {
    if (Object.FindObjectOfType<MenuManager>().requestingLeaderboard)
      return;
    if (this.fileNum == -1 && Object.FindObjectOfType<MenuManager>().hasChallengeBeenCompleted)
    {
      Object.FindObjectOfType<MenuManager>().EnableLeaderboardDisplay(true);
    }
    else
    {
      Object.FindObjectOfType<MenuManager>().EnableLeaderboardDisplay(false);
      if (this.fileNum == -1)
      {
        this.specialTipText.text = "This is the weekly challenge moon. You have one day to make as much profit as possible.";
        this.specialTipText.enabled = true;
      }
    }
    GameNetworkManager.Instance.currentSaveFileName = this.fileString;
    GameNetworkManager.Instance.saveFileNum = this.fileNum;
    this.SetButtonColorForAllFileSlots();
  }

  public void SetButtonColorForAllFileSlots()
  {
    foreach (SaveFileUISlot saveFileUiSlot in Object.FindObjectsOfType<SaveFileUISlot>())
      saveFileUiSlot.SetButtonColor();
  }
}
