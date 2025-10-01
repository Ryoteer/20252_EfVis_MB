using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramController : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator _hologramAnimator;
    [SerializeField] private Animator _pedestalAnimator;
    [SerializeField] private string _activeBoolName = "isHologramActive";

    [Header("Behaviours")]
    [SerializeField] private float _activationTime = 0.35f;

    [Header("Inputs")]
    [SerializeField] private KeyCode _activationKey = KeyCode.Space;

    [Header("Shaders")]
    [SerializeField] private Renderer _hologramRenderer;
    [SerializeField] private string _scaleFloatName = "_Scale";

    private bool _isHologramActive = false, _isCoroutineActive = false;

    private Material _hologramMaterial;

    private void Start()
    {
        _hologramAnimator.speed = 0.0f;
        _pedestalAnimator.SetBool(_activeBoolName, false);
        _hologramRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        _hologramMaterial = _hologramRenderer.material;
        _hologramMaterial.SetFloat(_scaleFloatName, 0.0f);
    }

    private void Update()
    {
        if(Input.GetKeyDown(_activationKey) && !_isCoroutineActive)
        {
            StartCoroutine(HologramActivation());
        }
    }

    private IEnumerator HologramActivation()
    {
        _isCoroutineActive = true;

        float t = 0.0f;

        if (!_isHologramActive)
        {
            _pedestalAnimator.SetBool( _activeBoolName, true);

            _hologramAnimator.speed = 1.0f;

            _hologramRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
        }

        while(t < 1.0f)
        {
            t += Time.deltaTime / _activationTime;

            if (_isHologramActive)
            {
                _hologramMaterial.SetFloat(_scaleFloatName, Mathf.Lerp(1.0f, 0.0f, t));
            }
            else
            {
                _hologramMaterial.SetFloat(_scaleFloatName, Mathf.Lerp(0.0f, 1.0f, t));
            }

            yield return null;
        }

        if (_isHologramActive)
        {
            _pedestalAnimator.SetBool(_activeBoolName, false);

            _hologramAnimator.speed = 0.0f;

            _hologramRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        _isHologramActive = !_isHologramActive;

        _isCoroutineActive = false;
    }
}
