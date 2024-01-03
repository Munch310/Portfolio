using Newtonsoft.Json;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SaveLoadManager : Singleton<SaveLoadManager>
{
    private const int SaveSlot = 0;
    private byte[] key;
    private byte[] iv;

    protected override void Awake()
    {
        base.Awake();
        InitializeKeyAndIV();
    }

    // 암호화 키와 초기화 벡터 생성 또는 로드
    private void InitializeKeyAndIV()
    {
        if (PlayerPrefs.HasKey("EncryptionKey") && PlayerPrefs.HasKey("EncryptionIV"))
        {
            key = Convert.FromBase64String(PlayerPrefs.GetString("EncryptionKey"));
            iv = Convert.FromBase64String(PlayerPrefs.GetString("EncryptionIV"));
        }
        else
        {
            using (var aesAlg = Aes.Create())
            {
                aesAlg.GenerateKey();
                aesAlg.GenerateIV();
                key = aesAlg.Key;
                iv = aesAlg.IV;

                SaveKeyAndIVToPlayerPrefs(key, iv);
            }
        }
    }

    // 암호화 키와 IV를 PlayerPrefs에 저장
    private void SaveKeyAndIVToPlayerPrefs(byte[] key, byte[] iv)
    {
        PlayerPrefs.SetString("EncryptionKey", Convert.ToBase64String(key));
        PlayerPrefs.SetString("EncryptionIV", Convert.ToBase64String(iv));
        PlayerPrefs.Save();
    }

    // 데이터 저장 : Json으로 직렬화하고, 암호화하여 해시값과 함께 파일에 저장
    public void SaveData(DataManager data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);
        string encrypted = Encrypt(json);
        string hash = ComputeSha256Hash(encrypted);

        string path = GetSaveFilePath();
        File.WriteAllText(path, encrypted);
        File.WriteAllText(path + ".hash", hash);
        Debug.Log(json);
    }

    // 데이터 로드 : 파이렝서 암호화된 데이터와 해시를 읽고, 무결성 검사 후 복호화
    public DataManager LoadData()
    {
        string path = GetSaveFilePath();
        if (File.Exists(path))
        {
            string encrypted = File.ReadAllText(path);
            string hash = File.ReadAllText(path + ".hash");
            string newHash = ComputeSha256Hash(encrypted);

            if (hash != newHash)
            {
                return null;
            }

            string decrypted = Decrypt(encrypted);
            return JsonConvert.DeserializeObject<DataManager>(decrypted);
        }

        return null;
    }

    // 저장된 데이터 삭제
    public void DeleteData()
    {
        string path = GetSaveFilePath();
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        if (File.Exists(path + ".hash"))
        {
            File.Delete(path + ".hash");
        }
    }

    // AES 암호화 알고리즘을 사용하여 암호화
    private string Encrypt(string plainText)
    {
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }
                }
                return Convert.ToBase64String(msEncrypt.ToArray());
            }
        }
    }

    // AES 암호화 알고리즘을 사용하여 주어진 암호문을 복호화
    private string Decrypt(string cipherText)
    {
        string plaintext = null;
        byte[] cipherTextBytes = Convert.FromBase64String(cipherText);
        using (Aes aesAlg = Aes.Create())
        {
            aesAlg.Key = key;
            aesAlg.IV = iv;

            ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);
            using (MemoryStream msDecrypt = new MemoryStream(cipherTextBytes))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        plaintext = srDecrypt.ReadToEnd();
                    }
                }
            }
        }
        return plaintext;
    }

    // Raw 데이터에 대한 SHA256 해시를 계산
    private string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256hash = SHA256.Create())
        {
            byte[] bytes = sha256hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                builder.Append(bytes[i].ToString("x2"));
            }
            return builder.ToString();
        }
    }

    // 실제 데이터가 저장될 파일의 경로를 생성
    private string GetSaveFilePath()
    {
        return Application.persistentDataPath + "/save" + SaveSlot + ".json";
    }
}