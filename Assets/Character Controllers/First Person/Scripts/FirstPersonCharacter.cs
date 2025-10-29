using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(Rigidbody))]
public class FirstPersonCharacter : MonoBehaviour
{
    [Header("<color=#6699CC>Camera</color>")]
    [Range(10.0f, 1000.0f)][SerializeField] private float _mouseSenitivity = 100.0f;

    [Header("<color=#6699CC>Rig</color>")]
    [SerializeField] private Transform _head;

    [Header("<color=#6699CC>Inputs</color>")]
    [SerializeField] private KeyCode _activatetKey = KeyCode.Q;
    [SerializeField] private KeyCode _changeKey = KeyCode.E;
    [SerializeField] private KeyCode _interactKey = KeyCode.F;
    [SerializeField] private KeyCode _jumpKey = KeyCode.Space;

    [Header("<color=#6699CC>Interactions</color>")]
    [SerializeField] private float _interactDistance = 1.5f;
    [SerializeField] private LayerMask _interactMask;

    [Header("<color=#6699CC>Movement</color>")]
    [SerializeField] private float _jumpForce = 7.5f;
    [SerializeField] private float _moveRange = 0.5f;
    [SerializeField] private float _moveSpeed = 4.5f;
    [SerializeField] private LayerMask _moveMask;

    [Header("<color=#6699CC>Physics</color>")]
    [SerializeField] private float _groundDistance = 0.25f;
    [SerializeField] private LayerMask _groundMask;

    [Header("<color=#6699CC>Renderer</color>")]
    [SerializeField] private Renderer _swordRenderer;
    [SerializeField] private float _auraTransitionTime = 1.0f;
    [SerializeField] private string _auraFloatName = "_AuraAmount";
    [SerializeField] private string _auraColorName = "_AuraColor";
    [SerializeField] private Animator _swordAnimator;
    [SerializeField] private string _stateBoolName = "isActivated";
    [SerializeField] private VisualEffect _swordVFX;
    [SerializeField] private string _VFXAuraColorName = "AuraColor";
    [SerializeField] private string _VFXTrailColorName = "TrailColor";

    private bool _isSwordEnabled = false, _isChangingElement = false;
    private float _mouseX = 0.0f;
    private Vector3 _dir = new(), _groundOffset = new(), _mouseInputs = new();

    private FirstPersonCamera _camera;
    private Material _swordMaterial;
    private Rigidbody _rb;

    private Color _actualColor, _newColor;
    private Ray _groundRay, _interactRay, _moveRay;
    private RaycastHit _interactHit;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _rb = GetComponent<Rigidbody>();
        _rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Start()
    {
        _camera = Camera.main.GetComponent<FirstPersonCamera>();
        _camera.Head = _head;

        _swordMaterial = _swordRenderer.material;
        _actualColor = _swordMaterial.GetColor(_auraColorName);
        _swordVFX.SetVector4(_VFXAuraColorName, _actualColor);
        _swordVFX.SetVector4(_VFXTrailColorName, _actualColor);
    }

    private void Update()
    {
        _dir.x = Input.GetAxis("Horizontal");
        _dir.z = Input.GetAxis("Vertical");

        _mouseInputs.x = Input.GetAxisRaw("Mouse X");
        _mouseInputs.y = Input.GetAxisRaw("Mouse Y");

        if(_mouseInputs.sqrMagnitude != 0.0f)
        {
            Rotate(_mouseInputs);
        }

        if (Input.GetKeyDown(_interactKey))
        {
            Interaction();
        }
        else if (Input.GetKeyDown(_activatetKey))
        {
            _isSwordEnabled = !_isSwordEnabled;

            _swordAnimator.SetBool(_stateBoolName, _isSwordEnabled);
        }
        else if(Input.GetKeyDown(_changeKey) && !_isChangingElement)
        {
            StartCoroutine(ChangeAuraColor());
        }

        if (Input.GetKeyDown(_jumpKey) && !IsOnAir())
        {
            Jump();
        }
    }

    private void FixedUpdate()
    {
        if(_dir.sqrMagnitude != 0.0f && !IsBlocked(_dir))
        {
            Movement(_dir);
        }
    }

    private IEnumerator ChangeAuraColor()
    {
        _isChangingElement = true;

        _newColor = new Color(Random.Range(0.0f, 1.0f),
                              Random.Range(0.0f, 1.0f), 
                              Random.Range(0.0f, 1.0f), 
                              1.0f);

        float t = 0.0f;

        while(t < 1.0f)
        {
            t += Time.deltaTime / _auraTransitionTime;

            _swordMaterial.SetColor(_auraColorName, Color.Lerp(_actualColor, _newColor, t));
            _swordVFX.SetVector4(_VFXAuraColorName, Color.Lerp(_actualColor, _newColor, t));
            _swordVFX.SetVector4(_VFXTrailColorName, Color.Lerp(_actualColor, _newColor, t));

            yield return null;
        }

        _swordVFX.SendEvent("OnElementChange");

        _actualColor = _newColor;

        _isChangingElement = false;
    }

    private bool IsOnAir()
    {
        _groundOffset = new Vector3(transform.position.x, transform.position.y + 0.1f, transform.position.z);

        _groundRay = new Ray(_groundOffset, -transform.up);

        return !Physics.Raycast(_groundRay, _groundDistance, _groundMask);
    }    

    private void Interaction()
    {
        _interactRay = new Ray(_camera.transform.position, _camera.transform.forward);

        if(Physics.Raycast(_interactRay, out _interactHit, _interactDistance, _interactMask))
        {
            if(_interactHit.collider.TryGetComponent(out IInteractable inter))
            {
                inter.OnInteract();
            }
        }
    }

    private bool IsBlocked(Vector3 dir)
    {
        _moveRay = new Ray(transform.position, dir);

        return Physics.Raycast(_moveRay, _moveRange, _moveMask);
    }

    private void Jump()
    {
        _rb.AddForce(transform.up * _jumpForce, ForceMode.Impulse);
    }

    private void Movement(Vector3 dir)
    {
        _rb.MovePosition(transform.position + (transform.right * dir.x + transform.forward * dir.z).normalized * _moveSpeed * Time.fixedDeltaTime);
    }

    private void Rotate(Vector3 mouse)
    {
        _mouseX += mouse.x * _mouseSenitivity * Time.deltaTime;

        if(_mouseX >= 360.0f || _mouseX <= -360.0f)
        {
            _mouseX -= 360.0f * Mathf.Sign(_mouseX);
        }

        mouse.y *= _mouseSenitivity * Time.deltaTime;

        transform.rotation = Quaternion.Euler(0.0f, _mouseX, 0.0f);

        _camera?.Rotate(_mouseX, mouse.y);
    }
}
