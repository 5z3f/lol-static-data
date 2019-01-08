using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;

namespace Riot.StaticData
{
    static class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            Console.WriteLine("\n\tLeague of Legends Static Data Gathering Tool | 1.0");
            Console.WriteLine("\t_______________________________________________________________________\n\n");

            int tries = 1;
            CheckAgain:
                Process pGame = Process.GetProcessesByName("LeagueClientUx").FirstOrDefault();
                if (pGame == null)
                {
                    if (tries > 0) ClearLastLine();
                    Console.WriteLine($"\tSearching for LeagueClient... ({tries}/5)");
                    Thread.Sleep(3000);

                    if (tries < 5)
                    {
                        tries++;
                        goto CheckAgain;
                    }

                    ClearLastLine();
                    Console.WriteLine($"\tSearching for LeagueClient... FAILED ({tries}/5)");
                    Console.ReadKey();
                    Environment.Exit(0);
                }

            ClearLastLine();
            Console.WriteLine($"\tSearching for LeagueClient...\t\t\t\t\tFOUND\n\t---");
            string LeagueExecutablePath = pGame.MainModule.FileName;

            string LeagueRootDir = Path.GetFullPath(Path.Combine(LeagueExecutablePath, @"..\..\..\..\..\..\..\"));
            string[] lockFileData = null;

            using (FileStream fileStream = File.Open(LeagueRootDir + "/lockfile", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (StreamReader streamReader = new StreamReader(fileStream))
                while (!streamReader.EndOfStream)
                    lockFileData = streamReader.ReadToEnd().Split(':');

            Console.WriteLine($"\tWould you like to download assets? [y/N]");
            Console.Write("\t>> ");
            bool dlAssets = (Console.ReadLine() == "y") ? true : false;
            ClearLastLine(); ClearLastLine();

            string locale = GetRequest("/riotclient/region-locale", lockFileData[3], lockFileData[2])["locale"];
            Console.WriteLine($"\tGetting client locale...\t\t\t\t\tOK");
            string gameBranch = GetRequest("/system/v1/builds", lockFileData[3], lockFileData[2])["gameBranch"];
            Console.WriteLine($"\tGetting current game branch...\t\t\t\t\tOK\n\t---");

            bool isPatched = GetRequest("/patcher/v1/products/league_of_legends/state", lockFileData[3], lockFileData[2])["percentPatched"] == (double)100.0 ? true : false;
            if (!isPatched)
            {
                Console.WriteLine($"\tERROR: Please update your LeagueClient first");
                Console.ReadKey();
                Environment.Exit(0);
            }

            dynamic championSummary = GetRequest($"/lol-game-data/assets/v1/champion-summary.json", lockFileData[3], lockFileData[2]);
            Console.WriteLine($"\tGetting champion summary...\t\t\t\t\tOK");

            dynamic championAdditionalInfo = GetRequest("https://universe-meeps.leagueoflegends.com/v1/en_us/champion-browse/index.json", lockFileData[3], lockFileData[2], false);
            Console.WriteLine($"\tGetting champion additional info...\t\t\t\tOK");

            // dynamic skinLines = GetRequest($"/lol-game-data/assets/v1/skinlines.json", lockFileData[3], lockFileData[2]);
            // Console.WriteLine($"\tGetting skin lines...\t\t\t\t\t\tOK");

            // dynamic skinUniverses = GetRequest($"/lol-game-data/assets/v1/universes.json", lockFileData[3], lockFileData[2]);
            // Console.WriteLine($"\tGetting skin universes...\t\t\t\t\tOK");

            dynamic iconCollection = GetRequest($"/lol-game-data/assets/v1/summoner-icons.json", lockFileData[3], lockFileData[2]);
            Console.WriteLine($"\tGetting summoner icons...\t\t\t\t\tOK");

            dynamic iconSets = GetRequest($"/lol-game-data/assets/v1/summoner-icon-sets.json", lockFileData[3], lockFileData[2]);
            Console.WriteLine($"\tGetting summoner icon sets...\t\t\t\t\tOK");

            dynamic wardCollection = GetRequest($"/lol-game-data/assets/v1/ward-skins.json", lockFileData[3], lockFileData[2]);
            Console.WriteLine($"\tGetting ward skins...\t\t\t\t\t\tOK");

            dynamic wardSets = GetRequest($"/lol-game-data/assets/v1/ward-skin-sets.json", lockFileData[3], lockFileData[2]);
            Console.WriteLine($"\tGetting ward skin sets...\t\t\t\t\tOK");

            dynamic emoteCollection = GetRequest($"/lol-game-data/assets/v1/summoner-emotes.json", lockFileData[3], lockFileData[2]);
            Console.WriteLine($"\tGetting summoner emotes...\t\t\t\t\tOK\n\t---");

            #region WadTesting
            /*
            string globalFileName = @"C:\Riot Games\League of Legends\RADS\projects\lol_game_client_" + locale + @"\managedfiles\" + new DirectoryInfo(@"C:\Riot Games\League of Legends\RADS\projects\lol_game_client_en_gb\managedfiles").GetDirectories().Last() + @"\DATA\FINAL\Localized\Global." + locale + ".wad.client";
            long globalFileLength = new FileInfo(globalFileName).Length;

            Console.WriteLine($"\tUnpacking Global.{locale}.wad.client...\t\t\t\tPROCESSING");
            var globalFileUnpackedContent = Encoding.UTF8.GetString(RiotWad.UnpackGlobalFile(globalFileName));
            ClearLastLine();
            Console.WriteLine($"\tUnpacking Global.{locale}.wad.client...\t\t\t\tOK");
            Console.WriteLine($"\tUnpacked { BytesToString(globalFileUnpackedContent.Length) } out of { BytesToString(globalFileLength) } file (+{BytesToString(globalFileUnpackedContent.Length - globalFileLength)})\t\t\tOK");

            Console.WriteLine($"\tParsing content...\t\t\t\t\tWORKING");

            var chromaDescriptionData = new Dictionary<long, string>();

            {
                using (var reader = new StringReader(globalFileUnpackedContent))
                {
                    for (string line = reader.ReadLine(); line != null; line = reader.ReadLine())
                        if (line.StartsWith("tr \"chroma_description"))
                            chromaDescriptionData.Add(Convert.ToInt64(GetStringBetween(line, "tr \"chroma_description_", "\"")), GetStringBetween(line, "\" = \"", "\""));
                }
            }

            ClearLastLine();
            Console.WriteLine($"\tParsing unpacked content...\t\t\t\t\tOK\n\t---");
            */
            #endregion

            Console.WriteLine(dlAssets ? "\tDownloading assets and building champion structure...\t\tWORKING" : "\tBuilding champion structure...\t\t\t\t\tWORKING");

            int skinCount = 0;
            int chromaCount = 0;

            dynamic ExportData = new ExpandoObject();
            ExportData.champions = new ExpandoObject();
            ExportData.champions = new dynamic[championSummary.Count];

            for (int i = 0; i < championSummary.Count; i++)
            {
                dynamic championData = GetRequest($"/lol-game-data/assets/v1/champions/{championSummary[i == 0 ? 1 : i]["id"]}.json", lockFileData[3], lockFileData[2]);

                ExportData.champions[i] = new ExpandoObject();
                ExportData.champions[i].id = (ulong)championData["id"];
                ExportData.champions[i].name = championData["name"];
                ExportData.champions[i].alias = championData["alias"];
                ExportData.champions[i].title = championData["title"];

                for (int j = 0; j < championAdditionalInfo["champions"].Count; j++)
                {
                    if (championAdditionalInfo["champions"][j]["slug"] == championData["alias"])
                    {
                        ExportData.champions[i].releaseDate = championAdditionalInfo["champions"][j]["release-date"];
                        ExportData.champions[i].associatedFactionSlug = championAdditionalInfo["champions"][j]["associated-faction-slug"];
                    }
                }

                ExportData.champions[i].shortBio = championData["shortBio"];

                string squarePortraitPath = "/assets/champions/portraits/" + Path.GetFileName((string)championData["squarePortraitPath"]);
                ExportData.champions[i].squarePortraitPath = squarePortraitPath;

                if (dlAssets)
                {
                    CreateIfMissing(Environment.CurrentDirectory + squarePortraitPath);

                    if (i > 0)
                    {
                        GetRequest((string)championData["squarePortraitPath"], lockFileData[3], lockFileData[2], true, false, true, Environment.CurrentDirectory + squarePortraitPath);
                        // bitmaps.Add(squarePortraitPath);
                    }
                }

                ExportData.champions[i].roles = championData["roles"];

                ExportData.champions[i].skins = new dynamic[championData["skins"].Count];

                for (int j = 0; j < championData["skins"].Count; j++)
                {
                    skinCount++;

                    ExportData.champions[i].skins[j] = new ExpandoObject();
                    ExportData.champions[i].skins[j].id = (long)championData["skins"][j]["id"];
                    ExportData.champions[i].skins[j].name = (championData["skins"][j]["name"] == championData["name"]) ? championData["skins"][j]["name"] + " (default)" : championData["skins"][j]["name"];
                    ExportData.champions[i].skins[j].description = championData["skins"][j]["description"];
                    ExportData.champions[i].skins[j].rarity = championData["skins"][j]["rarity"];
                    ExportData.champions[i].skins[j].isLegacy = (bool)championData["skins"][j]["isLegacy"];

                    string tilePath = "/assets/champions/tiles/" + Path.GetFileName((string)championData["skins"][j]["tilePath"]);
                    ExportData.champions[i].skins[j].tilePath = tilePath;

                    if (dlAssets)
                    {
                        CreateIfMissing(Environment.CurrentDirectory + tilePath);
                        GetRequest((string)championData["skins"][j]["tilePath"], lockFileData[3], lockFileData[2], true, false, true, Environment.CurrentDirectory + tilePath);
                    }

                    if (championData["skins"][j]["chromas"] != null)
                    {
                        ExportData.champions[i].skins[j].chromas = new dynamic[championData["skins"][j]["chromas"].Count];

                        for (int k = 0; k < championData["skins"][j]["chromas"].Count; k++)
                        {
                            chromaCount++;

                            ExportData.champions[i].skins[j].chromas[k] = new ExpandoObject();
                            ExportData.champions[i].skins[j].chromas[k].id = (long)championData["skins"][j]["chromas"][k]["id"];
                            ExportData.champions[i].skins[j].chromas[k].name = championData["skins"][j]["chromas"][k]["name"];
                            ExportData.champions[i].skins[j].chromas[k].description = null;

                            for (int l = 0; l < championData["skins"][j]["chromas"][k]["descriptions"].Count; l++)
                                if ((string)championData["skins"][j]["chromas"][k]["descriptions"][l]["region"] == "riot")
                                    ExportData.champions[i].skins[j].chromas[k].description = championData["skins"][j]["chromas"][k]["descriptions"][l]["description"];

                            ExportData.champions[i].skins[j].chromas[k].colors = championData["skins"][j]["chromas"][k]["colors"];
                        }
                    }
                    else
                        ExportData.champions[i].skins[j].chromas = null;
                
                }
            }

            ClearLastLine();
            Console.WriteLine(dlAssets ? "\tDownloading assets and building champion structure...\t\tOK" : "\tBuilding champion structure...\t\t\t\t\tOK");

            ExportData.icons = new ExpandoObject();
            ExportData.icons = new dynamic[iconCollection.Count];

            Console.WriteLine(dlAssets ? "\tDownloading assets and building icon structure...\t\tWORKING" : "\tBuilding icon structure...\t\t\t\t\tWORKING");

            for (int i = 0; i < iconCollection.Count; i++)
            {
                ExportData.icons[i] = new ExpandoObject();
                ExportData.icons[i].id = (long)iconCollection[i]["id"];
                ExportData.icons[i].title = iconCollection[i]["title"];
                ExportData.icons[i].description = null;
                ExportData.icons[i].set = null;

                for (int j = 0; j < iconCollection[i]["descriptions"].Count; j++)
                    if ((string)iconCollection[i]["descriptions"][j]["region"] == "riot")
                        ExportData.icons[i].description = iconCollection[i]["descriptions"][j]["description"];

                for (int k = 0; k < iconSets.Count; k++)
                {
                    for (int l = 0; l < iconSets[k]["icons"].Count; l++)
                        if((long)iconCollection[i]["id"] == (long)iconSets[k]["icons"][l])
                            ExportData.icons[i].set = iconSets[k]["displayName"];
                }

                ExportData.icons[i].yearReleased = (int)iconCollection[i]["yearReleased"];

                for (int j = 0; j < iconCollection[i]["rarities"].Count; j++)
                    if ((string)iconCollection[i]["rarities"][j]["region"] == "riot")
                        ExportData.icons[i].rarity = GetRarity((int)iconCollection[i]["rarities"][j]["rarity"]);

                ExportData.icons[i].isLegacy = (bool)iconCollection[i]["isLegacy"];

                string iconPath = "/assets/icons/" + Path.GetFileName((string)iconCollection[i]["imagePath"]);
                ExportData.icons[i].imagePath = iconPath;

                if (dlAssets)
                {
                    CreateIfMissing(Environment.CurrentDirectory + iconPath);
                    GetRequest((string)iconCollection[i]["imagePath"], lockFileData[3], lockFileData[2], true, false, true, Environment.CurrentDirectory + iconPath);
                }
            }

            ClearLastLine();
            Console.WriteLine(dlAssets ? "\tDownloading assets and building icon structure...\t\tOK" : "\tBuilding icon structure...\t\t\t\t\tOK");


            ExportData.wards = new ExpandoObject();
            ExportData.wards = new dynamic[wardCollection.Count];

            Console.WriteLine(dlAssets ? "\tDownloading assets and building ward structure...\t\tWORKING" : "\tBuilding ward structure...\t\t\t\t\tWORKING");

            for (int i = 0; i < wardCollection.Count; i++)
            {
                ExportData.wards[i] = new ExpandoObject();
                ExportData.wards[i].id = (long)wardCollection[i]["id"];
                ExportData.wards[i].name = wardCollection[i]["name"];
                ExportData.wards[i].defaultDescription = wardCollection[i]["description"] > 0 ? wardCollection[i]["description"] : null;
                ExportData.wards[i].description = null;
                ExportData.wards[i].set = null;

                for (int j = 0; j < wardCollection[i]["regionalDescriptions"].Count; j++)
                    if ((string)wardCollection[i]["regionalDescriptions"][j]["region"] == "riot")
                        ExportData.wards[i].description = wardCollection[i]["regionalDescriptions"][j]["description"];

                for (int k = 0; k < wardSets.Count; k++)
                {
                    for (int l = 0; l < wardSets[k]["wards"].Count; l++)
                        if ((long)wardCollection[i]["id"] == (long)wardSets[k]["wards"][l])
                            ExportData.wards[i].set = wardSets[k]["displayName"];
                }

                for (int m = 0; m < wardCollection[i]["rarities"].Count; m++)
                    if ((string)wardCollection[i]["rarities"][m]["region"] == "riot")
                        ExportData.wards[i].rarity = GetRarity((int)wardCollection[i]["rarities"][m]["rarity"]);

                ExportData.wards[i].isLegacy = (bool)wardCollection[i]["isLegacy"];

                string wardPath = $"/assets/wards/{(long)wardCollection[i]["id"]}.png";
                ExportData.wards[i].imagePath = wardPath;

                if (dlAssets)
                {
                    CreateIfMissing(Environment.CurrentDirectory + wardPath);
                    GetRequest((string)wardCollection[i]["wardImagePath"], lockFileData[3], lockFileData[2], true, false, true, Environment.CurrentDirectory + wardPath);
                }
            }

            ClearLastLine();
            Console.WriteLine(dlAssets ? "\tDownloading assets and building ward structure...\t\tOK" : "\tBuilding ward structure...\t\t\t\t\tOK");

            ExportData.emotes = new ExpandoObject();
            ExportData.emotes = new dynamic[emoteCollection.Count];

            Console.WriteLine(dlAssets ? "\tDownloading assets and building emote structure...\t\tWORKING" : "\tBuilding emote structure...\t\t\t\t\tWORKING");

            for (int i = 0; i < emoteCollection.Count; i++)
            {
                ExportData.emotes[i] = new ExpandoObject();
                ExportData.emotes[i].id = (long)emoteCollection[i]["id"];
                ExportData.emotes[i].name = emoteCollection[i]["name"];
                ExportData.emotes[i].description = emoteCollection[i]["description"] > 0 ? emoteCollection[i]["description"] : null;

                string emotePath = $"/assets/emotes/{(long)emoteCollection[i]["id"]}.png";
                ExportData.emotes[i].imagePath = emotePath.Contains(".png") ? emotePath : null;

                if (dlAssets)
                {
                    string apiPath = (string)emoteCollection[i]["inventoryIcon"];
                    if (apiPath.Contains(".png"))
                    {
                        CreateIfMissing(Environment.CurrentDirectory + emotePath);
                        GetRequest((string)emoteCollection[i]["inventoryIcon"], lockFileData[3], lockFileData[2], true, false, true, Environment.CurrentDirectory + emotePath);
                    }
                }
            }

            ClearLastLine();
            Console.WriteLine(dlAssets ? "\tDownloading assets and building emote structure...\t\tOK\n\t---" : "\tBuilding emote structure...\t\t\t\t\tOK\n\t---");
            Console.WriteLine($"\tSaved to static-data.{locale}.json \t\t\t\tDONE");

            dynamic ExportHeader = new ExpandoObject();
            ExportHeader.version = Convert.ToDouble(gameBranch);
            ExportHeader.locale = locale;
            string[] contentArray = { "CHAMPIONS", "SKINS", "CHROMAS", "ICONS", "WARDS", "EMOTES" };
            ExportHeader.content = contentArray;

            ExportHeader.info = new ExpandoObject();
            ExportHeader.info.champions = championSummary.Count - 1;
            ExportHeader.info.skins = skinCount - (championSummary.Count - 1);
            ExportHeader.info.chromas = chromaCount;
            ExportHeader.info.icons = iconCollection.Count;
            ExportHeader.info.emotes = emoteCollection.Count - 1;
            ExportHeader.info.wards = wardCollection.Count;

            ExportHeader.data = ExportData;

            string output = JsonConvert.SerializeObject(ExportHeader, Formatting.Indented);
            File.WriteAllText($"static-data.{locale}.json", output, Encoding.UTF8);

            // Bitmap portraitCombined = CombineBitmap(bitmaps.ToArray());
            // portraitCombined.Save(Environment.CurrentDirectory +  @"/assets/champions/portraitCombined.bmp", ImageFormat.Bmp);

            Console.ReadKey();
        }

        public static dynamic GetRequest(string url, string password = null, string port = null, bool toLeagueAPI = true, bool deserialize = true, bool binary = false, string fileName = null)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            WebClient webClient = new WebClient();
            webClient.Headers[HttpRequestHeader.Accept] = "application/json";
            webClient.Headers[HttpRequestHeader.UserAgent] = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.110 Safari/537.36";
            if (toLeagueAPI) webClient.Headers[HttpRequestHeader.Authorization] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes($"riot:{password}"));

            try
            {
                string uri = webClient.DownloadString(toLeagueAPI ? $"https://127.0.0.1:{port}{url}" : url);

                if (binary)
                    webClient.DownloadFile(toLeagueAPI ? $"https://127.0.0.1:{port}{url}" : url, fileName);
                else
                    return deserialize ? JsonConvert.DeserializeObject(uri) : url;

                return true;
            }
            catch
            {
                return false;
            }
        }

        static string GetRarity(int rarity)
        {
            string strRarity = null;
            switch (rarity)
            {
                case 0: strRarity = "kNoRarity"; break;
                case 1: strRarity = "kRare"; break;
                case 2: strRarity = "kEpic"; break;
                case 4: strRarity = "kMythic"; break;
                case 5: strRarity = "kUltimate"; break;
            }

            return strRarity;
        }

        static string GetStringBetween(this string token, string first, string second)
        {
            if (!token.Contains(first)) return "";
            var afterFirst = token.Split(new[] { first }, StringSplitOptions.None)[1];
            if (!afterFirst.Contains(second)) return "";
            var result = afterFirst.Split(new[] { second }, StringSplitOptions.None)[0];
            return result;
        }

        public static void CreateIfMissing(string path)
        {
            bool folderExists = Directory.Exists(Path.GetDirectoryName(path));
            if (!folderExists) Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        public static Bitmap CombineBitmap(string[] files)
        {
            List<Bitmap> images = new List<Bitmap>();
            Bitmap finalImage = null;

            try
            {
                int width = 0;
                int height = 0;

                foreach (string image in files)
                {
                    Bitmap bitmap = new Bitmap(image);

                    width += bitmap.Width;
                    height = bitmap.Height > height ? bitmap.Height : height;

                    images.Add(bitmap);
                }

                finalImage = new Bitmap(width, height);

                using (Graphics g = Graphics.FromImage(finalImage))
                {
                    g.Clear(Color.Black);

                    int offset = 0;
                    foreach (Bitmap image in images)
                    {
                        g.DrawImage(image, new Rectangle(offset, 0, image.Width, image.Height));
                        offset += image.Width;
                    }
                }

                return finalImage;
            }
            catch (Exception)
            {
                if (finalImage != null) finalImage.Dispose();
                throw;
            }
            finally
            {
                foreach (Bitmap image in images)
                    image.Dispose();
            }
        }

        public static void ClearLastLine()
        {
            Console.SetCursorPosition(0, Console.CursorTop - 1);
            int currentLineCursor = Console.CursorTop;
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, currentLineCursor);
        }

        static string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };

            if (byteCount == 0)
                return "0" + suf[0];

            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 2);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

    }
}
