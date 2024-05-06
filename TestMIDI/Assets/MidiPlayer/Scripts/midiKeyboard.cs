using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MidiPlayerTK;

public class midiKeyboard : MonoBehaviour
{
    public MidiStreamPlayer keyboard;

    MPTKEvent q;
    MPTKEvent[] keys;

    // Start is called before the first frame update
    void Start()
    {
        keys = new MPTKEvent[4];
        keys[0] = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn,
            Value = 60,
            Channel = 0,
            Duration = -1,
            Velocity = 100
        };
        keys[1] = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn,
            Value = 64,
            Channel = 0,
            Duration = -1,
            Velocity = 100
        };
        keys[2] = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn,
            Value = 67,
            Channel = 0,
            Duration = -1,
            Velocity = 100
        };
        keys[3] = new MPTKEvent()
        {
            Command = MPTKCommand.NoteOn,
            Value = 72,
            Channel = 0,
            Duration = -1,
            Velocity = 100
        };
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            keyboard.MPTK_PlayEvent(keys[0]);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            keyboard.MPTK_PlayEvent(keys[1]);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            keyboard.MPTK_PlayEvent(keys[2]);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            keyboard.MPTK_PlayEvent(keys[3]);
        }

        if (Input.GetKeyUp(KeyCode.Q))
        {
            keyboard.MPTK_StopEvent(keys[0]);
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            keyboard.MPTK_StopEvent(keys[1]);
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            keyboard.MPTK_StopEvent(keys[2]);
        }
        if (Input.GetKeyUp(KeyCode.R))
        {
            keyboard.MPTK_StopEvent(keys[3]);
        }
    }
}
