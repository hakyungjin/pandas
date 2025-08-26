using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.SceneManagement;
using DamageNumbersPro;
using UnityEngine.EventSystems;



public class GameManager : MonoBehaviour
{
    [Header("UI 관리")]
    public GameObject towerUI; // 인스펙터에서 연결
    public TextMeshProUGUI goldText; // 골드 UI 텍스트 (인스펙터에서 연결)
    public TextMeshProUGUI timerText; // 타이머 UI 텍스트 (인스펙터에서 연결)
   
   
    
    [Header("골드 시스템")]
    public int currentGold = 0; // 시작 골드
    public int goldPerSecond = 1; // 초당 골드 증가량
    
    [Header("타이머 시스템")]
    private float gameTime = 0f; // 게임 진행 시간 (초)
    
    [Header("몬스터 스폰 시스템")]
    public EnemySpawner enemySpawner; // 인스펙터에서 연결
    
    [Header("게임 시스템")]
    public GameObject gameOverUI;
    public GameObject gameClearUI;
    public GameObject gamePauseUI;
    public GameObject gameStopUI;

    [Header("타일 설치&파괴 시스템")]
    public int tileCount;

    
    public List<InstallZone> installZones = new List<InstallZone>(); // 모든 설치 구역 관리

    public static GameManager instance;
    public static GameManager Instance => instance;

    [Header("오디오 시스템")]
    public AudioSource bgmSource; // BGM 재생용 AudioSource (인스펙터에서 연결)
    public AudioSource sfxSource; // SFX 재생용 AudioSource (인스펙터에서 연결)
    public List<AudioClip> bgmMap = new List<AudioClip>(); // 인스펙터에서 키-클립 등록
    public List<AudioClip> sfxMap = new List<AudioClip>(); // 인스펙터에서 키-클립 등록
   

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
        }
        else
        {
            // 이미 인스턴스가 존재하면 이 오브젝트를 파괴
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시간 정상화 (리스타트 시에도 확실히)
        Time.timeScale = 1;
        
        // 골드 시스템 초기화
        UpdateGoldUI();
        StartCoroutine(GoldIncreaseCoroutine());

        
        // 타이머 시스템 초기화
        gameTime = 0f;
        UpdateTimerUI();
        
        
    }

  

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.P))
        {
            if (!gameStopUI.activeSelf)
            {
                gameStopUI.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                gameStopUI.SetActive(false);
                Time.timeScale = 1;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gamePauseUI.activeSelf)
            {
                gamePauseUI.SetActive(true);
                Time.timeScale = 0;
            }
            else
            {
                gamePauseUI.SetActive(false);
                Time.timeScale = 1;
            }
        }
       installZones.RemoveAll(zone => zone == null);
        
        // UI 토글
        if (Input.GetKeyDown(KeyCode.I))
        {
            towerUI.SetActive(!towerUI.activeSelf);
        }
        
        // 게임 시간 업데이트 (게임이 정지되지 않았을 때만)
        if (Time.timeScale > 0)
        {
            gameTime += Time.deltaTime;
            UpdateTimerUI();
        }
        if (installZones.Count == 0)
        {
            TriggerGameClear();
        }
        
    }
    
    




    // 골드 시스템 메서드들
    public void GoldIncrease(int amount)
    {
        goldPerSecond += amount;
    }
    
    // 1초마다 골드 증가시키는 코루틴
    private IEnumerator GoldIncreaseCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1초 대기
            AddGold(goldPerSecond);
        }
    }
    
    // 골드 추가
    public void AddGold(int amount)
    {
        currentGold += (int)amount;
        UpdateGoldUI();
        Debug.Log($"[GameManager] 골드 +{amount}, 현재 골드: {currentGold}");
    }
    
    // 골드 사용 (유닛 구매 등에 사용)
    public bool SpendGold(int amount)
    {
        if (currentGold >= amount)
        {
            currentGold -= amount;
            UpdateGoldUI();
            Debug.Log($"[GameManager] 골드 -{amount}, 현재 골드: {currentGold}");
            return true;
        }
        else
        {
            Debug.Log($"[GameManager] 골드 부족! 필요: {amount}, 보유: {currentGold}");
            return false;
        }
    }
    
    // 골드 UI 업데이트
    private void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = currentGold.ToString();
        }
        else
        {
            Debug.LogWarning("[GameManager] goldText가 연결되지 않았습니다!");
        }
    }
    
    // 현재 골드 반환 (다른 스크립트에서 골드 확인용)
    public int GetCurrentGold()
    {
        return currentGold;
    }
    
    // 타이머 시스템 메서드들
    
    // 타이머 UI 업데이트
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            timerText.text = FormatTime(gameTime);
        }
        else
        {
            Debug.LogWarning("[GameManager] timerText가 연결되지 않았습니다!");
        }
    }
    
    // 시간을 MM:SS 형식으로 포맷팅
    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    // 현재 게임 시간 반환 (다른 스크립트에서 시간 확인용)
    public float GetGameTime()
    {
        return gameTime;
    }
    
    // InstallZone 관리 메서드들
    
    // 모든 InstallZone을 찾아서 리스트에 등록
    private void InitializeInstallZones()
    {
        InstallZone[] zones = FindObjectsOfType<InstallZone>();
        installZones.Clear();
        
        foreach (InstallZone zone in zones)
        {
            installZones.Add(zone);
        }
        
        Debug.Log($"[GameManager] {installZones.Count}개의 InstallZone을 찾았습니다.");
    }
    
    // InstallZone이 파괴될 때 호출되는 메서드
    public void OnInstallZoneDestroyed(InstallZone destroyedZone)
    {
        if (installZones.Contains(destroyedZone))
        {
            installZones.Remove(destroyedZone);
            Debug.Log($"[GameManager] InstallZone이 파괴되었습니다. 남은 개수: {installZones.Count}");
            
            // 게임오버 체크
            CheckGameOverCondition();
        }
    }
    
    // 게임오버 조건 체크
    private void CheckGameOverCondition()
    {
        // 모든 InstallZone이 파괴되었는지 확인
        if (installZones.Count == 0)
        {
            Debug.Log("[GameManager] 모든 스폰포인트가 파괴되었습니다! 게임오버!");
            TriggerGameOver();
        }
    }
    public void TriggerGameClear()
    {
        Time.timeScale = 0;
        gameClearUI.SetActive(true);
    }

    // 게임오버 실행
    private void TriggerGameOver()
    {
        Time.timeScale = 0;
        
        // 게임오버 UI 표시
        gameOverUI.SetActive(true);
        
    }
    
    // 수동으로 InstallZone을 추가하는 메서드 (필요시 사용)
    public void RegisterInstallZone(InstallZone zone)
    {
        if (!installZones.Contains(zone))
        {
            installZones.Add(zone);
            Debug.Log($"[GameManager] InstallZone이 등록되었습니다. 총 개수: {installZones.Count}");
        }
    }
    
    // 현재 남은 InstallZone 개수 반환
    public int GetRemainingInstallZones()
    {
        return installZones.Count;
    }
    
    // 스폰 포인트가 모두 파괴되었을 때 호출되는 메서드
    public void OnAllSpawnPointsDestroyed()
    {
       
        TriggerGameClear();
    }

    public int GetGold()
    {
        return currentGold;
    }

    public void PlayBGM(int key, float volume = 1f, bool loop = true)
    {
        if (bgmSource == null)
        {
            Debug.LogWarning("[GameManager] bgmSource가 연결되지 않았습니다!");
            return;
        }

        // 인덱스/널 체크
        if (bgmMap == null || bgmMap.Count == 0)
        {
            Debug.LogWarning("[GameManager] bgmMap이 비어있습니다.");
            return;
        }
        if (key < 0 || key >= bgmMap.Count)
        {
            Debug.LogWarning($"[GameManager] BGM 인덱스 범위를 벗어났습니다: {key}");
            return;
        }
        if (bgmMap[key] == null)
        {
            Debug.LogWarning("[GameManager] 선택한 BGM 클립이 비어있습니다.");
            return;
        }
        if (bgmSource.clip == bgmMap[key] && bgmSource.isPlaying)
        {
            bgmSource.volume = volume;
            bgmSource.loop = loop;
            return;
        }
        bgmSource.clip = bgmMap[key];
        bgmSource.volume = volume;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource != null)
        {
            bgmSource.Stop();
            bgmSource.clip = null;
        }
    }

    public void SetBGMVolume(float volume)
    {
        if (bgmSource != null)
        {
            bgmSource.volume = Mathf.Clamp01(volume);
        }
    }

    public void SetSFXVolume(float volume)
    {
        if (sfxSource != null)
        {
            sfxSource.volume = Mathf.Clamp01(volume);
        }
    }

    // 직접 클립으로 SFX 재생
    public void PlaySFX(int key, float volume = 1f, float pitch = 1f)
    {
        if (sfxSource == null)
        {
            Debug.LogWarning("[GameManager] sfxSource가 연결되지 않았습니다!");
            return;
        }
        if (sfxMap == null || sfxMap.Count == 0)
        {
            Debug.LogWarning("[GameManager] sfxMap이 비어있습니다.");
            return;
        }
        if (key < 0 || key >= sfxMap.Count)
        {
            Debug.LogWarning($"[GameManager] SFX 인덱스 범위를 벗어났습니다: {key}");
            return;
        }
        if (sfxMap[key] == null)
        {
            Debug.LogWarning("[GameManager] 선택한 SFX 클립이 비어있습니다.");
            return;
        }
        sfxSource.pitch = pitch;
        sfxSource.loop = false;
        sfxSource.PlayOneShot(sfxMap[key], volume);
    }



    
} 