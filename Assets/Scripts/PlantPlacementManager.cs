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

 

    [SerializeField] private float minPlaneArea = 0.16f; //area minima para placement (metros quadrados)



    private Vector3 planePoint;
    private Vector3 planeNormal;
    private bool hasPlane = false;

    public float planeTolerance = 0.035f;

    [Header("Placement distance filter")]
    public float minPlacementDistance = 0.20f; // metros
 
    [SerializeField] private float plantScale = 0.2f;


    private int selectedPlantIndex = 0;
  



    private void Start()
    {
        deleteMode = false; //garante que começa a verde -> modo normal (adicionar plantas)
        LoadGarden();
    }


    public void SetAddMode()
    {deleteMode = false;}

    public void SetDeleteMode()
    {deleteMode = true;}

    public bool IsDeleteMode()
    {return deleteMode;}



    public void SelectPlant(int index)
    {selectedPlantIndex = index;}


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



    //deteta se o toque está sobre um elemento UI (para evitar colocar plantas quando se quer clicar num botão)
    public static bool IsPointOverUIObject(Vector2 pos)
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData eventPosition = new PointerEventData(EventSystem.current);
        eventPosition.position = pos;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventPosition, results);

        return results.Count > 0;
    }



    //verifica se há um plano suficientemente grande para permitir o placement (baseado na maior área dos planos detectados)
    private float findLargestPlane()
    {
        float largestArea = 0f;


        foreach (var plane in planeManager.trackables)
        {
            float area = plane.size.x * plane.size.y;

            if (area > largestArea)
                largestArea = area;
        }

        return largestArea;

    }



    public bool IsSurfaceReady()
    {
        float largestArea = findLargestPlane();
        return largestArea >= minPlaneArea;
    }



    //Este método é chamado automaticamente quando há um toque:
    private void HandleTouch(Finger finger)
    {
        // Sem jardim → não faz nada
        if (gardenRoot == null)
            return;

        if (IsPointOverUIObject(finger.screenPosition))
            return;


        float largestArea = findLargestPlane();
        
        if(largestArea < minPlaneArea)
        {
            //nao permitir placement
            //mostrar texto de "scanning plantable surface"

           

        }

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

     

        if (camDist < minPlacementDistance)
        {
            Debug.Log("Muito perto da câmara. Tenta tocar mais longe na superfície.");
            return;
        }

        GameObject flower = Instantiate(flowers[selectedPlantIndex], gardenRoot);


        // Conversão world → local (correta)
        flower.transform.localPosition =
            gardenRoot.InverseTransformPoint(pose.position);

        flower.transform.localRotation =
            Quaternion.Inverse(gardenRoot.rotation) * pose.rotation;

        flower.transform.localScale = Vector3.one * plantScale;


        flower.tag = "Plant";

        PlantData data = new PlantData
        {
            plantIndex = selectedPlantIndex,
            localPosition = flower.transform.localPosition,
            localRotation = flower.transform.localRotation
        };

        gardenData.plants.Add(data);
        SaveGarden(); // Salva o estado do jardim após adicionar a planta

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


}