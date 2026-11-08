using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum FighterState
{
    Approaching, Attacking, Down, KO
}

public class FighterData : MonoBehaviour
{
    [Header("기본 정보")]
    public string fighterName = "파이터";

    [Header("스탯")]
    public float maxHp = 100f;
    public float currentHp;
    public float maxStamina = 100f;
    public float currentStamina;
    public float attackPower = 20f;
    public float defensePower = 10f;
    public float attackRange = 1.5f;

    [Header("UI 연결 (선택 사항)")]
    public Slider hpSlider;         // 체력바 슬라이더
    public Slider staminaSlider;    // 스태미나바 슬라이더

    [HideInInspector] public FighterState currentState = FighterState.Approaching;
    private Renderer myRenderer;

    void Awake()
    {
        currentHp = maxHp;
        currentStamina = maxStamina;
        myRenderer = GetComponent<Renderer>(); // 2D Sprite나 3D Mesh 색상 변경용
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        currentHp -= amount;
        if (currentHp <= 0)
        {
            currentHp = 0;
            currentState = FighterState.KO;
        }
        UpdateUI();
        StartCoroutine(FlashColor(Color.red)); // 피격 시 빨간색 깜빡임
    }

    public void Dodge()
    {
        StartCoroutine(FlashColor(Color.cyan)); // 회피 시 파란색 깜빡임
    }

    public void ConsumeStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0) currentStamina = 0;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (hpSlider != null) hpSlider.value = currentHp / maxHp;
        if (staminaSlider != null) staminaSlider.value = currentStamina / maxStamina;
    }

    // 피격/회피 시 색상 깜빡임 연출
    private IEnumerator FlashColor(Color flashColor)
    {
        if (myRenderer == null) yield break;
        Color originalColor = myRenderer.material.color;
        myRenderer.material.color = flashColor;
        yield return new WaitForSeconds(0.15f);
        myRenderer.material.color = originalColor;
    }
}
