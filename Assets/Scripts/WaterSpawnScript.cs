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
    [SerializeField] private float stepSize = 0.001f;

    private const float startXPos = -10f;
    private const float startYPos = 5f;

    private float currentXPos;
    private float currentYPos;

    private List<GameObject> waterList = new List<GameObject>();
    private List<Rigidbody2D> rbOfParticles = new List<Rigidbody2D>();

    private Vector3 gizmoPoint;

    // Start is called before the first frame update
    void Start()
    {
        Physics2D.gravity = Vector2.zero;
        //Physics2D.gravity = Vector2.down * 0.5f;

        currentXPos = startXPos;
        currentYPos = startYPos;
        for (int i = 0; i < amountOfParticles; i++)
        {
            GameObject temp = Instantiate(waterPreFab, new Vector3(currentXPos, currentYPos), Quaternion.identity);
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
        //Vector2 force = CalculateDensityGradient(rbOfParticles[0].position);
        //rbOfParticles[0].AddForce(force);
        
        foreach(Rigidbody2D particle in rbOfParticles)
        {
            Vector2 force = CalculateDensityGradient(particle.position);
            particle.AddForce(force);
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

    Vector2 CalculateDensityGradient(Vector2 point)
    {
        float deltaX = CalculateDensity(point + Vector2.right * stepSize) - CalculateDensity(point + Vector2.left * stepSize);
        float deltaY = CalculateDensity(point + Vector2.up * stepSize) - CalculateDensity(point + Vector2.down * stepSize);
        Vector2 gradient = new Vector2(deltaX, deltaY) / stepSize;
        return -gradient;
    }

    private void OnDrawGizmos()
    {
        //Gizmos.color = UnityEngine.Color.yellow;
        //Gizmos.DrawSphere(densityPoints[0].transform.position, smoothingRadius);
    }

}
