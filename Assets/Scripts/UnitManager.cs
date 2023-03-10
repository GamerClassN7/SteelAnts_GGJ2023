using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitManager : MonoBehaviour
{
    public bool boss = false;
    public int health = 100;
    public int actualHealth;
    public int neededlevel = 0;
    public int damage = 15;
    public float damageRate = 0.5f;
    private float actualDamageRate;
    public int energy = 10;
    public string status = "walk"; //walk,cut,attack,die,wait
    [HideInInspector]public bool walk = true;
    public float walkSpeed = 5.0f;
    [HideInInspector]public bool haveWood = false;
    private SpriteRenderer UnitView;
    public List<Sprite> unitSprites = new List<Sprite>();
    public List<Sprite> cutAnimations = new List<Sprite>();
    public List<Sprite> walkAnimations = new List<Sprite>();
    public List<Sprite> deathAnimations = new List<Sprite>();
    public List<Sprite> walkWithWoodAnimations = new List<Sprite>();
    public float timeToCutWood = 2.0f;
    private float actualTimeToCutWood;
    public float animationTime = 0.1f;
    private float actualAnimationTime = 0.0f;
    private int animationIndex = 0;
    private TowerManager targetTower;
    private Tree tree;
    public AudioSource audioSource;
    public AudioSource wordsSource;
    public List<AudioClip> cutAudios = new List<AudioClip>();
    public List<AudioClip> deathAudios = new List<AudioClip>();
    public List<AudioClip> firstWordsAudios = new List<AudioClip>();
    public List<AudioClip> lastWordsAudios = new List<AudioClip>();
    public List<AudioClip> bloodAudios = new List<AudioClip>();
    public GameObject waitTarget;
    private ParticleSystem blood;
    private BoxCollider2D boxCollider;

    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        blood = GetComponentInChildren<ParticleSystem>();
        tree = GameObject.FindGameObjectWithTag("Tree").GetComponent<Tree>();
        actualHealth = health;
        UnitView = this.GetComponentInChildren<SpriteRenderer>();
        int index = Random.Range(0, firstWordsAudios.Count);
        if (wordsSource != null && firstWordsAudios[index] != null)
        {
            wordsSource.PlayOneShot(firstWordsAudios[index]);
        }
    }

    void Update()
    {
        UnitMove();
        UnitAnimation();
        gameObject.transform.localScale = new Vector3((haveWood ? -Mathf.Abs(gameObject.transform.localScale.x) : Mathf.Abs(gameObject.transform.localScale.x)), gameObject.transform.localScale.y, gameObject.transform.localScale.z);

        if (status == "cut" && actualTimeToCutWood < Time.time)
        {
            tree.GetDamage(damage * (boss ? 2 : 1));
            status = "walk";
            haveWood = true;
            animationIndex = 0;
        }

        if (status == "attack" && actualDamageRate <= Time.time)
        {
            actualDamageRate = Time.time + damageRate;
            targetTower.GetDamage(damage);
        }

        if (actualHealth <= 0)
        {
            if (status != "die")
            {
                boxCollider.enabled = false;
                blood.Play();
                animationIndex = 0;
                tree.ConsumeEnergy(-energy);
                Destroy(this.gameObject, (boss ? 5f : 15f));
            }
            status = "die";
            haveWood = false;
        }
        UnitAudio();

        if (waitTarget != null && waitTarget.gameObject.GetComponent<UnitManager>().status == "die")
        {
            waitTarget = null;
            status = "walk";
        }
    }

    private void UnitMove()
    {
        if (status == "walk")
        {
            transform.position += (haveWood ? transform.right : -transform.right) * walkSpeed * Time.deltaTime;
        }
    }

    private void UnitAnimation()
    {
        if (status == "die")
        {
            if (actualAnimationTime < Time.time)
            {
                if (deathAnimations.Count > animationIndex + 1)
                {
                    animationIndex++;
                } 
                actualAnimationTime = Time.time + animationTime;
            }
            if (deathAnimations.Count > animationIndex)
            {
                UnitView.sprite = deathAnimations[animationIndex];
            }
            return;
        }

        if (status == "cut" || status == "attack")
        {
            if (actualAnimationTime < Time.time)
            {
                if (cutAnimations.Count > animationIndex + 1)
                {
                    animationIndex++;
                } 
                else
                {
                    animationIndex = 0;
                }
                actualAnimationTime = Time.time + animationTime;
            }
            if (cutAnimations.Count > animationIndex)
            {
                UnitView.sprite = cutAnimations[animationIndex];
            }
            return;
        }
        if (haveWood)
        {
            if (actualAnimationTime < Time.time)
            {
                if (walkWithWoodAnimations.Count > animationIndex + 1)
                {
                    animationIndex++;
                }
                else
                {
                    animationIndex = 0;
                }
                actualAnimationTime = Time.time + animationTime;
            }
            if (walkWithWoodAnimations.Count > animationIndex)
            {
                UnitView.sprite = walkWithWoodAnimations[animationIndex];
            }
            return;
        }
        if (actualAnimationTime < Time.time)
        {
            if (walkAnimations.Count > animationIndex + 1)
            {
                animationIndex++;
            }
            else
            {
                animationIndex = 0;
            }
            actualAnimationTime = Time.time + animationTime;
        }
        if (walkAnimations.Count > animationIndex)
        {
            UnitView.sprite = walkAnimations[animationIndex];
        }
    }

    public void UnitAudio()
    {
        if (audioSource != null)
        {
            if (!audioSource.isPlaying) {
                switch (status)
                {
                    case "cut":
                    case "attack":
                        audioSource.PlayOneShot(cutAudios[Random.Range(0, cutAudios.Count)]);
                        break;
                    case "die":
                        if (animationIndex == 0) {
                            audioSource.PlayOneShot(bloodAudios[Random.Range(0, bloodAudios.Count)]);
                            int index = Random.Range(0, lastWordsAudios.Count);
                            if (lastWordsAudios[index] != null) {
                                wordsSource.PlayOneShot(lastWordsAudios[index]);
                            }
                            audioSource.PlayOneShot(deathAudios[Random.Range(0, deathAudios.Count)]);
                        }
                        break;
                }
            }
        }
    }

    public void GetDamage(int damage)
    {
        actualHealth -= damage;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (status != "die")
        {
            if (other.gameObject.tag == "Tree")
            {
                actualTimeToCutWood = Time.time + timeToCutWood;
                status = "cut";
                animationIndex = 0;
            }

            if (other.gameObject.tag == "Tower")
            {
                if ((!haveWood && other.gameObject.transform.position.x < gameObject.transform.position.x) || (haveWood && other.gameObject.transform.position.x > gameObject.transform.position.x)) {
                    targetTower = other.gameObject.GetComponent<TowerManager>();
                    status = "attack";
                    animationIndex = 0;
                }
            }

            if (other.gameObject.tag == "Castle")
            {
                if (haveWood)
                {
                    CastleManager castle = other.gameObject.GetComponent<CastleManager>();
                    castle.GetWood();
                    castle.GetWood();
                    castle.GetWood();
                    castle.GetWood();
                    haveWood = false;
                }
            }

            if (other.gameObject.tag == "Enemy")
            {
                if (!boss) {
                    UnitManager unit = other.gameObject.GetComponent<UnitManager>();
                    if (!unit.haveWood && unit.status != "die" && unit.waitTarget != this.gameObject && status == "walk" && !haveWood && !unit.boss)
                    {
                        waitTarget = other.gameObject;
                        status = "wait";
                    }
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (status != "die")
        {
            if (other.gameObject.tag == "Tower")
            {
                if (!boss)
                {
                    haveWood = true;
                }
                targetTower = null;
                status = "walk";
                animationIndex = 0;
            }

            if (other.gameObject.tag == "Enemy")
            {
                UnitManager unit = other.gameObject.GetComponent<UnitManager>();
                if (status == "wait" && waitTarget == other.gameObject && !unit.boss)
                {
                    waitTarget = null;
                    status = "walk";
                }
            }
        }
    }
}
