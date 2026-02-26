using UnityEngine;
using UnityEngine.UI;

public class BG_GradientAnimator : MonoBehaviour
{
    public Image targetImage;
    public Gradient gradient;
    public float duration = 2.0f;

    void Update()
    {
        // 시간에 따라 0~1 사이를 왕복
        float t = Mathf.PingPong(Time.time / duration, 1f);
        targetImage.color = gradient.Evaluate(t);
    }
}
