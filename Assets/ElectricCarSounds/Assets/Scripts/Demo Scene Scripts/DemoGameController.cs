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
    public class DemoGameController : MonoBehaviour // this script is only made for the car controller demo scenes to change the sound packs on runtime, it is not required for the asset, will work without it
    {
        [Header("This script is only made for car controller DEMO scenes")]
        public SkrilStudio.ElectricCarSounds[] allChildren;
        public Toggle hasIdleSound;
        public Toggle hasWindSound;
        public Toggle hasStaticSound;
        public Image[] carSoundButtons;
        public Button[] nextPreviousButtons; // 0 = wind previous button, 1 = wind next button,
        public Text[] soundEffectsTexts; // 0 = wind noise selection text,
        int windID = 1; // set wind clip id
        int idleID = 1; // set idle clip id
        int staticID = 1; // set static sound clips id
        public AudioClip[] windClips;
        public AudioClip[] idleClips;
        public AudioClip[] staticClips;
        private void Start()
        {
            allChildren = GetComponentsInChildren<SkrilStudio.ElectricCarSounds>();
            // set value
            for (int i = 0; i < allChildren.Length; i++)
            {
                // set wind clip in all prefabs to the first clip
                allChildren[i].windClipID = windID;
                if (i % 2 == 0) // exterior
                    allChildren[i].windNoiseClip = windClips[(windID - 1)];
                if (i % 2 == 1) // interior
                    allChildren[i].windNoiseClip = windClips[(windID - 1 + 5)]; // + 5 are interior sound clips

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
			ChangeCarSound(0);
        }
        // change car sound buttons
        public void ChangeCarSound(int a)
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
            carSoundButtons[a/2].color = Color.green; // set the choosed button's color to green
            for (int i = 0; i<carSoundButtons.Length; i++)
            {
                if (i*2!= a) // set all other buttons color to white
                    carSoundButtons[i].color = Color.white;
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
            if (hasWindSound.isOn)
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
            if (!nextOrPrevious) // previous sound clip
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
                            allChildren[i].windNoiseClip = windClips[(windID - 1 + 5)]; // + 5 are interior sound clips
                    }
                    nextPreviousButtons[0].interactable = true; // enable button
                    nextPreviousButtons[1].interactable = true; // enable button
                }
                if (windID == 1)
                {
                    nextPreviousButtons[0].interactable = false; // disable button
                }
            }
            else // next sound clip
            {
                if (windID < 5)
                {
                    windID += 1;
                    for (int i = 0; i < allChildren.Length; i++)
                    {
                        allChildren[i].windClipID = windID;
                        if (i % 2 == 0) // exterior
                            allChildren[i].windNoiseClip = windClips[(windID - 1)];
                        if (i % 2 == 1) // interior
                            allChildren[i].windNoiseClip = windClips[(windID - 1 + 5)]; // + 5 are interior sound clips
                    }
                    nextPreviousButtons[0].interactable = true; // enable button
                    nextPreviousButtons[1].interactable = true; // enable button
                }
                if (windID == 5)
                {
                    nextPreviousButtons[1].interactable = false; // disable button
                }
            }
            // update text
            soundEffectsTexts[0].text = "Wind Noise: " + windID + "/5";
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
