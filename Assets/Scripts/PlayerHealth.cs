using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class PlayerHealth : MonoBehaviour
{
    public int PlayerIndex { get; set; }
    public int lives = 5;
    public bool keyboardSound = true;

    public Image Heart1;
    public Image Heart2;
    public Image Heart3;
    public Image Heart4;
    public Image Heart5;
    public Image Heart6;
    public Image Heart7;
    public Image Heart8;
    public Image Heart9;
    public Image Heart10;

    [SerializeField] TextMeshProUGUI GameOver;
    [SerializeField] UITimeSlider obeliskCaptureSlider;
    [SerializeField] TextMeshProUGUI capTimeLeft;
    [SerializeField] TextMeshProUGUI capTimeTotal;

    public Image blackBar;
    public Image cursed;
    public Image frozen;
    public Image stuck;

    public ParticleSystem deathExplosion;
    private AudioSource audioSource;
    public AudioClip explosionClip;
    public AudioClip tickingStickyGrenadeClip;

    private PlayerSpawnManager spawnManager;
    [SerializeField] private FirstPersonController fpc; //only used in UDied(), to prevent you from moving

    
    void Awake()
    {
        spawnManager = PlayerSpawnManager.Instance;
        GameOver.gameObject.SetActive(false);
        capTimeLeft.gameObject.SetActive(false);
        capTimeTotal.gameObject.SetActive(false);
        blackBar.gameObject.SetActive(false);
        cursed.gameObject.SetActive(false);
        frozen.gameObject.SetActive(false);
        stuck.gameObject.SetActive(false);

        Heart6.color = Color.green;
        Heart7.color = Color.green;
        Heart8.color = Color.green;
        Heart9.color = Color.green;
        Heart10.color = Color.green;

        audioSource = GetComponent<AudioSource>();
        if(audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();}
        audioSource.spatialBlend = 1; // 3D sound
    }

    void Update()
    {
    }

    private void OnEnable()
    {
        GameManager.Instance.OnLivesChanged += RefreshHearts;
    }//

    private void OnDisable()
    {
        GameManager.Instance.OnLivesChanged -= RefreshHearts;
    }


//Todo Note to self: ObeliskSpawnedText is in InventoryUI. Should Obelisk stuff be in inventoryUI, or PlayerHealth?
//Todo for now, I think playerhealth, which will be for everything that's not cooldown-related.
    public void DisplayObeliskTimer(float heldFor, float timeTotal, bool isLookingAndHolding)
    {

        if (isLookingAndHolding == true)
        {
           
            capTimeLeft.gameObject.SetActive(true);
            capTimeTotal.gameObject.SetActive(true);
            blackBar.gameObject.SetActive(true);

            capTimeLeft.text = Mathf.RoundToInt(heldFor).ToString();
            capTimeTotal.text = Mathf.RoundToInt(timeTotal).ToString();

            obeliskCaptureSlider.Show();
            obeliskCaptureSlider.SetNormalized(1f); // full fuse at start
            float normalized = 1f - (heldFor / timeTotal);
            obeliskCaptureSlider.SetNormalized(normalized);
        }
        if (isLookingAndHolding == false)
        {
             
            capTimeLeft.gameObject.SetActive(false);
            capTimeTotal.gameObject.SetActive(false);
            blackBar.gameObject.SetActive(false);

            obeliskCaptureSlider.Hide();
        }
    }

    public void ApplyInverseCurseGraphic(bool areYouCursed)
    {
        if (areYouCursed){
            cursed.gameObject.SetActive(true);}

        Heart5.color = Color.green;
        Heart4.color = Color.green;
        Heart3.color = Color.green;
        Heart2.color = Color.green;
        Heart1.color = Color.green;
        
        if (!areYouCursed){
            cursed.gameObject.SetActive(false);
            Heart5.color = Color.white;
            Heart4.color = Color.white;
            Heart3.color = Color.white;
            Heart2.color = Color.white;
            Heart1.color = Color.white;
            }
    }

    public void ApplyFrozenCurseGraphic(bool areYouFrozen)
    {
        if (areYouFrozen){
            frozen.gameObject.SetActive(true);}
        if (!areYouFrozen){
            frozen.gameObject.SetActive(false);}
    }

    public void StuckStickyGrenade(bool isStuck)
    {
        if (tickingStickyGrenadeClip == null) return;
        if (isStuck)
            {
                audioSource.clip = tickingStickyGrenadeClip;
                audioSource.loop = true;
                audioSource.spatialBlend = 1f; // or 0f if you want 2D
                audioSource.volume = 1f;
                audioSource.Play(); //Todo need to change this so that it doesn't overlap with music
                stuck.gameObject.SetActive(true);
            }
        else
        {
            audioSource.Stop();
            audioSource.clip = null;
            stuck.gameObject.SetActive(false);
        }
    }

    public void LoseLife()
    {
        GameManager.Instance.UpdateLives(PlayerIndex, lives);
    }

    private void RefreshHearts(int p1Lives, int p2Lives)
    {
        if (PlayerIndex == 0)
        {
            // Player 1 hearts
        Heart1.gameObject.SetActive(p1Lives >= 1);
        Heart2.gameObject.SetActive(p1Lives >= 2);
        Heart3.gameObject.SetActive(p1Lives >= 3);
        Heart4.gameObject.SetActive(p1Lives >= 4);
        Heart5.gameObject.SetActive(p1Lives >= 5);

        // Player 2 hearts
        Heart6.gameObject.SetActive(p2Lives >= 1);
        Heart7.gameObject.SetActive(p2Lives >= 2);
        Heart8.gameObject.SetActive(p2Lives >= 3);
        Heart9.gameObject.SetActive(p2Lives >= 4);
        Heart10.gameObject.SetActive(p2Lives >= 5);
        }
        if (PlayerIndex == 1)
        {
            // Player 1 hearts
        Heart1.gameObject.SetActive(p2Lives >= 1);
        Heart2.gameObject.SetActive(p2Lives >= 2);
        Heart3.gameObject.SetActive(p2Lives >= 3);
        Heart4.gameObject.SetActive(p2Lives >= 4);
        Heart5.gameObject.SetActive(p2Lives >= 5);

        // Player 2 hearts
        Heart6.gameObject.SetActive(p1Lives >= 1);
        Heart7.gameObject.SetActive(p1Lives >= 2);
        Heart8.gameObject.SetActive(p1Lives >= 3);
        Heart9.gameObject.SetActive(p1Lives >= 4);
        Heart10.gameObject.SetActive(p1Lives >= 5);
        }
        
    }

    public void UDied()
    {
        lives--;
        Instantiate(deathExplosion, transform.position, deathExplosion.transform.rotation);
        if(explosionClip != null){
            AudioSource.PlayClipAtPoint(explosionClip, transform.position, 1f);}
        GameManager.Instance.UpdateLives(PlayerIndex, lives);

        if (lives > 0)
        {
            fpc.inverseCurse = false;
            cursed.gameObject.SetActive(false);
            fpc.frozenCurse = false;
            frozen.gameObject.SetActive(false);
            stuck.gameObject.SetActive(false);
            spawnManager.RespawnPlayer(gameObject);
        }
        else
        {
            GameOver.gameObject.SetActive(true);
            fpc.inUpgradeMenu = true;  //just here so you can't move while dead
            fpc.wasJustInMenu = true;
        }
    }

}
/*
When either player dies:

UDied() runs on that player

GameManager.UpdateLives() fires

OnLivesChanged event fires

Both PlayerHealth components receive it

Both HUDs redraw*/