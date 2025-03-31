using System.Collections;
using UnityEngine;
using UnityEngine.UI; // <- important!

public class BlinkSpacebar : MonoBehaviour
{
    public Sprite unpressedSprite;
    public Sprite pressedSprite;
    private Image uiImage;

    private bool isPressed = false;

    void Start()
    {
        uiImage = GetComponent<Image>(); // <- UI image, not SpriteRenderer
        StartCoroutine(SwapSprite());
    }

    IEnumerator SwapSprite()
    {
        while (true)
        {
            isPressed = !isPressed;
            uiImage.sprite = isPressed ? pressedSprite : unpressedSprite;
            yield return new WaitForSeconds(1f); // 1 second interval
        }
    }
}
