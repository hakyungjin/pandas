using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameOverManager : MonoBehaviour
{
    [Header("게임오버 UI 요소")]
    public GameObject gameOverPanel; // 게임오버 패널
    public TextMeshProUGUI gameOverText; // 게임오버 메시지
    public Button restartButton; // 재시작 버튼
    public Button mainMenuButton; // 메인 메뉴 버튼
    
    [Header("게임오버 설정")]
    public string gameOverMessage = "GAME OVER";
    public string subMessage = "모든 스폰포인트가 파괴되었습니다!";
    
    private bool isGameOver = false;

    void Start()
    {
        // 초기화
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        
        // 버튼 이벤트 연결
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(RestartGame);
        }
        
        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(GoToMainMenu);
        }
    }

    public void ShowGameOver()
    {
        if (isGameOver) return; // 이미 게임오버 상태면 중복 실행 방지
        
        isGameOver = true;
        Debug.Log("[GameOverManager] 게임오버!");
        
        // 게임 시간 정지
        Time.timeScale = 0f;
        
        // 게임오버 UI 표시
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        
        // 게임오버 메시지 설정
        if (gameOverText != null)
        {
            gameOverText.text = gameOverMessage + "\n" + subMessage;
        }
        
        // 게임오버 이벤트 호출 (다른 시스템에서 필요한 경우)
        OnGameOver();
    }

    public void RestartGame()
    {
        Debug.Log("[GameOverManager] 게임 재시작");
        
        // 게임 시간 복구
        Time.timeScale = 1f;
        
        // 현재 씬 다시 로드
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMainMenu()
    {
        Debug.Log("[GameOverManager] 메인 메뉴로 이동");
        
        // 게임 시간 복구
        Time.timeScale = 1f;
        
        // 메인 메뉴 씬으로 이동 (씬 이름은 프로젝트에 맞게 수정)
        SceneManager.LoadScene("MainMenu");
    }

    // 게임오버 이벤트 (필요시 다른 스크립트에서 구독 가능)
    private void OnGameOver()
    {
        // 추가 게임오버 처리가 필요한 경우 여기에 구현
        // 예: 점수 저장, 통계 기록 등
    }
    
    public bool IsGameOver()
    {
        return isGameOver;
    }
}