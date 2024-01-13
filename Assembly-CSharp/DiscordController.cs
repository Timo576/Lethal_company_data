// Decompiled with JetBrains decompiler
// Type: DiscordController
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Discord;
using System;
using UnityEngine;

#nullable disable
public class DiscordController : MonoBehaviour
{
  public Discord.Discord discord;
  public string status_Details = "In Menu";
  public string status_State;
  public string status_largeText;
  public string status_smallText;
  public string status_largeImage;
  public string status_smallImage;
  public int currentPartySize;
  public int maxPartySize = 4;
  public int timeElapsed;
  public string status_partyId;
  private float timeAtLastStatusUpdate;
  private bool activityEnabled;
  private bool appQuitting;

  public static DiscordController Instance { get; private set; }

  private void Awake()
  {
    if ((UnityEngine.Object) DiscordController.Instance == (UnityEngine.Object) null)
      DiscordController.Instance = this;
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) this);
  }

  private void Start()
  {
    this.discord = new Discord.Discord(1174275017694007318L, 1UL);
    Application.quitting += new Action(this.Application_quitting);
  }

  private void OnDisable()
  {
    Application.quitting -= new Action(this.Application_quitting);
    this.discord.Dispose();
  }

  private void Application_quitting() => this.appQuitting = true;

  private void Update()
  {
    try
    {
      this.discord.RunCallbacks();
    }
    catch
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this);
    }
  }

  public void UpdateStatus(bool clear)
  {
    if (clear && !this.activityEnabled || (double) Time.realtimeSinceStartup - (double) this.timeAtLastStatusUpdate < 2.0)
      return;
    this.timeAtLastStatusUpdate = Time.realtimeSinceStartup;
    try
    {
      ActivityManager activityManager = this.discord.GetActivityManager();
      activityManager.RegisterSteam(1966720U);
      Activity activity = new Activity()
      {
        Details = this.status_Details,
        State = this.status_State,
        Assets = {
          LargeImage = this.status_largeImage,
          LargeText = this.status_largeText,
          SmallImage = this.status_smallImage,
          SmallText = this.status_smallText
        },
        Party = {
          Id = this.status_partyId,
          Size = {
            CurrentSize = this.currentPartySize,
            MaxSize = this.maxPartySize
          }
        }
      };
      if (clear)
      {
        activity.Details = "In menu";
        activity.State = "";
        activity.Party.Size.CurrentSize = 1;
      }
      activityManager.UpdateActivity(activity, (ActivityManager.UpdateActivityHandler) (result =>
      {
        if (result != Result.Ok)
          Debug.LogWarning((object) "Error while updating Discord activity status!");
        else
          this.activityEnabled = true;
      }));
    }
    catch
    {
      UnityEngine.Object.Destroy((UnityEngine.Object) this);
    }
  }
}
