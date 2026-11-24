// PokemonCardContentController.cs
// TILE12 · Unidad 3 · Clase 3 – Modelos 3D e interacción en RA
// Compatible con AR Foundation 5.x (Unity 2022.3).
//
// Qué hace: cuando AR Foundation reconoce el reverso de la
// carta (TargetCarta), crea UNA instancia del Prefab de
// contenido (ARContentRoot con la Poké Ball) como HIJA del
// ARTrackedImage, y la muestra solo mientras el estado de
// seguimiento sea Tracking.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PokemonCardContentController : MonoBehaviour
{
    // ---------- PARTE 1: referencias y variables ----------

    // Componente que detecta las imágenes (está en XR Origin).
    [SerializeField]
    private ARTrackedImageManager imageManager;

    // Prefab que aparecerá sobre la carta (ARContentRoot).
    [SerializeField]
    private GameObject contentPrefab;

    // Selector opcional; las imágenes sin una entrada usan contentPrefab.
    [SerializeField]
    private PokemonCardSelector cardSelector;

    // Relaciona cada imagen seguida (TrackableId) con su contenido creado.
    private readonly Dictionary<TrackableId, GameObject>
        spawnedContent =
            new Dictionary<TrackableId, GameObject>();

    private void Awake()
    {
        if (imageManager == null)
            imageManager = GetComponent<ARTrackedImageManager>();

        if (cardSelector == null)
            cardSelector = GetComponent<PokemonCardSelector>();

        if (imageManager == null)
            Debug.LogError(
                "ARTrackedImageManager is not assigned or attached to this GameObject.",
                this);
    }

    // ---------- PARTE 2: suscripción a eventos ----------

    // Al activarse, empezamos a escuchar al manager.
    private void OnEnable()
    {
        if (imageManager != null)
            imageManager.trackedImagesChanged +=
                OnTrackedImagesChanged;
    }

    // Al desactivarse, dejamos de escuchar.
    private void OnDisable()
    {
        if (imageManager != null)
            imageManager.trackedImagesChanged -=
                OnTrackedImagesChanged;
    }

    // AR Foundation llama a este método cuando hay cambios.
    private void OnTrackedImagesChanged(
        ARTrackedImagesChangedEventArgs eventArgs)
    {
        // Imágenes reconocidas por primera vez → crear.
        foreach (var image in eventArgs.added)
            CreateContent(image);

        // Imágenes que cambiaron (posición o estado) → actualizar.
        foreach (var image in eventArgs.updated)
            UpdateContent(image);

        // Imágenes que AR Foundation eliminará → eliminar.
        foreach (var image in eventArgs.removed)
            RemoveContent(image);
    }

    // ---------- PARTE 3: crear el contenido ----------

    private void CreateContent(ARTrackedImage trackedImage)
    {
        string imageName = trackedImage.referenceImage.name;
        Debug.Log($"Recognized AR card: '{imageName}'.", this);

        GameObject prefabToInstantiate = contentPrefab;
        Vector3 localPosition = Vector3.zero;
        Quaternion localRotation = Quaternion.identity;
        Vector3 localScale = Vector3.one;
        bool hasSelectedContent = cardSelector != null &&
            cardSelector.TryGetContent(
                imageName,
                out prefabToInstantiate,
                out localPosition,
                out localRotation,
                out localScale);

        if (prefabToInstantiate == null)
        {
            Debug.LogError(
                $"No content prefab is configured for card '{imageName}'.",
                this);
            return;
        }

        // Si esta imagen ya tiene contenido, no lo duplicamos.
        TrackableId id = trackedImage.trackableId;
        if (spawnedContent.ContainsKey(id)) return;

        // Creamos el Prefab como HIJO del ARTrackedImage:
        // hereda su posición y orientación sobre la carta.
        GameObject content = Instantiate(
            prefabToInstantiate, trackedImage.transform, false);
        Debug.Log(
            $"Displaying prefab '{prefabToInstantiate.name}' for card '{imageName}'.",
            content);

        if (hasSelectedContent)
        {
            content.transform.localPosition = localPosition;
            content.transform.localRotation = localRotation;
            content.transform.localScale = localScale;
        }
        else
        {
            // Mantiene el comportamiento anterior para imágenes sin entrada.
            content.transform.localPosition = Vector3.zero;
            content.transform.localRotation = Quaternion.identity;
        }

        // Guardamos la relación imagen → contenido.
        spawnedContent.Add(id, content);
        UpdateVisibility(trackedImage, content);
    }

    // ---------- PARTE 4: actualizar el contenido ----------

    private void UpdateContent(ARTrackedImage trackedImage)
    {
        // Si no existe contenido para esta imagen, lo creamos.
        if (!spawnedContent.TryGetValue(
                trackedImage.trackableId, out GameObject content))
        {
            CreateContent(trackedImage);
            return;
        }
        UpdateVisibility(trackedImage, content);
    }

    // Visible solo cuando ARCore sigue activamente la carta.
    // Limited o None → se oculta; reaparece con Tracking.
    private void UpdateVisibility(
        ARTrackedImage trackedImage, GameObject content)
    {
        bool isTracking =
            trackedImage.trackingState == TrackingState.Tracking;
        content.SetActive(isTracking);
    }

    // ---------- PARTE 5: eliminar el contenido ----------

    private void RemoveContent(ARTrackedImage trackedImage)
    {
        if (spawnedContent.TryGetValue(
                trackedImage.trackableId, out GameObject content))
        {
            Destroy(content);
            spawnedContent.Remove(trackedImage.trackableId);
        }
    }
}
