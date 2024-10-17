using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public enum ControlMode
    {
        Keyboard,
        Buttons
    };

    public enum Axel
    {
        Front,
        Rear
    }

    public enum Player
    {
        Red,
        Blue
    }

    [Serializable]
    public struct Wheel
    {
        public GameObject wheelModel;
        public WheelCollider wheelCollider;
        public GameObject wheelEffectObj;
        public Axel axel;
    }

    public ControlMode control;

    public float maxAcceleration = 30.0f;
    public float brakeAcceleration = 50.0f;

    public float turnSensitivity = 1.0f;
    public float maxSteerAngle = 30.0f;

    public Vector3 _centerOfMass;

    public List<Wheel> wheels;

    float moveInput;
    float steerInput;

    [SerializeField] private GameObject[] healthbar;
    int healthbarCount;

    private Rigidbody carRb;

    public int CheckPointCount = 0;

    public Player _player;
    void Start()
    {
        carRb = GetComponent<Rigidbody>();
        carRb.centerOfMass = _centerOfMass;
        healthbarCount= healthbar.Length-1;
    }

    void Update()
    {
        GetInputs();
        AnimateWheels();
        WheelEffects();
    }

    void LateUpdate()
    {
        Move();
        Steer();
        Brake();
    }

    public void MoveInput(float input)
    {
        moveInput = input;
    }

    public void SteerInput(float input)
    {
        steerInput = input;
    }

    void GetInputs()
    {
        if (control == ControlMode.Keyboard)
        {
            moveInput = Input.GetAxis("Vertical");
            steerInput = Input.GetAxis("Horizontal");
        }
        else if(control == ControlMode.Buttons)
        {
            moveInput = Input.GetAxis("Vertical1");
            steerInput = Input.GetAxis("Horizontal1");
        }
        
    }

    void Move()
    {
        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.motorTorque = moveInput * 600 * maxAcceleration * Time.deltaTime;
        }
    }

    void Steer()
    {
        foreach (var wheel in wheels)
        {
            if (wheel.axel == Axel.Front)
            {
                var _steerAngle = steerInput * turnSensitivity * maxSteerAngle;
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, _steerAngle, 0.6f);
            }
        }
    }

    void Brake()
    {
        if (Input.GetKey(KeyCode.Space) || moveInput == 0)
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 300 * brakeAcceleration * Time.deltaTime;
            }
        }
        else
        {
            foreach (var wheel in wheels)
            {
                wheel.wheelCollider.brakeTorque = 0;
            }
        }
    }

    void AnimateWheels()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rot;
            Vector3 pos;
            wheel.wheelCollider.GetWorldPose(out pos, out rot);
            wheel.wheelModel.transform.position = pos;
            wheel.wheelModel.transform.rotation = rot;
        }
    }

    void WheelEffects()
    {
        foreach (var wheel in wheels)
        {
            if (Input.GetKey(KeyCode.Space) && wheel.axel == Axel.Rear && wheel.wheelCollider.isGrounded == true && carRb.velocity.magnitude >= 10.0f)
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = true;
            }
            else
            {
                wheel.wheelEffectObj.GetComponentInChildren<TrailRenderer>().emitting = false;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (Player.Red == _player)
        {
            Debug.Log(collision.gameObject.name);

            if (collision.gameObject.CompareTag("Red") || collision.gameObject.CompareTag("Blue"))
            {
                Rigidbody otherplayer = collision.gameObject.GetComponent<Rigidbody>();
                if (otherplayer.velocity.magnitude > carRb.velocity.magnitude)
                {
                    GameManager.Instance.ReduceHp(true,this);
                }
                else
                {
                    GameManager.Instance.ReduceHp(false,collision.gameObject.GetComponent<CarController>());
                }
            }
        }
    }
    bool ch2 = true;
    bool ch1 = true;

    private void OnTriggerEnter(Collider other)
    {
        if ((other.gameObject.CompareTag("checkpoint1")) && ch1)
        {
            CheckPointCount++;
            ch1 = false;
        }
        if ((other.gameObject.CompareTag("checkpoint2")) && ch2)
        {
            CheckPointCount++;
            ch2 = false;
        }
    }

    public void HealthManager()
    {
        if(healthbar.Length != 0)
        {
            healthbar[healthbarCount].SetActive(false);
            healthbarCount--;
        }
    }
}
