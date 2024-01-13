// Decompiled with JetBrains decompiler
// Type: DeleteFileButton
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;

#nullable disable
public class DeleteFileButton : MonoBehaviour
{
  public int fileToDelete;
  public AudioClip deleteFileSFX;
  public TextMeshProUGUI deleteFileText;

  public void SetFileToDelete(int fileNum)
  {
    this.fileToDelete = fileNum;
    this.deleteFileText.text = string.Format("Do you want to delete File {0}?", (object) (fileNum + 1));
  }

  public void DeleteFile()
  {
    if (this.fileToDelete >= 3 || this.fileToDelete < 0)
      return;
    string filePath;
    switch (this.fileToDelete)
    {
      case 0:
        filePath = "LCSaveFile1";
        break;
      case 1:
        filePath = "LCSaveFile2";
        break;
      case 2:
        filePath = "LCSaveFile3";
        break;
      default:
        filePath = "LCSaveFile1";
        break;
    }
    if (ES3.FileExists(filePath))
    {
      ES3.DeleteFile(filePath);
      Object.FindObjectOfType<MenuManager>().MenuAudio.PlayOneShot(this.deleteFileSFX);
    }
    SaveFileUISlot[] objectsOfType = Object.FindObjectsOfType<SaveFileUISlot>(true);
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      Debug.Log((object) "AAAAAA");
      Debug.Log((object) this.fileToDelete);
      Debug.Log((object) objectsOfType[index].fileNum);
      if (objectsOfType[index].fileNum == this.fileToDelete)
      {
        objectsOfType[index].fileNotCompatibleAlert.enabled = false;
        Object.FindObjectOfType<MenuManager>().filesCompatible[this.fileToDelete] = true;
      }
    }
  }
}
