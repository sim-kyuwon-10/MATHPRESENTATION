using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("전투 로그 UI")]
    public Text combatLogText;
    public int maxLogLines = 15; // 최대 표시할 줄 수 (화면 밖으로 나가는 것 방지)

    private List<string> logLines = new List<string>();

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void AddLog(string message)
    {
        logLines.Add(message);
        
        // 로그가 설정한 줄 수를 넘어가면 가장 오래된 것 삭제
        if (logLines.Count > maxLogLines)
        {
            logLines.RemoveAt(0);
        }

        if (combatLogText != null)
        {
            combatLogText.text = string.Join("\n", logLines);
        }
        Debug.Log(message); // 유니티 콘솔창에도 동시 출력
    }
}
