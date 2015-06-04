﻿using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using UnityEngine;


[AddComponentMenu("FrameCapturer/ExrOffscreenCapturer")]
public class ExrOffscreenCapturer : MonoBehaviour
{
    [System.Serializable]
    public class ChannelData
    {
        public string name;
        public int channel;
    }

    [System.Serializable]
    public class CaptureData
    {
        public RenderTexture target;
        public ChannelData[] channels;
    }

    public CaptureData[] m_targets;

    public string m_output_directory = "ExrOutput";
    public string m_output_filename;
    public int m_begin_frame = 0;
    public int m_end_frame = 100;
    public int m_max_active_tasks = 1;
    IntPtr m_exr;
    int m_frame;



    void OnEnable()
    {
        System.IO.Directory.CreateDirectory(m_output_directory);

        FrameCapturer.fcExrConfig conf;
        conf.max_active_tasks = m_max_active_tasks;
        m_exr = FrameCapturer.fcExrCreateContext(ref conf);
    }

    void OnDisable()
    {
        FrameCapturer.fcExrDestroyContext(m_exr);
    }

    void Update()
    {
        StartCoroutine(Capture());
    }

    IEnumerator Capture()
    {
        int frame = m_frame++;
        if (frame >= m_begin_frame && frame <= m_end_frame)
        {
            if (m_targets.Length == 0) { yield break; }
            yield return new WaitForEndOfFrame();


            Debug.Log("ExrOffscreenCapturer: frame " + frame);

            var rt = m_targets[0].target;
            string path = m_output_directory + "/" + m_output_filename + "_" + frame.ToString("0000") + ".exr";

            FrameCapturer.fcExrBeginFrame(m_exr, path, rt.width, rt.height);
            for (int ti = 0; ti < m_targets.Length; ++ti)
            {
                var target = m_targets[ti];
                for (int ci = 0; ci < target.channels.Length; ++ti)
                {
                    var ch = target.channels[ti];
                    AddLayer(target.target, ch.channel, ch.name);
                }
            }
            FrameCapturer.fcExrEndFrame(m_exr);
        }
    }
    void AddLayer(RenderTexture rt, int ch, string name)
    {
        FrameCapturer.fcExrAddLayer(m_exr, rt.GetNativeTexturePtr(), rt.format, ch, name);
    }
}