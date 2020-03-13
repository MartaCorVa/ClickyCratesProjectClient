using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    // declare a new private Rigidbody targetRb; and initialize it in Start()
    private Rigidbody targetRb;
    // Declare and initialize new private float variables for minSpeed, maxSpeed, maxTorque, xRange, and ySpawnPos;
    private float minSpeed = 12;
    private float maxSpeed = 16;
    private float maxTorque = 10;
    private float xRange = 4;
    private float ySpawnPos = 1;
    // Game Manager
    private GameManager gameManager;
    // Value of the points. 
    public int pointValue;
    // Explosion for the targets
    public ParticleSystem explosionParticle;

    void Start()
    {
        // initialize
        targetRb = gameObject.GetComponent<Rigidbody>();
        //In Start(), add an upward force multiplied by a randomized speed
        targetRb.AddForce(RandomForce(), ForceMode.Impulse);
        //Add a torque with randomized xyz values
        targetRb.AddTorque(RandomTorque(), RandomTorque(), RandomTorque(), ForceMode.Impulse);
        //Set the position with a randomized X value
        transform.position = RandomSpawnPos();
        //Initialize GameManager in Start()  using the Find() method
        gameManager = GameObject.Find("Game Manager").GetComponent<GameManager>();
    }

    void Update()
    {
        
    }


// Create a new function for Vector3 RandomForce() and call it in Start()
    Vector3 RandomForce () { return Vector3.up * Random.Range(minSpeed, maxSpeed); }

// Create a new function for float RandomTorque(), and call it in Start()
    float RandomTorque() { return Random.Range(-maxTorque, maxTorque); }

// Create a new function for RandomSpawnPos(), have it return a new Vector3 and call it in Start()
    Vector3 RandomSpawnPos() { return new Vector3(Random.Range(-xRange, xRange), ySpawnPos); }

    // In Target.cs, add a new method for private void OnMouseDown() { } , and inside that method, destroy the gameObject
    private void OnMouseDown()
    {
        if(gameManager.isGameActive)
        {
            Instantiate(explosionParticle, transform.position, explosionParticle.transform.rotation);
            gameManager.UpdateScore(pointValue);
            Destroy(gameObject);
        }
    }

    // Add a new method for private void OnTriggerEnter(Collider other) and inside that function, destroy the gameObject
    private void OnTriggerEnter(Collider other)
    {        
        Destroy(gameObject);
        if (!gameObject.CompareTag("Bad"))
        {
            gameManager.GameOver();
        }
    }
    
}