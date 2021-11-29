﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{
    private static float sight = 150;
    private static float space = 100f;
    private static float movementSpeed = 200;
    private static float rotateSpeed = 3f;
    private static float distToBoundary = 100f;

    private BoxCollider2D boundary;

    public float dX;
    public float dY;

    public bool isZombie;
    public Vector2 position;
    public SpriteRenderer sprRenderer;

    private Sprite zombieSprite;

    [SerializeField]
    static float separateWeight = 1, cohereWeight = 1, alignWeight = 1;

    public void Initialize(bool zombie, Sprite zombieSprite, Sprite regularSprite, BoxCollider2D boundary)
    {
        position = new Vector2(Random.Range(boundary.bounds.min.x + distToBoundary, boundary.bounds.max.x - distToBoundary), Random.Range(boundary.bounds.min.y + distToBoundary, boundary.bounds.max.y - distToBoundary));
        transform.position = position;

        this.boundary = boundary;

        isZombie = zombie;

        sprRenderer = GetComponent<SpriteRenderer>();

        this.zombieSprite = zombieSprite;

        if (isZombie)
            sprRenderer.sprite = zombieSprite;
        else
            sprRenderer.sprite = regularSprite;
    }

    public void ChangeSight(float value)
    {
        sight = value;
    }

    public void ChangeSpace(float value)
    {
        space = value;
    }

    public void ChangeSeparate(float value)
    {
        separateWeight = value;
    }

    public void ChangeCohere(float value)
    {
        cohereWeight = value;
    }

    public void ChangeAlign(float value)
    {
        alignWeight = value;
    }

    public void Move(List<Agent> agents)
    {
        //Agents flock, zombie's hunt 
        if (!isZombie) Hunt(agents);
        else Flock(agents);
        CheckBounds();
        CheckSpeed();

        position.x += dX;
        position.y += dY;

        Vector2 direction = (Vector3)position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotateSpeed * Time.deltaTime);

        transform.position = position;
    }

    private void Flock(List<Agent> agents)
    {
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        foreach (Agent a in agents)
        {
            float distance = Distance(position, a.position);
            float distanceMouse = Distance(position, worldPosition);
            if (a != this && !a.isZombie)
            {
                if (distance < space)
                {
                    // Separation
                    dX += (position.x - a.position.x) * separateWeight;
                    dY += (position.y - a.position.y) * separateWeight;
                }
                else if (distance < sight)
                {
                    // Cohesion
                    dX += (a.position.x - position.x) * cohereWeight;
                    dY += (a.position.y - position.y) * cohereWeight;
                }
                if (distance < sight)
                {
                    // Alignment
                    dX += a.dX * alignWeight;
                    dY += a.dY * alignWeight;
                }
            }
            if (a.isZombie && distance < sight)
            {
                // Evade
                dX += (position.x - a.position.x);
                dY += (position.y - a.position.y);
            }
            if (distanceMouse < sight)
            {
                dX += (position.x - worldPosition.x);
                dY += (position.y - worldPosition.y);
            }
        }
    }

    private void Hunt(List<Agent> agents)
    {
        float range = float.MaxValue;
        Agent prey = null;
        foreach (Agent a in agents)
        {
            if (!a.isZombie)
            {
                float distance = Distance(position, a.position);
                if (distance < sight && distance < range)
                {
                    range = distance;
                    prey = a;
                }
            }
        }
        if (prey != null)
        {
            // Move towards prey.
            dX += (prey.position.x - position.x);
            dY += (prey.position.y - position.y);
        }
    }

    private static float Distance(Vector2 p1, Vector2 p2)
    {
        float val = Mathf.Pow(p1.x - p2.x, 2) + Mathf.Pow(p1.y - p2.y, 2);
        return Mathf.Sqrt(val);
    }

    private void CheckBounds()
    {
        if (position.x < boundary.bounds.min.x + distToBoundary)
            dX += boundary.bounds.min.x + distToBoundary - position.x;
        if (position.y < boundary.bounds.min.y + distToBoundary)
            dY += boundary.bounds.min.y + distToBoundary - position.y;

        if (position.x > boundary.bounds.max.x - distToBoundary)
            dX += boundary.bounds.max.x - distToBoundary - position.x;
        if (position.y > boundary.bounds.max.y - distToBoundary)
            dY += boundary.bounds.max.y - distToBoundary - position.y;
    }

    private void CheckSpeed()
    {
        float s;
        if (!isZombie) s = movementSpeed * Time.deltaTime;
        else s = movementSpeed / 2 * Time.deltaTime; //Zombies are slower

        float val = Distance(Vector2.zero, new Vector2(dX, dY));
        if (val > s)
        {
            dX = dX * s / val;
            dY = dY * s / val;
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        Agent otherAgent = other.gameObject.GetComponent<Agent>();

        if (otherAgent != null)
        {
            // if im not a zombie and the other is, become a zombie
            if (otherAgent.isZombie && !this.isZombie)
                BecomeZombie();
        }
    }

    private void BecomeZombie()
    {
        isZombie = true;
        sprRenderer.sprite = zombieSprite;
    }
}
