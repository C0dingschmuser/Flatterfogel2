using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.Storage;
using UnityEngine.Networking;
using UnityEngine.Localization;
using UnityEngine.ResourceManagement.AsyncOperations;
#if UNITY_ANDROID || UNITY_IOS
using UnityEngine.SocialPlatforms;
using Firebase.Analytics;
#endif
#if UNITY_ANDROID
using GooglePlayGames;
#endif
using TMPro;

public enum AccountStates
{
    LoggedOut = 0,
    LoggedIn = 1,
    Synced = 2,
}

public class AccountHandler : MonoBehaviour
{
    public ScoreHandler scoreHandler;

    public List<GameObject> pages = new List<GameObject>();

    public GameObject regUsername, regPasswort, regEmail, regInfo, regSubmit;
    public GameObject loginUsername, loginPassword, loginInfo;
    public GameObject restoreEmail, restorePasswort, restoreCode, restoreInfo, restoreButton;
    public GameObject alphaUsername, alphaPasswort, alphaEmail, alphaInfo;
    public GameObject signInfo;

    public AccountStates accountState = AccountStates.LoggedOut;
    public string username = "";

    public LocalizedString connecting, connectingFailed, invalidName, invalidPassword, checking,
        length, invalidEmail, nameExists, emailRegistered, loginFailed, invalidCode,
        lookInSpam, resetPassword;
    public string connectionString, connectionFailedString, invalidNameString,
        invalidPasswordString, checkingString, lengthString, invalidEmailString,
        nameExistsString, emailRegisteredString, loginFailedString, invalidCodeString,
        lookInSpamString, resetPasswordString;

    public static bool running = false;
    public static AccountHandler Instance = null;

    public static string[] tempNames = 
        new string[] { "Hans", "Max", "Peter", "Gustav", "Anna" };

    public static string authKey = ""; //Removed on Commit
    private int restoreStep = 0;

    public void Initialize()
    {
        Instance = this;

        if(authKey.Length == 0)
        {
            Debug.LogError("AuthKey not set!");
        }

        username = ObscuredPrefs.GetString("Player_Username", "");
        accountState = (AccountStates)ObscuredPrefs.GetInt("Player_AccountState", 0);

#if UNITY_ANDROID
        PlayGamesPlatform.DebugLogEnabled = false;
        PlayGamesPlatform.Activate();
#endif
    }

    public void StartGplayLogin()
    {
#if UNITY_ANDROID || UNITY_IOS
        if (!OptionHandler.playStore)
        {
            FirebaseAnalytics.SetUserProperty("PlayStoreVersion", "0");
            return;
        } else
        {
            FirebaseAnalytics.SetUserProperty("PlayStoreVersion", "1");
        }

        if (!Social.localUser.authenticated)
        {
            Social.localUser.Authenticate((bool success) =>
            {
                if (success)
                {
                    Debug.Log("Welcome " + Social.localUser.userName);
                }
                else
                {
                    Debug.Log("Authentication failed.");
                }
            });
        }
        else
        {
            Debug.Log("User Authenticated");
        }
#endif
    } 

    // Start is called before the first frame update
    void Start()
    {
        ResetMenu();
    }

    public void ResetMenu(bool signIn = false)
    {
        pages[0].SetActive(true);
        for (int i = 1; i < pages.Count; i++)
        {
            pages[i].SetActive(false);
        }
        restoreStep = 0;

        if(accountState == AccountStates.LoggedOut && username.Length > 1)
        { //alpha user
            alphaUsername.GetComponent<TMP_InputField>().text = username;
            EnablePage(4);
        }

        if(signIn)
        {
            signInfo.GetComponent<TextMeshProUGUI>().color = Color.black;
            signInfo.GetComponent<TextMeshProUGUI>().text = connectionString;//"Verbinde mit Server...";
            EnablePage(5);
        }

        restoreEmail.GetComponent<TMP_InputField>().enabled = true;
        restorePasswort.GetComponent<TMP_InputField>().enabled = true;
        restoreCode.GetComponent<TMP_InputField>().enabled = true;
    }

    public void EnablePage(int id)
    {
        if (running) return;

        for (int i = 0; i < pages.Count; i++)
        {
            if (i != id)
            {
                pages[i].SetActive(false);
            }
            else
            {
                pages[i].SetActive(true);
            }
        }
    }

    private bool CheckUsername(string username)
    {
        bool userOk = false;
        for (int i = 0; i < username.Length; i++)
        { //prüfen ob name zulässig
            if ((username[i] >= 'a' && username[i] <= 'z') ||
                (username[i] >= 'A' && username[i] <= 'Z') ||
                username[i] == '.' ||
                username[i] == ',' ||
                username[i] == '_' ||
                username[i] == '-' ||
                (username[i] >= '0' && username[i] <= '9'))
            {
                userOk = true;
            }
            else
            {
                userOk = false;
                break;
            }
        }

        if (username.Length < 2 || username.Length > 20)
        {
            userOk = false;
        }

        return userOk;
    }

    private bool CheckPasswort(string passwort)
    {
        bool pwOk = false;
        for (int i = 0; i < passwort.Length; i++)
        { //prüfen ob pw zulässig
            if ((passwort[i] >= 'a' && passwort[i] <= 'z') ||
                (passwort[i] >= 'A' && passwort[i] <= 'Z') ||
                passwort[i] == '.' ||
                passwort[i] == ',' ||
                passwort[i] == '_' ||
                passwort[i] == '-' ||
                (passwort[i] >= '0' && passwort[i] <= '9'))
            {
                pwOk = true;
            }
            else
            {
                pwOk = false;
                break;
            }
        }

        if (passwort.Length < 4 || passwort.Length > 20)
        {
            pwOk = false;
        }

        return pwOk;
    }

    private bool CheckEmail(string email)
    {
        bool emailOk = false;
        int check = 0;
        for (int i = 0; i < email.Length; i++)
        {
            if (email[i] == '@' && check == 0)
            {
                check = 1;
            }
            else if (email[i] == '.' && check == 1)
            { //. nach @ gefunden
                emailOk = true;
                break;
            }
        }

        return emailOk;
    }

    private bool CheckRestoreCode(string code)
    {
        bool ok = false;

        for(int i = 0; i < code.Length; i++)
        {
            if(code[i] >= '0' && code[i] <= '9')
            {
                ok = true;
            } else
            {
                ok = false;
                break;
            }
        }

        if(code.Length != 6)
        {
            ok = false;
        }

        return ok;
    }

    public void StartGplayRegister()
    {
        if (running) return;
        running = true;

        string username = Social.localUser.userName;

#if UNITY_EDITOR
        username = "C0dingschmuser";
#endif

        this.username = username;

        StartCoroutine(SignInRegister(username));
    }

    IEnumerator SignInRegister(string username)
    {
        string authHash = Md5Sum(username + authKey);

        string url = "https://bruh.games/manager.php?signedin=1&name=" + username + 
            "&hash=" + authHash;

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            signInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            signInfo.GetComponent<TextMeshProUGUI>().text = connectionFailedString; //"Verbindung fehlgeschlagen!";
        } else
        {
            string response = www.downloadHandler.text;

            string[] split = response.Split('|');

            if(split[0].Equals("1"))
            { //user in cloud gefunden, übernehme scores
                for(int i = 0; i < 3; i++)
                { //loop durch diff
                    string[] scoreData = split[1 + i].Split('#');

                    for(int a = 0; a < scoreData.Length - 1; a++)
                    {
                        FlatterFogelHandler.Instance.SetHighscore(a, i, ulong.Parse(scoreData[a]));
                    }
                }

                Debug.Log("Daten übernehmen");
            } else if(split[0].Equals("0"))
            { //user nicht gefunden, neuregistrierung
                Debug.Log("Neuregistrierung");
            }

            EndLoginStart();
        }

        running = false;
    }

    public void StartRegister()
    {
        if (running) return;
        running = true;

        string username = regUsername.GetComponent<TMP_InputField>().text;
        string passwort = regPasswort.GetComponent<TMP_InputField>().text;
        string email = regEmail.GetComponent<TMP_InputField>().text;

        bool userOk = CheckUsername(username);

        if(!userOk)
        {
            regInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            regInfo.GetComponent<TextMeshProUGUI>().text = invalidNameString + "\n" +
                                                            "(a-z A-Z 0-9 .,-_)\n" +
                                                            "2-20 " + lengthString;
            return;
        }

        bool pwOk = CheckPasswort(passwort);

        if (!pwOk)
        {
            regInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            regInfo.GetComponent<TextMeshProUGUI>().text = invalidPasswordString + "\n" +
                                                            "(a-z A-Z 0-9 .,-_)\n" +
                                                            "4-20 " + lengthString;
            return;
        }

        bool emailOk = CheckEmail(email);

        if(!emailOk)
        {
            regInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            regInfo.GetComponent<TextMeshProUGUI>().text = invalidEmailString;
            return;
        }

        regInfo.GetComponent<TextMeshProUGUI>().color = Color.black;
        regInfo.GetComponent<TextMeshProUGUI>().text = checkingString;

        StartCoroutine(RegisterUser(username, passwort, email));
    }

    public static string Md5Sum(string strToEncrypt)
    {
        System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
        byte[] bytes = ue.GetBytes(strToEncrypt);

        // encrypt bytes
        System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
        byte[] hashBytes = md5.ComputeHash(bytes);

        // Convert the encrypted bytes back to a string (base 16)
        string hashString = "";

        for (int i = 0; i < hashBytes.Length; i++)
        {
            hashString += System.Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
        }

        return hashString.PadLeft(32, '0');
    }

    IEnumerator RegisterUser(string newName, string passwort, string email)
    {
        passwort = Md5Sum(passwort);

        string authHash = Md5Sum(newName + authKey);

        string url = "https://bruh.games/manager.php?register=1&name=" +
            newName + "&pw=" + passwort + "&email=" + email + "&hash=" + authHash;

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            regInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            regInfo.GetComponent<TextMeshProUGUI>().text = connectionFailedString;
        } else
        {
            string response = www.downloadHandler.text;

            string[] split = response.Split('#');

            if(split[0].Equals("1"))
            { //registrierung erfolgreich
                this.username = newName;

                EndLoginStart();
            } else
            { //name bereits vorhanden
                regInfo.GetComponent<TextMeshProUGUI>().color = Color.red;

                if(split[1].Equals("0"))
                {
                    regInfo.GetComponent<TextMeshProUGUI>().text = nameExistsString;
                } else
                {
                    regInfo.GetComponent<TextMeshProUGUI>().text = emailRegisteredString;
                }
            }
        }

        running = false;
    }

    public void LoginClicked()
    {
        if (running) return;
        running = true;

        string username = loginUsername.GetComponent<TMP_InputField>().text;
        string passwort = loginPassword.GetComponent<TMP_InputField>().text;

        if(!CheckUsername(username))
        {
            loginInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            loginInfo.GetComponent<TextMeshProUGUI>().text = invalidNameString + "\n" +
                                                            "(a-z A-Z 0-9 .,-_)\n" +
                                                            "2-20 " + lengthString;
            return;
        }

        if(!CheckPasswort(passwort))
        {
            loginInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            loginInfo.GetComponent<TextMeshProUGUI>().text = invalidPasswordString + "\n" +
                                                            "(a-z A-Z 0-9 .,-_)\n" +
                                                            "4-20 " + lengthString;
            return;
        }

        StartCoroutine(StartLogin(username, passwort));
    }

    IEnumerator StartLogin(string username, string passwort)
    {
        passwort = Md5Sum(passwort);

        string authHash = Md5Sum(username + authKey);

        string url = "https://bruh.games/manager.php?login=1&name=" +
            username + "&pw=" + passwort + "&hash=" + authHash;

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            loginInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            loginInfo.GetComponent<TextMeshProUGUI>().text = connectionFailedString;
        } else
        {
            string response = www.downloadHandler.text;

            if(response.Contains("1"))
            { //login erfolgreich
                this.username = username;

                EndLoginStart();
            } else
            { 
                loginInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
                loginInfo.GetComponent<TextMeshProUGUI>().text = loginFailedString;
            }
        }

        running = false;
    }

    public void RestoreClicked()
    {
        if (running) return;
        running = true;

        string email = restoreEmail.GetComponent<TMP_InputField>().text;
        string passwort = restorePasswort.GetComponent<TMP_InputField>().text;
        string code = restoreCode.GetComponent<TMP_InputField>().text;

        if(!CheckEmail(email))
        {
            restoreInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            restoreInfo.GetComponent<TextMeshProUGUI>().text = invalidEmailString;

            return;
        }

        if(restoreStep == 1)
        {
            if (!CheckPasswort(passwort))
            {
                restoreInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
                restoreInfo.GetComponent<TextMeshProUGUI>().text = invalidPasswordString + "\n" +
                                                            "(a-z A-Z 0-9 .,-_)\n" +
                                                            "4-20 " + lengthString;
                return;
            }

            if (!CheckRestoreCode(code))
            {
                restoreInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
                restoreInfo.GetComponent<TextMeshProUGUI>().text = invalidCodeString;

                return;
            }
        }


        if(restoreStep == 0)
        {
            restoreEmail.GetComponent<TMP_InputField>().enabled = false;
        } else
        {
            restorePasswort.GetComponent<TMP_InputField>().enabled = false;
            restoreCode.GetComponent<TMP_InputField>().enabled = false;
        }

        StartCoroutine(StartRestore(email, passwort, code));
    }

    IEnumerator StartRestore(string email, string passwort, string code)
    {
        passwort = Md5Sum(passwort);

        string tempName = tempNames[Random.Range(0, tempNames.Length)];

        string authHash = Md5Sum(tempName + authKey);

        if (restoreStep == 0)
        { //email
            string url = "https://bruh.games/manager.php?restore=0&email=" +
                email + "&name=" + tempName + "&hash=" + authHash;

            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                restoreInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
                restoreInfo.GetComponent<TextMeshProUGUI>().text = connectionFailedString;
            }
            else
            {
                restoreStep = 1;

                restorePasswort.transform.parent.gameObject.SetActive(true);
                restoreCode.transform.parent.gameObject.SetActive(true);

                restoreInfo.GetComponent<TextMeshProUGUI>().color = Color.blue;
                restoreInfo.GetComponent<TextMeshProUGUI>().text = lookInSpamString;

                restoreButton.GetComponent<TextMeshProUGUI>().text = resetPasswordString;
            }
        }
        else
        { //code
            string url = "https://bruh.games/manager.php?restore=1&email=" +
                email + "&code=" + code + "&passwort=" + passwort + "&name=" + tempName + "&hash=" + authHash;

            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.isNetworkError || www.isHttpError)
            {
                restoreInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
                restoreInfo.GetComponent<TextMeshProUGUI>().text = connectionFailedString;
            } else
            {
                string response = www.downloadHandler.text;

                string[] split = response.Split('#');

                if(split.Length == 2)
                {
                    if (split[0].Equals("0"))
                    { //failed
                        restoreInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
                        restoreInfo.GetComponent<TextMeshProUGUI>().text = invalidCodeString;
                    }
                    else if (split[0].Equals("1"))
                    { //success
                        string name = split[1];
                        this.username = name;

                        EndLoginStart();
                    }
                } else
                {
                    restoreInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
                    restoreInfo.GetComponent<TextMeshProUGUI>().text = connectionFailedString;
                }
            }
        }

        running = false;
    }

    public void AlphaClicked()
    {
        if (running) return;
        running = true;

        string originalName = ObscuredPrefs.GetString("Player_Username");
        string newName = alphaUsername.GetComponent<TMP_InputField>().text;
        string passwort = alphaPasswort.GetComponent<TMP_InputField>().text;
        string email = alphaEmail.GetComponent<TMP_InputField>().text;

        if(!CheckUsername(newName))
        {
            alphaInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            alphaInfo.GetComponent<TextMeshProUGUI>().text = invalidNameString + "\n" +
                                                            "(a-z A-Z 0-9 .,-_)\n" +
                                                            "2-20 " + lengthString;
            return;
        }

        if(!CheckPasswort(passwort))
        {
            alphaInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            alphaInfo.GetComponent<TextMeshProUGUI>().text = invalidPasswordString + "\n" +
                                                            "(a-z A-Z 0-9 .,-_)\n" +
                                                            "4-20 " + lengthString;
            return;
        }

        if(!CheckEmail(email))
        {
            alphaInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            alphaInfo.GetComponent<TextMeshProUGUI>().text = invalidEmailString;

            return;
        }

        StartCoroutine(StartAlphaRegister(originalName, newName, passwort, email));
    }

    IEnumerator StartAlphaRegister(string oldName, string newUser, string passwort, string email)
    {
        passwort = Md5Sum(passwort);

        string authHash = Md5Sum(oldName + authKey);

        string url = "https://bruh.games/manager.php?alpha=1&name=" + oldName +
            "&newName=" + newUser + "&email=" + email + "&passwort=" + passwort +
            "&hash=" + authHash;

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();

        if (www.isNetworkError || www.isHttpError)
        {
            alphaInfo.GetComponent<TextMeshProUGUI>().color = Color.red;
            alphaInfo.GetComponent<TextMeshProUGUI>().text = connectionFailedString;
        } else
        {
            string response = www.downloadHandler.text;

            string[] split = response.Split('#');

            if(split[0].Equals("0"))
            { //fehlgeschlagen
                alphaInfo.GetComponent<TextMeshProUGUI>().color = Color.red;

                if (split[1].Equals("0"))
                { //name bereits vorhanden
                    alphaInfo.GetComponent<TextMeshProUGUI>().text = nameExistsString;
                } else
                {
                    alphaInfo.GetComponent<TextMeshProUGUI>().text = emailRegisteredString;
                }
            } else
            {
                this.username = newUser;
                EndLoginStart();
            }
        }

        running = false;
    }

    private void EndLoginStart()
    {
        accountState = AccountStates.Synced;

        ObscuredPrefs.SetString("Player_Username", username);
        ObscuredPrefs.SetInt("Player_AccountState", (int)accountState);

        scoreHandler.ForceHighscoreStart();
    }
}
