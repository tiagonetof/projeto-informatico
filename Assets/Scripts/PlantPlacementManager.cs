using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;


public class PlantPlacementManager : MonoBehaviour
{
    public GameObject[] flowers;
    public Transform gardenRoot;

    public GardenData gardenData = new GardenData();

    public ARRaycastManager raycastManager;
    public ARPlaneManager planeManager;

    private List<ARRaycastHit> raycastHits = new List<ARRaycastHit>();

    private bool deleteMode;

    public Image deleteButtonImage;
    public Color normalColor = Color.green;
    public Color deleteActiveColor = Color.red;


    private Vector3 planePoint;
    private Vector3 planeNormal;
    private bool hasPlane = false;

    public float planeTolerance = 0.035f;

    [Header("Placement distance filter")]
    public float minPlacementDistance = 0.20f; // metros
    public bool showDebugDistance = false;


    private void Start()
    {
        deleteMode = false; //garante que começa a verde -> modo normal (adicionar plantas)
        UpdateButtonColor();

        LoadGarden();
    }

    private void UpdateButtonColor()
    {
        if (deleteButtonImage != null)
        {
            deleteButtonImage.color = deleteMode ? deleteActiveColor : normalColor;
        }
    }


    //chamado no Unity, onClick() event do botão de delete
    public void ToggleDeleteMode()
    {
        deleteMode = !deleteMode;
        UpdateButtonColor();

    }


    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
        Touch.onFingerDown += HandleTouch; //Quando um dedo tocar no ecrã → chama este método
    }

    private void OnDisable() 
    {
        Touch.onFingerDown -= HandleTouch; //remove o evento (boa prática)
        EnhancedTouchSupport.Disable();
    }


    private void RemovePlantData(Transform plant)
    {
        Vector3 localPos = plant.localPosition;

        for (int i = 0; i < gardenData.plants.Count; i++)
        {
            if (Vector3.Distance(gardenData.plants[i].localPosition, localPos) < 0.001f)
            {
                gardenData.plants.RemoveAt(i);
                SaveGarden(); // guarda o estado atualizado do jardim após remover a planta
                return;
            }
        }

    }



    public void SetPlane(Vector3 point, Vector3 normal)
    {
        planePoint = point;
        planeNormal = normal.normalized;
        hasPlane = true;
    }


    //Este método é chamado automaticamente quando há um toque:
    private void HandleTouch(Finger finger)
    {
        // Sem jardim → não faz nada
        if (gardenRoot == null)
            return;

        // Ignorar UI
        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject(finger.index))
            return;

        Vector2 touchPosition = finger.screenPosition;

        // Delete mode
        if (deleteMode)
        {
            Ray ray = Camera.main.ScreenPointToRay(touchPosition);
            if (Physics.Raycast(ray, out RaycastHit hit) &&
                hit.transform.CompareTag("Plant"))
            {

                Transform plant = hit.transform;

                // remover dos dados
                RemovePlantData(plant);

                Destroy(plant.gameObject);

                
            }
            return;
        }

        // Raycast normal a planos
        if (!raycastManager.Raycast(
            touchPosition,
            raycastHits,
            TrackableType.PlaneWithinPolygon | TrackableType.PlaneEstimated))
            return;



        var pose = raycastHits[0].pose;

        // Se ainda não há plano definido → não permite
        if (!hasPlane)
        {
            Debug.Log("Plano ainda não definido pelo marker");
            return;
        }

        // calcular distância ponto → plano
        float heightDiff = Mathf.Abs(pose.position.y - planePoint.y);
        if (heightDiff > planeTolerance)
        {
            Debug.Log("Fora da altura do marker");
            return;
        }



        // distância do ponto candidato até à câmara (em metros)
        float camDist = Vector3.Distance(Camera.main.transform.position, pose.position);

        if (showDebugDistance)
            Debug.Log($"Distância à câmara: {camDist:F2} m");

        if (camDist < minPlacementDistance)
        {
            Debug.Log("Muito perto da câmara. Tenta tocar mais longe na superfície.");
            return;
        }

        int randomIndex = Random.Range(0, flowers.Length);
        GameObject flower = Instantiate(flowers[randomIndex], gardenRoot);


        // Conversão world → local (correta)
        flower.transform.localPosition =
            gardenRoot.InverseTransformPoint(pose.position);

        flower.transform.localRotation =
            Quaternion.Inverse(gardenRoot.rotation) * pose.rotation;

        flower.tag = "Plant";

        PlantData data = new PlantData
        {
            plantIndex = randomIndex,
            localPosition = flower.transform.localPosition,
            localRotation = flower.transform.localRotation
        };

        gardenData.plants.Add(data);
        SaveGarden(); // Salva o estado do jardim após adicionar a planta

    }


    public void SaveGarden()
    {
        string json = JsonUtility.ToJson(gardenData);
        PlayerPrefs.SetString("GardenData", json);
        PlayerPrefs.Save();

        Debug.Log("Garden saved: " + json);
    }



    public void LoadGarden()
    {
        if (!PlayerPrefs.HasKey("GardenData"))
        {
            Debug.Log("No saved garden found.");
            return;
        }

        string json = PlayerPrefs.GetString("GardenData");
        gardenData = JsonUtility.FromJson<GardenData>(json);

        Debug.Log("Garden loaded: " + json);
    }




    public void RebuildGarden()
    {
        if (gardenRoot == null)
        {
            Debug.Log("GardenRoot not set yet. Cannot rebuild.");
            return;
        }

        // Limpa plantas existentes (caso haja)
        foreach (Transform child in gardenRoot)
        {
            Destroy(child.gameObject);
        }

        foreach (var data in gardenData.plants)
        {
            GameObject plant = Instantiate(flowers[data.plantIndex], gardenRoot);
            plant.transform.localPosition = data.localPosition;
            plant.transform.localRotation = data.localRotation;
            plant.tag = "Plant";
        }

        Debug.Log("Garden rebuilt with " + gardenData.plants.Count + " plants.");
    }


}