using UnityEngine;
using UnityEngine.UI;

public class PlacementUIController : MonoBehaviour
{
    [Header("References")]
    public PlantPlacementManager placementManager;

    public Image addButtonImage;
    public Image deleteButtonImage;

    public Outline addOutline;
    public Outline removeOutline;

    public GameObject inventoryUI;

    [Header("Scaling")]
    public float activeScale = 1.2f;
    public float normalScale = 1f;

    private void Start()
    {
        // estado inicial

        if (addOutline != null)
            addOutline.enabled = false;

        if (removeOutline != null)
            removeOutline.enabled = false;

        CloseInventory();   
    }

    public void OnAddButton()
    {
        placementManager.SetAddMode();

        OpenInventory();

        UpdateUI();
    }

    public void OnDeleteButton()
    {
        placementManager.SetDeleteMode();

        CloseInventory();

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (placementManager == null) return;

        bool deleteMode = placementManager.IsDeleteMode();

        if (!deleteMode)
        {
            // ADD ativo
            if (addOutline != null)
                addOutline.enabled = true;

            if (removeOutline != null)
                removeOutline.enabled = false;

            if (addButtonImage != null)
                addButtonImage.transform.localScale = Vector3.one * activeScale;

            if (deleteButtonImage != null)
                deleteButtonImage.transform.localScale = Vector3.one * normalScale;
        }
        else
        {
            // DELETE ativo
            if (removeOutline != null)
                removeOutline.enabled = true;

            if (addOutline != null)
                addOutline.enabled = false;

            if (addButtonImage != null)
                addButtonImage.transform.localScale = Vector3.one * normalScale;

            if (deleteButtonImage != null)
                deleteButtonImage.transform.localScale = Vector3.one * activeScale;
        }
    }

    // opcional: quando clicas no inventário
    public void OnSelectPlant(int index)
    {
        placementManager.SelectPlant(index);

        CloseInventory();
    }

   public void CloseInventory()
    {
        if (inventoryUI != null)
            inventoryUI.SetActive(false);
    }

    public void OpenInventory()
    {
        if (inventoryUI != null)
            inventoryUI.SetActive(true);
    }
}