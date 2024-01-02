// Decompiled with JetBrains decompiler
// Type: SaveFileUISlot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;

#nullable disable
public class SaveFileUISlot : MonoBehaviour
{
  public Animator buttonAnimator;
  public TextMeshProUGUI fileStatsText;
  public int fileNum;
  private string fileString;
  public TextMeshProUGUI fileNotCompatibleAlert;

  private void Awake()
  {
    switch (this.fileNum)
    {
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

  private void OnEnable()
  {
    if (ES3.FileExists(this.fileString))
      this.fileStatsText.text = string.Format("${0}\nDays: {1}", (object) ES3.Load<int>("GroupCredits", this.fileString, 30), (object) ES3.Load<int>("Stats_DaysSpent", this.fileString, 0));
    else
      this.fileStatsText.text = "";
    if (Object.FindObjectOfType<MenuManager>().filesCompatible[this.fileNum])
      return;
    this.fileNotCompatibleAlert.enabled = true;
  }

  public void SetButtonColor()
  {
    this.buttonAnimator.SetBool("isPressed", GameNetworkManager.Instance.currentSaveFileName == this.fileString);
  }

  public void SetFileToThis()
  {
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
