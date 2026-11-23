using IczpNet.Chat.EntryValues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace IczpNet.Chat.EntryNames;

public class EntrySeedModel
{
    public string Code { get; set; }
    public string Name { get; set; }
    public string InputType { get; set; }
    public string Regex { get; set; }
    public string ErrorMessage { get; set; }
    public int? MaxLength { get; set; }
    public int? MinLength { get; set; }
    public bool IsStatic { get; set; }
    public bool IsChoice { get; set; }
    public bool IsPublic { get; set; } = true;
    public List<string> Options { get; set; } = [];
}

public class EntryNameDataSeedContributor(
    IConfiguration configuration,
    ICurrentTenant currentTenant,
    IRepository<EntryName, Guid> repository,
    IRepository<EntryValue, Guid> entryValueRepository,
    ILogger<EntryNameDataSeedContributor> logger,
    IGuidGenerator guidGenerator) : IDataSeedContributor, ITransientDependency
{
    protected ILogger<EntryNameDataSeedContributor> Logger { get; } = logger;
    protected IConfiguration Configuration { get; } = configuration;
    protected ICurrentTenant CurrentTenant { get; } = currentTenant;

    protected IRepository<EntryName, Guid> Repository { get; } = repository;
    protected IRepository<EntryValue, Guid> EntryValueRepository { get; } = entryValueRepository;
    protected IGuidGenerator GuidGenerator { get; } = guidGenerator;

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        using (CurrentTenant.Change(context?.TenantId))
        {
            await CreateAsync();
        }
    }

    private static readonly List<EntrySeedModel> SystemDefinitions =
    [
        new()
        {
            Code = EntryNameConsts.Phone, Name = "手机", InputType = "Phone",
            Regex = @"^1[3-9]\d{9}$", ErrorMessage = "请输入有效的11位中国大陆手机号码",
            MaxLength = 11, MinLength = 11, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.Tel, Name = "电话", InputType = "Phone",
            Regex = @"^(\d{3,4}-)?\d{7,8}$", ErrorMessage = "请输入有效的固定电话号码",
            MaxLength = 20, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.Email, Name = "Email", InputType = "Email",
            Regex = @"^[\w\.-]+@[\w\.-]+\.\w{2,}$", ErrorMessage = "请输入有效的电子邮箱地址",
            MaxLength = 100, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.WeiChat, Name = "微信号", InputType = "Text",
            Regex = @"^[a-zA-Z][a-zA-Z0-9_-]{5,19}$", ErrorMessage = "微信号必须以字母开头，6-20位字符",
            MaxLength = 20, MinLength = 6, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.Gender, Name = "性别", InputType = "Radio",
            IsStatic = true, IsChoice = true,
            Options = ["男", "女", "保密"]
        },
        new()
        {
            Code = EntryNameConsts.Birthday, Name = "生日", InputType = "Date",
            Regex = @"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "日期格式必须为YYYY-MM-DD",
            MaxLength = 10, MinLength = 10, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.Company, Name = "公司", InputType = "Text",
            MaxLength = 100, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.Department, Name = "部门", InputType = "Text",
            MaxLength = 50, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.Position, Name = "职务", InputType = "Text",
            MaxLength = 50, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.Address, Name = "地址", InputType = "Text",
            MaxLength = 200, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.WorkAddress, Name = "办公地址", InputType = "Textarea",
            MaxLength = 200, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.Website, Name = "网址", InputType = "Url",
            Regex = @"^(https?:\/\/)?[\w.-]+(\.[\w.-]+)+.*$", ErrorMessage = "请输入合法的网站地址",
            MaxLength = 200, IsStatic = true
        },
        new()
        {
            Code = EntryNameConsts.CustomerLevel, Name = "客户等级", InputType = "Select",
            IsStatic = false, IsChoice = true,
            Options = ["普通客户", "重要客户", "核心战略客户", "已流失"]
        },
        new()
        {
            Code = EntryNameConsts.BackupPhone, Name = "备用电话", InputType = "Phone",
            Regex = @"^(\d{3,4}-)?\d{7,8}$|^1[3-9]\d{9}$", ErrorMessage = "请输入合法的手机或固定电话",
            MaxLength = 20, IsStatic = false
        }
    ];

    private async Task CreateAsync()
    {
        foreach (var def in SystemDefinitions)
        {
            var entity = await Repository.FirstOrDefaultAsync(x => x.Code == def.Code);
            if (entity == null)
            {
                entity = await Repository.InsertAsync(new EntryName(GuidGenerator.Create(), def.Name, null)
                {
                    Code = def.Code,
                    InputType = def.InputType,
                    Regex = def.Regex,
                    ErrorMessage = def.ErrorMessage,
                    MaxLenth = def.MaxLength,
                    MinLenth = def.MinLength,
                    IsStatic = def.IsStatic,
                    IsChoice = def.IsChoice,
                    IsPublic = def.IsPublic,
                }, autoSave: true);
                Logger.LogInformation($"Add {nameof(EntryName)}, id={entity.Id}, Code={entity.Code}, Name={entity.Name}");
            }
            else
            {
                // 补齐元数据
                if (string.IsNullOrEmpty(entity.InputType)) entity.InputType = def.InputType;
                if (string.IsNullOrEmpty(entity.Regex)) entity.Regex = def.Regex;
                if (string.IsNullOrEmpty(entity.ErrorMessage)) entity.ErrorMessage = def.ErrorMessage;
                if (!entity.MaxLenth.HasValue && def.MaxLength.HasValue) entity.MaxLenth = def.MaxLength;
                if (!entity.MinLenth.HasValue && def.MinLength.HasValue) entity.MinLenth = def.MinLength;
                entity.IsChoice = def.IsChoice;
                await Repository.UpdateAsync(entity, autoSave: true);
            }

            // 预置选项值
            if (def.IsChoice && def.Options.Count > 0)
            {
                foreach (var opt in def.Options)
                {
                    var optExists = await EntryValueRepository.AnyAsync(x => x.EntryNameId == entity.Id && x.Value == opt);
                    if (!optExists)
                    {
                        await EntryValueRepository.InsertAsync(new EntryValue(GuidGenerator.Create(), entity.Id, opt)
                        {
                            IsOption = true,
                            IsStatic = def.IsStatic,
                            IsPublic = true
                        });
                        Logger.LogInformation($"Add Option {nameof(EntryValue)}, EntryName={entity.Name}, Option={opt}");
                    }
                }
            }
        }
    }
}
