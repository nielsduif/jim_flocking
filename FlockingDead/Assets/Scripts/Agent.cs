using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Agent : MonoBehaviour
{
    private static float sight = 150f;
    private static float space = 100f;
    private static float movementSpeed = 75f;
    private static float rotateSpeed = 3f;
    private static float distToBoundary = 100f;

    private BoxCollider2D boundary;

    public float dX;
    public float dY;

    public bool isZombie;
    public Vector2 position;
    public SpriteRenderer sprRenderer;

    private Sprite zombieSprite;

    private float separateWeight = 0, cohereWeight = .1f, alignWeight = 1;

    [SerializeField]
    static Slider sSight, sSpace, sSeparate, sCohere, sAlign;
    List<Text> sliderText = new List<Text>();

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

        sSight = GameObject.Find("sight").GetComponent<Slider>();
        sSpace = GameObject.Find("space").GetComponent<Slider>();
        sSeparate = GameObject.Find("separate").GetComponent<Slider>();
        sCohere = GameObject.Find("cohere").GetComponent<Slider>();
        sAlign = GameObject.Find("align").GetComponent<Slider>();

        sSight.onValueChanged.AddListener(ChangeSight);
        sSpace.onValueChanged.AddListener(ChangeSpace);
        sSeparate.onValueChanged.AddListener(ChangeSeparate);
        sCohere.onValueChanged.AddListener(ChangeCohere);
        sAlign.onValueChanged.AddListener(ChangeAlign);

        sliderText.Add(sSight.GetComponentInChildren<Text>());
        sliderText[sliderText.Count - 1].text = sSight.value.ToString();
        sliderText.Add(sSpace.GetComponentInChildren<Text>());
        sliderText[sliderText.Count - 1].text = sSpace.value.ToString();
        sliderText.Add(sSeparate.GetComponentInChildren<Text>());
        sliderText[sliderText.Count - 1].text = sSeparate.value.ToString();
        sliderText.Add(sCohere.GetComponentInChildren<Text>());
        sliderText[sliderText.Count - 1].text = sCohere.value.ToString();
        sliderText.Add(sAlign.GetComponentInChildren<Text>());
        sliderText[sliderText.Count - 1].text = sAlign.value.ToString();

    }

    void ChangeSight(float value)
    {
        sight = value;
        sliderText[0].text = sSight.value.ToString();
    }

    void ChangeSpace(float value)
    {
        space = value;
        sliderText[1].text = sSpace.value.ToString();
    }

    void ChangeSeparate(float value)
    {
        separateWeight = value;
        sliderText[2].text = sSeparate.value.ToString();
    }

    void ChangeCohere(float value)
    {
        cohereWeight = value;
        sliderText[3].text = sCohere.value.ToString();
    }

    void ChangeAlign(float value)
    {
        alignWeight = value;
        sliderText[4].text = sAlign.value.ToString();
    }

    public void Move(List<Agent> agents)
    {
        //Agents flock, zombie's hunt 
        if (!isZombie) Flock(agents, separateWeight, cohereWeight, alignWeight);
        else Hunt(agents);
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

    private void Flock(List<Agent> agents, float _separateWeight, float _cohereWeight, float _alignWeight)
    {
        foreach (Agent a in agents)
        {
            float distance = Distance(position, a.position);
            if (a != this && !a.isZombie)
            {
                if (distance < space)
                {
                    // Separation
                    dX += (position.x - a.position.x) * _separateWeight;
                    dY += (position.y - a.position.y) * _separateWeight;
                }
                else if (distance < sight)
                {
                    // Cohesion
                    dX += (a.position.x - position.x) * _cohereWeight;
                    dY += (a.position.y - position.y) * _cohereWeight;
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
        else s = movementSpeed / 3f * Time.deltaTime; //Zombies are slower

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
