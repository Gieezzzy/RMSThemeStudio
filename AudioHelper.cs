#nullable enable
using System;
using System.IO;
using System.Media;
using System.Text;

namespace RMSThemeStudio
{
    public static class AudioHelper
    {
        private static SoundPlayer? _player;

        public static void PlayWav(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    _player?.Stop();
                    _player?.Dispose();
                    _player = new SoundPlayer(filePath);
                    _player.Play();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Audio playback error: " + ex.Message);
            }
        }

        public static void StopAudio()
        {
            try
            {
                _player?.Stop();
                _player?.Dispose();
                _player = null;
            }
            catch { }
        }

        public static void SynthesizeSound(string style, string outputPath)
        {
            int sampleRate = 44100;
            short channels = 1;
            short bitsPerSample = 16;

            byte[] pcmData = style.ToLower() switch
            {
                "christmas" or "bells" => GenerateBellChime(sampleRate),
                "halloween" or "spooky" => GenerateSpookyDrone(sampleRate),
                "heartbeat" or "freddy" => GenerateHeartbeat(sampleRate),
                "jumpscare" or "scream" or "sadako" => GenerateJumpscareScream(sampleRate),
                "bonifacio" or "fanfare" => GenerateFanfare(sampleRate),
                _ => GenerateDefaultPing(sampleRate)
            };

            using var fs = new FileStream(outputPath, FileMode.Create, FileAccess.Write);
            using var bw = new BinaryWriter(fs);

            // RIFF chunk
            bw.Write(Encoding.ASCII.GetBytes("RIFF"));
            bw.Write(36 + pcmData.Length);
            bw.Write(Encoding.ASCII.GetBytes("WAVE"));

            // fmt chunk
            bw.Write(Encoding.ASCII.GetBytes("fmt "));
            bw.Write(16); // Subchunk1Size (16 for PCM)
            bw.Write((short)1); // AudioFormat (1 for PCM)
            bw.Write(channels);
            bw.Write(sampleRate);
            bw.Write(sampleRate * channels * (bitsPerSample / 8)); // ByteRate
            bw.Write((short)(channels * (bitsPerSample / 8))); // BlockAlign
            bw.Write(bitsPerSample);

            // data chunk
            bw.Write(Encoding.ASCII.GetBytes("data"));
            bw.Write(pcmData.Length);
            bw.Write(pcmData);
        }

        private static byte[] GenerateDefaultPing(int sampleRate)
        {
            double duration = 0.55;
            int totalSamples = (int)(sampleRate * duration);
            byte[] bytes = new byte[totalSamples * 2];

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double env = Math.Exp(-7.0 * t);
                double freq = 987.77; // B5 note
                double sample = Math.Sin(2.0 * Math.PI * freq * t) * env;

                short val = (short)(sample * 24000);
                bytes[i * 2] = (byte)(val & 0xFF);
                bytes[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
            }
            return bytes;
        }

        private static byte[] GenerateBellChime(int sampleRate)
        {
            double duration = 1.2;
            int totalSamples = (int)(sampleRate * duration);
            byte[] bytes = new byte[totalSamples * 2];

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double env = Math.Exp(-3.2 * t);
                // Dual chime harmonics (E6 + G#6)
                double s1 = Math.Sin(2.0 * Math.PI * 1318.5 * t);
                double s2 = 0.6 * Math.Sin(2.0 * Math.PI * 1661.2 * t);
                double s3 = 0.3 * Math.Sin(2.0 * Math.PI * 2637.0 * t);
                double sample = (s1 + s2 + s3) * env * 0.7;

                short val = (short)(sample * 22000);
                bytes[i * 2] = (byte)(val & 0xFF);
                bytes[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
            }
            return bytes;
        }

        private static byte[] GenerateSpookyDrone(int sampleRate)
        {
            double duration = 1.4;
            int totalSamples = (int)(sampleRate * duration);
            byte[] bytes = new byte[totalSamples * 2];

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double env = Math.Sin(Math.PI * (t / duration));
                // Tremolo modulation
                double tremolo = 0.8 + 0.2 * Math.Sin(2.0 * Math.PI * 8.0 * t);
                double pitchGlide = 140.0 + 80.0 * Math.Sin(2.0 * Math.PI * 0.7 * t);
                double sample = Math.Sin(2.0 * Math.PI * pitchGlide * t) * tremolo * env;

                short val = (short)(sample * 22000);
                bytes[i * 2] = (byte)(val & 0xFF);
                bytes[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
            }
            return bytes;
        }

        private static byte[] GenerateHeartbeat(int sampleRate)
        {
            double duration = 0.9;
            int totalSamples = (int)(sampleRate * duration);
            byte[] bytes = new byte[totalSamples * 2];

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double sample = 0;

                // Thump 1 at t = 0.05
                if (t >= 0.05 && t < 0.30)
                {
                    double subT = t - 0.05;
                    sample += Math.Sin(2.0 * Math.PI * 55.0 * subT) * Math.Exp(-14.0 * subT);
                }
                // Thump 2 at t = 0.32
                if (t >= 0.32 && t < 0.65)
                {
                    double subT = t - 0.32;
                    sample += 0.8 * Math.Sin(2.0 * Math.PI * 65.0 * subT) * Math.Exp(-12.0 * subT);
                }

                short val = (short)(sample * 26000);
                bytes[i * 2] = (byte)(val & 0xFF);
                bytes[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
            }
            return bytes;
        }

        private static byte[] GenerateJumpscareScream(int sampleRate)
        {
            double duration = 1.0;
            int totalSamples = (int)(sampleRate * duration);
            byte[] bytes = new byte[totalSamples * 2];
            var rnd = new Random(42);

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double env = (t < 0.08) ? (t / 0.08) : Math.Exp(-3.5 * (t - 0.08));

                // Piercing dissonance + harsh noise
                double s1 = Math.Sin(2.0 * Math.PI * 1850.0 * t);
                double s2 = 0.7 * Math.Sin(2.0 * Math.PI * 2280.0 * t);
                double noise = (rnd.NextDouble() * 2.0 - 1.0) * 0.45;

                double sample = (s1 + s2 + noise) * env * 0.7;
                short val = (short)(Math.Clamp(sample, -1.0, 1.0) * 28000);
                bytes[i * 2] = (byte)(val & 0xFF);
                bytes[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
            }
            return bytes;
        }

        private static byte[] GenerateFanfare(int sampleRate)
        {
            double duration = 1.3;
            int totalSamples = (int)(sampleRate * duration);
            byte[] bytes = new byte[totalSamples * 2];

            for (int i = 0; i < totalSamples; i++)
            {
                double t = (double)i / sampleRate;
                double freq = (t < 0.3) ? 440.0 : ((t < 0.6) ? 554.37 : 659.25);
                double env = (t < 1.0) ? 0.9 : Math.Exp(-8.0 * (t - 1.0));

                double s1 = Math.Sin(2.0 * Math.PI * freq * t);
                double s2 = 0.4 * Math.Sin(2.0 * Math.PI * (freq * 2) * t);
                double sample = (s1 + s2) * env * 0.65;

                short val = (short)(sample * 24000);
                bytes[i * 2] = (byte)(val & 0xFF);
                bytes[i * 2 + 1] = (byte)((val >> 8) & 0xFF);
            }
            return bytes;
        }
    }
}
