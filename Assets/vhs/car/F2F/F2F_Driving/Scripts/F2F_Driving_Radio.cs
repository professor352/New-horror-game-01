using UnityEngine;

public class F2F_Driving_Radio : MonoBehaviour
{

    [Header("Audio Settings")]
    public AudioSource radioAudioSource; // the source we use to play audios
    public AudioClip[] playlist; // a list of all the musics or sounds we want in the game 
    public KeyCode nextAudioKey = KeyCode.T; // the key to press to go to the next music
    [Range(0f, 1f)] public float DefaultVolume = 0.5f;

    [Header("State")]
    public bool isRadioPlaying = false; // checks if the radio is playing 
    private int currentTrackindex = 0; // we know which music we are playing 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(radioAudioSource != null && playlist != null && playlist.Length > 0)
        {
            radioAudioSource.clip = playlist[currentTrackindex]; // we put the first music in our source
            radioAudioSource.loop = true; // loop current music
            radioAudioSource.playOnAwake = false; // it won't active music at the start of the game
            radioAudioSource.volume = DefaultVolume; // make the audio the same as our default volume
            radioAudioSource.mute = !isRadioPlaying; // if the radio is off, it won't start at the START
            radioAudioSource.Play(); // Starts playback 
        }   
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(nextAudioKey))
        {
            // go to the next track
            NextTrack();
        }
    }

    public void ToggleRadio()
    {
        if(radioAudioSource == null || playlist == null && playlist.Length <= 0) return; // we don't do anything if either of the conditions are met

        isRadioPlaying = !isRadioPlaying; // flips between states    

        radioAudioSource.mute = !isRadioPlaying; // Make it mute if it should

    }

    public void NextTrack()
    {
        
        if(radioAudioSource == null || playlist == null && playlist.Length <= 0) return; // we don't do anything if either of the conditions are met

        currentTrackindex = (currentTrackindex + 1) % playlist.Length; // if the array is at the end, we go back to [0]

        radioAudioSource.clip = playlist[currentTrackindex]; // Swaps the clip to the next track
        
        radioAudioSource.Play(); // starts playing the new Music

    }

}
