﻿using Azure;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace ProjectAssistant.Business.Services.SpeechToText;

#region 定義使用到的類別
// 定義 JSON 結構對應的 POCO
class TranscriptionJson
{
    [JsonProperty("recognizedPhrases")]
    public RecognizedPhrase[] RecognizedPhrases { get; set; }

    [JsonProperty("combinedRecognizedPhrases")]
    public CombinedPhrase[] CombinedRecognizedPhrases { get; set; }
}

class RecognizedPhrase
{
    [JsonProperty("channel")]
    public int Channel { get; set; }

    [JsonProperty("speaker")]
    public int Speaker { get; set; }

    [JsonProperty("offset")]
    public string Offset { get; set; }

    [JsonProperty("duration")]
    public string Duration { get; set; }

    [JsonProperty("offsetInTicks")]
    public long OffsetInTicks { get; set; }

    [JsonProperty("durationInTicks")]
    public long DurationInTicks { get; set; }

    [JsonProperty("nBest")]
    public NBest[] NBest { get; set; }
}

class NBest
{
    [JsonProperty("confidence")]
    public double Confidence { get; set; }

    [JsonProperty("lexical")]
    public string Lexical { get; set; }

    [JsonProperty("itn")]
    public string ITN { get; set; }

    [JsonProperty("maskedITN")]
    public string MaskedITN { get; set; }

    [JsonProperty("display")]
    public string Display { get; set; }

    [JsonProperty("sentiment")]
    public Sentiment Sentiment { get; set; }
}

class Sentiment
{
    [JsonProperty("negative")]
    public double Negative { get; set; }

    [JsonProperty("neutral")]
    public double Neutral { get; set; }

    [JsonProperty("positive")]
    public double Positive { get; set; }
}

class CombinedPhrase
{
    [JsonProperty("channel")]
    public int Channel { get; set; }

    [JsonProperty("display")]
    public string Display { get; set; }
}
#endregion

public class SpeechToTextService
{
    private readonly ILogger<SpeechToTextService> logger;
    private readonly FFmpegDownloader fFmpegDownloader;
    private readonly ConverAudioHelper converAudioHelper;
    List<string> audioFiles = new List<string>();
    public SpeechToTextService(ILogger<SpeechToTextService> logger,
        FFmpegDownloader fFmpegDownloader, ConverAudioHelper converAudioHelper)
    {
        this.logger = logger;
        this.fFmpegDownloader = fFmpegDownloader;
        this.converAudioHelper = converAudioHelper;
    }

    public async Task InitializeAsync()
    {
        // 確保 FFmpeg 已經下載
        await fFmpegDownloader.DownloadFFmpegAsync();
        logger.LogInformation("FFMpeg檔案已準備好了");
    }

    public async Task ConvertToWavAsync()
    {
        // 確保音訊檔案已經轉換為 WAV 格式
        await converAudioHelper.ConvertToWav();
        logger.LogInformation("音訊檔案已轉換為 WAV 格式");

    }

    public async Task ConvertToMp3Async()
    {
        // 確保音訊檔案已經轉換為 MP3 格式
        await converAudioHelper.ConvertToMp3();
        logger.LogInformation("音訊檔案已轉換為 MP3 格式");

    }

    public async Task BuildAsync(string audioFileType)
    {
        string currentDirectory = Directory.GetCurrentDirectory();
        audioFiles = Directory.GetFiles(currentDirectory, $"*.{audioFileType}").ToList();
        if (audioFiles.Count == 0)
        {
            logger.LogError($"沒有找到音檔，請確認當前目錄是否有 {audioFileType} 檔案");
            return;
        }
        else
        {
            foreach (var file in audioFiles)
            {
                logger.LogInformation("找到音檔：{0}", file);
                string fileItem = Path.GetFileName(file);
                string filename = Path.Combine(currentDirectory, fileItem);
                string textScript = await ProcessAsync(filename, audioFileType);
                logger.LogInformation("語音文稿解析完成 ： {0}", fileItem);
            }
        }

    }

    public async Task<string> ProcessAsync(string filename, string audioFileType)
    {
        string result = string.Empty;
        // 1. 上傳音檔到 Azure Blob Storage
        string sasToken = await UploadToAzureBlobStorage(filename);
        // 2. 轉錄音檔
        result = await ParseSpeechToText(sasToken, audioFileType);
        await Save(filename, result);
        return result;
    }

    async Task<string> UploadToAzureBlobStorage(string filename)
    {
        string result = string.Empty;
        try
        {
            // 配置 BlobClientOptions 以增加逾時時間和重試策略
            var blobClientOptions = new BlobClientOptions()
            {
                Retry = {
                Delay = TimeSpan.FromSeconds(2),        // 重試間隔
                MaxDelay = TimeSpan.FromSeconds(30),    // 最大重試間隔
                MaxRetries = 5,                         // 最大重試次數
                NetworkTimeout = TimeSpan.FromMinutes(10) // 網路逾時設為 10 分鐘
            }
            };

            // 讀取環境變數內的 Azure Blob Storage 的連線字串
            string connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            string containerName = "audio-files";     // 要上傳到的 container
                                                      // 取得當前目錄
            string currentDirectory = Directory.GetCurrentDirectory();
            string localFilePath = Path.Combine(Directory.GetCurrentDirectory(), filename);
            string blobName = Path.GetFileName(localFilePath); // blob 名稱

            // 取得檔案大小以決定上傳策略
            var fileInfo = new FileInfo(localFilePath);
            long fileSize = fileInfo.Length;
            // --------------------------------

            // 建立 BlobServiceClient (方法 A)
            var blobServiceClient = new BlobServiceClient(connectionString, blobClientOptions);

            // 取得 container client，若 container 不存在則自動建立
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            // 取得指定 blob 的 client
            var blobClient = containerClient.GetBlobClient(blobName);

            logger.LogInformation($"開始上傳 {localFilePath} → {containerClient.Uri}/{blobName} ...");

            // 以檔案串流上傳，並設定 ContentType 以利瀏覽器正確播放
            using FileStream uploadFileStream = File.OpenRead(localFilePath);
            var blobHttpHeaders = new BlobHttpHeaders { ContentType = "audio/mpeg" };

            double lastPercentage = 0.0;

            // 針對大檔案使用分段上傳
            var uploadOptions = new BlobUploadOptions
            {
                HttpHeaders = blobHttpHeaders,
                TransferOptions = new StorageTransferOptions
                {
                    // 設定分段大小 (4MB)
                    MaximumTransferSize = 4 * 1024 * 1024,
                    // 設定平行度
                    MaximumConcurrency = 4
                },
                // 設定進度回報
                ProgressHandler = new Progress<long>(bytesTransferred =>
                {
                    double percentage = (double)bytesTransferred / fileSize * 100;
                    if (percentage - lastPercentage >= 5.0)
                    {
                        logger.LogInformation($"上傳進度: {percentage:F1}% ({bytesTransferred}/{fileSize} bytes)");
                        lastPercentage = percentage;
                    }
                })
            };


            // 如果檔案很大，可以傳入 BlobUploadOptions 並設定 TransferOptions 分段上傳
            await blobClient.UploadAsync(uploadFileStream, uploadOptions);

            uploadFileStream.Close();
            logger.LogInformation("上傳完成！");

            var blobItemClient = containerClient.GetBlobClient(blobName);

            // 直接拿 URL
            Uri blobUri = blobItemClient.Uri;
            Console.WriteLine(blobUri.ToString());

            // 取得與顯示 Blob Storage 的音檔 SAS URI
            var sasToken = blobItemClient.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(5));
            logger.LogInformation($"SAS URI: {sasToken}");

            result = sasToken.ToString();
        }
        catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
        {
            logger.LogError($"上傳逾時: {ex.Message}");
            throw new Exception("檔案上傳逾時，請檢查網路連線或嘗試上傳較小的檔案");
        }
        catch (RequestFailedException ex)
        {
            logger.LogError($"Azure Storage 請求失敗: {ex.Message}");
            throw new Exception($"Azure Storage 操作失敗: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError($"上傳過程中發生錯誤: {ex.Message}");
            throw;
        }
        return result;
    }

    async Task<string> ParseSpeechToText(string sasToken, string audioFileType)
    {
        string result = string.Empty;

        // Speech 服務金鑰與區域
        string SubscriptionKey = Environment.GetEnvironmentVariable("AzureSpeechServiceSubscriptionKey");
        string ServiceRegion = Environment.GetEnvironmentVariable("AzureSpeechServiceRegion");

        // 上傳到 Blob Storage 的音檔 SAS URI
        string AudioFileSasUri = sasToken;

        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);

        // 1. 建立轉錄工作
        var createUrl = $"https://{ServiceRegion}.api.cognitive.microsoft.com/speechtotext/v3.2/transcriptions";

        dynamic createBody = new { };
        if (audioFileType == "mp3")
        {
            createBody = new
            {
                contentUrls = new[] { AudioFileSasUri },
                locale = "zh-TW",
                displayName = "My Batch Transcription",
                description = "含说话人分离的会议内容转录",
                properties = new
                {
                    diarizationEnabled = false, // 是否啟用說話人分離
                    wordLevelTimestampsEnabled = false, // 是否啟用單詞級時間戳
                    punctuationMode = "DictatedAndAutomatic", // 啟用標點符號
                    maxSpeakerCount = 10, // 最大说话人数量
                    addSentiment = true, // 启用情感分析
                    profanityFilterMode = "Masked", // 启用脏话过滤
                                                    // 增加說話人分離的敏感度設定
                                                    // 多语言识别
                    languageIdentification = new
                    {
                        candidateLocales = new[] { "zh-TW", "zh-CN", "en-US" },
                        mode = "Continuous"
                    },

                    // 结果存储
                    timeToLive = "P1D",  // 保留结果1天
                }
            };
        }
        else if (audioFileType == "wav")
        {
            createBody = new
            {
                contentUrls = new[] { AudioFileSasUri },
                locale = "zh-TW",
                displayName = "My Batch Transcription",
                description = "含说话人分离的会议内容转录",
                properties = new
                {
                    diarizationEnabled = true, // 是否啟用說話人分離
                    wordLevelTimestampsEnabled = false, // 是否啟用單詞級時間戳
                    punctuationMode = "DictatedAndAutomatic", // 啟用標點符號
                    maxSpeakerCount = 10, // 最大说话人数量
                    addSentiment = true, // 启用情感分析
                    profanityFilterMode = "Masked", // 启用脏话过滤
                                                    // 增加說話人分離的敏感度設定
                    speechContext = new
                    {
                        phrases = new string[] { } // 可以加入特定詞彙提高識別率
                    },
                    // 多语言识别
                    languageIdentification = new
                    {
                        candidateLocales = new[] { "zh-TW", "zh-CN", "en-US" },
                        mode = "Continuous"
                    },

                    // 结果存储
                    timeToLive = "P1D",  // 保留结果1天
                }
            };
        }

        StringContent jsonContent = new StringContent(JsonConvert.SerializeObject(createBody));
        //if (audioFileType == "mp3")
        //{
        //    jsonContent = new StringContent(JsonConvert.SerializeObject(createBody));
        //}
        //else if (audioFileType == "wav")
        //{
        //    jsonContent = new StringContent(JsonConvert.SerializeObject(createBody));
        //}
        jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        var createResponse = await client.PostAsync(createUrl, jsonContent);
        createResponse.EnsureSuccessStatusCode();

        var createResult = await createResponse.Content.ReadAsStringAsync();

        // 解析 self URL
        dynamic createJson = JsonConvert.DeserializeObject(createResult);
        string transcriptionUrl = createJson.self;
        logger.LogInformation($"查詢轉錄狀態 URL {transcriptionUrl}");

        // 2. 輪詢狀態
        logger.LogInformation("開始輪詢轉錄狀態…");
        TimeSpan elapsedTime;
        DateTime startTime = DateTime.Now;
        List<string> allSpeakers = new();

        while (true)
        {
            elapsedTime = DateTime.Now - startTime;
            // 顯示已經花費時間 小時:分鐘:秒
            logger.LogInformation($"已經花費時間：{elapsedTime.Hours:D2}:{elapsedTime.Minutes:D2}:{elapsedTime.Seconds:D2}");

            var statusResponse = await client.GetAsync(transcriptionUrl);
            statusResponse.EnsureSuccessStatusCode();

            var statusJson = await statusResponse.Content.ReadAsStringAsync();
            dynamic statusObj = JsonConvert.DeserializeObject(statusJson);
            string status = statusObj.status;
            logger.LogInformation($"目前狀態：{status}");

            if (status == "Succeeded")
            {
                // 3. 取得並下載轉錄結果
                string filesUrl = statusObj.links.files;
                var filesResponse = await client.GetAsync(filesUrl);
                filesResponse.EnsureSuccessStatusCode();

                var filesJson = await filesResponse.Content.ReadAsStringAsync();
                dynamic filesObj = JsonConvert.DeserializeObject(filesJson);

                foreach (var file in filesObj.values)
                {
                    if ((string)file.kind == "Transcription")
                    {
                        var fileUrl = (string)file.links.contentUrl;
                        var transcriptionResult = await client.GetStringAsync(fileUrl);
                        var resultObj = JsonConvert.DeserializeObject<TranscriptionJson>(transcriptionResult);

                        // 使用 recognizedPhrases 來取得說話人分離資訊
                        if (resultObj.RecognizedPhrases != null && resultObj.RecognizedPhrases.Length > 0)
                        {
                            var speakerTexts = new List<string>();

                            foreach (var phrase in resultObj.RecognizedPhrases)
                            {
                                if (phrase.NBest != null && phrase.NBest.Length > 0)
                                {
                                    var bestResult = phrase.NBest[0]; // 取最佳結果
                                    var timeOffset = TimeSpan.FromTicks(phrase.OffsetInTicks);
                                    var duration = TimeSpan.FromTicks(phrase.DurationInTicks);

                                    // 增加偵錯日誌
                                    logger.LogInformation($"檢測到說話人 {phrase.Speaker}，置信度: {bestResult.Confidence:P2}");

                                    if (allSpeakers.Contains($"說話人 {phrase.Speaker}") == false)
                                    {
                                        allSpeakers.Add($"說話人 {phrase.Speaker}");
                                        logger.LogInformation($"新增說話人 {phrase.Speaker} 到清單中，目前共有 {allSpeakers.Count} 位說話人");
                                    }

                                    //speakerTexts.Add($"說話人 {phrase.Speaker} [{timeOffset:hh\\:mm\\:ss}-{timeOffset.Add(duration):hh\\:mm\\:ss}]:");
                                    //speakerTexts.Add($"說話人 {phrase.Speaker} [{timeOffset:hh\\:mm\\:ss}-{timeOffset.Add(duration):hh\\:mm\\:ss}]:");
                                    if (audioFileType == "mp3")
                                    {
                                        speakerTexts.Add($"{bestResult.Display?.Trim()}");
                                    }
                                    else if (audioFileType == "wav")
                                    {
                                        speakerTexts.Add($"說話人 {phrase.Speaker} [{timeOffset:hh\\:mm\\:ss}-{timeOffset.Add(duration):hh\\:mm\\:ss}]:");
                                        speakerTexts.Add($"說話人{phrase.Speaker} : {bestResult.Display?.Trim()}");
                                    }

                                    // 如果有情感分析結果，也加入
                                    if (bestResult.Sentiment != null)
                                    {
                                        speakerTexts.Add($"情感分析: 正面({bestResult.Sentiment.Positive:P1}) 中性({bestResult.Sentiment.Neutral:P1}) 負面({bestResult.Sentiment.Negative:P1})");
                                    }
                                    //speakerTexts.Add(""); // 空行分隔
                                }
                            }

                            result = string.Join(Environment.NewLine, speakerTexts);
                            string allSpeakerText = string.Join(Environment.NewLine, allSpeakers);
                            if (audioFileType == "wav")
                            {
                                result = $"發言者清單{Environment.NewLine}{allSpeakerText}" +
                   $"{Environment.NewLine}{Environment.NewLine}{result}";
                            }

                        }
                        else
                        {
                            // 如果沒有 recognizedPhrases，退回使用 combinedRecognizedPhrases
                            string fullText = string.Join(Environment.NewLine,
                                resultObj.CombinedRecognizedPhrases
                                         .Select(p => $"channel {p.Channel}:\n{p.Display?.Trim()}\n\n")
                                         .Where(s => !string.IsNullOrEmpty(s))
                            );
                            result = fullText;
                        }

                        logger.LogInformation("---- 轉錄完成 ----");
                    }
                }
                break;
            }
            else if (status == "Failed")
            {
                logger.LogError($"轉錄失敗 : {statusJson}");
                break;
            }

            await Task.Delay(TimeSpan.FromSeconds(60));
        }

        return result;
    }

    async Task Save(string filename, string content)
    {
        // 儲存轉錄結果到檔案
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filename);
        string filenameRaw = $"{fileNameWithoutExtension} RAW.md";
        string filenameGpt = $"{fileNameWithoutExtension} GPT.md";
        string outputPath = Path.Combine(Directory.GetCurrentDirectory(), filenameRaw);
        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            await writer.WriteAsync(content);
        }

        outputPath = Path.Combine(Directory.GetCurrentDirectory(), filenameGpt);
        using (StreamWriter writer = new StreamWriter(outputPath))
        {
            await writer.WriteAsync("將這份錄音文稿，整理出一份會議紀錄，說明此次會議的主題、問題處理狀況、討論的重點、代辦事項、決議或者確認事項、潛在問題或疑問、其他補充事項\r\n\r\n");
        }
    }
}
