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
    // this script was only made for slider demo scene for demonstration purposes only, you can delete this demo script if you not using it
    public class CarSimulatorSample: MonoBehaviour
    {
        public bool gasPedalPressing = false;
        public float speed = 0;
        public float accelerationSpeed = 1000f;
        public float decelerationSpeed = 1200f;
        public Slider accelSlider;

        void Update()
        {
            if (gasPedalPressing)
            {
                if (speed <= 400)
                    speed = Mathf.Lerp(speed, speed + accelerationSpeed * accelSlider.value, Time.deltaTime);
            }
            else
            {
                if (speed >= 0)
                    speed = Mathf.Lerp(speed, speed - decelerationSpeed * accelSlider.value, Time.deltaTime);
            }
        }
        public void onPointerDownRaceButton()
        {
            gasPedalPressing = true;
        }
        public void onPointerUpRaceButton()
        {
            gasPedalPressing = false;
        }
    }
}
