using UnityEngine;
using Gameboard.EventArgs;
using System.Linq;
using UnityEngine.UI;

namespace Gameboard.Examples
{
    public class DeviceEventControllerExample : MonoBehaviour
    {
        public Text Results;
        private DeviceEventController deviceEventController;
        private UserPresenceController userPresenceController;
        private int labelSetCount = 0;

        private void Awake()
        {
            GameboardLogging.Verbose("DeviceEventExample Awake");
            GameObject gameboardObject = GameObject.FindWithTag("Gameboard");

            deviceEventController = gameboardObject.GetComponent<DeviceEventController>();
            userPresenceController = gameboardObject.GetComponent<UserPresenceController>();
            GameboardLogging.Verbose("DeviceEventExample Awake Success");
        }

        /// <summary>
        /// Display a pop up message on Companion of the first user with the for 5 seconds. 
        /// </summary>
        public async void ShowDevicePopup()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.DisplaySystemPopup(userId, "This is a test message.\nThis is a continuation of the message.", 5);
            Results.text = $"Sent popup to user {userId}, Response: {response}";
        }

        /// <summary>
        /// Reset the first companion user's play panel
        /// </summary>
        public async void ResetUserPlayPanel()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            CompanionMessageResponseArgs response = await deviceEventController.ResetPlayPanel(userId);
            Results.text = $"reset user play panel for {userId}, Response: {response}";
        }

        /// <summary>
        /// Sets the label text at the top of the target user's Companion device
        /// </summary>
        public async void SetTopLabelCounter()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            labelSetCount++;
            CompanionMessageResponseArgs response = await deviceEventController.SetTopLabel(userId, $"Label Set {labelSetCount} times.");
            Results.text = $"top label was set for {userId}, Response: {response}";
        }

        /// <summary>
        /// Sets the label text at the top of the target user's Companion device with long text demonstrating the 18 character limit
        /// </summary>
        public async void SetTopLabelLong()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            labelSetCount++;
            CompanionMessageResponseArgs response = await deviceEventController.SetTopLabel(userId, $"1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890");
            Results.text = $"top label was set for {userId}, Response: {response}";
        }

        /// <summary>
        /// Sets the label text at the top of the target user's Companion device with empty text demonstrating removal of the label
        /// </summary>
        public async void SetTopLabelEmpty()
        {
            var userId = Utils.GetFirstCompanionUserId(userPresenceController);
            if (userId == string.Empty)
            {
                Results.text = "There are no companion users connected.";
                return;
            }

            labelSetCount++;
            CompanionMessageResponseArgs response = await deviceEventController.SetTopLabel(userId, $"");
            Results.text = $"top label was set for {userId}, Response: {response}";
        }
    }
}

