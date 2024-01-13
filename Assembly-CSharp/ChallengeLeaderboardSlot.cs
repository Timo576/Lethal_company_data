// Decompiled with JetBrains decompiler
// Type: ChallengeLeaderboardSlot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class ChallengeLeaderboardSlot : MonoBehaviour
{
  public RawImage profileIcon;
  public TextMeshProUGUI userNameText;
  public TextMeshProUGUI rankNumText;
  public TextMeshProUGUI scrapCollectedText;
  public SteamId steamId;

  public void SetSlotValues(
    string userName,
    int rankNum,
    int scrapCollected,
    SteamId playerSteamId,
    int entryDetails)
  {
    this.userNameText.text = userName.Substring(0, Mathf.Min(userName.Length, 15));
    this.rankNumText.text = string.Format("#{0}", (object) rankNum);
    switch (entryDetails)
    {
      case 2:
        this.scrapCollectedText.text = "(Removed score)";
        break;
      case 3:
        this.scrapCollectedText.text = "Deceased";
        break;
      default:
        this.scrapCollectedText.text = string.Format("${0} Collected", (object) scrapCollected);
        break;
    }
    this.steamId = playerSteamId;
    this.profileIcon.color = Color.white;
    HUDManager.FillImageWithSteamProfile(this.profileIcon, (SteamId) (ulong) playerSteamId, false);
  }

  public void ClickProfileIcon()
  {
    if (GameNetworkManager.Instance.disableSteam || (ulong) this.steamId == 0UL)
      return;
    SteamFriends.OpenUserOverlay(this.steamId, "steamid");
  }
}
