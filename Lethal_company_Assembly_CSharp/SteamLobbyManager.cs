// Decompiled with JetBrains decompiler
// Type: SteamLobbyManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using Steamworks.Data;
using Steamworks.ServerList;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

#nullable disable
public class SteamLobbyManager : MonoBehaviour
{
  private Internet Request;
  private Lobby[] currentLobbyList;
  public TextMeshProUGUI serverListBlankText;
  public Transform levelListContainer;
  public GameObject LobbySlotPrefab;
  private float lobbySlotPositionOffset;
  public int sortByDistanceSetting = 2;
  private float refreshServerListTimer;
  public bool censorOffensiveLobbyNames = true;
  private Coroutine loadLobbyListCoroutine;

  private void Start()
  {
  }

  public void ChangeDistanceSort(int newValue) => this.sortByDistanceSetting = newValue;

  private void DebugLogServerList()
  {
    if (this.currentLobbyList != null)
    {
      for (int index = 0; index < this.currentLobbyList.Length; ++index)
      {
        Debug.Log((object) string.Format("Lobby #{0} id: {1}; members: {2}", (object) index, (object) this.currentLobbyList[index].Id, (object) this.currentLobbyList[index].MemberCount));
        uint ip = 0;
        ushort port = 0;
        SteamId serverId = new SteamId();
        Debug.Log((object) string.Format("Is lobby #{0} valid?: {1}", (object) index, (object) this.currentLobbyList[index].GetGameServer(ref ip, ref port, ref serverId)));
      }
    }
    else
      Debug.Log((object) "Server list null");
  }

  public void RefreshServerListButton()
  {
    if ((double) this.refreshServerListTimer < 1.0)
      return;
    this.LoadServerList();
  }

  public async void LoadServerList()
  {
    SteamLobbyManager steamLobbyManager = this;
    if (GameNetworkManager.Instance.waitingForLobbyDataRefresh)
      return;
    if (steamLobbyManager.loadLobbyListCoroutine != null)
      steamLobbyManager.StopCoroutine(steamLobbyManager.loadLobbyListCoroutine);
    steamLobbyManager.refreshServerListTimer = 0.0f;
    steamLobbyManager.serverListBlankText.text = "Loading server list...";
    steamLobbyManager.currentLobbyList = (Lobby[]) null;
    foreach (Component component in Object.FindObjectsOfType<LobbySlot>())
      Object.Destroy((Object) component.gameObject);
    LobbyQuery lobbyQuery;
    switch (steamLobbyManager.sortByDistanceSetting)
    {
      case 0:
        lobbyQuery = SteamMatchmaking.LobbyList;
        lobbyQuery.FilterDistanceClose();
        break;
      case 1:
        lobbyQuery = SteamMatchmaking.LobbyList;
        lobbyQuery.FilterDistanceFar();
        break;
      case 2:
        lobbyQuery = SteamMatchmaking.LobbyList;
        lobbyQuery.FilterDistanceWorldwide();
        break;
    }
    steamLobbyManager.currentLobbyList = (Lobby[]) null;
    Debug.Log((object) "Requested server list");
    GameNetworkManager.Instance.waitingForLobbyDataRefresh = true;
    switch (steamLobbyManager.sortByDistanceSetting)
    {
      case 0:
        lobbyQuery = SteamMatchmaking.LobbyList;
        lobbyQuery = lobbyQuery.FilterDistanceClose();
        lobbyQuery = lobbyQuery.WithSlotsAvailable(1);
        lobbyQuery = lobbyQuery.WithKeyValue("vers", GameNetworkManager.Instance.gameVersionNum.ToString());
        Lobby[] lobbyArray1 = await lobbyQuery.RequestAsync();
        steamLobbyManager.currentLobbyList = lobbyArray1;
        break;
      case 1:
        lobbyQuery = SteamMatchmaking.LobbyList;
        lobbyQuery = lobbyQuery.FilterDistanceFar();
        lobbyQuery = lobbyQuery.WithSlotsAvailable(1);
        lobbyQuery = lobbyQuery.WithKeyValue("vers", GameNetworkManager.Instance.gameVersionNum.ToString());
        Lobby[] lobbyArray2 = await lobbyQuery.RequestAsync();
        steamLobbyManager.currentLobbyList = lobbyArray2;
        break;
      default:
        lobbyQuery = SteamMatchmaking.LobbyList;
        lobbyQuery = lobbyQuery.FilterDistanceWorldwide();
        lobbyQuery = lobbyQuery.WithSlotsAvailable(1);
        lobbyQuery = lobbyQuery.WithKeyValue("vers", GameNetworkManager.Instance.gameVersionNum.ToString());
        Lobby[] lobbyArray3 = await lobbyQuery.RequestAsync();
        steamLobbyManager.currentLobbyList = lobbyArray3;
        break;
    }
    GameNetworkManager.Instance.waitingForLobbyDataRefresh = false;
    if (steamLobbyManager.currentLobbyList != null)
    {
      Debug.Log((object) "Got lobby list!");
      steamLobbyManager.DebugLogServerList();
      if (steamLobbyManager.currentLobbyList.Length == 0)
        steamLobbyManager.serverListBlankText.text = "No available servers to join.";
      else
        steamLobbyManager.serverListBlankText.text = "";
      steamLobbyManager.lobbySlotPositionOffset = 0.0f;
      steamLobbyManager.loadLobbyListCoroutine = steamLobbyManager.StartCoroutine(steamLobbyManager.loadLobbyListAndFilter(steamLobbyManager.currentLobbyList));
    }
    else
    {
      Debug.Log((object) "Lobby list is null after request.");
      steamLobbyManager.serverListBlankText.text = "No available servers to join.";
    }
  }

  private IEnumerator loadLobbyListAndFilter(Lobby[] lobbyList)
  {
    string[] offensiveWords = new string[21]
    {
      "nigger",
      "faggot",
      "n1g",
      "nigers",
      "cunt",
      "pussies",
      "pussy",
      "minors",
      "chink",
      "buttrape",
      "molest",
      "rape",
      "coon",
      "negro",
      "beastiality",
      "cocks",
      "cumshot",
      "ejaculate",
      "pedophile",
      "furfag",
      "necrophilia"
    };
    for (int i = 0; i < this.currentLobbyList.Length; ++i)
    {
      Friend[] array = SteamFriends.GetBlocked().ToArray<Friend>();
      if (array != null)
      {
        for (int index = 0; index < array.Length; ++index)
        {
          Debug.Log((object) string.Format("blocked user: {0}; id: {1}", (object) array[index].Name, (object) array[index].Id));
          if (this.currentLobbyList[i].IsOwnedBy(array[index].Id))
            Debug.Log((object) ("Hiding lobby by blocked user: " + array[index].Name));
        }
      }
      else
        Debug.Log((object) "Blocked users list is null");
      string lobbyName = this.currentLobbyList[i].GetData("name");
      if (lobbyName.Length != 0)
      {
        string lobbyNameNoCapitals = lobbyName.ToLower();
        if (this.censorOffensiveLobbyNames)
        {
          bool nameIsOffensive = false;
          for (int b = 0; b < offensiveWords.Length; ++b)
          {
            if (lobbyNameNoCapitals.Contains(offensiveWords[b]))
            {
              nameIsOffensive = true;
              break;
            }
            if (b % 5 == 0)
              yield return (object) null;
          }
          if (nameIsOffensive)
            continue;
        }
        GameObject gameObject = Object.Instantiate<GameObject>(this.LobbySlotPrefab, this.levelListContainer);
        gameObject.GetComponent<RectTransform>().anchoredPosition = new Vector2(0.0f, 0.0f + this.lobbySlotPositionOffset);
        this.lobbySlotPositionOffset -= 42f;
        LobbySlot componentInChildren = gameObject.GetComponentInChildren<LobbySlot>();
        componentInChildren.LobbyName.text = lobbyName.Substring(0, Mathf.Min(lobbyName.Length, 40));
        componentInChildren.playerCount.text = string.Format("{0} / 4", (object) this.currentLobbyList[i].MemberCount);
        componentInChildren.lobbyId = this.currentLobbyList[i].Id;
        componentInChildren.thisLobby = this.currentLobbyList[i];
        lobbyName = (string) null;
        lobbyNameNoCapitals = (string) null;
      }
    }
  }

  private void Update() => this.refreshServerListTimer += Time.deltaTime;
}
