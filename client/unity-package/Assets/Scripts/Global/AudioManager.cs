using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource MusicSource;
    [SerializeField] AudioSource SFXSource;

    public AudioClip BGM;
    public AudioClip ButtonClick;

    // Start is called before the first frame update
    void Start()
    {
        MusicSource.Play();
    }

    // Update is called once per frame
    void Awake()
    {
        DontDestroyOnLoad(this);
    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }
}
