// Decompiled with JetBrains decompiler
// Type: LobbySlot
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Steamworks;
using Steamworks.Data;
using System;
using TMPro;
using UnityEngine;

#nullable disable
public class LobbySlot : MonoBehaviour
{
  public MenuManager menuScript;
  public TextMeshProUGUI LobbyName;
  public TextMeshProUGUI playerCount;
  public SteamId lobbyId;
  public Lobby thisLobby;
  private static Coroutine timeOutLobbyRefreshCoroutine;

  private void Awake() => this.menuScript = UnityEngine.Object.FindObjectOfType<MenuManager>();

  public void JoinButton()
  {
    if (GameNetworkManager.Instance.waitingForLobbyDataRefresh)
      return;
    LobbySlot.JoinLobbyAfterVerifying(this.thisLobby, this.lobbyId);
  }

  public static void JoinLobbyAfterVerifying(Lobby lobby, SteamId lobbyId)
  {
    if (GameNetworkManager.Instance.waitingForLobbyDataRefresh)
      return;
    MenuManager objectOfType = UnityEngine.Object.FindObjectOfType<MenuManager>();
    if ((UnityEngine.Object) objectOfType == (UnityEngine.Object) null)
      return;
    objectOfType.serverListUIContainer.SetActive(false);
    objectOfType.menuButtons.SetActive(true);
    Debug.Log((object) string.Format("Lobby id joining: {0}", (object) lobbyId));
    SteamMatchmaking.OnLobbyDataChanged += new Action<Lobby>(LobbySlot.OnLobbyDataRefresh);
    GameNetworkManager.Instance.waitingForLobbyDataRefresh = true;
    Debug.Log((object) "refreshing lobby...");
    if (lobby.Refresh())
    {
      LobbySlot.timeOutLobbyRefreshCoroutine = GameNetworkManager.Instance.StartCoroutine(GameNetworkManager.Instance.TimeOutLobbyRefresh());
      Debug.Log((object) "Waiting for lobby data refresh");
      UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(true);
    }
    else
    {
      Debug.Log((object) "Could not refresh lobby");
      SteamMatchmaking.OnLobbyDataChanged -= new Action<Lobby>(LobbySlot.OnLobbyDataRefresh);
      UnityEngine.Object.FindObjectOfType<MenuManager>().SetLoadingScreen(false, overrideMessage: "Error! Could not get the lobby data. Are you offline?");
    }
  }

  public static void OnLobbyDataRefresh(Lobby lobby)
  {
    if (LobbySlot.timeOutLobbyRefreshCoroutine != null)
    {
      GameNetworkManager.Instance.StopCoroutine(LobbySlot.timeOutLobbyRefreshCoroutine);
      LobbySlot.timeOutLobbyRefreshCoroutine = (Coroutine) null;
    }
    if (!GameNetworkManager.Instance.waitingForLobbyDataRefresh)
    {
      Debug.Log((object) "Not waiting for lobby data refresh; returned");
    }
    else
    {
      GameNetworkManager.Instance.waitingForLobbyDataRefresh = false;
      SteamMatchmaking.OnLobbyDataChanged -= new Action<Lobby>(LobbySlot.OnLobbyDataRefresh);
      Debug.Log((object) string.Format("Got lobby data refresh!; {0}", (object) lobby.Id));
      Debug.Log((object) string.Format("Members in lobby: {0}", (object) lobby.MemberCount));
      if (!GameNetworkManager.Instance.LobbyDataIsJoinable(lobby))
        return;
      GameNetworkManager.Instance.JoinLobby(lobby, lobby.Id);
    }
  }

  private void Update()
  {
  }
}
