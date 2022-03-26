using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using MelonLoader;

namespace MyBhapticsTactsuit
{
    public class TactsuitVR
    {
        public bool suitDisabled = true;
        public bool systemInitialized = false;
        // Event to start and stop the heartbeat thread
        private static ManualResetEvent HeartBeat_mrse = new ManualResetEvent(false);
        private static ManualResetEvent GrabUsedRight_mrse = new ManualResetEvent(false);
        private static ManualResetEvent GrabUsedLeft_mrse = new ManualResetEvent(false);
        // dictionary of all feedback patterns found in the bHaptics directory
        public Dictionary<String, FileInfo> FeedbackMap = new Dictionary<String, FileInfo>();

        private static bHaptics.RotationOption defaultRotationOption = new bHaptics.RotationOption(0.0f, 0.0f);

        public TactsuitVR()
        {
            LOG("Initializing suit");
            if (!bHaptics.WasError)
            {
                suitDisabled = false;
            }
            RegisterAllTactFiles();
            Thread HeartBeatThread = new Thread(HeartBeatFunc);
            HeartBeatThread.Start();
            Thread GrabUsedRightThread = new Thread(GrabUsedRightFunc);
            GrabUsedRightThread.Start();
            Thread GrabUsedLeftThread = new Thread(GrabUsedLeftFunc);
            GrabUsedLeftThread.Start();
        }

        public void LOG(string logStr)
        {
#pragma warning disable CS0618 // remove warning that the logger is deprecated
            MelonLogger.Msg(logStr);
#pragma warning restore CS0618
        }

        void RegisterAllTactFiles()
        {
            // Get location of the compiled assembly and search through "bHaptics" directory and contained patterns
            string configPath = Directory.GetCurrentDirectory() + "\\Mods\\bHaptics";
            DirectoryInfo d = new DirectoryInfo(configPath);
            FileInfo[] Files = d.GetFiles("*.tact", SearchOption.AllDirectories);
            for (int i = 0; i < Files.Length; i++)
            {
                string filename = Files[i].Name;
                string fullName = Files[i].FullName;
                string prefix = Path.GetFileNameWithoutExtension(filename);
                // LOG("Trying to register: " + prefix + " " + fullName);
                if (filename == "." || filename == "..")
                    continue;
                string tactFileStr = File.ReadAllText(fullName);
                try
                {
                    bHaptics.RegisterFeedbackFromTactFile(prefix, tactFileStr);
                    LOG("Pattern registered: " + prefix);
                }
                catch (Exception e) { LOG(e.ToString()); }

                FeedbackMap.Add(prefix, Files[i]);
            }
            systemInitialized = true;
        }

        public void PlaybackHaptics(String key, float intensity = 1.0f, float duration = 1.0f)
        {
            if (FeedbackMap.ContainsKey(key))
            {
                bHaptics.ScaleOption scaleOption = new bHaptics.ScaleOption(intensity, duration);
                bHaptics.SubmitRegistered(key, key, scaleOption, defaultRotationOption);
            }
            else
            {
                LOG("Feedback not registered: " + key);
            }
        }

        public void PlayBackHit(String key, float xzAngle, float yShift)
        {
            // two parameters can be given to the pattern to move it on the vest:
            // 1. An angle in degrees [0, 360] to turn the pattern to the left
            // 2. A shift [-0.5, 0.5] in y-direction (up and down) to move it up or down
            bHaptics.ScaleOption scaleOption = new bHaptics.ScaleOption(1f, 1f);
            bHaptics.RotationOption rotationOption = new bHaptics.RotationOption(xzAngle, yShift);
            bHaptics.SubmitRegistered(key, key, scaleOption, rotationOption);
        }

        public void HeartBeatFunc()
        {
            while (true)
            {
                // Check if reset event is active
                HeartBeat_mrse.WaitOne();
                bHaptics.SubmitRegistered("HeartBeat");
                Thread.Sleep(600);
            }
        }

        public void GrabUsedLeftFunc()
        {
            while (true)
            {
                // Check if reset event is active
                GrabUsedLeft_mrse.WaitOne();
                bHaptics.SubmitRegistered("GrabVest_L");
                bHaptics.SubmitRegistered("GrabArms_L");
                Thread.Sleep(1000);
            }
        }

        public void GrabUsedRightFunc()
        {
            while (true)
            {
                // Check if reset event is active
                GrabUsedRight_mrse.WaitOne();
                bHaptics.SubmitRegistered("GrabVest_R");
                bHaptics.SubmitRegistered("GrabArms_R");
                Thread.Sleep(1000);
            }
        }

        public void StartHeartBeat()
        {
            HeartBeat_mrse.Set();
        }

        public void StopHeartBeat()
        {
            HeartBeat_mrse.Reset();
        }

        public void StartGrabUsedLeft()
        {
            GrabUsedLeft_mrse.Set();
        }

        public void StopGrabUsedLeft()
        {
            GrabUsedLeft_mrse.Reset();
        }

        public void StartGrabUsedRight()
        {
            GrabUsedRight_mrse.Set();
        }

        public void StopGrabUsedRight()
        {
            GrabUsedRight_mrse.Reset();
        }

        public bool IsPlaying(String effect)
        {
            return bHaptics.IsPlaying(effect);
        }

        public void StopHapticFeedback(String effect)
        {
            bHaptics.TurnOff(effect);
        }

        public void StopAllHapticFeedback()
        {
            StopThreads();
            foreach (String key in FeedbackMap.Keys)
            {
                bHaptics.TurnOff(key);
            }
        }

        public void StopThreads()
        {
            // Yes, looks silly here, but if you have several threads like this, this is
            // very useful when the player dies or starts a new level
            StopHeartBeat();
            StopGrabUsedLeft();
            StopGrabUsedRight();
        }


    }
}
