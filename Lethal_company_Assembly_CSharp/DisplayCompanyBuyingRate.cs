// Decompiled with JetBrains decompiler
// Type: DisplayCompanyBuyingRate
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using TMPro;
using UnityEngine;

#nullable disable
public class DisplayCompanyBuyingRate : MonoBehaviour
{
  public TextMeshProUGUI displayText;

  private void Update()
  {
    this.displayText.text = string.Format("{0}%", (object) Mathf.RoundToInt(StartOfRound.Instance.companyBuyingRate * 100f));
  }
}
