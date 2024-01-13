// Decompiled with JetBrains decompiler
// Type: KillLocalPlayer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
public class KillLocalPlayer : MonoBehaviour
{
  public bool dontSpawnBody;
  public CauseOfDeath causeOfDeath = CauseOfDeath.Gravity;
  public bool justDamage;
  public StartOfRound playersManager;
  public int deathAnimation;
  [Space(5f)]
  public RoundManager roundManager;
  public Transform spawnEnemyPosition;
  [Space(5f)]
  public int enemySpawnNumber;
  public int playAudioOnDeath = -1;

  public void KillPlayer(PlayerControllerB playerWhoTriggered)
  {
    if (this.justDamage)
    {
      playerWhoTriggered.DamagePlayer(25);
      Debug.Log((object) "DD TRIGGER");
    }
    else
    {
      if (this.playAudioOnDeath != -1)
        SoundManager.Instance.PlayAudio1AtPositionForAllClients(playerWhoTriggered.transform.position, this.playAudioOnDeath);
      playerWhoTriggered.KillPlayer(Vector3.zero, !this.dontSpawnBody, this.causeOfDeath, this.deathAnimation);
    }
  }

  public void SpawnEnemy()
  {
    if (GameNetworkManager.Instance.localPlayerController.playerClientId != 0UL)
      return;
    this.roundManager.SpawnEnemyOnServer(this.spawnEnemyPosition.position, 0.0f, this.enemySpawnNumber);
  }
}
