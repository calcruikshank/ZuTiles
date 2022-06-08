using System;
using UnityEngine;

namespace Gameboard
{
    [RequireComponent(typeof(Gameboard))]
    public class DrawerController : MonoBehaviour
    {
        private Gameboard gameboard => Gameboard.Instance;
        public bool? DrawersVisible = null;
        public event Action GameboardDrawersHidden;
        public event Action GameboardDrawersShown;

        private void Start()
        {
            
        }

        /// <summary>
        /// Hides the Gameboard System Drawers.
        /// </summary>
        public void HideDrawers()
        {
            SetDrawerVisibility(false);
        }

        /// <summary>
        /// Displays the Gameboard System Drawers.
        /// </summary>
        public void ShowDrawers()
        {
            SetDrawerVisibility(true);
        }

        /// <summary>
        /// Set the visibility state of the Gameboard system drawers.
        /// </summary>
        /// <param name="visible"></param>
        public void SetDrawerVisibility(bool visible)
        {
            DrawersVisible = visible;
            
            AndroidJavaClass drawerHelper = gameboard.config.drawerHelper;
            if (drawerHelper == null)
            {
                GameboardLogging.Warning("Drawer Helper is not available. Unable to set drawers as " + (visible ? "Visible" : "Hidden") + ".");
                return;
            }

            AndroidApplicationContext context = gameboard.config.androidApplicationContext;
            if (drawerHelper == null)
            {
                GameboardLogging.Warning("Android Application Context is not available. Unable to set drawers as " + (visible ? "Visible" : "Hidden") + ".");
                return;
            }

            var drawerHelperArgs = context.GetNativeContext();
            drawerHelper.CallStatic("setDrawerVisibility", drawerHelperArgs, visible);

            if (visible)
                GameboardDrawersShown?.Invoke();
            else
                GameboardDrawersHidden?.Invoke();
        }
    }

}
