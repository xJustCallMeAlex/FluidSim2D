using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class WaterSpawnScript : MonoBehaviour
{
    [SerializeField] private GameObject[] densityPoints;

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

    private List<GameObject> waterList = new List<GameObject>();
    private List<Rigidbody2D> rbOfParticles = new List<Rigidbody2D>();
    private List<float> densityList = new List<float>();


    // Start is called before the first frame update
    void Start()
    {
        densityList = new List<float>(new float[amountOfParticles]);

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
            waterList.Add(temp);
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
        foreach (GameObject waterParticle in waterList)
        {
            densityList[Int32.Parse(waterParticle.gameObject.name)] = CalculateDensity(waterParticle.transform.position);
        }
        
        foreach(Rigidbody2D particle in rbOfParticles)
        {
            Vector2 force = CalculateDensityGradient(particle.position);
            particle.AddForce(force);
            //particle.velocity = force;
        }
        
    }

    float CalculateDensity(Vector3 point)
    {
        float density = 0;
        Collider2D[] foundParticles = Physics2D.OverlapCircleAll(point, smoothingRadius);
        foreach (Collider2D foundParticle in foundParticles)
        {
            if (foundParticle.gameObject.tag == "Water")
            {
                float distance = (foundParticle.transform.position - point).magnitude;
                float influence = CalculateInfluence(distance);
                density += waterParticleMass * influence;
            }
        }
        return density;
    }

    float CalculateInfluence(float distance)
    {
        float volume = Mathf.PI * Mathf.Pow(smoothingRadius, 5) / 10;
        float value = Mathf.Max(0.0f, smoothingRadius - distance);
        return value * value * value / volume;
    }

    float CalculateInfluenceDerivative(float distance)
    {
        float volume = -30 / (Mathf.PI * Mathf.Pow(smoothingRadius, 5));
        float value = smoothingRadius - distance;
        return value * value * volume;
    }

    Vector3 CalculateDensityGradient(Vector3 point)
    {
        Vector3 gradient = Vector3.zero;
        Collider2D[] foundParticles = Physics2D.OverlapCircleAll(point, smoothingRadius);
        foreach (Collider2D foundParticle in foundParticles)
        {
            if (foundParticle.gameObject.tag == "Water")
            {
                Vector3 difference = foundParticle.transform.position - point;
                float distance = difference.magnitude;
                Vector3 direction = distance == 0 ? Vector3.zero : difference / distance;
                float slope = CalculateInfluenceDerivative(distance);
                float density = densityList[Int32.Parse(foundParticle.gameObject.name)];
                gradient += ConvertDensityToPressure(density) * slope * waterParticleMass * direction / density;
            }
        }
        return gradient;
    }

    float ConvertDensityToPressure(float density)
    {
        float densityError = density - targetDensity;
        float pressure = densityError * pressureMultiplier;
        return pressure;
    }
}
