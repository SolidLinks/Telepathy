﻿using System;
using UnityEngine;
using System.Threading;

public class HUD : MonoBehaviour
{
    [Header("Stress test")]
    public int packetsPerTick = 1000;
    public byte[] stressBytes = new byte[]{0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01, 0xAF, 0xFE, 0x01};
    bool stressTestRunning = false;

    void Awake()
    {
        // update even if window isn't focused, otherwise we don't receive.
        Application.runInBackground = true;

        // use Debug.Log functions for TCP so we can see it in the console
        Logger.LogMethod = Debug.Log;
        Logger.LogWarningMethod = Debug.LogWarning;
        Logger.LogErrorMethod = Debug.LogError;
    }

    void Update()
    {
        if (GoodOldTCPClient.Connected)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GoodOldTCPClient.Send(new byte[]{0xAF, 0xFE});
                GoodOldTCPClient.Send(new byte[]{0xBA, 0xBE});
                //GoodOldTCPClient.Send(stressBytes);
            }

            if (Input.GetKeyDown(KeyCode.S))
            {
                stressTestRunning = !stressTestRunning;
                if (stressTestRunning)
                    Debug.Log("client start stress test with: " + packetsPerTick + " packets per tick");
            }

            // SPAM
            if (stressTestRunning)
            {
                for (int i = 0; i < packetsPerTick; ++i)
                    GoodOldTCPClient.Send(stressBytes);
            }

            // any new message?
            GoodOldEventType eventType;
            byte[] data;
            if (GoodOldTCPClient.GetNextMessage(out eventType, out data))
            {
                Debug.Log("received event=" + eventType + " msg: " + (data != null ? BitConverter.ToString(data) : "null"));
            }
        }

        if (GoodOldTCPServer.Active)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                GoodOldTCPServer.Send(0, new byte[]{0xAF, 0xFE});
                GoodOldTCPServer.Send(0, new byte[]{0xBA, 0xBE});
            }

            // any new message?
            // -> calling it once per frame is okay, but really why not just
            //    process all messages and make it empty..
            byte[] data;
            GoodOldEventType eventType;
            uint connectionId;
            int receivedCount = 0;
            while (GoodOldTCPServer.GetNextMessage(out connectionId, out eventType, out data))
            {
                Debug.Log("received connectionId=" + connectionId + " event=" + eventType + " msg: " + (data != null ? BitConverter.ToString(data) : "null"));
                ++receivedCount;
            }
            if (receivedCount > 0) Debug.Log("Server received " + receivedCount + " messages this frame."); // easier on CPU to log this way
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, 0, 300, 300));

        // client
        GUILayout.BeginHorizontal();
        GUI.enabled = !GoodOldTCPClient.Connected;
        if (GUILayout.Button("Connect Client"))
        {
            GoodOldTCPClient.Connect("127.0.0.1", 1337);
        }
        GUI.enabled = GoodOldTCPClient.Connected;
        if (GUILayout.Button("Disconnect Client"))
        {
            GoodOldTCPClient.Disconnect();
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        // server
        GUILayout.BeginHorizontal();
        GUI.enabled = !GoodOldTCPServer.Active;
        if (GUILayout.Button("Start Server"))
        {
            GoodOldTCPServer.StartServer("127.0.0.1", 1337);
        }
        GUI.enabled = GoodOldTCPServer.Active;
        if (GUILayout.Button("Stop Server"))
        {
            GoodOldTCPServer.StopServer();
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }
}
