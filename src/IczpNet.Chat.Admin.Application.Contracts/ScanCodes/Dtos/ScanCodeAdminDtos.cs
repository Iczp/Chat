using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace IczpNet.Chat.Admin.ScanCodes.Dtos;

public class ScanCodeAdminGetListInput : PagedAndSortedResultRequestDto
{
    public string Keyword { get; set; }

    public string HandlerFullName { get; set; }

    public bool? Success { get; set; }
}

public class ScanCodeAdminHandlerDto : EntityDto<Guid>
{
    public string Action { get; set; }

    public string Handler { get; set; }

    public string HandlerFullName { get; set; }

    public double Execution { get; set; }

    public string Message { get; set; }

    public string Data { get; set; }

    public bool IsSuccess { get; set; }
}

public class ScanCodeAdminDto : EntityDto<Guid>
{
    public string Type { get; set; }

    public string Content { get; set; }

    public string DeviceId { get; set; }

    public Guid? UserId { get; set; }

    public string ClientId { get; set; }

    public double Execution { get; set; }

    public int HandlerCount { get; set; }

    public DateTime CreationTime { get; set; }

    public List<ScanCodeAdminHandlerDto> ScanHandlerList { get; set; } = new();
}

public class ScanCodeAdminSimulateInput
{
    [Required]
    [MaxLength(1024)]
    public string Content { get; set; }

    [MaxLength(36)]
    public string Type { get; set; } = "QR_CODE";

    public string DeviceId { get; set; }
}
