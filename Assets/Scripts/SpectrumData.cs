using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

/*
    Reads the spectrum data from an audio listener.
    Code partially borrowed from SimpleSpectrum asset.
 */

public class SpectrumData : MonoBehaviour {
    public int sampleNumber = 512; // number of samples used for sampling. must be power of 2.
    public float attackDamp = 0.25f;
    public float decayDamp = 0.25f;

    // audio math variables
    public bool useLogarithmicFrequency = true; // if audio data is scaled logarithmically.
    public bool multiplyByFrequency = true; // if the values of the spectrum are multiplied based on their frequency, to keep the values proportionate.
    public float highFrequencyTrim = 1; // Determines what percentage of the full frequency range to use (1 being the full range, reducing the value towards 0 cuts off high frequencies).
    public float linearSampleStretch = 1; // When useLogarithmicFrequency is false, this value stretches the spectrum data onto the bars.

    private float[] rawSpectrum;


    void Start() {
        sampleNumber = Mathf.ClosestPowerOfTwo(sampleNumber);
        rawSpectrum = new float[sampleNumber];
    }


    void Update() {
        AudioListener.GetSpectrumData(rawSpectrum, 0, FFTWindow.BlackmanHarris);
    }


    // fits the spectrum into the given output array
    public void GetOutputSpectrum(float[] output) {
        //gets the highest possible logged frequency, used to calculate which sample of the spectrum to use for a bar
        float highestLogFreq = Mathf.Log(output.Length + 1, 2); 
        float logFreqMultiplier = sampleNumber / highestLogFreq;

        for (int i = 0; i < output.Length; i++) {
            float value;
            float trueSampleIndex;

            //GET SAMPLES
            if (useLogarithmicFrequency) {
                //LOGARITHMIC FREQUENCY SAMPLING
                trueSampleIndex = highFrequencyTrim * (highestLogFreq - Mathf.Log(output.Length + 1 - i, 2)) * logFreqMultiplier; //gets the index equiv of the logified frequency

                //^that really needs explaining.
                //'logarithmic frequencies' just means we want more of the lower frequencies and less of the high ones.
                //a normal log2 graph will quickly go past 1-5 and spend much more time on stuff above that, but we want the opposite
                //so by doing log2(max(i)) - log2(max(i) - i), we get a flipped log graph
                //(make a graph of log2(64)-log2(64-x) to see what I mean)
                //this isn't finished though, because that graph doesn't actually map the bar index (x) to the spectrum index (y).
                //logFreqMultiplier stretches the grpah upwards so that the highest value (log2(max(i)))hits the highest frequency.
                //also 1 gets added to barAmount pretty much everywhere, because without it, the log hits (barAmount-1,max(freq))

            }
            else {
                //LINEAR (SCALED) FREQUENCY SAMPLING 
                trueSampleIndex = i * linearSampleStretch;
            }

            //the true sample is usually a decimal, so we need to lerp between the floor and ceiling of it.

            int sampleIndexFloor = Mathf.FloorToInt(trueSampleIndex);
            sampleIndexFloor = Mathf.Clamp(sampleIndexFloor, 0, rawSpectrum.Length - 2); //just keeping it within the spectrum array's range

            float sampleIndexDecimal = trueSampleIndex % 1; //gets the decimal point of the true sample, for lerping

            value = Mathf.SmoothStep(rawSpectrum[sampleIndexFloor], rawSpectrum[sampleIndexFloor + 1], sampleIndexDecimal); //smoothly interpolate between the two samples using the true index's decimal.

            //MANIPULATE & APPLY SAMPLES
            if (multiplyByFrequency) {
                //multiplies the amplitude by the true sample index
                value = value * (trueSampleIndex + 1);
            }

            value = Mathf.Sqrt(value); //compress the amplitude values by sqrt(x)

            // DAMPENING
            float oldOutput = output[i];
            float newOutput;
            if (value > oldOutput) {
                newOutput = Mathf.Lerp(oldOutput, value, attackDamp);
            }
            else {
                newOutput = Mathf.Lerp(oldOutput, value, decayDamp);
            }

            output[i] = newOutput;
        }
    }

}
