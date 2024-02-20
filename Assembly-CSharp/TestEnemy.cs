// Decompiled with JetBrains decompiler
// Type: TestEnemy
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using UnityEngine;

#nullable disable
public class TestEnemy : EnemyAI
{
  public float detectionRadius = 12f;
  private Collider[] allPlayerColliders = new Collider[4];
  private float closestPlayerDist;
  private Collider tempTargetCollider;
  public bool detectingPlayers;
  private bool tempDebug;

  public override void Start()
  {
    base.Start();
    this.movingTowardsTargetPlayer = true;
  }

  public override void DoAIInterval()
  {
    int num1 = Physics.OverlapSphereNonAlloc(this.transform.position, this.detectionRadius, this.allPlayerColliders, StartOfRound.Instance.playersMask);
    if (num1 > 0)
    {
      this.detectingPlayers = true;
      this.closestPlayerDist = 255555f;
      for (int index = 0; index < num1; ++index)
      {
        float num2 = Vector3.Distance(this.transform.position, this.allPlayerColliders[index].transform.position);
        if ((double) num2 < (double) this.closestPlayerDist)
        {
          this.closestPlayerDist = num2;
          this.tempTargetCollider = this.allPlayerColliders[index];
        }
      }
      this.SetMovingTowardsTargetPlayer(this.tempTargetCollider.gameObject.GetComponent<PlayerControllerB>());
    }
    else
    {
      this.agent.speed = 5f;
      this.detectingPlayers = false;
    }
    base.DoAIInterval();
  }

  public override void Update()
  {
    if (this.IsOwner && this.detectingPlayers)
      this.agent.speed = Mathf.Clamp(this.agent.speed + Time.deltaTime / 3f, 0.0f, 12f);
    base.Update();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  protected internal override string __getTypeName() => nameof (TestEnemy);
}
