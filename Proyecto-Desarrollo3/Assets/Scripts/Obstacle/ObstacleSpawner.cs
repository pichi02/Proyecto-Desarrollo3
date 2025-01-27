
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class ObstacleSpawner : MonoBehaviour
{

    [SerializeField] private List<GameObject> obstacles;
    [SerializeField] private CardsDeck deck;

    [SerializeField] private CarController player1;

    public event Action<float> OnSetCooldown;
    public event Action OnCantSpawnObstacle;
    public event Action OnBeginCooldown;
    public event Action<bool> OnBeginCooldownBool;
    public event Action OnFinishCooldown;
    public event Action<bool> OnFinishCooldownBool;
    public event Action OnCanSpawnObstacle;
    bool inCooldown;

    const int cooldownTime = 5;
    private float cooldown = 5;

    private bool isObstacleSpawned;
    private bool isPrevisualizeInstantiated = false;
    private bool isPreVisualizeRed = false;
    private bool canSpawnObstacle = false;

    [SerializeField] private LayerMask layer;
    GameObject preVisualize = null;



    private void Update()
    {
        RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition), 500, layer);

        if (hits.Length > 0)
        {
            if (deck.GetSelectedCard() != null && !isPrevisualizeInstantiated)
            {
                preVisualize = Instantiate(deck.GetSelectedCard().GetPreVisualize(), hits[0].point, Quaternion.identity);
                isPrevisualizeInstantiated = true;
            }
            if (preVisualize != null)
            {
                preVisualize.transform.position = hits[0].point;
                preVisualize.transform.LookAt(player1.transform.position);

                if (!CanSpawnInPoint(hits[0].point) && !isPreVisualizeRed)
                {
                    Debug.Log("ROJOOOOO");
                    OnCantSpawnObstacle?.Invoke();
                    isPreVisualizeRed = true;
                }
                else if (CanSpawnInPoint(hits[0].point) && isPreVisualizeRed)
                {
                    Debug.Log("VERDEEE");
                    OnCanSpawnObstacle?.Invoke();
                    isPreVisualizeRed = false;
                }

            }
        }
        if (Input.GetMouseButtonDown(0))
        {

            if (!Utils.IsPointerOverUIObject(Input.mousePosition))
            {
                if (deck.GetSelectedCard() != null)
                {

                    if (hits != null && hits.Length > 0)
                    {
                        if (!inCooldown)
                        {
                            if (CanSpawnInPoint(hits[0].point))
                            {
                                if (canSpawnObstacle)
                                {
                                    SpawnObject(hits[0].point);
                                    Destroy(preVisualize);
                                    isPrevisualizeInstantiated = false;
                                }

                                if (isObstacleSpawned)
                                {
                                    OnBeginCooldown?.Invoke();
                                    OnBeginCooldownBool?.Invoke(true);
                                    Debug.Log(hits[0].transform.gameObject.layer);
                                    StartCoroutine(DisableCooldown());
                                }
                            }
                        }
                    }
                }
            }
        }

        if (inCooldown)
        {
            cooldown -= Time.deltaTime;
            OnSetCooldown?.Invoke(cooldown);

            if (cooldown <= 0)
            {
                OnFinishCooldownBool?.Invoke(false);
                OnFinishCooldown?.Invoke();
                cooldown = 5;
            }
        }
    }

    public IEnumerator DisableCooldown()
    {
        inCooldown = true;
        yield return new WaitForSeconds(cooldownTime);
        inCooldown = false;
    }

    public void SpawnObstacle(Vector3 pos, Vector3 lookAt)
    {
        isObstacleSpawned = true;

        List<RaycastHit> hits = new List<RaycastHit>();

        RaycastHit hit;

        if (Physics.Raycast(pos, Vector3.forward * 100, out hit))
        {
            hits.Add(hit);
            Debug.DrawLine(pos, hit.point, Color.green, 10);
        }
        if (Physics.Raycast(pos, Vector3.right * 100, out hit))
        {
            hits.Add(hit);
            Debug.DrawLine(pos, hit.point, Color.blue, 10);
        }
        if (Physics.Raycast(pos, Vector3.back * 100, out hit))
        {
            hits.Add(hit);
            Debug.DrawLine(pos, hit.point, Color.black, 10);
        }
        if (Physics.Raycast(pos, Vector3.left * 100, out hit))
        {
            hits.Add(hit);
            Debug.DrawLine(pos, hit.point, Color.red, 10);
        }

        float minDistance = float.MaxValue;
        int indexNear = 0;

        for (int i = 0; i < hits.Count; i++)
        {
            float currentDistance = Vector3.Distance(hits[i].point, pos);

            if (currentDistance < minDistance)
            {
                minDistance = currentDistance;
                indexNear = i;
            }
        }

        Vector3 direction = hits[indexNear].point - pos;
        GameObject bojeInstance = null;

        CardUI selectedCard = deck.GetSelectedCard();
        CardUI inactiveCard = deck.GetInactiveCard();

        bojeInstance = Instantiate(selectedCard.GetPrefab(), pos, Quaternion.LookRotation(player1.transform.position));
        bojeInstance.transform.LookAt(player1.transform.position);

        inactiveCard.gameObject.SetActive(true);
        inactiveCard.transform.SetSiblingIndex(selectedCard.transform.GetSiblingIndex());

        selectedCard.gameObject.SetActive(false);
        selectedCard.transform.SetAsLastSibling();
        deck.GetSelectedCard().SetCardDefault();
    }

    public bool CanSpawnInPoint(Vector3 pos)
    {
        return Vector3.Distance(pos, player1.transform.position) > player1.SafeZone;
    }

    private void SpawnObject(Vector3 pos)
    {
        if (player1)
        {
            SpawnObstacle(pos, player1.GetPosition());
        }
    }

    public void DisableCanSpawnObstacle()
    {
        canSpawnObstacle = false;
    }

    public void EnableCanSpawnObstacle()
    {
        canSpawnObstacle = true;
    }
    public void DisableIsPrevisualizeInstantiated(int ID)
    {
        isPrevisualizeInstantiated = false;
        Destroy(preVisualize);
    }
}
