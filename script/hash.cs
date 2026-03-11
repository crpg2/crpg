#:package System.IO.Hashing@10.0.3

using System.IO.Hashing;
using System.Xml;

if (args.Length < 4)
{
    Console.WriteLine("Usage: dotnet run hash.cs -- --hash <folderPath> --output <outputPath>");
    return 1;
}

string? hashFolderPath = null;
string? hashOutputPath = null;

for (int i = 0; i < args.Length; i++)
{
    if (args[i] == "--hash" && i + 1 < args.Length)
    {
        hashFolderPath = args[i + 1];
    }

    if (args[i] == "--output" && i + 1 < args.Length)
    {
        hashOutputPath = args[i + 1];
    }
}

if (hashFolderPath == null || hashOutputPath == null)
{
    Console.WriteLine("Usage: dotnet run hash.cs -- --hash <folderPath> --output <outputPath>");
    return 1;
}

Console.WriteLine($"Hashing now {hashFolderPath}, it should take less than 60s");
var hashXml = await GenerateCrpgFolderHashMap(hashFolderPath);
hashXml.Save(hashOutputPath);
Console.WriteLine($"Hashing done, hash can be found at {hashOutputPath}");
return 0;

static async Task<ulong> HashFile(string filePath)
{
    XxHash64 hasher = new();
    await using (FileStream stream = new(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true))
    {
        await hasher.AppendAsync(stream);
    }

    return hasher.GetCurrentHashAsUInt64();
}

static async Task<ulong> HashFolder(string folderPath)
{
    string[] allFiles = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
    ulong[] hashResults = await Task.WhenAll(allFiles.Select(f => HashFile(f)));
    Console.WriteLine($"Hashed {Path.GetFileName(folderPath)}");
    ulong hash = 0;
    foreach (ulong fileHash in hashResults)
    {
        hash ^= fileHash;
    }

    return hash;
}

static async Task<XmlDocument> GenerateCrpgFolderHashMap(string path)
{
    XmlDocument document = new();
    var root = document.CreateElement("CrpgHashMap");
    document.AppendChild(root);
    if (!Directory.Exists(path))
    {
        Console.WriteLine($"cRPG is not installed at {path}");
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
            restHash ^= await HashFolder(folder);
        }
    }

    foreach (string file in topFiles)
    {
        restHash ^= await HashFile(file);
    }

    var restNode = document.CreateElement("Rest");
    restNode.SetAttribute("Name", "Rest");
    restNode.SetAttribute("Hash", restHash.ToString());
    document.DocumentElement!.AppendChild(restNode);
    return document;
}

static async Task GenerateCrpgAssetsHashMap(string assetsPath, XmlDocument doc)
{
    string[] assetFiles = Directory.GetFiles(assetsPath);

    // Hash all files in parallel, then build XML sequentially (XmlDocument is not thread-safe).
    var hashes = await Task.WhenAll(assetFiles.Select(async file =>
    {
        Console.WriteLine($"Hashing {Path.GetFileName(file)}");
        ulong hash = await HashFile(file);
        return (Name: Path.GetFileName(file), Hash: hash);
    }));

    var assetsNode = doc.CreateElement("Assets");
    foreach (var (name, hash) in hashes)
    {
        var assetElement = doc.CreateElement("Asset");
        assetElement.SetAttribute("Name", name);
        assetElement.SetAttribute("Hash", hash.ToString());
        assetsNode.AppendChild(assetElement);
    }

    doc.DocumentElement!.AppendChild(assetsNode);
}

static async Task GenerateCrpgSceneObjHashMap(string sceneObjPath, XmlDocument doc)
{
    string[] mapFolders = Directory.GetDirectories(sceneObjPath);

    var hashes = await Task.WhenAll(mapFolders.Select(async map =>
    {
        ulong hash = await HashFolder(map);
        return (Name: Path.GetFileName(map), Hash: hash);
    }));

    var mapsNode = doc.CreateElement("Maps");
    foreach (var (name, hash) in hashes)
    {
        var mapElement = doc.CreateElement("Map");
        mapElement.SetAttribute("Name", name);
        mapElement.SetAttribute("Hash", hash.ToString());
        mapsNode.AppendChild(mapElement);
    }

    doc.DocumentElement!.AppendChild(mapsNode);
}
