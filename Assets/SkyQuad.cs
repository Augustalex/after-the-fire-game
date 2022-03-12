using UnityEngine;

public class SkyQuad : MonoBehaviour
{
    private const float Length = 2.5f;

    private float _fadeStart;
    private bool _fadingIn;
    private MeshRenderer _meshRenderer;
    private bool _fadingOut;
    private float _originalFogDensity;

    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        
        _originalFogDensity = RenderSettings.fogDensity;
    }

    void Update()
    {
        if (_fadingIn)
        {
            var duration = Time.time - _fadeStart;
            var progressFactor = duration / Length;

            if (progressFactor >= 1f)
            {
                SetAlpha(1f);
                RenderSettings.fogDensity = 0f;

                _fadingIn = false;
            }
            else
            {
                SetAlpha(progressFactor);
                RenderSettings.fogDensity = _originalFogDensity * Mathf.Clamp(1f - progressFactor, 0, 1f);
            }
        }
        else if (_fadingOut)
        {
            var duration = Time.time - _fadeStart;
            var progressFactor = duration / Length;

            if (progressFactor >= 1f)
            {
                SetAlpha(0f);
                RenderSettings.fogDensity = _originalFogDensity;
                
                _fadingOut = false;
            }
            else
            {
                SetAlpha(Mathf.Clamp(1f - progressFactor, 0, 1f));
                RenderSettings.fogDensity = _originalFogDensity * progressFactor;
            }
        }
    }

    private void SetAlpha(float alpha)
    {
        var newMaterial = _meshRenderer.materials[0];
        var currentColor = newMaterial.GetColor("_BaseColor");
        newMaterial.SetColor("_BaseColor",
            new Color(currentColor.r, currentColor.g, currentColor.b, alpha));
        _meshRenderer.materials[0] = newMaterial;
    }

    public void SetFadedOut()
    {
        if (_fadingIn || _fadingOut) return;

        SetAlpha(0);
    }

    public void FadeIn()
    {
        if (_fadingIn || _fadingOut) return;

        _fadingIn = true;
        _fadeStart = Time.time;
    }

    public void FadeOut()
    {
        if (_fadingIn || _fadingOut) return;

        _fadingOut = true;
        _fadeStart = Time.time;
    }
}