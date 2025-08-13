using UnityEngine;

public class DissolveController : MonoBehaviour
{
    [Header("효과 설정")]
    [SerializeField] private float dissolveSpeed = 0.5f;

    [Header("색상 (HDR)")]
    [ColorUsage(true, true)]
    [SerializeField] private Color outColor; // 사라질 때의 색상

    [ColorUsage(true, true)]
    [SerializeField] private Color inColor;  // 나타날 때의 색상

    // 내부에서 사용할 변수들
    private Material mat;
    private float dissolveAmount;
    private bool isDissolvingOut; // true면 사라지는 중, false면 나타나는 중

    void Start()
    {
        mat = GetComponent<SpriteRenderer>().material;

        // 시작 시 완전히 사라진 상태(dissolveAmount = 1)로 설정
        dissolveAmount = -0.5f;
        mat.SetFloat("_DissolveAmount", dissolveAmount);

        // 즉시 '나타나기(In)' 효과를 시작
        StartDissolveIn();
    }

    void Update()
    {
        // isDissolvingOut 플래그 값에 따라 적절한 애니메이션을 재생
        if (isDissolvingOut)
        {
            // 사라지는 중...
            if (dissolveAmount >-0.5f)
            {
                dissolveAmount -= Time.deltaTime * dissolveSpeed;
            }
        }
        else
        {
            // 나타나는 중...
            if (dissolveAmount <1f)
            {
                dissolveAmount += Time.deltaTime * dissolveSpeed;
            }
        }

        // 매 프레임 dissolveAmount 값을 셰이더로 전달하여 애니메이션
        mat.SetFloat("_DissolveAmount", dissolveAmount);
    }

    /// <summary>
    /// 오브젝트가 나타나는 효과를 시작합니다.
    /// </summary>
    public void StartDissolveIn()
    {
        isDissolvingOut = false;
        mat.SetColor("_DissolveColored", inColor);
    }

    /// <summary>
    /// 오브젝트가 사라지는 효과를 시작합니다. 이 함수를 다른 스크립트에서 호출하세요.
    /// </summary>
    public void StartDissolveOut()
    {
        isDissolvingOut = true;
        mat.SetColor("_DissolveColored", outColor);
    }
}