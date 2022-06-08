using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Gameboard.Examples
{
    public class DrawerControllerExample : MonoBehaviour
    {
        public Text Results;
        private DrawerController drawerController;

        private void Awake()
        {
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");
            Gameboard gameboard = gameboardObject.GetComponent<Gameboard>();
            gameboard.GameboardInitializationCompleted += OnGameboardInit;

            drawerController = gameboardObject.GetComponent<DrawerController>();
        }
         
        /// <summary>
        /// Repeatedly enable and disable the drawers to showcase the feataure.
        /// </summary>
        private void OnGameboardInit()
        {
            StartCoroutine(FlashDrawers());
        }

        /// <summary>
        /// Set the visibility of the drawer containing player presence items on the gameboard device.
        /// </summary>
        public void ToggleDrawers()
        {
            var desiredState = !(drawerController.DrawersVisible ?? false);
            Results.text = $"Setting drawer visibility to {desiredState}";
            drawerController.SetDrawerVisibility(desiredState);
        }

        private IEnumerator FlashDrawers()
        {
            drawerController.ShowDrawers();
            yield return new WaitForSeconds(1);
            drawerController.HideDrawers();
            yield return new WaitForSeconds(1);
            drawerController.ShowDrawers();
            yield return new WaitForSeconds(1);
            drawerController.HideDrawers();
            yield return new WaitForSeconds(1);
            drawerController.ShowDrawers();
            yield return new WaitForSeconds(1);
            drawerController.HideDrawers();
            yield return new WaitForSeconds(1);
            drawerController.ShowDrawers();
            yield return new WaitForSeconds(1);
        }
    }
}

