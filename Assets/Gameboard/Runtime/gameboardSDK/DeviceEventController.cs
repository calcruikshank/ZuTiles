using UnityEngine;
using Gameboard.EventArgs;
using System.Threading.Tasks;

namespace Gameboard
{
    [RequireComponent(typeof(Gameboard))]
    public class DeviceEventController : MonoBehaviour
    {
        private Gameboard gameboard => Gameboard.Instance;

        private void Start()
        {
            
        }

        /// <summary>
        /// Displays a system-level popup on the target user's Companion device for the requested number of seconds.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="textToDisplay"></param>
        /// <param name="timeInSecondsToDisplay"></param>
        public async Task<CompanionMessageResponseArgs> DisplaySystemPopup(string userId, string textToDisplay, float timeInSecondsToDisplay)
        {
            return await gameboard.services.companionHandler.DisplaySystemPopup(Gameboard.COMPANION_VERSION, userId, textToDisplay, timeInSecondsToDisplay);
        }

        /// <summary>
        /// Sets the label text at the top of the target user's Companion device
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="textToDisplay"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> SetTopLabel(string userId, string textToDisplay)
        {
            return await gameboard.services.companionHandler.SetTopLabel(Gameboard.COMPANION_VERSION, userId, textToDisplay);
        }

        /// <summary>
        /// Entirely wipes the PlayPanel of a companion clean, removing all data.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<CompanionMessageResponseArgs> ResetPlayPanel(string userId)
        {
            return await gameboard.services.companionHandler.ResetPlayPanel(Gameboard.COMPANION_VERSION, userId);
        }
    }
}
