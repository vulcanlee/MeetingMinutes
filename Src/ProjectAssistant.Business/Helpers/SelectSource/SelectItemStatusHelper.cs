using AutoMapper;
using ProjectAssistant.AdapterModels;
using ProjectAssistant.Dto.Models;
using ProjectAssistant.EntityModel.Models;
using ProjectAssistant.Share.Enums;

namespace ProjectAssistant.Business.Helpers;

public class SelectItemStatusHelper
{
    public static List<SelectItemModel> Build()
    {
        return new List<SelectItemModel>
        {
            new SelectItemModel { Key = StatusEnum.未開始.ToString(),  Value = StatusEnum.未開始.ToString() },
            new SelectItemModel { Key = StatusEnum.進行中.ToString(),  Value = StatusEnum.進行中.ToString() },
            new SelectItemModel { Key = StatusEnum.已完成.ToString(),  Value = StatusEnum.已完成.ToString() },
            new SelectItemModel { Key = StatusEnum.已暫停.ToString(),  Value = StatusEnum.已暫停.ToString() },
            new SelectItemModel { Key = StatusEnum.已取消.ToString(),  Value = StatusEnum.已取消.ToString() },
        };
    }
}
