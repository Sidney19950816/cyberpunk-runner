//______________________________________________//
//_____________Electric Car Sounds______________//
//______________________________________________//
//_______Copyright © 2021 Skril Studio__________//
//______________________________________________//
//__________ http://skrilstudio.com/ ___________//
//______________________________________________//
//________ http://fb.com/yugelmobile/ __________//
//______________________________________________//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SkrilStudio.ECS
{
    public class DemoController : MonoBehaviour // "DemoController" script is only used for demonstration purposes and for testing, it is not deisgned for live products, only just to show off Electric Car Sounds features in a demo scene. This scipt is designed to work only in "Slider" demo scenes, it will not work in other scenes. For other scenes import the right *.unitypackage for your car controller or contact me in email for adding support for your custom car controller.
    {
        [Header("This script is only made for main DEMO scene")]
        public SkrilStudio.ElectricCarSounds[] allChildren;
        public GameObject gasPedalButton; // UI button
        public Slider currentSpeedSlider; // UI slider to set speed
        public Slider pitchSlider; // UI sliter to set maximum pitch
        public Text pitchText; // UI text to show pitch multiplier value
        public Text currentSpeedText; // UI text to show current speed
        public Toggle gasPedalPressing;
        public Toggle hasIdleSound;
        public Toggle hasWindSound;
        public Toggle hasStaticSound;
        public Image[] carSoundButtons;
        public Dropdown exteriorOrInterior;
        public GameObject accelerationSpeed; // UI slider for acceleration speed
        public Button[] nextPreviousButtons; // 0 = wind previous button, 1 = wind next button,
        public Text[] soundEffectsTexts; // 0 = wind noise selection text,
        public bool simulated = true; // is speed simulated with gaspedal button or with current speed slider by hand
        int windID = 1; // set wind clip id
        int idleID = 1; // set idle clip id
        int staticID = 1; // set static sound clips id
        public AudioClip[] windClips;
        public AudioClip[] idleClips;
        public AudioClip[] staticClips;
        SkrilStudio.ECS.CarSimulatorSample carSimulator;
        private void Start()
        {
            allChildren = GetComponentsInChildren<SkrilStudio.ElectricCarSounds>();
            carSimulator = gasPedalButton.GetComponent<SkrilStudio.ECS.CarSimulatorSample>();
            ChangeCarSound(0);
            // turn off all interior prefabs
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (i % 2 == 0)
                    allChildren[i + 1].gameObject.SetActive(false);
            }
            // set value
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<SkrilStudio.ElectricCarSounds>() != null)
                    allChildren[i].GetComponent<SkrilStudio.ElectricCarSounds>().maxTreshold = 400;

                // set wind clip in all prefabs to the first clip
                allChildren[i].windClipID = windID;
                if (i % 2 == 0) // exterior
                    allChildren[i].windNoiseClip = windClips[(windID - 1)];
                if (i % 2 == 1) // interior
                    allChildren[i].windNoiseClip = windClips[(windID - 1 + 6)]; // + 6 are interior sound clips

                //set idle clip in all prefabs to the first clip
                allChildren[i].idleClipID = idleID;
                if (i % 2 == 0) // exterior
                    allChildren[i].idleClip = idleClips[(idleID - 1)];
                if (i % 2 == 1) // interior
                    allChildren[i].idleClip = idleClips[(idleID - 1 + 5)]; // + 5 are interior sound clips

                // set static clips in all prefabs to the first clip
                allChildren[i].staticClipID = staticID;
                if (i % 2 == 0) // exterior
                {
                    allChildren[i].engineOnStaticClip = staticClips[((staticID * 2) - 2)]; // on load
                    allChildren[i].engineOffStaticClip = staticClips[((staticID * 2) - 1)]; // off load
                }
                if (i % 2 == 1) // interior
                {
                    allChildren[i].engineOnStaticClip = staticClips[((staticID * 2) - 2 + 10)]; // on load // + 10 are interior sound clips
                    allChildren[i].engineOffStaticClip = staticClips[((staticID * 2) - 1 + 10)]; // off load // + 10 are interior sound clips
                }
            }
        }
        private void Update()
        {
            currentSpeedText.text = "Vehicle Speed is at: " + (int)currentSpeedSlider.value / 4 + "%"; // show current speed - this generate GC
            pitchText.text = "" + pitchSlider.value; // set pitch multiplier value for ui text
            // speed values
            for (int i = 0; i < allChildren.Length; i++)
            {
                allChildren[i].pitchMultiplier = pitchSlider.value;
                if (simulated)
                {
                    allChildren[i].currentSpeed = carSimulator.speed;
                    currentSpeedSlider.value = carSimulator.speed; // set ui sliders value to rpm
                }
                else
                {
                    allChildren[i].currentSpeed = currentSpeedSlider.value;
                    carSimulator.speed = currentSpeedSlider.value;
                }
            }
            if (simulated) // gas pedal pressing toggle
                if (gasPedalPressing != null)
                    gasPedalPressing.isOn = carSimulator.gasPedalPressing;
        }
        // is simulated speed
        public void IsSimulated(Dropdown drpDown)
        {
            if (drpDown.value == 0)
            {
                simulated = true;
                accelerationSpeed.SetActive(true);
                gasPedalButton.SetActive(true);
            }
            if (drpDown.value == 1)
            {
                simulated = false;
                accelerationSpeed.SetActive(false);
                gasPedalButton.SetActive(false);
            }
        }
        // change car sound buttons
        public void ChangeCarSound(int a) // a = exterior, a+1 = interior prefabs id numbers in allChildren[]
        {
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<SkrilStudio.ElectricCarSounds>() != null)
                {
                    if (i != a && i != a + 1)
                        allChildren[i].GetComponent<SkrilStudio.ElectricCarSounds>().enabled = false;
                }
                if (allChildren[a].GetComponent<SkrilStudio.ElectricCarSounds>() != null)
                    allChildren[a].GetComponent<SkrilStudio.ElectricCarSounds>().enabled = true;
                if (allChildren[a + 1].GetComponent<SkrilStudio.ElectricCarSounds>() != null)
                    allChildren[a + 1].GetComponent<SkrilStudio.ElectricCarSounds>().enabled = true;
            }
            carSoundButtons[a / 2].color = Color.green; // set the choosed button's color to green
            for (int i = 0; i < carSoundButtons.Length; i++)
            {
                if (i * 2 != a) // set all other buttons color to white
                    carSoundButtons[i].color = Color.white;
            }
        }
        // gas pedal checkbox
        public void UpdateGasPedal(Toggle togl)
        {
            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].GetComponent<SkrilStudio.ElectricCarSounds>() != null)
                    allChildren[i].GetComponent<SkrilStudio.ElectricCarSounds>().gasPedalPressing = togl.isOn;
            }
        }
        public void ExteriorOrInterior()
        {
            if(exteriorOrInterior.value == 0) // exterior
            {
                for (int i = 0; i < allChildren.Length; i++)
                    if (i % 2 == 0) // is even
                        allChildren[i].gameObject.SetActive(true); // enable exterior prefabs
                for (int i = 0; i < allChildren.Length; i++)
                    if (i % 2 == 1) // is odd
                        allChildren[i].gameObject.SetActive(false); // disable interior prefabs
            }
            else // interior
            {
                for (int i = 0; i < allChildren.Length; i++)
                    if (i % 2 == 0) // is even
                        allChildren[i].gameObject.SetActive(false); // disable exterior prefabs
                for (int i = 0; i < allChildren.Length; i++)
                    if (i % 2 == 1) // is odd
                        allChildren[i].gameObject.SetActive(true); // enable interior prefabs
            }
        }
        public void IdleSound() // enable/disable idle sound
        {
            for (int i = 0; i < allChildren.Length; i++)
                allChildren[i].hasIdleSound = hasIdleSound.isOn;
            nextPreviousButtons[2].gameObject.SetActive(hasIdleSound.isOn);
            nextPreviousButtons[3].gameObject.SetActive(hasIdleSound.isOn);
            if (hasIdleSound.isOn)
                // update text
                soundEffectsTexts[1].text = "Idle Sound: " + idleID + "/5";
            else
                soundEffectsTexts[1].text = "Idle Sound";
        }
        public void WindSound() // enable/disable rolling sound
        {
            for (int i = 0; i < allChildren.Length; i++)
                allChildren[i].windNoiseEnabled = hasWindSound.isOn;
            nextPreviousButtons[0].gameObject.SetActive(hasWindSound.isOn);
            nextPreviousButtons[1].gameObject.SetActive(hasWindSound.isOn);
            if(hasWindSound.isOn)
                // update text
                soundEffectsTexts[0].text = "Wind Noise: " + windID + "/5";
            else
                soundEffectsTexts[0].text = "Wind Noise";
        }
        public void StaticSound() // enable/disable rolling sound
        {
            for (int i = 0; i < allChildren.Length; i++)
                allChildren[i].hasStaticSound = hasStaticSound.isOn;
            nextPreviousButtons[4].gameObject.SetActive(hasStaticSound.isOn);
            nextPreviousButtons[5].gameObject.SetActive(hasStaticSound.isOn);
            if (hasStaticSound.isOn)
                // update text
                soundEffectsTexts[2].text = "Static Sound: " + staticID + "/5";
            else
                soundEffectsTexts[2].text = "Static Sound";
        }
        public void ChangeWindClip(bool nextOrPrevious) // true = next, false = previous
        {
            if(!nextOrPrevious) // previous sound clip
            {
                if (windID > 1)
                {
                    windID -= 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].windClipID = windID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].windNoiseClip = windClips[(windID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].windNoiseClip = windClips[(windID - 1 + 6)]; // + 6 are interior sound clips
                    }
                    nextPreviousButtons[0].interactable = true; // enable button
                    nextPreviousButtons[1].interactable = true; // enable button
                }
                if(windID == 1)
                {
                    nextPreviousButtons[0].interactable = false; // disable button
                }               
            }
            else // next sound clip
            {
                if (windID < 6)
                {
                    windID += 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].windClipID = windID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].windNoiseClip = windClips[(windID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].windNoiseClip = windClips[(windID - 1 + 6)]; // + 6 are interior sound clips
                    }
                    nextPreviousButtons[0].interactable = true; // enable button
                    nextPreviousButtons[1].interactable = true; // enable button
                }
                if(windID == 6)
                {
                    nextPreviousButtons[1].interactable = false; // disable button
                }
            }
            // update text
            soundEffectsTexts[0].text = "Wind Noise: "+windID + "/6";
        }
        public void ChangeIdleClip(bool nextOrPrevious) // true = next, false = previous
        {
            if (!nextOrPrevious) // previous sound clip
            {
                if (idleID > 1)
                {
                    idleID -= 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].idleClipID = idleID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].idleClip = idleClips[(idleID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].idleClip = idleClips[(idleID - 1 + 5)]; // + 5 are interior sound clips
                    }
                    nextPreviousButtons[2].interactable = true; // enable button
                    nextPreviousButtons[3].interactable = true; // enable button
                }
                if (idleID == 1)
                {
                    nextPreviousButtons[2].interactable = false; // disable button
                }
            }
            else // next sound clip
            {
                if (idleID < 5)
                {
                    idleID += 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].idleClipID = idleID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].idleClip = idleClips[(idleID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].idleClip = idleClips[(idleID - 1 + 5)]; // + 5 are interior sound clips
                    }
                    nextPreviousButtons[2].interactable = true; // enable button
                    nextPreviousButtons[3].interactable = true; // enable button
                }
                if (idleID == 5)
                {
                    nextPreviousButtons[3].interactable = false; // disable button
                }
            }
            // update text
            soundEffectsTexts[1].text = "Idle Sound: " + idleID + "/5";
        }
        public void ChangeStaticClip(bool nextOrPrevious) // true = next, false = previous
        {
            if (!nextOrPrevious) // previous sound clip
            {
                if (staticID > 1)
                {
                    staticID -= 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].staticClipID = staticID;
                        if (i % 2 == 0) // exterior
                        {
                            allChildren[i].engineOnStaticClip = staticClips[((staticID * 2) - 2)]; // on load
                            allChildren[i].engineOffStaticClip = staticClips[((staticID * 2) - 1)]; // off load
                        }
                        if (i % 2 == 1) // interior
                        {
                            allChildren[i].engineOnStaticClip = staticClips[((staticID * 2) - 2 + 10)]; // on load // + 10 are interior sound clips
                            allChildren[i].engineOffStaticClip = staticClips[((staticID * 2) - 1 + 10)]; // off load // + 10 are interior sound clips
                        }
                    }
                    nextPreviousButtons[4].interactable = true; // enable button
                    nextPreviousButtons[5].interactable = true; // enable button
                }
                if (staticID == 1)
                {
                    nextPreviousButtons[4].interactable = false; // disable button
                }
            }
            else // next sound clip
            {
                if (staticID < 5)
                {
                    staticID += 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].staticClipID = staticID;
                        if (i % 2 == 0) // exterior
                        {
                            allChildren[i].engineOnStaticClip = staticClips[((staticID * 2) - 2)]; // on load
                            allChildren[i].engineOffStaticClip = staticClips[((staticID * 2) - 1)]; // off load
                        }
                        if (i % 2 == 1) // interior
                        {
                            allChildren[i].engineOnStaticClip = staticClips[((staticID * 2) - 2 + 10)]; // on load // + 5 are interior sound clips
                            allChildren[i].engineOffStaticClip = staticClips[((staticID * 2) - 1 + 10)]; // off load // + 5 are interior sound clips
                        }
                    }
                    nextPreviousButtons[4].interactable = true; // enable button
                    nextPreviousButtons[5].interactable = true; // enable button
                }
                if (staticID == 5)
                {
                    nextPreviousButtons[5].interactable = false; // disable button
                }
            }
            // update text
            soundEffectsTexts[2].text = "Static Sound: " + staticID + "/5";
        }
    }
}
