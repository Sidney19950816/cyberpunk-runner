using System;
using System.IO;
using UnityEngine;

public class FileDataHandler
{
    private string dataDirPath = String.Empty;
    private string dataFileName = String.Empty;

    private bool useEncryption = false;

    private readonly string encryptionCodeWord = "fooplix";

    public FileDataHandler(string dataDirPath, string dataFileName, bool useEncryption)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEncryption;
    }

    public GameData Load()
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        GameData loadedData = null;
        if(File.Exists(fullPath))
        {
            try
            {
                // Load the serialized data from the file
                string dataToLoad = String.Empty;
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                if(useEncryption)
                {
                    dataToLoad = EncryptDecrypt(dataToLoad);
                }

                // Deserialize the data from JSON
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occured when trying to load data from file: {fullPath}\n {e}");
            }
        }

        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullPath = Path.Combine(dataDirPath, dataFileName);

        try
        {
            // Create the directory path the file will be written to if it doesn't already exist
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            // Serialize the game data int JSON
            string dataToStore = JsonUtility.ToJson(data, true);

            if(useEncryption)
            {
                dataToStore = EncryptDecrypt(dataToStore);
            }

            // Write the serialized data to the file
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error occured when trying to save data to file: {fullPath}\n {e}");
        }
    }


    // Simple implementation of XOR Encryption method
    private string EncryptDecrypt(string data)
    {
        string modifiedData = String.Empty;

        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] & encryptionCodeWord[i % encryptionCodeWord.Length]);
        }

        return modifiedData;
    }
}
