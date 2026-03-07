using System.Diagnostics;
using System.IO;
using System.Xml;

namespace LauncherV3.LauncherHelper;

internal static class CrpgHashMethods
{
    public static void WriteToConsole(string text)
    {
#if Launcher_Gui
        MainWindow.Instance?.WriteToConsole(text);
#else
        Console.WriteLine(text);
#endif
    }

    public static string ReadHash(XmlDocument doc, Dictionary<string, string> assets, Dictionary<string, string> maps)
    {
        foreach (var node in doc!.DocumentElement!.ChildNodes.Cast<XmlNode>().ToArray())
        {
            if (node.Name == "Assets")
            {
                foreach (var node1 in node.ChildNodes.Cast<XmlNode>().ToArray())
                {
                    assets[node1!.Attributes!["Name"]!.Value] = node1!.Attributes!["Hash"]!.Value;
                }
            }

            if (node.Name == "Maps")
            {
                foreach (var node1 in node.ChildNodes.Cast<XmlNode>().ToArray())
                {
                    maps[node1!.Attributes!["Name"]!.Value] = node1!.Attributes!["Hash"]!.Value;
                }
            }

            if (node.Name == "Rest")
            {
                return node!.Attributes!["Hash"]!.Value;
            }
        }

        return string.Empty;
    }

    public static async Task VerifyGameFiles(string bannerlordPath, string outputFolderPath, string filename)
    {
        WriteToConsole($"Verifying Game Files now");
        Stopwatch stopwatch = new(); // Create a Stopwatch instance

        if (Directory.Exists(bannerlordPath))
        {
            stopwatch.Start(); // Start the timing
            var xmlDoc = await GenerateCrpgFolderHashMap(Path.Combine(bannerlordPath, "Modules/cRPG"));
            stopwatch.Stop(); // Stop the timing
            if (!Directory.Exists(outputFolderPath))
            {
                Directory.CreateDirectory(outputFolderPath);
            }

            xmlDoc.Save(Path.Combine(outputFolderPath, filename));
            WriteToConsole($"Execution Time: {stopwatch.ElapsedMilliseconds} ms");
        }
        else
        {
            WriteToConsole("Please specify the bannerlord folder location before");
        }
    }

    public static async Task<XmlDocument> GenerateCrpgFolderHashMap(string path)
    {
        XmlDocument document = new();
        var root = document.CreateElement("CrpgHashMap");
        document.AppendChild(root);
        if (!Directory.Exists(path))
        {
            WriteToConsole($"cRPG is not installed at {path}");
            return document;
        }

        string[] folders = Directory.GetDirectories(path);
        string[] topFiles = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly);
        ulong restHash = 0;
        foreach (string folder in folders)
        {
            string folderName = Path.GetFileName(folder);
            if (folderName == "AssetPackages")
            {
                await GenerateCrpgAssetsHashMap(folder, document);
            }
            else if (folderName == "SceneObj")
            {
                await GenerateCrpgSceneObjHashMap(folder, document);
            }
            else
            {
                restHash = await HashFolder(folder) ^ restHash;
            }
        }

        foreach (string file in topFiles)
        {
            {
                restHash = await HashFile(file, false) ^ restHash;
            }
        }

        var restNode = document.CreateElement("Rest");
        restNode.SetAttribute("Name", "Rest");
        restNode.SetAttribute("Hash", restHash.ToString());

        document.DocumentElement!.AppendChild(restNode);
        return document;
    }

    public static async Task GenerateCrpgSceneObjHashMap(string sceneObjPath, XmlDocument doc)
    {
        var mapsNode = doc.CreateElement("Maps");
        string[] mapFolders = Directory.GetDirectories(sceneObjPath);

        foreach (string map in mapFolders)
        {
            // Await the HashFolder method for each folder
            ulong hashResult = await HashFolder(map);

            // Create the map element
            var mapElement = doc.CreateElement("Map");
            mapElement.SetAttribute("Name", Path.GetFileName(map));
            mapElement.SetAttribute("Hash", hashResult.ToString());

            // Append the map element to the mapsNode
            mapsNode.AppendChild(mapElement);
        }

        doc.DocumentElement!.AppendChild(mapsNode);
    }

    public static async Task GenerateCrpgAssetsHashMap(string assetsPath, XmlDocument doc)
    {
        var assetsNode = doc.CreateElement("Assets");
        string[] assetFiles = Directory.GetFiles(assetsPath);

        var assetHashTasks = assetFiles.Select(file =>
            HashFile(file, true).ContinueWith(t =>
            {
                var assetElement = doc.CreateElement("Asset");
                assetElement.SetAttribute("Name", Path.GetFileName(file));
                assetElement.SetAttribute("Hash", t.Result.ToString());
                return assetElement;
            }));

        var assetElements = await Task.WhenAll(assetHashTasks);
        foreach (var assetElement in assetElements)
        {
            assetsNode.AppendChild(assetElement);
        }

        doc.DocumentElement!.AppendChild(assetsNode);
    }

    public static async Task<ulong> HashFolder(string folderPath)
    {
        string[] allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
        List<Task<ulong>> hashTasks = new();
        foreach (string file in allFiles)
        {
            hashTasks.Add(HashFile(file, false));
        }

        ulong[] hashResults = await Task.WhenAll(hashTasks);
        WriteToConsole($"Hashed {Path.GetFileName(folderPath)}");
        ulong hash = 0;
        foreach (ulong fileHash in hashResults)
        {
            hash ^= fileHash;
        }

        return hash;
    }

    public static async Task<ulong> HashFile(string filePath, bool writeToConsole)
    {
        ulong fileHash;
        if (writeToConsole)
        {
            WriteToConsole($"Hashing {Path.GetFileName(filePath)}");
        }

        using (FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, useAsync: true))
        {
            fileHash = await XXHash.xxHash64.ComputeHashAsync(stream);
        }

        return fileHash;
    }
}
