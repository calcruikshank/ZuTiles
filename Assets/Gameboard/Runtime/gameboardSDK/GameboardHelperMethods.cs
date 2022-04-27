using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Gameboard.Helpers
{
    public static class GameboardHelperMethods
    {
        /// <summary>
        /// Converts the entered object into a bytearray.
        /// </summary>
        /// <param name="obj">Object to convert</param>
        /// <returns>byte[]</returns>
        public static byte[] ObjectToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        /// <summary>
        /// Converts a screen position sent from the Gameboard into a Vector3 position in the scene. Will use the Camera assigned to the Gameboard component, otherwise will use Camera.main if no camera is assigned.
        /// </summary>
        /// <param name="inCamera">Scene camera to use for positioning.</param>
        /// <param name="inScreenPoint">Position sent from the Gameboard.</param>
        /// <returns>Vector3 scene position.</returns>
        public static Vector3 GameboardScreenPointToScenePoint(Camera inCamera, Vector2 inScreenPoint)
        {
            try
            {
                GameboardConfig config = Gameboard.singleton.config;
                Vector3 position = new Vector3(inScreenPoint.x * config.deviceResolution.x,
                                                              (1 - inScreenPoint.y) * config.deviceResolution.y,
                                                              Gameboard.singleton.gameCamera != null ? Gameboard.singleton.gameCamera.transform.position.y : Camera.main.transform.position.y);

                Vector3 worldPoint = inCamera.ScreenToWorldPoint(position);
                return worldPoint;
            }
            catch(Exception e)
            {
                Debug.LogError($"GameboardScreenPointToScenePoint exception: {e.Message} / {e.InnerException}");
                return Vector3.zero;
            }
        }

        /// <summary>
        /// Converter for acquiring the actual enum value from a string name entry in that enum.
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="inEnumString"></param>
        /// <returns></returns>
        public static T GetEnumValueFromString<T>(string inEnumString)
        {
            return (T)Enum.Parse(typeof(T), inEnumString);
        }

        /// <summary>
        /// Method of loading a Texture from the requested Path.
        /// </summary>
        /// <param name="inPath"></param>
        /// <returns></returns>
        public static Texture2D LoadTextureFromPath(string inPath)
        {
            if (!File.Exists(inPath))
            {
                Debug.LogError("--- GameboardHelperMethods::LoadTextureFromPath called with an invalid path: " + inPath);
                return null;
            }

            Texture2D loadedImage = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);

            try
            {
                byte[] imageBytes = File.ReadAllBytes(inPath);
                loadedImage.LoadImage(imageBytes, false);
                loadedImage.name = inPath;
            }
            catch (Exception e)
            {
                Debug.LogError($"--- Loading image at path {inPath} from byte array failed with exception {e.Message} / {e.InnerException}");
            }

            return loadedImage;
        }

        /// <summary>
        /// Method of loading a byte array from the requested Path.
        /// </summary>
        /// <param name="inPath"></param>
        /// <returns></returns>
        public static byte[] LoadByteArrayFromPath(string inPath)
        {
            if (!File.Exists(inPath))
            {
                Debug.LogError("--- GameboardHelperMethods::LoadByteArrayFromPath called with an invalid path: " + inPath);
                return null;
            }

            try
            {
                byte[] buffer = null;
                using (FileStream fs = new FileStream(inPath, FileMode.Open, FileAccess.Read))
                {
                    buffer = new byte[fs.Length];
                    fs.Read(buffer, 0, (int)fs.Length);
                }

                return buffer;
            }
            catch (Exception e)
            {
                Debug.LogError($"--- Loading byte array at path {inPath} from failed with exception {e.Message} / {e.InnerException}");
                return new byte[0];
            }

        }
    }
}