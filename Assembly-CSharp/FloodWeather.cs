// Decompiled with JetBrains decompiler
// Type: FloodWeather
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class FloodWeather : MonoBehaviour
{
  public AudioSource waterAudio;
  private float floodLevelOffset;
  private float previousGlobalTime;

  private void OnEnable()
  {
    if ((Object) TimeOfDay.Instance == (Object) null)
      return;
    this.transform.position = new Vector3(0.0f, TimeOfDay.Instance.currentWeatherVariable, 0.0f);
    TimeOfDay.Instance.onTimeSync.AddListener(new UnityAction(this.OnGlobalTimeSync));
  }

  private void OnDisable()
  {
    this.waterAudio.volume = 0.0f;
    this.floodLevelOffset = 0.0f;
    TimeOfDay.Instance.onTimeSync.RemoveListener(new UnityAction(this.OnGlobalTimeSync));
    this.transform.position = new Vector3(0.0f, -50f, 0.0f);
  }

  private void OnGlobalTimeSync()
  {
    this.floodLevelOffset = Mathf.Clamp(TimeOfDay.Instance.globalTime / 1080f, 0.0f, 100f) * TimeOfDay.Instance.currentWeatherVariable2;
  }

  private void Update()
  {
    if ((Object) TimeOfDay.Instance == (Object) null)
      return;
    this.transform.position = Vector3.MoveTowards(this.transform.position, new Vector3(0.0f, TimeOfDay.Instance.currentWeatherVariable, 0.0f) + Vector3.up * this.floodLevelOffset, 0.5f * Time.deltaTime);
    if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
    {
      this.waterAudio.volume = 0.0f;
    }
    else
    {
      this.waterAudio.transform.position = new Vector3(GameNetworkManager.Instance.localPlayerController.transform.position.x, this.transform.position.y + 3f, GameNetworkManager.Instance.localPlayerController.transform.position.z);
      if (Physics.Linecast(GameNetworkManager.Instance.localPlayerController.gameplayCamera.transform.position, this.waterAudio.transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        this.waterAudio.volume = Mathf.Lerp(this.waterAudio.volume, 0.0f, 0.5f * Time.deltaTime);
      else
        this.waterAudio.volume = Mathf.Lerp(this.waterAudio.volume, 1f, 0.5f * Time.deltaTime);
    }
  }
}
