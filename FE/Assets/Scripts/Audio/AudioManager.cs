using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    [Tooltip("Âm thanh khi player tấn công/chém")]
    public AudioClip attackSound;

    [Tooltip("Âm thanh khi mở inventory")]
    public AudioClip inventoryOpenSound;

    [Tooltip("Âm thanh khi đóng inventory")]
    public AudioClip inventoryCloseSound;

    [Tooltip("Âm thanh bước chân khi di chuyển")]
    public AudioClip footstepSound;

    [Tooltip("Âm thanh khi enemy tấn công")]
    public AudioClip enemyAttackSound;

    [Tooltip("Âm thanh khi sử dụng item (hồi máu/ki)")]
    public AudioClip itemUseSound;

    [Header("Settings")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;

    [Range(0f, 1f)]
    public float footstepVolume = 0.5f;

    private AudioSource audioSource;
    private AudioSource footstepSource;

    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Tạo AudioSource cho SFX
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;

        // Tạo AudioSource riêng cho footstep (để có thể loop)
        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
    }


    public void PlayAttackSound()
    {
        if (attackSound != null)
        {
            audioSource.PlayOneShot(attackSound, sfxVolume);
        }
        else
        {
            Debug.LogWarning("[AudioManager] Attack sound clip chưa được gán!");
        }
    }


    public void PlayInventoryOpen()
    {
        if (inventoryOpenSound != null)
        {
            audioSource.PlayOneShot(inventoryOpenSound, sfxVolume);
        }
        else
        {
            Debug.LogWarning("[AudioManager] Inventory open sound clip chưa được gán!");
        }
    }


    public void PlayInventoryClose()
    {
        if (inventoryCloseSound != null)
        {
            audioSource.PlayOneShot(inventoryCloseSound, sfxVolume);
        }
        else
        {
            Debug.LogWarning("[AudioManager] Inventory close sound clip chưa được gán!");
        }
    }


    public void PlayFootstep()
    {
        if (footstepSound != null && !footstepSource.isPlaying)
        {
            footstepSource.clip = footstepSound;
            footstepSource.volume = footstepVolume;
            footstepSource.Play();
        }
    }

    public void StopFootstep()
    {
        if (footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

    public void PlayFootstepOneShot()
    {
        if (footstepSound != null)
        {
            audioSource.PlayOneShot(footstepSound, footstepVolume);
        }
    }


    public void PlayEnemyAttackSound()
    {
        if (enemyAttackSound != null)
        {
            audioSource.PlayOneShot(enemyAttackSound, sfxVolume);
        }
        else
        {
            Debug.LogWarning("[AudioManager] Enemy attack sound clip chưa được gán!");
        }
    }

    public void PlayItemUseSound()
    {
        if (itemUseSound != null)
        {
            audioSource.PlayOneShot(itemUseSound, sfxVolume);
        }
        else
        {
            Debug.LogWarning("[AudioManager] Item use sound clip chưa được gán!");
        }
    }
}
