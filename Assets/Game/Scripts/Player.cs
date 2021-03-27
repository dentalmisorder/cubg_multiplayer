using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class Player : NetworkBehaviour
{
   public PlayerChangableValues changableValues;

    [SerializeField, SyncVar(hook = nameof(OnChangeColor))] private Color myColor;
    public int playersKilled = 0;
    public Skill mainSkill;
    public Skill passiveSkill;
    [SerializeField] private Canvas canvas;

    private Button mainSkillButton;
    private Button passiveSkinButton;
    private Text cdText;

    private BoxCollider[] colliders;
    private Rigidbody rb;

    private float cd = 1;
    private bool isBuffTime = false;
    private MeshRenderer meshRenderer;
    private Color defColorButton;

    [HideInInspector] public bool isDead = false;

    public override void OnStartLocalPlayer()
    {
        canvas.enabled = true;

        CameraFollow.instance.objToFollow = transform;
        CameraFollow.instance.CalculateCamOffset();
    }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();

        colliders = GetComponentsInChildren<BoxCollider>();
        mainSkillButton = GetComponentsInChildren<Button>()[0];
        cdText = mainSkillButton.GetComponentInChildren<Text>();
        passiveSkinButton = GetComponentsInChildren<Button>()[1];

        Init();
    }

    private void Init()
    {
        defColorButton = mainSkillButton.image.color;
        myColor = meshRenderer.material.color;

        mainSkillButton.image.sprite = mainSkill.skillIcon;
        passiveSkinButton.image.sprite = passiveSkill.skillIcon;
        mainSkill.isBuffTime = false;

        CheckPassiveOnStart();
    }

    private void CheckPassiveOnStart()
    {
        if((passiveSkill.isPassive && passiveSkill.skillSpeed > 0) || (passiveSkill.isPassive && passiveSkill.skillScale > 0) && passiveSkill.isPassive)
        {
            changableValues.moveSpeed += passiveSkill.skillSpeed;
            transform.localScale += new Vector3(passiveSkill.skillScale, passiveSkill.skillScale, passiveSkill.skillScale);
        }
    }

    private void Update()
    {
        if (!isLocalPlayer)
            return;

        CooldownTimer();
        Movements();
        ActionInputs();
    }

    private void Movements()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        transform.position += new Vector3(horizontal, 0f, vertical) * changableValues.moveSpeed * Time.deltaTime;
    }

    private void ActionInputs()
    {
        if (isDead)
            return;

        if (Input.GetKeyDown(KeyCode.C))
        {
            CmdChangeColor();
        }

        if (Input.GetKeyDown(KeyCode.Q) && cd <= 1)
        {
            if(mainSkill.isBuffTime == false)
                Cast();
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            Spawn();
        }
    }

    private void Cast()
    {
        transform.localScale += new Vector3(mainSkill.skillScale, mainSkill.skillScale, mainSkill.skillScale);
        changableValues.moveSpeed += mainSkill.skillSpeed;
        mainSkillButton.image.color = Color.cyan;
        mainSkill.isBuffTime = true;
        
        if(mainSkill.buffStateTime != 0)
        {
            StartCoroutine(WaitForBuffToExpire());
        }
        else
        {
            mainSkill.isBuffTime = false;
            mainSkillButton.image.color = defColorButton;
            cd = mainSkill.skillCooldown;
        }
    }

    public IEnumerator WaitForBuffToExpire()
    {
        yield return new WaitForSeconds(mainSkill.buffStateTime);

        mainSkillButton.image.color = defColorButton;
        transform.localScale -= new Vector3(mainSkill.skillScale, mainSkill.skillScale, mainSkill.skillScale);
        changableValues.moveSpeed -= mainSkill.skillSpeed;
        mainSkill.isBuffTime = false;

        cd = mainSkill.skillCooldown;                                //reset CD to let it be active again
        yield return null;
    }

    [Command]
    private void Spawn()
    {
        NetworkServer.Spawn(Instantiate(NetworkManager.singleton.spawnPrefabs[0]));
    }

    [Command]
    private void CmdChangeColor()
    {
        myColor = UnityEngine.Random.ColorHSV();
    }

    private void OnChangeColor(Color oldColor, Color newColor)
    {
        meshRenderer.material.color = newColor;
    }

    private void CooldownTimer()
    {
        if (cd <= mainSkill.skillCooldown && cd > 1)
        {
            cd -= Time.deltaTime;
            cdText.text = ((int)cd).ToString();
            mainSkillButton.interactable = false;
        }
        else if (cd <= 1 && !mainSkill.isBuffTime) //buff time so buttons isnt interactable while under the buff
        {
            cdText.text = string.Empty;
            mainSkillButton.interactable = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(transform.localScale.x > other.transform.localScale.x)
            {
                playersKilled += 1;
                CameraFollow.instance.cameraOffset.y += 0.2f;
                transform.localScale += new Vector3(0.2f, 0.2f, 0.2f); //scaling for every kill
                if(changableValues.moveSpeed <= 1f)
                    changableValues.moveSpeed -= 0.2f;
                PassiveSkill();                                                                 //checking what is passive doing and give stats
                StartCoroutine(Restart(5, other.transform));          //respawning player who lose
            }
        }
    }

    private void PassiveSkill()
    {
        if(passiveSkill.skillKilledTask > 0 && playersKilled >= passiveSkill.skillKilledTask && passiveSkill.isPassive)
        {
            transform.localScale += new Vector3(passiveSkill.skillScale, passiveSkill.skillScale, passiveSkill.skillScale);
            changableValues.moveSpeed += passiveSkill.skillSpeed;
            passiveSkill.isPassive = false;
        }
    }

    private IEnumerator Restart(float _spawnAfter, Transform _other)
    {
        KillPlayer(_other);
        yield return new WaitForSeconds(_spawnAfter);

        #region for repeatable gameplay (disabled)
        //_other.position = NetworkManager.singleton.transform.position;
        //_other.localScale = new Vector3(1f, 1f, 1f);

        //SetPlayerActive(true, _other);

        //CameraFollow.instance.cameraOffset = CameraFollow.instance.baseCameraOffset;

        yield return null;
        #endregion
    }

    private void SetPlayerActive(bool _isActive, Transform _other)
    {
        Rigidbody killedRb = _other.GetComponent<Rigidbody>();
        killedRb.useGravity = _isActive;
        killedRb.freezeRotation = _isActive;
        killedRb.isKinematic = !_isActive;

        _other.GetComponent<MeshRenderer>().enabled = _isActive;
        BoxCollider[] colliders = _other.GetComponentsInChildren<BoxCollider>();
        foreach (var collider in colliders)
            collider.enabled = _isActive;
    }

    private void KillPlayer(Transform _other)
    {
        Rigidbody killedRb = _other.GetComponent<Rigidbody>();
        killedRb.useGravity = false;
        killedRb.freezeRotation = false;
        killedRb.isKinematic = true;

        Player killedPlr = _other.GetComponent<Player>();
        killedPlr.isDead = true;
        killedPlr.mainSkillButton.gameObject.SetActive(false);
        killedPlr.passiveSkinButton.gameObject.SetActive(false);

        _other.GetComponent<MeshRenderer>().enabled = false;
        BoxCollider[] colliders = _other.GetComponentsInChildren<BoxCollider>();
        foreach (var collider in colliders)
            collider.enabled = false;
    }


    [System.Serializable]
    public class PlayerChangableValues
    {
        public float moveSpeed = 10f;
    }

}
