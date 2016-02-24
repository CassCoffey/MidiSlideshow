using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Sanford.Multimedia.Midi;

public class MidiManager : MonoBehaviour
{
    public string trackName;
    public int channel;
    public ParticleSystem particles;

    public Image[] images;

    public Sprite[] loaded;

    private Sequencer sequencer;
    private Sequence track;
    private OutputDevice outDevice;
    private int outDeviceID = 0;
    private int currentImage = 0;

    private float cooldown = 0;
    private bool notePlayed = false;
    private bool singleChannel = false;
    private bool playing = false;

    void Start()
    {
        track = new Sequence();
        sequencer = new Sequencer();
        outDevice = new OutputDevice(outDeviceID);
        track.Format = 1;
        sequencer.Position = 0;
        sequencer.Sequence = track;
        sequencer.ChannelMessagePlayed += new System.EventHandler<Sanford.Multimedia.Midi.ChannelMessageEventArgs>(HandleChannelMessagePlayed);
        sequencer.Chased += new System.EventHandler<Sanford.Multimedia.Midi.ChasedEventArgs>(HandleChased);

        loaded = Resources.LoadAll<Sprite>("Images");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            playing = !playing;
            if (playing)
            {
                Play();
            }
            else
            {
                Stop();
            }
        }

        if (notePlayed && cooldown <= 0)
        {
            images[currentImage].sprite = loaded[Random.Range(0, loaded.Length)];
            if (particles != null)
            {
                particles.gameObject.GetComponent<ParticleSystemRenderer>().material.mainTexture = images[0].mainTexture;
                particles.Emit(5);
            }

            currentImage++;
            if (currentImage >= images.Length)
            {
                currentImage = 0;
            }

            cooldown = 0.1f;
        }

        notePlayed = false;
        cooldown -= Time.unscaledDeltaTime;
    }

    public void LoadTrack()
    {
        track.Load("Assets/Midis/" + trackName + ".mid");
    }

    public void ChangeTrackName(string name)
    {
        trackName = name;
    }

    public void SetSingleChannel(bool set)
    {
        singleChannel = set;
    }

    public void ChangeChannel(float newChannel)
    {
        channel = (int)newChannel;
    }

    private void HandleChannelMessagePlayed(object sender, ChannelMessageEventArgs e)
    {
        if (singleChannel && e.Message.MidiChannel != channel)
        {
            return;
        }
        if (e.Message.Command == ChannelCommand.NoteOn && e.Message.Data2 != 0)
        {
            outDevice.Send(e.Message);
            NotePlayed(e);
        }
    }

    private void HandleChased(object sender, ChasedEventArgs e)
    {
        foreach (ChannelMessage message in e.Messages)
        {
            outDevice.Send(message);
        }
    }

    private void NotePlayed(ChannelMessageEventArgs e)
    {
        //Debug.Log("Channel: " + e.Message.MidiChannel + ", Pitch: " + e.Message.Data1 + ", Volume: " + e.Message.Data2);

        notePlayed = true;
    }

    public void Play()
    {
        sequencer.Start();
    }

    public void Stop()
    {
        sequencer.Stop();
    }

    public void Cleanup()
    {
        sequencer.Dispose();
        track.Dispose();
        outDevice.Dispose();
    }

    void OnDestroy()
    {
        Stop();
        Cleanup();
    }
}
