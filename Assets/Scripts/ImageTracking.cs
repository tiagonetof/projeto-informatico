using UnityEngine;
using UnityEngine.XR.ARFoundation;
using TMPro;
using System.Collections;


using UnityEngine.XR.ARSubsystems; // para TrackableType e ARRaycastHit
using System.Collections.Generic;  // para List<>


public class ImageTracking : MonoBehaviour
{
    public ARTrackedImageManager manager;
    public GameObject gardenRootPrefab;

    private GameObject currentGardenRoot;

    public ARRaycastManager raycastManager;
    private static List<ARRaycastHit> _hits = new List<ARRaycastHit>();

    public TMP_Text markerStatusText;
    private bool markerDetected = false;


    public GameObject scanSurfaceUI; //painel pra scanear a superfície


    //valores estão definidos no inspetor!
    [SerializeField] private float cameraOpeningDuration;
    [SerializeField] private float scanSurfaceDuration;


    private void Start()
    {
        

        if (scanSurfaceUI != null)
        {
            scanSurfaceUI.SetActive(false); //garante que o painel começa escondido
            StartCoroutine(CameraAndScanFlow());
        }


    }


    private void OnEnable()
    {
        manager.trackedImagesChanged += OnChanged;


        if (markerStatusText != null)
        {
            markerStatusText.text = "No marker detected";
            markerStatusText.color = Color.red;
            markerStatusText.gameObject.SetActive(true);

        }
        


    }

    private void OnDisable()
    {
        manager.trackedImagesChanged -= OnChanged;
       
    }

  

    private void OnChanged(ARTrackedImagesChangedEventArgs args)
    {
        foreach (var img in args.added)
        {
            PlaceOrMoveGarden(img);
        }

    }

    private void PlaceOrMoveGarden(ARTrackedImage img)
    {
        // Se ainda não existe jardim, cria-o
        if (currentGardenRoot == null)
        {
            currentGardenRoot = Instantiate(gardenRootPrefab);
        }

        if (!markerDetected)
        {
            // Coloca (ou move) o jardim para a posição do marker
            currentGardenRoot.transform.position = img.transform.position;


            //currentGardenRoot.transform.rotation = img.transform.rotation;

            currentGardenRoot.transform.rotation = Quaternion.Euler(0f, img.transform.eulerAngles.y, 0f);

            markerDetected = true;


            // Feedback visual para o utilizador
            if (markerStatusText != null)
            {
                markerStatusText.text = "Marker detected";
                markerStatusText.color = Color.green;
                StartCoroutine(HideMarkerText());
            }

            // Marker já cumpriu a sua função → desligar tracking
            //manager.enabled = false;




            // Informa o sistema de placement onde está o jardim
            var placement = FindObjectOfType<PlantPlacementManager>();
            if (placement != null)
            {
            
                StartCoroutine(InitGarden(img, placement));
                
            }

        }


     

    

    }


    private IEnumerator InitGarden(ARTrackedImage img, PlantPlacementManager placement)
    {
        // Segurança básica
        if (placement == null || currentGardenRoot == null)
            yield break;

        // Garante que o placement sabe qual é o root
        placement.gardenRoot = currentGardenRoot.transform;

        // Vamos tentar durante alguns frames obter uma pose "ok"
        // (câmara existe + marker está à frente da câmara)
        const int maxFrames = 20;

        for (int i = 0; i < maxFrames; i++)
        {
            // Se não houver Camera.main ainda, espera mais um frame
            var cam = Camera.main;
            if (cam == null)
            {
                yield return null;
                continue;
            }

            // Se o marker ainda não estiver tracking, espera mais um frame
            if (img.trackingState != TrackingState.Tracking)
            {
                yield return null;
                continue;
            }

            // Verifica se o marker está à frente da câmara (z > 0 no viewport)
            Vector3 sp = cam.WorldToViewportPoint(img.transform.position);
            if (sp.z > 0f)
            {
                //  Pose aceitável: define plano horizontal ao nível do marker
                if (raycastManager.Raycast(new Vector2(Screen.width / 2f, Screen.height / 2f),_hits,TrackableType.PlaneWithinPolygon))
                {
                    Vector3 realPlanePoint = _hits[0].pose.position;

                    // 1) Define o plano (referência lógica)
                    placement.SetPlane(realPlanePoint, Vector3.up);

                    currentGardenRoot.transform.position = new Vector3( realPlanePoint.x, realPlanePoint.y,realPlanePoint.z);

                }
                else
                {
                    // fallback
                    placement.SetPlane(img.transform.position, Vector3.up);
                }
                placement.RebuildGarden();
                yield break;
            }

            yield return null;
        }

        // Fallback garantido: mesmo que a câmara/pose não estejam ok,
        // definimos plano horizontal ao nível do GardenRoot para nunca bloquear o placement.
        placement.SetPlane(currentGardenRoot.transform.position, Vector3.up);
        placement.RebuildGarden();
    }



    private IEnumerator HideMarkerText()
    {
        yield return new WaitForSeconds(2f);
        markerStatusText.gameObject.SetActive(false);
    }



    private IEnumerator CameraAndScanFlow()
    {
        Debug.Log("cameraOpeningDuration = " + cameraOpeningDuration);

        float t = Time.realtimeSinceStartup;
        
        Debug.Log("START REAL TIME: " + t);


        //esperar a câmara abrir
        yield return new WaitForSecondsRealtime(cameraOpeningDuration);

        Debug.Log("DELAY REAL: " + (Time.realtimeSinceStartup - t));

        //mostrar UI
        if (scanSurfaceUI != null)
            scanSurfaceUI.SetActive(true);

        //esperar duração do aviso
        yield return new WaitForSecondsRealtime(scanSurfaceDuration);

        //esconder UI
        if (scanSurfaceUI != null)
            scanSurfaceUI.SetActive(false);
    }


    /*
private IEnumerator HideScanSurfaceAfterDelay()
{
    yield return new WaitForSeconds(scanSurfaceDuration);

    if (scanSurfaceUI != null)
        scanSurfaceUI.SetActive(false);


}

private IEnumerator WaitForCameraToOpen()
{
    yield return new WaitForSeconds(cameraOpeningDuration);

}*/
}

