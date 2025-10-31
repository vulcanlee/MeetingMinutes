using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ProjectAssistant.Business.Helpers;
using ProjectAssistant.Business.Repositories;
using ProjectAssistant.Dto.Commons;
using ProjectAssistant.Dto.Models;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;
using System.Linq.Expressions;

namespace ProjectAssistant.Web.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RecordedMediaFileController : ControllerBase
{
    private readonly ILogger<RecordedMediaFileController> logger;
    private readonly RecordedMediaFileRepository RecordedMediaFileRepository;
    private readonly IMapper mapper;
    private readonly Microsoft.AspNetCore.Hosting.IWebHostEnvironment env;

    public RecordedMediaFileController(
        ILogger<RecordedMediaFileController> logger,
        RecordedMediaFileRepository RecordedMediaFileRepository,
        IMapper mapper,
        Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        this.logger = logger;
        this.RecordedMediaFileRepository = RecordedMediaFileRepository;
        this.mapper = mapper;
        this.env = env;
    }

    #region 查詢 API

    /// <summary>
    /// 根據 ID 取得會議影音檔
    /// </summary>
    /// <param name="id">會議影音檔 ID</param>
    /// <param name="includeRelatedData">是否包含關聯資料</param>
    /// <returns></returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResult<RecordedMediaFileDto>>> GetById(int id, [FromQuery] bool includeRelatedData = false)
    {
        try
        {
            var RecordedMediaFile = await RecordedMediaFileRepository.GetByIdAsync(id, includeRelatedData);

            if (RecordedMediaFile == null)
            {
                return NotFound(ApiResult<RecordedMediaFileDto>.NotFoundResult($"找不到 ID 為 {id} 的會議影音檔"));
            }

            var RecordedMediaFileDto = mapper.Map<RecordedMediaFileDto>(RecordedMediaFile);
            return Ok(ApiResult<RecordedMediaFileDto>.SuccessResult(RecordedMediaFileDto, "取得會議影音檔成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "取得會議影音檔 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult<RecordedMediaFileDto>.ServerErrorResult("取得會議影音檔時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 分頁查詢會議影音檔(支援排序、過濾)
    /// </summary>
    /// <param name="request">查詢請求參數</param>
    /// <returns></returns>
    [HttpPost("search")]
    public async Task<ActionResult<ApiResult<PagedResult<RecordedMediaFileDto>>>> Search([FromBody] RecordedMediaFileSearchRequestDto request)
    {
        try
        {
            // 執行分頁查詢
            PagedResult<RecordedMediaFile> pagedResult = await RecordedMediaFileRepository.GetPagedAsync(request);
            var RecordedMediaFileDtos = mapper.Map<List<RecordedMediaFileDto>>(pagedResult.Items);

            var result = new PagedResult<RecordedMediaFileDto>
            {
                Items = RecordedMediaFileDtos,
                TotalCount = pagedResult.TotalCount,
                PageIndex = request.PageIndex,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(pagedResult.TotalCount / (double)request.PageSize)
            };

            return Ok(ApiResult<PagedResult<RecordedMediaFileDto>>.SuccessResult(result, "搜尋會議影音檔成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "搜會議影音檔時發生錯誤");
            return StatusCode(500, ApiResult<PagedResult<RecordedMediaFileDto>>.ServerErrorResult("搜尋會議影音檔時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 新增 API

    /// <summary>
    /// 新增會議影音檔
    /// </summary>
    /// <param name="RecordedMediaFileDto">會議影音檔資料</param>
    /// <returns></returns>
    [HttpPost]
    public async Task<ActionResult<ApiResult<RecordedMediaFileDto>>> Create([FromBody] RecordedMediaFileDto RecordedMediaFileDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult<RecordedMediaFileDto>.ValidationError(errors));
            }

            // 檢查會議影音檔名稱是否重複
            if (await RecordedMediaFileRepository.ExistsByNameAsync(RecordedMediaFileDto.Name))
            {
                return Conflict(ApiResult<RecordedMediaFileDto>.ConflictResult($"會議影音檔名稱 '{RecordedMediaFileDto.Name}' 已存在"));
            }

            // DTO 轉 Entity
            var RecordedMediaFile = mapper.Map<RecordedMediaFile>(RecordedMediaFileDto);
            var createdRecordedMediaFile = await RecordedMediaFileRepository.AddAsync(RecordedMediaFile);

            // Entity 轉 DTO
            var createdRecordedMediaFileDto = mapper.Map<RecordedMediaFileDto>(createdRecordedMediaFile);
            return Ok(ApiResult<RecordedMediaFileDto>.SuccessResult(createdRecordedMediaFileDto, "新增會議影音檔成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "新增會議影音檔時發生錯誤");
            return StatusCode(500, ApiResult<RecordedMediaFileDto>.ServerErrorResult("新增會議影音檔時發生錯誤", ex.Message));
        }
    }

    /// <summary>
    /// 上傳會議影音檔
    /// </summary>
    /// <param name="file">影音檔案</param>
    /// <param name="name">會議影音檔名稱</param>
    /// <param name="meetingId">會議 ID</param>
    /// <returns></returns>
    [HttpPost("upload")]
    [RequestFormLimits(MultipartBodyLengthLimit = 1_073_741_824)] // 1GB
    [RequestSizeLimit(1_073_741_824)]
    public async Task<ActionResult<ApiResult<RecordedMediaFileDto>>> Upload(
        Microsoft.AspNetCore.Http.IFormFile file,
        [FromForm] string name,
        [FromForm] int meetingId)
    {
        try
        {
            // 4) 驗證參數
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResult<RecordedMediaFileDto>.ValidationError("未選擇檔案或檔案內容為空"));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(ApiResult<RecordedMediaFileDto>.ValidationError("名稱不可為空白"));
            }

            if (meetingId <= 0)
            {
                return BadRequest(ApiResult<RecordedMediaFileDto>.ValidationError("會議 ID 不正確"));
            }

            // 檔案類型白名單
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                ".mp3", ".mp4", ".wav", ".m4a", ".aac", ".webm", ".ogg", ".mov", ".mkv"
            };

            var ext = Path.GetExtension(file.FileName);
            if (string.IsNullOrEmpty(ext) || !allowedExtensions.Contains(ext))
            {
                return BadRequest(ApiResult<RecordedMediaFileDto>.ValidationError($"不支援的檔案格式: {ext}"));
            }

            // 名稱重複檢查
            if (await RecordedMediaFileRepository.ExistsByNameAsync(name))
            {
                return Conflict(ApiResult<RecordedMediaFileDto>.ConflictResult($"會議影音檔名稱 '{name}' 已存在"));
            }

            // 5) 準備儲存路徑與檔名
            var webRoot = env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
            {
                webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            }

            var uploadDir = Path.Combine(webRoot, "uploads", "recorded-media");
            Directory.CreateDirectory(uploadDir);

            var saveFileName = $"{Guid.NewGuid():N}{ext}";
            var fullPath = Path.Combine(uploadDir, saveFileName);

            // 寫入實體檔案
            await using (var stream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream);
            }

            // 6) 建立資料
            var entity = new RecordedMediaFile
            {
                Name = name.Trim(),
                FileName = file.FileName,
                SaveFileName = saveFileName,
                Message = null,
                CreatedAt = DateTime.UtcNow,
                MeetingId = meetingId
            };

            var created = await RecordedMediaFileRepository.AddAsync(entity);

            // 7) 回傳結果
            var dto = mapper.Map<RecordedMediaFileDto>(created);
            return Ok(ApiResult<RecordedMediaFileDto>.SuccessResult(dto, "上傳會議影音檔成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "上傳會議影音檔時發生錯誤");
            return StatusCode(500, ApiResult<RecordedMediaFileDto>.ServerErrorResult("上傳會議影音檔時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 更新 API

    /// <summary>
    /// 更新會議影音檔
    /// </summary>
    /// <param name="id">會議影音檔 ID</param>
    /// <param name="RecordedMediaFileDto">會議影音檔資料</param>
    /// <returns></returns>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResult>> Update(int id, [FromBody] RecordedMediaFileDto RecordedMediaFileDto)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                var errors = string.Join("; ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BadRequest(ApiResult.ValidationError(errors));
            }

            if (id != RecordedMediaFileDto.Id)
            {
                return BadRequest(ApiResult.ValidationError("路由 ID 與會議影音檔 ID 不符"));
            }

            // 檢查會議影音檔名稱是否與其他會議影音檔重複
            if (await RecordedMediaFileRepository.ExistsByNameAsync(RecordedMediaFileDto.Name, id))
            {
                return Conflict(ApiResult.ConflictResult($"會議影音檔名稱 '{RecordedMediaFileDto.Name}' 已被其他會議影音檔使用"));
            }

            // DTO 轉 Entity
            var RecordedMediaFile = mapper.Map<RecordedMediaFile>(RecordedMediaFileDto);
            var success = await RecordedMediaFileRepository.UpdateAsync(RecordedMediaFile);

            if (!success)
            {
                return NotFound(ApiResult.NotFoundResult($"找不到 ID 為 {id} 的會議影音檔"));
            }

            return Ok(ApiResult.SuccessResult("更新會議影音檔成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "更新會議影音檔 ID {Id} 時發生錯誤", id);
            return StatusCode(500, ApiResult.ServerErrorResult("更新會議影音檔時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 刪除 API

    /// <summary>
    /// 刪除會議影音檔
    /// </summary>
    /// <param name="id">會議影音檔 ID</param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResult>> Delete(int id)
    {
        try
        {
            var success = await RecordedMediaFileRepository.DeleteAsync(id);

            if (!success)
            {
                return NotFound(ApiResult.NotFoundResult($"找不到 ID 為 {id} 的會議影音檔"));
            }

            return Ok(ApiResult.SuccessResult("刪除會議影音檔成功"));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "刪除會議影音檔 ID {Id} 時發生錯誤", id);

            // 檢查是否為外鍵約束錯誤
            if (ex.InnerException?.Message.Contains("DELETE statement conflicted") == true)
            {
                return BadRequest(ApiResult.FailureResult("無法刪除此會議影音檔,因為有相關的子資料(任務、會議等)存在"));
            }

            return StatusCode(500, ApiResult.ServerErrorResult("刪除會議影音檔時發生錯誤", ex.Message));
        }
    }

    #endregion

    #region 輔助方法

    /// <summary>
    /// 組合 Expression 條件 (AND)
    /// </summary>
    private Expression<Func<RecordedMediaFile, bool>> CombinePredicates(
        Expression<Func<RecordedMediaFile, bool>> first,
        Expression<Func<RecordedMediaFile, bool>> second)
    {
        var parameter = Expression.Parameter(typeof(RecordedMediaFile), "p");

        var leftVisitor = new ReplaceExpressionVisitor(first.Parameters[0], parameter);
        var left = leftVisitor.Visit(first.Body);

        var rightVisitor = new ReplaceExpressionVisitor(second.Parameters[0], parameter);
        var right = rightVisitor.Visit(second.Body);

        return Expression.Lambda<Func<RecordedMediaFile, bool>>(
            Expression.AndAlso(left, right), parameter);
    }

    /// <summary>
    /// 套用排序
    /// </summary>
    private List<RecordedMediaFile> ApplySorting(List<RecordedMediaFile> RecordedMediaFiles, string? sortBy, bool descending)
    {
        if (string.IsNullOrEmpty(sortBy))
        {
            return RecordedMediaFiles;
        }

        return sortBy.ToLower() switch
        {
            "name" => descending
                ? RecordedMediaFiles.OrderByDescending(p => p.Name).ToList()
                : RecordedMediaFiles.OrderBy(p => p.Name).ToList(),
            "status" => descending
                ? RecordedMediaFiles.OrderByDescending(p => p.Status).ToList()
                : RecordedMediaFiles.OrderBy(p => p.Status).ToList(),
            "createdat" => descending
                ? RecordedMediaFiles.OrderByDescending(p => p.CreatedAt).ToList()
                : RecordedMediaFiles.OrderBy(p => p.CreatedAt).ToList(),
            _ => RecordedMediaFiles
        };
    }

    #endregion
}

