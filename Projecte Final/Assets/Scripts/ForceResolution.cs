using UnityEngine;

public class ForceResolution : MonoBehaviour
{
    [SerializeField] private int width = 1920;
    [SerializeField] private int height = 1080;
    [SerializeField] private FullScreenMode screenMode = FullScreenMode.FullScreenWindow;
    [SerializeField] private bool useNativeRefreshRate = true;
    [SerializeField] private int targetRefreshRate = 60; // Opcional: per sobrescriure

    void Start()
    {
        RefreshRate refreshRate = useNativeRefreshRate 
            ? Screen.currentResolution.refreshRateRatio 
            : new RefreshRate { numerator = (uint)targetRefreshRate, denominator = 1 };

        Screen.SetResolution(width, height, screenMode, refreshRate);
    }
}