using System.Collections;
using UnityEngine;

public class CombatEngine : MonoBehaviour
{
    [Header("파이터 설정")]
    public FighterData fighterA;
    public FighterData fighterB;

    [Header("전투 환경 설정")]
    public float moveSpeed = 2f;
    public float tickInterval = 0.8f; // 0.8초마다 턴 진행

    private bool isBattleOver = false;

    void Start()
    {
        UIManager.Instance.AddLog("🔥 === 경기가 시작되었습니다! === 🔥");
        StartCoroutine(BattleRoutine());
    }

    IEnumerator BattleRoutine()
    {
        while (!isBattleOver)
        {
            yield return new WaitForSeconds(tickInterval);

            // 1. 유클리드 거리 연산
            float distance = Vector2.Distance(fighterA.transform.position, fighterB.transform.position);

            // 2. 두 파이터 중 사거리가 짧은 쪽 기준으로 접근
            float engageRange = Mathf.Max(fighterA.attackRange, fighterB.attackRange);

            if (distance > engageRange)
            {
                MoveTowardsEachOther();
                UIManager.Instance.AddLog($"[거리 조절] 현재 거리: {distance:F1}m");
                continue; // 거리가 멀면 타격 턴은 건너뜀
            }

            // 3. 턴 순서 랜덤화 (누가 먼저 때릴지 50% 확률로 결정)
            bool aAttacksFirst = Random.value > 0.5f;

            if (aAttacksFirst)
            {
                ExecuteAttack(fighterA, fighterB);
                if (CheckBattleEnd()) break;
                ExecuteAttack(fighterB, fighterA);
            }
            else
            {
                ExecuteAttack(fighterB, fighterA);
                if (CheckBattleEnd()) break;
                ExecuteAttack(fighterA, fighterB);
            }
            
            if (CheckBattleEnd()) break;
        }
    }

    // 핵심 수학: 지수함수 기반 시그모이드 확률 연산
    float CalculateSigmoidProbability(float attackerPower, float defenderDefense, float attackerStamina)
    {
        // 스태미나가 낮을수록(피로도가 높을수록) 공격력 가중치 감소
        float staminaRatio = attackerStamina / 100f; 
        float statDifference = (attackerPower * staminaRatio) - defenderDefense;

        // 지수(-x) 값 산출 (수치가 너무 극단적이 되지 않도록 0.15 곱합)
        float x = statDifference * 0.15f;
        
        // S(x) = 1 / (1 + e^-x) 공식 적용
        float sigmoidProbability = 1f / (1f + Mathf.Exp(-x));
        
        // 최소 명중률 10%, 최대 명중률 90%로 제한 (현실성 부여)
        return Mathf.Clamp(sigmoidProbability, 0.1f, 0.9f); 
    }

    // 공격 판정 및 결과 처리
    void ExecuteAttack(FighterData attacker, FighterData defender)
    {
        if (attacker.currentState == FighterState.KO || defender.currentState == FighterState.KO) return;

        attacker.ConsumeStamina(6f); // 주먹을 뻗으면 스태미나 6 소모

        // 지수함수로 명중 확률 계산
        float hitProbability = CalculateSigmoidProbability(attacker.attackPower, defender.defensePower, attacker.currentStamina);

        // 몬테카를로 난수로 성공 여부 판정
        float randomDice = Random.value;

        if (randomDice <= hitProbability) // 타격 성공
        {
            float damage = attacker.attackPower * Random.Range(0.8f, 1.2f);
            defender.TakeDamage(damage);

            UIManager.Instance.AddLog($"⚔️ [적중] {attacker.fighterName} 타격! (명중률:{hitProbability * 100f:F0}%) 💥 {defender.fighterName}에게 {damage:F0} 데미지!");

            // 그로기(다운) 판정: 체력 30% 이하일 때 지수함수 역확률로 판정
            if (defender.currentHp > 0 && defender.currentHp <= defender.maxHp * 0.3f && Random.value < 0.3f)
            {
                defender.currentState = FighterState.Down;
                UIManager.Instance.AddLog($"⚠️ [상태 이상] {defender.fighterName} 선수가 큰 충격을 받고 그로기에 빠집니다!");
            }
        }
        else // 타격 회피
        {
            defender.Dodge(); // 회피 시각 효과 실행
            UIManager.Instance.AddLog($"💨 [회피] {defender.fighterName} 선수가 공격을 피했습니다! (명중률:{hitProbability * 100f:F0}%)");
        }
    }

    void MoveTowardsEachOther()
    {
        Vector3 dirA = (fighterB.transform.position - fighterA.transform.position).normalized;
        // 서로를 향해 이동
        fighterA.transform.position += dirA * moveSpeed * Time.deltaTime * 50f;
        fighterB.transform.position -= dirA * moveSpeed * Time.deltaTime * 50f;
    }

    bool CheckBattleEnd()
    {
        if (fighterA.currentState == FighterState.KO)
        {
            UIManager.Instance.AddLog($"\n🏆 [경기 종료] {fighterB.fighterName} 선수가 KO 승리를 거둡니다!!");
            isBattleOver = true;
            return true;
        }
        if (fighterB.currentState == FighterState.KO)
        {
            UIManager.Instance.AddLog($"\n🏆 [경기 종료] {fighterA.fighterName} 선수가 KO 승리를 거둡니다!!");
            isBattleOver = true;
            return true;
        }
        return false;
    }
}
