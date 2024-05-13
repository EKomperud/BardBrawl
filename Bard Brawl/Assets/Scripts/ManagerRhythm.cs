using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Windows;

// Where I left off:
// Want to make this more stylish
// 

public class ManagerRhythm : MonoBehaviour
{
    [SerializeField]
    [Tooltip("BPM (beats per minute) of song.")]
    private float bpm;
    [SerializeField]
    [Tooltip("Length of song, in seconds.")]
    private float length;
    [SerializeField]
    [Tooltip("Speed of scroller, in seconds from noteSpawn to noteArrival.")]
    private float spawnAdvance;
    [SerializeField]
    [Tooltip("Window of time on either side of a note, to hit it.")]
    private float hitForgiveness;

    [Space(16f)]

    [SerializeField]
    private MidiStreamPlayer streamPlayer;
    public MusicTheory.Majors chordBase = MusicTheory.Majors.C;
    public MusicTheory.Triads chordType = MusicTheory.Triads.Major;
    public int octaveShift = 0;
    public int instrument = 0;

    [Space(16f)]

    [SerializeField]
    [Tooltip("Note prefabs, in order of Q, W, E, and R.")]
    private GameObject[] notePrefabs;

    [Space(16f)]

    [SerializeField]
    [Tooltip("Particle Prefabs that spawn when hitting a note.")]
    private GameObject[] particlePrefabs;

    private const float spawnDistance = 3.2f;
    private float spawnScaling;

    private float[] beats;                  // Tracks at what time each beat will arrive at
    private int[,] notes;                    // Tracks which notes are associated with each beat
    private GameObject[] noteObjects;       // Holds the note gameobjects
    private int noteObjectsCount;           // Tracks how many notes are actively on the Rhythm Holder
    private string[] noteNames;             // Holds the note names for printing
    private int spawnIterator;              // Tracks which note/beat is next to spawn on the Rhythm Holder
    private int hitIterator;                // Tracks which note/beat is next to be hit on the Rhythm Holder
    private float progressTimer;            // Tracks how far along the song is
    private float countdown;                // Tracks how much time elapses before the song starts
    private float scrollSpeed;              // Tracks how fast notes move along the Rhythm Holder
    private float hitOpen;                  // Tracks how far ahead of time a note will be affected by player input
                                            // Within this time, missing a note will dequeue it. 4x hitForgiveness

    private MPTKEvent[] midiNotes;
    private MPTKEvent[] midiChord;
    private GameObject[] noteSpawners;
    private GameObject particleSpawner;

    // Start is called before the first frame update
    void Start()
    {
        // Set up the Beats array
        float bps = bpm / 60f;
        int beatsArrayLength = (int)Mathf.Floor(bps * length);
        beats = new float[beatsArrayLength];
        float b = 0f;
        for (int i = 0; i < beatsArrayLength; i++)
        {
            beats[i] = b;
            b = b + (1 / bps);
        }
        spawnIterator = 0;

        // Set up the Notes array
        notes = new int[beatsArrayLength, 4];
        System.DateTime now = System.DateTime.Now;
        UnityEngine.Random.InitState(now.Month + now.Day + now.Hour + now.Second + now.Millisecond);
        for (int i = 0; i < beatsArrayLength; i++)
        {
            float rF = UnityEngine.Random.Range(0.00f, 3.99f);
            int rI = Mathf.FloorToInt(rF);
            notes[i,0] = rI;

            if (UnityEngine.Random.Range(0f, 1f) > 0.8f)
            {
                int j = (rI + 1) % 4;
                notes[i, 1] = j;
            }
        }
        hitIterator = 0;
        hitOpen = 4f * hitForgiveness;

        // Set up Note Queue
        noteObjects = new GameObject[beatsArrayLength];
        noteObjectsCount = 0;

        // Set up Spawners
        noteSpawners = new GameObject[4];
        GameObject rhythmBack = gameObject.transform.GetChild(0).gameObject;
        noteSpawners[0] = rhythmBack.transform.GetChild(0).gameObject;
        noteSpawners[1] = rhythmBack.transform.GetChild(1).gameObject;
        noteSpawners[2] = rhythmBack.transform.GetChild(2).gameObject;
        noteSpawners[3] = rhythmBack.transform.GetChild(3).gameObject;
        particleSpawner = transform.GetChild(1).GetChild(0).gameObject;
        spawnScaling = transform.localScale.x;

        // Initialize Timers
        countdown = 5f;
        progressTimer = -countdown;
        scrollSpeed = spawnDistance / spawnAdvance;

        // All the rest
        noteNames = new string[] { "Q", "W", "E", "R"};
        midiNotes = new MPTKEvent[36];
        for (int i = 0; i < 36; i++)
        {
            midiNotes[i] = new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = i + 48,
                Channel = 0,
                Duration = -1,
                Velocity = 100
            };
        }
        int[] chord;
        try
        {
            chord = MusicTheory.GetTriad(chordBase, chordType, octaveShift);
        }
        catch
        {
            chord = MusicTheory.GetTriad(chordBase, chordType, 0);
        }
        midiChord = new MPTKEvent[]
        {
            new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = chord[0],
                Channel = 0,
                Duration = -1,
                Velocity = 100
            },
            new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = chord[1],
                Channel = 0,
                Duration = -1,
                Velocity = 100
            },
            new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = chord[2],
                Channel = 0,
                Duration = -1,
                Velocity = 100
            },
            new MPTKEvent()
            {
                Command = MPTKCommand.NoteOn,
                Value = chord[0] + 12,
                Channel = 0,
                Duration = -1,
                Velocity = 100
            },
        };

    }

    void Update()
    {
        // Advance progress timer
        progressTimer += Time.deltaTime;

        // Note Update
        UpdateNoteLocations();

        // Note Spawn
        // * Might want more efficient way to check if notes should still spawn
        UpdateNoteSpawning();

        // Check for player input, and to see if they hit any notes
        UpdateInputChecking();

    }

    #region Update Functions

    private void UpdateNoteLocations()
    {
        for (int i = hitIterator; i < hitIterator + noteObjectsCount; i++)
        {
            noteObjects[i].transform.Translate(new Vector3(0f, -scrollSpeed * Time.deltaTime * spawnScaling, 0f));
        }
    }

    private void UpdateNoteSpawning()
    {
        // Spawn Notes
        if (spawnIterator < beats.Length)
        {
            if (progressTimer >= (beats[spawnIterator] - spawnAdvance))
            {
                float beatToSpawn = beats[spawnIterator];
                int noteToSpawn = notes[spawnIterator,0];
                SpawnNote(noteToSpawn);

                spawnIterator++;
            }
        }

        // Despawn Notes
        if (hitIterator < beats.Length)
        {
            if (progressTimer > beats[hitIterator] + hitForgiveness)
            {
                Debug.Log("Missed: " + beats[hitIterator] + "! You input: Nothing!");
                MissNote(hitIterator);
            }
        }
    }

    private void UpdateInputChecking()
    {
        KeyCode[] inputs = CheckKeysDown();
        if (inputs.Length != 6 && CheckWithinHitOpen())
        {
            bool hit = CheckForHit(inputs);
            int[] inputInts = CheckInputInts(inputs);
            if (hit)
            {
                // correct!
                // do something cool with note from notes[hitIterator]
                Debug.Log("Hit: " + noteNames[notes[hitIterator, 0]] + "!");
                foreach (int i in inputInts)
                {
                    streamPlayer.MPTK_PlayEvent(midiChord[i]);
                }
                StartCoroutine(KnockNote(hitIterator));
                StartCoroutine(ShakeScreen());
                StartCoroutine(SpawnParticle());
            }
            else
            {
                // wrong!
                // do something mean with note from notes[hitIterator]
                Debug.Log("Missed: " + beats[hitIterator] + "! You input: " + noteNames[inputInts[0]] + "...");
                MissNote(hitIterator);
            }
        }

        KeyCode[] ups = CheckKeysUp();
        if (ups.Length != 0)
        {
            int[] upInts = CheckInputInts(ups);
            for (int i = 0; i < upInts.Length; i++)
            {
                streamPlayer.MPTK_StopEvent(midiChord[upInts[i]]);
            }
        }

        //KeyCode input = CheckInputKey();
        //if (input != KeyCode.None && CheckWithinHitOpen())
        //{
            
        //}
    }

    #endregion

    #region Checker Functions

    private KeyCode CheckInputKey()
    {
        // Multi input?
        //      Would need an array of KeyCode
        //      Plan on this, but let's just keep it at 1 for now.
        // Held input?
        //      Would need Input.GetKey();

        if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
            return KeyCode.Q;
        if (UnityEngine.Input.GetKeyDown(KeyCode.W))
            return KeyCode.W;
        if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            return KeyCode.E;
        if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            return KeyCode.R;
        if (UnityEngine.Input.anyKeyDown)
            return KeyCode.Tilde;

        return KeyCode.None;
    }

    private KeyCode[] CheckKeysUp()
    {
        KeyCode[] k = new KeyCode[4];
        int i = 0;
        if (UnityEngine.Input.GetKeyUp(KeyCode.Q))
        {
            k[i] = KeyCode.Q;
            i++;
        }
        if (UnityEngine.Input.GetKeyUp(KeyCode.W))
        {
            k[i] = KeyCode.W;
            i++;
        }
        if (UnityEngine.Input.GetKeyUp(KeyCode.E))
        {
            k[i] = KeyCode.E;
            i++;
        }
        if (UnityEngine.Input.GetKeyUp(KeyCode.R))
        {
            k[i] = KeyCode.R;
            i++;
        }
        Array.Resize(ref k, i);

        return k;
    }

    private KeyCode[] CheckKeysDown()
    {
        KeyCode[] k = new KeyCode[6];
        int i = 0;
        string s = UnityEngine.Input.inputString;
        if (UnityEngine.Input.anyKeyDown)
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
            {
                k[i] = KeyCode.Q;
                i++;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.W))
            {
                k[i] = KeyCode.W;
                i++;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
            {
                k[i] = KeyCode.E;
                i++;
            }
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                k[i] = KeyCode.R;
                i++;
            }
            if (i != s.Length)
            {
                k[i] = KeyCode.Tilde;
                i++;
            }
            Array.Resize(ref k, i);
        }

        return k;
    }

    private int CheckInputInt(KeyCode input)
    {
        switch (input)
        {
            case KeyCode.Q:
                return 0;
            case KeyCode.W:
                return 1;
            case KeyCode.E:
                return 2;
            case KeyCode.R:
                return 3;
            case KeyCode.Tilde:
                return 4;
            default:
                return -1;
        }
    }

    private int[] CheckInputInts(KeyCode[] inputs)
    {
        int[] r = new int[inputs.Length];
        for (int i = 0; i < inputs.Length; i++)
        {
            r[i] = CheckInputInt(inputs[i]);
        }
        return r;
    }

    private bool CheckWithinHitOpen()
    {
        try
        {
            return progressTimer >= beats[hitIterator] - hitOpen;
        }
        catch
        {
            return false;
        }
    }

    private bool CheckForHit(KeyCode input)
    {
        if (progressTimer >= beats[hitIterator] - hitForgiveness && progressTimer <= beats[hitIterator] + hitForgiveness)
        {
            int inputInt = CheckInputInt(input);

            return notes[hitIterator,0] == inputInt;
        }
        return false;
    }

    private bool CheckForHit(KeyCode[] inputs)
    {
        if (progressTimer >= beats[hitIterator] - hitForgiveness && progressTimer <= beats[hitIterator] + hitForgiveness)
        {
            int[] inputInts = CheckInputInts(inputs);

            bool r = true;
            for (int i = 0; i < inputInts.Length; i++)
            {
                if (notes[hitIterator,i] != inputInts[i])
                {
                    r = false;
                }
            }
            return r;
        }
        return false;
    }

    #endregion

    #region Spawner Functions

    private void SpawnNote(int noteToSpawn)
    {
        GameObject prefabToSpawn = notePrefabs[noteToSpawn];
        //GameObject spawnedNote = Instantiate(prefabToSpawn, new Vector3(noteToSpawn / 10f, spawnDistance, 0f), new Quaternion(), spawners[noteToSpawn].transform);
        GameObject spawnedNote = Instantiate(prefabToSpawn, noteSpawners[noteToSpawn].transform, false);
        spawnedNote.transform.Translate(new Vector3(0f, spawnDistance * spawnScaling, 0f));
        noteObjects[spawnIterator] = spawnedNote;
        noteObjectsCount++;

        MPTKEvent patchChange = new MPTKEvent()
        {
            Command = MPTKCommand.PatchChange,
            Value = instrument,
            Channel = 0
        };
        streamPlayer.MPTK_PlayEvent(patchChange);
    }

    private void DespawnNote(int i)
    {
        Destroy(noteObjects[i]);
    }

    private void MissNote(int i)
    {
        noteObjectsCount--;
        hitIterator++;
        DespawnNote(i);
    }

    IEnumerator KnockNote(int i)
    {
        noteObjectsCount--;
        hitIterator++;

        float xVelocity = UnityEngine.Random.Range(0.0015f, 0.0065f);
        float yVelocity = UnityEngine.Random.Range(0.01f, 0.02f);
        float zVelocity = UnityEngine.Random.Range(-0.055f, -0.015f);
        float zSpin = UnityEngine.Random.Range(-0.75f, -1.5f);
        Transform note = noteObjects[i].transform;
        float lifetime = 3f;

        while (lifetime > 0f)
        {
            Vector3 t = new Vector3(xVelocity, yVelocity, zVelocity);
            note.transform.Translate(t, Space.World);
            note.transform.Rotate(0f, 0f, zSpin);

            xVelocity *= 0.99f;
            yVelocity -= 0.0002f;
            zVelocity *= 0.99f;

            lifetime -= Time.deltaTime;
            yield return null;
        }

        DespawnNote(i);
    }

    #endregion

    #region Flashy Functions

    private IEnumerator ShakeScreen()
    {
        Transform rh = transform;
        Vector3 op = transform.position;
        float lifetime = 0.15f;
        float power = 0.02f;

        while (lifetime > 0)
        {
            float xTran = UnityEngine.Random.Range(-power, power);
            float yTran = UnityEngine.Random.Range(-power, power);
            power *= 0.985f;

            rh.transform.position = new Vector3(op.x + xTran, op.y + yTran, op.z);

            lifetime -= Time.deltaTime;
            yield return null;
        }
        rh.transform.position = op;
    }

    private IEnumerator SpawnParticle()
    {
        float xVelocity = UnityEngine.Random.Range(-0.0015f, 0.0025f) * spawnScaling;
        float yVelocity = UnityEngine.Random.Range(0.001f, 0.002f) * spawnScaling;
        float zVelocity = UnityEngine.Random.Range(-0.01f, -0.005f) * spawnScaling;

        GameObject note = Instantiate(particlePrefabs[Mathf.FloorToInt(UnityEngine.Random.Range(0, particlePrefabs.Length - 0.001f))]);
        note.transform.position = particleSpawner.transform.position;
        note.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        note.transform.localScale = new Vector3(spawnScaling,spawnScaling, spawnScaling);

        float lifetime = 1f;
        
        while (lifetime > 0)
        {
            Vector3 t = new Vector3(xVelocity, yVelocity, zVelocity);
            note.transform.Translate(t, Space.World);

            xVelocity *= 0.99f;
            zVelocity *= 0.99f;

            lifetime -= Time.deltaTime;

            yield return null;
        }

        Destroy(note);
    }

    #endregion
}
