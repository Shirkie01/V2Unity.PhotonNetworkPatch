using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace V2Unity.PhotonNetworkPatch
{
    [HarmonyPatch(typeof(Login), "LoginUser")]
    public class Login_LoginUser_Patch
    {
        class LoginEnumerator : IEnumerable
        {
            private Login __instance;

            public LoginEnumerator(Login __instance)
            {
                this.__instance = __instance;
            }

            public IEnumerator GetEnumerator()
            {
                bool IsValidUsername(string s)
                {
                    return !string.IsNullOrWhiteSpace(s) && !"Player".Equals(s.Trim(), System.StringComparison.InvariantCultureIgnoreCase);
                }

                string name = __instance.registerField.text;
                if (!IsValidUsername(name))
                {
                    __instance.connectAnim.SetTrigger("Close");
                    __instance.registerAnim.SetTrigger("Open");
                    __instance.registerField.SelectWithoutSound(true, false);
                    __instance.registerField.ActivateInputField();
                    __instance.registerField.onSubmit.AddListener(s =>
                    {
                        string trimmedName = __instance.registerField.text.Trim();
                        if (IsValidUsername(trimmedName))
                        {
                            // Save the username to the config file
                            Plugin.configUsername.SetSerializedValue(trimmedName);

                            // Overwrite just in case .Trim() did something
                            __instance.registerField.text = trimmedName;

                            __instance.registerAnim.SetTrigger("Close");
                            
                            // Set the "name" var so the below while loop breaks
                            name = trimmedName;
                        }
                    });
                }

                while (!IsValidUsername(name))
                {
                    yield return null;
                }

                if (!PhotonManager.IsConnected())
                {
                    // Make sure the user has a name, since we're bypassing the database.                    
                    if (!IsValidUsername(name))
                    {
                        __instance.connectAnim.SetTrigger("Close");
                        __instance.errorAnim.SetTrigger("Open");
                        __instance.errorText.text = "Please enter a name.";
                        __instance.errorButton.SelectWithoutSound(true, false);
                        __instance.errorButton.onClick.RemoveAllListeners();
                        __instance.errorButton.onClick.AddListener(delegate ()
                        {
                            __instance.errorAnim.SetTrigger("Close");
                        });
                        PhotonManager.instance.Disconnect(true);
                        yield return null;
                    }

                    PhotonManager.instance.AuthenticateUser(name);
                    PhotonManager.instance.Connect();
                    int i = 0;
                    while (!PhotonManager.IsConnected())
                    {
                        int num = i;
                        i = num + 1;
                        if (num == 10)
                        {
                            __instance.connectAnim.SetTrigger("Close");
                            __instance.errorAnim.SetTrigger("Open");
                            __instance.errorText.text = "A connection error has occured.";
                            __instance.errorButton.SelectWithoutSound(true, false);
                            __instance.errorButton.onClick.RemoveAllListeners();
                            __instance.errorButton.onClick.AddListener(delegate ()
                            {
                                __instance.errorAnim.SetTrigger("Close");
                            });
                            __instance.errorButton.onClick.AddListener(delegate ()
                            {
                                __instance.TitleScreen();
                            });
                            PhotonManager.instance.Disconnect(true);
                            break;
                        }
                        __instance.connectText.text = "Connecting...";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Connecting....";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Connecting.....";
                        yield return new WaitForSeconds(0.5f);
                        __instance.connectText.text = "Connecting......";
                        yield return new WaitForSeconds(0.5f);
                    }
                    if (PhotonManager.IsConnected())
                    {
                        __instance.connectAnim.SetTrigger("Close");
                        __instance.TitleScreen();
                    }
                }
                else
                {
                    __instance.connectAnim.SetTrigger("Close");
                    __instance.TitleScreen();
                }
            }
        }

        [HarmonyPrefix]
        public static bool LoginUser(Login __instance, ref IEnumerator __result)
        {
            // Load the saved username value from the config file
            __instance.registerField.text = Plugin.configUsername.Value.Trim();

            var loginEnumerator = new LoginEnumerator(__instance);
            __result = loginEnumerator.GetEnumerator();
            return false;
        }
    }
}
