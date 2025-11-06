using UnityEngine;

/// <summary>
/// Utility class for generating simple audio clips programmatically
/// Useful for testing when no audio assets are available
/// </summary>
public static class AudioClipGenerator
{
    /// <summary>
    /// Creates a simple beep sound
    /// </summary>
    public static AudioClip CreateBeep(float frequency = 440f, float duration = 0.1f, int sampleRate = 44100)
    {
        int sampleCount = Mathf.RoundToInt(duration * sampleRate);
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * time) * 0.5f;
            
            // Apply fade out to prevent clicking
            if (i > sampleCount * 0.8f)
            {
                float fadeOut = 1f - (float)(i - sampleCount * 0.8f) / (sampleCount * 0.2f);
                samples[i] *= fadeOut;
            }
        }
        
        AudioClip clip = AudioClip.Create($"Beep_{frequency}Hz", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    
    /// <summary>
    /// Creates a simple click sound
    /// </summary>
    public static AudioClip CreateClick(float duration = 0.05f, int sampleRate = 44100)
    {
        int sampleCount = Mathf.RoundToInt(duration * sampleRate);
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float envelope = Mathf.Exp(-time * 50f); // Exponential decay
            samples[i] = Random.Range(-1f, 1f) * envelope * 0.3f;
        }
        
        AudioClip clip = AudioClip.Create("Click", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    
    /// <summary>
    /// Creates a simple sweep sound
    /// </summary>
    public static AudioClip CreateSweep(float startFreq = 200f, float endFreq = 800f, float duration = 0.3f, int sampleRate = 44100)
    {
        int sampleCount = Mathf.RoundToInt(duration * sampleRate);
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float progress = time / duration;
            float frequency = Mathf.Lerp(startFreq, endFreq, progress);
            
            samples[i] = Mathf.Sin(2 * Mathf.PI * frequency * time) * 0.4f;
            
            // Apply envelope
            float envelope = Mathf.Sin(progress * Mathf.PI);
            samples[i] *= envelope;
        }
        
        AudioClip clip = AudioClip.Create($"Sweep_{startFreq}-{endFreq}Hz", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
    
    /// <summary>
    /// Creates a simple noise burst
    /// </summary>
    public static AudioClip CreateNoiseBurst(float duration = 0.2f, int sampleRate = 44100)
    {
        int sampleCount = Mathf.RoundToInt(duration * sampleRate);
        float[] samples = new float[sampleCount];
        
        for (int i = 0; i < sampleCount; i++)
        {
            float time = (float)i / sampleRate;
            float envelope = Mathf.Exp(-time * 10f); // Exponential decay
            samples[i] = Random.Range(-1f, 1f) * envelope * 0.3f;
        }
        
        AudioClip clip = AudioClip.Create("NoiseBurst", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}