using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
namespace Project.Model_Loader
{
    /// <summary>
    /// Base object for loaded models. Used to set up model and it's visuals
    /// </summary>
    public class SpawnBaseObject : MonoBehaviour
    {
        [SerializeField]
        private BoxCollider fitCube;
        [SerializeField]
        private XRGrabInteractable interactable;
        [SerializeField]
        private Outline modelOutline = null;

        public Bounds Bounds { get; set; }
        public BoxCollider FitCube => fitCube;

        public void InitializeModelVisuals(GameObject model)
        {
            Outline outline = model.AddComponent<Outline>();
            outline.enabled = false;
            modelOutline = outline;

            Bounds = GetBounds(model);
            //Add box collider to full model
            BoxCollider boxCollider = model.AddComponent<BoxCollider>();
            boxCollider.center = model.transform.InverseTransformPoint(Bounds.center);
            boxCollider.size = Bounds.size;

            //resize model to fit cube sizes
            Vector3 targetScale = FitCube.bounds.size;
            Vector3 modelScale = Bounds.size;
            if (modelScale == Vector3.zero) modelScale = Vector3.one;
            Debug.Log($"Model {modelScale} vs Target {targetScale}");

            var xFraction = modelScale.x / targetScale.x;
            var yFraction = modelScale.y / targetScale.y;
            var zFraction = modelScale.z / targetScale.z;

            float fraction = Mathf.Max(xFraction, yFraction, zFraction);
            Debug.Log($"Fraction: {fraction}");
            fraction = fraction == 0 ? 1 : fraction;

            model.transform.localScale /= fraction;
            //Bounds = GetBounds(model);
            //model.transform.localPosition = model.transform.position - Bounds.center;



            //interactable.selectEntered.AddListener(OnSelected); //doesn't work
            //interactable.selectExited.AddListener(OnDeselected); //doesn't work

            Debug.Log($"InitializeModelVisuals: Outline({modelOutline})", interactable);
        }

        public Bounds GetBounds(GameObject gameObject)
        {
            Bounds bounds = new Bounds(gameObject.transform.position, Vector3.zero);
            var rList = gameObject.GetComponentsInChildren<Renderer>();
            foreach (var renderer in rList)
            {
                bounds.Encapsulate(renderer.bounds);
            }
            return bounds;
        }

        public void OnInteractableSelectEntered()
        {
            //Debug.Log($"OnSelected... Outline({modelOutline})", interactable);
            if (modelOutline != null)
            {
                modelOutline.enabled = true;
            }
        }
        public void OnInteractableSelectExited()
        {
            if (modelOutline != null)
            {
                modelOutline.enabled = false;
            }
            //Debug.Log($"OnDeselected... Outline({modelOutline})", interactable);
        }
        private void OnSelected(SelectEnterEventArgs arg0)
        {
            OnInteractableSelectEntered();
        }
        private void OnDeselected(SelectExitEventArgs arg0)
        {
            OnInteractableSelectExited();
        }
    }
}