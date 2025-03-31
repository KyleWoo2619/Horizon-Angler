using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FishingProgress : MonoBehaviour
{
    public float progress;
    public Slider progressSlider;
    [HideInInspector] public Test2Script T2S;

    // Start is called before the first frame update
    void Start()
    {
        T2S = GetComponent<Test2Script>();
        Initialize();
    }

    public void Initialize()
    {
        progress = 50f;
    }

    // Update is called once per frame
    void Update()
    {
        ProgressSliderVisual();
        ProgressTracker();
        PrototypeProgressIncrease();
    }

    void ProgressSliderVisual()
    {
        progressSlider.value = progress;
    }

    void Progress100()
    {
        T2S.microgamesActive = false;
        T2S.ClearAll();
    }

    void Progress0()
    {
        T2S.microgamesActive = false;
        T2S.ClearAll();
    }

    void ProgressTracker()
    {
        if (progress >= 100)
        {
            Progress100();
        }
        if (progress <= 0)
        {
            Progress0();
        }
    }

    void PrototypeProgressIncrease()
    {if (T2S.microgamesActive)
        {
            progress += 1.5f * Time.deltaTime;
        }
    }
}
