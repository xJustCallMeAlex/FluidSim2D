using System.Collections;
using System.Collections.Generic;
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

    private const float startXPos = -10f;
    private const float startYPos = 5f;

    private float currentXPos;
    private float currentYPos;

    private List<GameObject> waterList = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        Physics2D.gravity = Vector2.zero;

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
        }

        int c = 1;
        foreach (GameObject point in densityPoints)
        {
            Debug.Log("Density Value " + c + ": " + CalculateDensity(point.transform.position));
            c++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
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

}
