using System;
using System.Collections.Generic;
using UnityEngine;

struct Particle
{
    public int name;
    public Vector3 position;
    public float density;
    public Vector3 force;
}

public class GPUWaterSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] densityPoints;

    [SerializeField] private ComputeShader compute;
    [SerializeField] private GameObject waterPreFab;
    [SerializeField] private int amountOfParticles = 200;
    [SerializeField] private float spawnGap = 0.1f;
    [SerializeField] private float waterParticleSize = 0.5f;
    [SerializeField] private float waterParticleMass = 1f;
    [SerializeField] private float smoothingRadius = 1f;
    [SerializeField] private float targetDensity = 5f;
    [SerializeField] private float pressureMultiplier = 1f;
    [SerializeField] private float downwardsForce = 0f;

    private const float startXPos = -10f;
    private const float startYPos = 5f;

    private List<Rigidbody2D> rbOfParticles = new List<Rigidbody2D>();

    private int densityKernel = 0;
    private int gradientKernel = 1;
    private Particle[] particleData;


    // Start is called before the first frame update
    void Start()
    {
        particleData = new Particle[amountOfParticles];

        float currentXPos = startXPos;
        float currentYPos = startYPos;
        for (int i = 0; i < amountOfParticles; i++)
        {
            GameObject temp = Instantiate(waterPreFab, new Vector3(currentXPos, currentYPos), Quaternion.identity);
            temp.name = i.ToString();
            temp.transform.localScale = new Vector3(waterParticleSize, waterParticleSize, waterParticleSize);
            currentXPos += spawnGap;
            if (currentXPos > 9.5f)
            {
                currentXPos = startXPos;
                currentYPos -= spawnGap;
            }
            if (currentYPos < -6.5)
            {
                break;
            }
            rbOfParticles.Add(temp.GetComponent<Rigidbody2D>());

            Particle p = new Particle();
            p.name = i;
            p.position = temp.transform.position;
            p.density = 0;
            p.force = Vector3.zero;
            particleData[i] = p;
        }

        //Debug.Log("Density Value: " + CalculateDensity(densityPoints[0].transform.position));
        /*
        int c = 1;
        foreach (GameObject point in densityPoints)
        {
            Debug.Log("Density Value " + c + ": " + CalculateDensity(point.transform.position));
            c++;
        }
        */
    }

    // Update is called once per frame
    void Update()
    {
        Physics2D.gravity = Vector2.down * downwardsForce;
        
        int j = 0;
        foreach (Rigidbody2D rb in rbOfParticles)
        {
            particleData[j].position = rb.position;
            j++;
        }
        
        int nameSize = sizeof(int);
        int vector3Size = sizeof(float) * 3;
        int densitySize = sizeof(float);
        int forceSize = sizeof(float) * 3;
        int totalSize = nameSize + vector3Size + densitySize + forceSize;

        ComputeBuffer particlesBuffer = new ComputeBuffer(particleData.Length, totalSize);
        particlesBuffer.SetData(particleData);
        compute.SetBuffer(densityKernel, "particles", particlesBuffer);
        compute.SetFloat("waterParticleMass", waterParticleMass);
        compute.SetFloat("smoothingRadius", smoothingRadius);
        compute.SetFloat("amountOfParticles", amountOfParticles);
        compute.Dispatch(densityKernel, amountOfParticles, 1, 1);
        particlesBuffer.GetData(particleData);

        ComputeBuffer forceBuffer = new ComputeBuffer(particleData.Length, totalSize);
        forceBuffer.SetData(particleData);
        compute.SetBuffer(gradientKernel, "particleForces", forceBuffer);
        compute.SetFloat("targetDensity", targetDensity);
        compute.SetFloat("pressureMultiplier", pressureMultiplier);
        compute.Dispatch(gradientKernel, amountOfParticles, 1, 1);
        forceBuffer.GetData(particleData);

        int i = 0;
        foreach (Rigidbody2D particle in rbOfParticles)
        {
            Vector2 force = particleData[i].force;
            particle.AddForce(force);
            //particle.velocity = force + Vector2.down * downwardsForce;
            //particleData[i].position = particle.position;
            i++;
        }
        particlesBuffer.Dispose();
        forceBuffer.Dispose();

    }

}
